import pandas as pd
import joblib
import os
from sqlalchemy import create_engine
from sklearn.model_selection import train_test_split
from sklearn.tree import DecisionTreeClassifier
from sklearn.metrics import classification_report, accuracy_score

# Lees de SQL-verbinding uit een omgevingsvariabele
conn_str = os.getenv("SQL_CONNECTION_STRING")
if not conn_str:
    raise ValueError("❌ Missing SQL_CONNECTION_STRING environment variable")

# Verbind met database
engine = create_engine(conn_str)

# Laad data uit database
query = """
SELECT 
    [DetectedObject], [Humidity],
    [Hour], [Precipitation], [Temp], [Windforce], [IsHoliday]
FROM [dbo].[TrashDetections]
"""
df = pd.read_sql(query, engine)

# Check of dataframe niet leeg is
if df.empty:
    raise ValueError("❌ Geen data opgehaald uit database.")

# Splits data in features en label
X = df.drop("DetectedObject", axis=1)
y = df["DetectedObject"]

#  Train/test split
X_train, X_test, y_train, y_test = train_test_split(
    X, y, test_size=0.2, random_state=42
)

# Complexere Decision Tree instellen
model = DecisionTreeClassifier(
    criterion="entropy",     # Gebruik informatie-gain
    max_depth=10,            # Maximale diepte van boom
    min_samples_split=5,     # Minimaal aantal samples voor split
    min_samples_leaf=3,      # Minimaal aantal samples in leaf
    max_features="sqrt",     # Gebruik subset van features
    random_state=42
)

# Train model
model.fit(X_train, y_train)

# Voorspel en evalueer
y_pred = model.predict(X_test)
print("Accuracy:", accuracy_score(y_test, y_pred))
print("Classification Report:\n", classification_report(y_test, y_pred))

# Sla model op
try:
    joblib.dump(model, "model.pkl")
    print(" Model saved to model.pkl")
except Exception as e:
    print(" Error saving model:", e)

# Toon werkdirectory voor controle
print(" Working directory:", os.getcwd())
