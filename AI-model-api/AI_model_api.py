from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from sqlalchemy import create_engine
from sklearn.ensemble import RandomForestRegressor
from sklearn.ensemble import RandomForestClassifier
from sklearn.multioutput import MultiOutputRegressor
from sklearn.model_selection import train_test_split
from sklearn.tree import export_text, plot_tree
from sklearn.metrics import classification_report, accuracy_score
from fastapi.responses import FileResponse
from typing import Optional
import pandas as pd
import numpy as np
import joblib
import os
import logging
import matplotlib.pyplot as plt

# Configuratie paden
TREE_MODEL_PATH = "/app/data/treemodel.pkl"
MULTI_MODEL_PATH = "/app/data/multimodel.pkl"
PNG_PATH = "/app/data/decision_tree.png"
DATA_PATH = "/app/data"
os.makedirs(DATA_PATH, exist_ok=True)

# Logging setup
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# FastAPI app
app = FastAPI()

# Database connectie
conn_str = os.getenv("SQL_CONNECTION_STRING")
if not conn_str:
    raise ValueError("Missing SQL_CONNECTION_STRING environment variable")
engine = create_engine(conn_str)

# Pydantic modellen
class TrainRequest(BaseModel):
    n_estimators: Optional[int] = 100
    criterion: Optional[str] = "entropy"
    max_depth: Optional[int] = 15
    min_samples_split: Optional[int] = 5
    min_samples_leaf: Optional[int] = 3
    max_features: Optional[str] = "sqrt"
    random_state: Optional[int] = 42

class PredictionRequest(BaseModel):
    Humidity: float
    Precipitation: float
    Temp: float
    Windforce: float
    IsHoliday: bool
    IsBredaEvent: bool

# Globale modelvariabele
tree_model = None
multi_model = None

def load_model():
    global tree_model
    if os.path.exists(TREE_MODEL_PATH):
        tree_model = joblib.load(TREE_MODEL_PATH)
        logger.info("Tree Model loaded from disk.")
    else:
        logger.warning("No trained model found.")
    global multi_model
    if os.path.exists(MULTI_MODEL_PATH):
        multi_model = joblib.load(MULTI_MODEL_PATH)
        logger.info("Multi Model loaded from disk.")
    else:
        logger.warning("No trained model found.")

load_model()

@app.post("/predict")
def predict(req: PredictionRequest):
    try:
        if not tree_model:
            raise HTTPException(status_code=503, detail="Model not trained yet.")
        input_df = pd.DataFrame([req.dict()])
        prediction = tree_model.predict(input_df)
        return {"prediction": prediction[0]}
    except Exception as e:
        logger.error(f"Prediction error: {e}")
        raise HTTPException(status_code=400, detail=str(e))

@app.post("/train")
def train_model(params: TrainRequest):
    try:
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

        tree_model = RandomForestClassifier(
            n_estimators=params.n_estimators,
            criterion=params.criterion,
            max_depth=params.max_depth,
            min_samples_split=params.min_samples_split,
            min_samples_leaf=params.min_samples_leaf,
            max_features=params.max_features,
            random_state=params.random_state,
            n_jobs=-1
        )

        tree_model.fit(X_train, y_train)
        y_pred = tree_model.predict(X_test)

        accuracy = accuracy_score(y_test, y_pred)
        report = classification_report(y_test, y_pred, output_dict=True)

        joblib.dump(tree_model, TREE_MODEL_PATH)


        

        query = """
        SELECT 
            CAST([Timestamp] AS DATE) AS [Date],
            DetectedObject,
            COUNT(*) AS Count,
            MAX([Humidity]) AS Humidity,
            MAX([Precipitation]) AS Precipitation,
            MAX([Temp]) AS Temp,
            MAX([Windforce]) AS Windforce,
            MAX(CAST([IsHoliday] AS INT)) AS IsHoliday,
            MAX(CAST([IsBredaEvent] AS INT)) AS IsBredaEvent
        FROM [dbo].[TrashDetections]
        GROUP BY CAST([Timestamp] AS DATE), DetectedObject
        ORDER BY [Date]
        """
        df = pd.read_sql(query, engine)
        if df.empty:
            raise ValueError("No data retrieved from database.")

        # Pivot: rows = dates & features, columns = trash types with counts
        df_pivot = df.pivot_table(
            index=["Date", "Humidity", "Precipitation", "Temp", "Windforce", "IsHoliday", "IsBredaEvent"],
            columns="DetectedObject",
            values="Count",
            fill_value=0
        ).reset_index()

        feature_cols = ["Humidity", "Precipitation", "Temp", "Windforce", "IsHoliday", "IsBredaEvent"]
        target_cols = df_pivot.columns.difference(["Date"] + feature_cols).tolist()

        X = df_pivot[feature_cols]
        y = df_pivot[target_cols]

        # Use RandomForestRegressor inside MultiOutputRegressor for multi-target regression
        base_model = RandomForestRegressor(
            n_estimators=params.n_estimators,
            max_depth=params.max_depth,
            min_samples_split=params.min_samples_split,
            min_samples_leaf=params.min_samples_leaf,
            max_features=params.max_features,
            random_state=params.random_state,
            n_jobs=-1
        )
        multi_model = MultiOutputRegressor(base_model)
        multi_model.fit(X, y)

        # Save the multi-output regression model and target columns (trash types)
        joblib.dump(multi_model, MULTI_MODEL_PATH)
        joblib.dump(target_cols, os.path.join(DATA_PATH, "target_columns.joblib"))

        logger.info("Both models retrained and saved successfully.")
        load_model()

        return {
            
            "message": "Model trained successfully.",
            "accuracy": accuracy,
            "report": report,
           "trash_types": target_cols}

    except Exception as e:
        logger.error(f"Training error: {e}")
        raise HTTPException(status_code=500, detail=str(e))


