import pandas as pd
import joblib
from sqlalchemy import create_engine
from sklearn.model_selection import train_test_split
from sklearn.tree import DecisionTreeClassifier
import os

# Read connection string from env varimport os
conn_str = os.getenv("SQL_CONNECTION_STRING")
if not conn_str:
    raise ValueError("Missing SQL_CONNECTION_STRING environment variable")

engine = create_engine(conn_str)

# Load data
query = """
SELECT 
    [DetectedObject], [Humidity],
    [Hour], [Precipitation], [Temp], [Windforce], [IsHoliday]
FROM [dbo].[TrashDetections]
"""
df = pd.read_sql(query, engine)

# Preprocess
X = df.drop("DetectedObject", axis=1)
y = df["DetectedObject"]

# Train
X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42)
model = DecisionTreeClassifier()
model.fit(X_train, y_train)

# Save model
try:
    joblib.dump(model, "model.pkl")
    print("Model saved to model.pkl")
except Exception as e:
    print("Error saving model:", e)

print("Working directory:", os.getcwd())
