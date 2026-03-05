import os
import joblib
import numpy as np
import pandas as pd
from sklearn.model_selection import train_test_split
from sklearn.preprocessing import StandardScaler
from xgboost import XGBClassifier
from imblearn.over_sampling import SMOTE

ARTIFACT_DIR = os.path.join(os.path.dirname(__file__), "artifacts")

FEATURE_COLUMNS = [
    "amount",
    "hour",
    "device_risk",
    "location_risk",
    "velocity_risk",
    "note_length",
]


def load_or_generate_data():
    credit_path = os.path.join(os.path.dirname(__file__), "creditcard.csv")
    if os.path.exists(credit_path):
        df = pd.read_csv(credit_path)
        df = df.sample(5000, random_state=42) if len(df) > 5000 else df
        df["amount"] = df["Amount"]
        df["hour"] = (df["Time"] // 3600) % 24
        df["device_risk"] = np.random.choice([0, 1], size=len(df), p=[0.85, 0.15])
        df["location_risk"] = np.random.choice([0, 1], size=len(df), p=[0.8, 0.2])
        df["velocity_risk"] = np.random.beta(2, 5, size=len(df))
        df["note_length"] = np.random.randint(0, 120, size=len(df))
        df["is_fraud"] = df["Class"]
        return df[FEATURE_COLUMNS + ["is_fraud"]]

    rng = np.random.default_rng(42)
    size = 3000
    df = pd.DataFrame({
        "amount": rng.gamma(2.0, 1500.0, size=size),
        "hour": rng.integers(0, 24, size=size),
        "device_risk": rng.choice([0, 1], size=size, p=[0.85, 0.15]),
        "location_risk": rng.choice([0, 1], size=size, p=[0.8, 0.2]),
        "velocity_risk": rng.beta(2, 5, size=size),
        "note_length": rng.integers(0, 120, size=size),
    })
    base_risk = (
        0.25 * (df["amount"] > 5000).astype(int)
        + 0.2 * df["device_risk"]
        + 0.2 * df["location_risk"]
        + 0.2 * (df["velocity_risk"] > 0.7).astype(int)
    )
    df["is_fraud"] = (base_risk + rng.normal(0, 0.1, size=size) > 0.5).astype(int)
    return df


def main():
    os.makedirs(ARTIFACT_DIR, exist_ok=True)
    df = load_or_generate_data()
    X = df[FEATURE_COLUMNS]
    y = df["is_fraud"]

    X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42, stratify=y)

    smote = SMOTE(random_state=42)
    X_res, y_res = smote.fit_resample(X_train, y_train)

    scaler = StandardScaler()
    X_scaled = scaler.fit_transform(X_res)

    model = XGBClassifier(
        n_estimators=200,
        max_depth=4,
        learning_rate=0.1,
        subsample=0.9,
        colsample_bytree=0.9,
        eval_metric="logloss",
        random_state=42,
    )
    model.fit(X_scaled, y_res)

    joblib.dump(model, os.path.join(ARTIFACT_DIR, "model.pkl"))
    joblib.dump(scaler, os.path.join(ARTIFACT_DIR, "scaler.pkl"))
    joblib.dump(FEATURE_COLUMNS, os.path.join(ARTIFACT_DIR, "feature_columns.pkl"))
    print("Model artifacts saved to", ARTIFACT_DIR)


if __name__ == "__main__":
    main()