@app.get("/tree")
def get_tree():
    try:
        if not tree_model:
            raise HTTPException(status_code=503, detail="Model not trained yet.")
        tree_text = export_text(tree_model.estimators_[0], feature_names=["Humidity", "Precipitation", "Temp", "Windforce", "IsHoliday", "IsBredaEvent"])
        return {"tree": tree_text}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/decision-tree")
def get_decision_tree():
    try:
        if not tree_model:
            raise HTTPException(status_code=503, detail="Model not trained yet.")
        estimator = tree_model.estimators_[0]

        plt.figure(figsize=(20, 10))
        plot_tree(
            estimator,
            filled=True,
            feature_names=["Humidity", "Precipitation", "Temp", "Windforce", "IsHoliday", "IsBredaEvent"],
            class_names=tree_model.classes_,
            rounded=True
        )
        plt.savefig(PNG_PATH)
        plt.close()

        return FileResponse(PNG_PATH, media_type="image/png")
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Failed to generate decision tree: {e}")


class DayPredictionRequest(BaseModel):
    date: str  # e.g., "2025-06-16"
    Humidity: float
    Precipitation: float
    Temp: float
    Windforce: float
    IsHoliday: bool
    IsBredaEvent: bool
# Updated /predict-day endpoint
@app.post("/predict-day")
def predict_single_day(input: DayPredictionRequest):
    try:
        if not multi_model:
            raise HTTPException(status_code=503, detail="Model not trained.")

        # Load the target trash types
        target_cols = joblib.load(os.path.join(DATA_PATH, "target_columns.joblib"))

        features = pd.DataFrame([{
            "Humidity": input.Humidity,
            "Precipitation": input.Precipitation,
            "Temp": input.Temp,
            "Windforce": input.Windforce,
            "IsHoliday": input.IsHoliday,
            "IsBredaEvent": input.IsBredaEvent
        }])

        predicted_counts = multi_model.predict(features)[0]

        # Return dict {trash_type: predicted_amount}
        predictions = {trash_type: float(round(count, 2)) for trash_type, count in zip(target_cols, predicted_counts)}

        # Optionally, predict total trash count with linear regression (like before) or sum predicted counts
        total_predicted_count = sum(predicted_counts)

        return {
            "date": input.date,
            "predicted_total_trash_count": total_predicted_count,
            "predicted_trash_counts": predictions
        }

    except Exception as e:
        logger.error(f"Single day prediction error: {e}")
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/predict-object")
def predict_single_object(input: PredictionRequest):
    try:
        if not multi_model:
            raise HTTPException(status_code=503, detail="Model not trained.")

        features = pd.DataFrame([input.dict()])
        prediction = multi_model.predict(features)[0]

        return {
            "predicted_object": str(prediction)
        }

    except Exception as e:
        logger.error(f"Single object prediction error: {e}")
        raise HTTPException(status_code=500, detail=str(e))

if __name__ == "__main__":
    import uvicorn
    uvicorn.run("AI_model_api:app", host="127.0.0.1", port=8000, reload=True)
