from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from sqlalchemy import create_engine
from sklearn.ensemble import RandomForestClassifier
from sklearn.model_selection import train_test_split
from sklearn.tree import export_text, plot_tree
from sklearn.metrics import classification_report, accuracy_score
from sklearn.linear_model import LinearRegression
from fastapi.responses import FileResponse
from typing import Optional
from datetime import datetime, timedelta
import pandas as pd
import numpy as np
import joblib
import os
import logging
import matplotlib.pyplot as plt

# Configuratie paden
MODEL_PATH = "/app/data/model.pkl"
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

class PredictionRequest(BaseModel):
    Humidity: float
    Precipitation: float
    Temp: float
    Windforce: float
    IsHoliday: bool
    IsBredaEvent: bool

# Globale modelvariabele
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

        global model
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
        estimator = model.estimators_[0]

        plt.figure(figsize=(20, 10))
        plot_tree(
            estimator,
            filled=True,
            feature_names=["Humidity", "Precipitation", "Temp", "Windforce", "IsHoliday", "IsBredaEvent"],
            class_names=model.classes_,
            rounded=True
        )
        plt.savefig(PNG_PATH)
        plt.close()

        return FileResponse(PNG_PATH, media_type="image/png")
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Failed to generate decision tree: {e}")

@app.get("/predict-7days")
def predict_7_days():
    try:
        query = """
        SELECT 
            CAST([Timestamp] AS DATE) AS [Date],
            COUNT(*) AS TrashCount,
            MAX([Humidity]) AS Humidity,
            MAX([Precipitation]) AS Precipitation,
            MAX([Temp]) AS Temp,
            MAX([Windforce]) AS Windforce,
            MAX(CAST([IsHoliday] AS INT)) AS IsHoliday,
            MAX(CAST([IsBredaEvent] AS INT)) AS IsBredaEvent
        FROM [dbo].[TrashDetections]
        GROUP BY CAST([Timestamp] AS DATE)
        ORDER BY [Date]
        """
        df = pd.read_sql(query, engine)
        if df.empty:
            raise HTTPException(status_code=404, detail="Geen data beschikbaar voor voorspelling.")

        # Omzetten naar juiste types
        df["IsHoliday"] = df["IsHoliday"].astype(bool)
        df["IsBredaEvent"] = df["IsBredaEvent"].astype(bool)

        # Lineaire regressie voor hoeveelheid afval per dag
        df["DateOrdinal"] = pd.to_datetime(df["Date"]).map(datetime.toordinal)
        X = df[["DateOrdinal"]]
        y = df["TrashCount"]

        lr = LinearRegression()
        lr.fit(X, y)

        future_dates = [datetime.today().date() + timedelta(days=i) for i in range(1, 8)]
        future_ordinals = np.array([[d.toordinal()] for d in future_dates])
        predicted_counts = lr.predict(future_ordinals).round().astype(int)

        if not model:
            raise HTTPException(status_code=503, detail="Afvaltype-model niet geladen.")

        predictions = []
        for i, date in enumerate(future_dates):
            match = df[df["Date"] == pd.Timestamp(date)]
            if match.empty:
                # Dummy weerdata, kan vervangen worden door live weerdata
                sample_input = pd.DataFrame([{
                    "Humidity": 70,
                    "Precipitation": 0.3,
                    "Temp": 18,
                    "Windforce": 3,
                    "IsHoliday": False,
                    "IsBredaEvent": False
                }])
            else:
                sample_input = match[["Humidity", "Precipitation", "Temp", "Windforce", "IsHoliday", "IsBredaEvent"]]

            obj_prediction = model.predict(sample_input)[0]

            predictions.append({
                "date": date.strftime("%Y-%m-%d"),
                "predicted_trash_count": int(predicted_counts[i]),
                "predicted_object": obj_prediction
            })

        return {"forecast": predictions}
    except Exception as e:
        logger.error(f"7-day prediction error: {e}")
        raise HTTPException(status_code=500, detail=str(e))


if __name__ == "__main__":
    import uvicorn
    uvicorn.run("AI_model_api:app", host="127.0.0.1", port=8000, reload=True)
