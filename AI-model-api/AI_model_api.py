from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
import joblib
import pandas as pd
import logging

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# Load your pre-trained model file
model = joblib.load("model.pkl")

# Define the request schema
class PredictionRequest(BaseModel):
    Timestamp: str
    DetectedObject: str
    Humidity: float
    Date: str
    Hour: int
    Precipitation: float
    Temp: float
    Windforce: float
    HolidayName: str
    IsHoliday: bool

app = FastAPI()

@app.post("/predict")
def predict(req: PredictionRequest):
    try:
        logger.info(f"Received request: {req.dict()}")
        # Convert input to DataFrame
        input_data = pd.DataFrame([req.dict()])

        # Drop non-numeric or unused columns
        input_data = input_data.drop(columns=["Timestamp", "Date", "HolidayName", "DetectedObject"])

        prediction = model.predict(input_data)
        logger.info(f"Prediction: {prediction[0]}")

        return {"prediction": prediction[0]}
    except Exception as e:
        logger.error(f"Error during prediction: {e}")
        raise HTTPException(status_code=400, detail=str(e))

if __name__ == "__main__":
    import uvicorn
    uvicorn.run("AI_model_api:app", host="0.0.0.0", port=8000, reload=True)
