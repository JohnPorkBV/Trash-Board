from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from sqlalchemy import create_engine
from sklearn.ensemble import RandomForestClassifier
from sklearn.model_selection import train_test_split
from sklearn.tree import export_text
from sklearn.metrics import classification_report, accuracy_score
import joblib
import pandas as pd
import os
import logging
import matplotlib.pyplot as plt
from sklearn.tree import plot_tree
from fastapi.responses import FileResponse
from typing import Optional
from pydantic import BaseModel

MODEL_PATH = "/app/data/model.pkl"
PNG_PATH = "/app/data/decision_tree.png"
DATA_PATH = "/app/data"
os.makedirs(DATA_PATH, exist_ok=True)

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)
app = FastAPI()

# SQL connection
conn_str = os.getenv("SQL_CONNECTION_STRING")
if not conn_str:
    raise ValueError("Missing SQL_CONNECTION_STRING environment variable")
engine = create_engine(conn_str)

class TrainRequest(BaseModel):
    n_estimators: Optional[int] = 100
    criterion: Optional[str] = "entropy"
    max_depth: Optional[int] = 15
    min_samples_split: Optional[int] = 5
    min_samples_leaf: Optional[int] = 3
    max_features: Optional[str] = "sqrt"

# Request model
class PredictionRequest(BaseModel):
    Humidity: float
    Precipitation: float
    Temp: float
    Windforce: float
    IsHoliday: bool
    IsBredaEvent: bool

model = None

def load_model():
    global model
    if os.path.exists(MODEL_PATH):
        model = joblib.load(MODEL_PATH)
        logger.info("Model loaded from disk.")
    else:
        logger.warning("No trained model found.")

load_model()

@app.post("/predict")
def predict(req: PredictionRequest):
    try:
        if not model:
            raise HTTPException(status_code=503, detail="Model not trained yet.")
        input_df = pd.DataFrame([req.dict()])
        prediction = model.predict(input_df)
        return {"prediction": prediction[0]}
    except Exception as e:
        logger.error(f"Prediction error: {e}")
        raise HTTPException(status_code=400, detail=str(e))

@app.post("/train")
def train_model(params: TrainRequest):
    try:
        conn_str = os.getenv("SQL_CONNECTION_STRING")
        if not conn_str:
            raise ValueError("Missing SQL_CONNECTION_STRING environment variable")
        engine = create_engine(conn_str)

        query = """
        SELECT 
            [DetectedObject], [Humidity], [Precipitation], [Temp], [Windforce], [IsHoliday], [IsBredaEvent]
        FROM [dbo].[TrashDetections]
        """
        df = pd.read_sql(query, engine)
        if df.empty:
            raise ValueError("No data retrieved from database.")

        X = df.drop("DetectedObject", axis=1)
        y = df["DetectedObject"]

        X_train, X_test, y_train, y_test = train_test_split(
            X, y, test_size=0.3, random_state=42, stratify=y
        )

        model = RandomForestClassifier(
            n_estimators=params.n_estimators,
            criterion=params.criterion,
            max_depth=params.max_depth,
            min_samples_split=params.min_samples_split,
            min_samples_leaf=params.min_samples_leaf,
            max_features=params.max_features,
            random_state=42,
            n_jobs=-1
        )

        model.fit(X_train, y_train)
        y_pred = model.predict(X_test)

        accuracy = accuracy_score(y_test, y_pred)
        report = classification_report(y_test, y_pred, output_dict=True)

        joblib.dump(model, MODEL_PATH)

        logger.info("Model retrained and saved successfully.")
        load_model()
        


        return {
            "message": "Model trained successfully.",
            "accuracy": accuracy,
            "report": report
        }

    except Exception as e:
        logger.error(f"Training error: {e}")
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/tree")
def get_tree():
    try:
        if not model:
            raise HTTPException(status_code=503, detail="Model not trained yet.")
        tree_text = export_text(model.estimators_[0], feature_names=["Humidity", "Precipitation", "Temp", "Windforce", "IsHoliday", "IsBredaEvent"])
        return {"tree": tree_text}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/decision-tree")
def get_decision_tree():
    try:
        if not model:
            raise HTTPException(status_code=503, detail="Model not trained yet.")
        # Load model (if not already loaded globally)
        #model = joblib.load("data/model.pkl")

        # Use first tree from forest
        estimator = model.estimators_[0]

        # Create figure
        plt.figure(figsize=(20, 10))
        plot_tree(
            estimator,
            filled=True,
            feature_names=["Humidity", "Precipitation", "Temp", "Windforce", "IsHoliday", "IsBredaEvent"],
            class_names=model.classes_,
            rounded=True
        )

        # Save to file
        plt.savefig(PNG_PATH)
        plt.close()

        # Serve the image file
        return FileResponse(PNG_PATH, media_type="image/png")
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Failed to generate decision tree: {e}")



if __name__ == "__main__":
    import uvicorn
    uvicorn.run("AI_model_api:app", host="127.0.0.1", port=8000, reload=True)