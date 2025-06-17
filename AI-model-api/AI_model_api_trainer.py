import pandas as pd
import joblib
import os
from sqlalchemy import create_engine
from sklearn.model_selection import train_test_split
from sklearn.ensemble import RandomForestClassifier

from sklearn.metrics import classification_report, accuracy_score

# Lees de SQL-verbinding uit een omgevingsvariabele
conn_str = os.getenv("SQL_CONNECTION_STRING")
if not conn_str:
    raise ValueError("Missing SQL_CONNECTION_STRING environment variable")

engine = create_engine(conn_str)

query = """
SELECT 
    [DetectedObject], [Humidity],
    [Hour], [Precipitation], [Temp], [Windforce], [IsHoliday]
FROM [dbo].[TrashDetections]
"""
df = pd.read_sql(query, engine)

if df.empty:
    raise ValueError("Geen data opgehaald uit database.")

X = df.drop("DetectedObject", axis=1)
y = df["DetectedObject"]

# Data splitsen in training (70%) en test (30%)
X_train, X_test, y_train, y_test = train_test_split(
    X, y, test_size=0.3, random_state=42, stratify=y
)

# Random Forest model instellen
model = RandomForestClassifier(
    n_estimators=200,          # Aantal bomen in het bos (meer bomen = stabielere resultaten)
    criterion="entropy",       # Gebruik information gain (i.p.v. gini)
    max_depth=20,              # Beperk boomdiepte om overfitting te verminderen
    min_samples_split=2,       # Minimaal aantal samples nodig om een knoop te splitsen
    min_samples_leaf=2,        # Minimaal aantal samples per blad (blad = eindknoop)
    max_features="sqrt",       # Aantal features om te overwegen bij splitsing (wortel van totaal)
    random_state=42,           # Voor reproduceerbaarheid
    n_jobs=-1                  # Gebruik alle beschikbare CPU cores om te versnellen
)

# Train het model
model.fit(X_train, y_train)

# Voorspel op testdata
y_pred = model.predict(X_test)

# Resultaten tonen
print("Accuracy:", accuracy_score(y_test, y_pred))
print("Classification Report:\n", classification_report(y_test, y_pred))

# Model opslaan
try:
    joblib.dump(model, "model.pkl")
    print("Model saved to model.pkl")
except Exception as e:
    print("Error saving model:", e)

print("Working directory:", os.getcwd())
