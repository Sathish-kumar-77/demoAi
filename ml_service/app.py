import os
import joblib
import numpy as np
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel

ARTIFACT_DIR = os.path.join(os.path.dirname(__file__), "artifacts")

app = FastAPI(title="UPI Fraud ML सेवा")

class PredictRequest(BaseModel):
    features: dict


def load_artifacts():
    model_path = os.path.join(ARTIFACT_DIR, "model.pkl")
    scaler_path = os.path.join(ARTIFACT_DIR, "scaler.pkl")
    cols_path = os.path.join(ARTIFACT_DIR, "feature_columns.pkl")
    if not (os.path.exists(model_path) and os.path.exists(scaler_path) and os.path.exists(cols_path)):
        raise FileNotFoundError("Model artifacts not found. Run train.py first.")
    model = joblib.load(model_path)
    scaler = joblib.load(scaler_path)
    cols = joblib.load(cols_path)
    return model, scaler, cols

model, scaler, feature_columns = None, None, None

@app.on_event("startup")
def startup_event():
    global model, scaler, feature_columns
    model, scaler, feature_columns = load_artifacts()

@app.post("/predict")
def predict(request: PredictRequest):
    if model is None:
        raise HTTPException(status_code=500, detail="Model not loaded")
    vector = [request.features.get(col, 0.0) for col in feature_columns]
    X = np.array([vector])
    X_scaled = scaler.transform(X)
    prob = float(model.predict_proba(X_scaled)[0][1])
    is_fraud = prob >= 0.5
    return {"fraud_probability": prob, "is_fraud": is_fraud}
