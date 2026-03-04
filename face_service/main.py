import base64
import io
import os
from typing import Optional

import numpy as np
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from PIL import Image
import insightface

FACE_MODEL_NAME = os.getenv("FACE_MODEL_NAME", "buffalo_l")
FACE_CTX_ID = int(os.getenv("FACE_CTX_ID", "-1"))
FACE_DET_CONFIDENCE = float(os.getenv("FACE_DET_CONFIDENCE", "0.6"))
FACE_MATCH_THRESHOLD = float(os.getenv("FACE_MATCH_THRESHOLD", "0.45"))
FACE_MODEL_VERSION = os.getenv("FACE_MODEL_VERSION", "arcface-onnx-v1")

app = FastAPI(title="Face Verification Service")


class RegisterFaceRequest(BaseModel):
    image_base64: str


class VerifyFaceRequest(BaseModel):
    image_base64: str
    stored_embedding_base64: str
    threshold: Optional[float] = None


face_app: Optional[insightface.app.FaceAnalysis] = None


def _decode_image(base64_image: str) -> np.ndarray:
    try:
        image_bytes = base64.b64decode(base64_image)
    except Exception as exc:
        raise HTTPException(status_code=400, detail="Invalid base64 image") from exc

    try:
        img = Image.open(io.BytesIO(image_bytes)).convert("RGB")
    except Exception as exc:
        raise HTTPException(status_code=400, detail="Invalid image payload") from exc

    return np.array(img)[:, :, ::-1]


def _normalize_embedding(emb: np.ndarray) -> np.ndarray:
    emb = np.asarray(emb, dtype=np.float32).reshape(-1)
    norm = np.linalg.norm(emb)
    if norm == 0:
        raise HTTPException(status_code=400, detail="Invalid face embedding")
    return emb / norm


def _extract_embedding(image_bgr: np.ndarray) -> np.ndarray:
    assert face_app is not None
    faces = face_app.get(image_bgr)
    if len(faces) == 0:
        raise HTTPException(status_code=400, detail="No face detected")
    if len(faces) > 1:
        raise HTTPException(status_code=400, detail="Multiple faces detected")

    face = faces[0]
    det_score = float(getattr(face, "det_score", 0.0))
    if det_score < FACE_DET_CONFIDENCE:
        raise HTTPException(status_code=400, detail="Low detection confidence")

    return _normalize_embedding(face.embedding)


def _to_base64(embedding: np.ndarray) -> str:
    return base64.b64encode(embedding.astype(np.float32).tobytes()).decode("utf-8")


def _from_base64_embedding(value: str) -> np.ndarray:
    try:
        raw = base64.b64decode(value)
    except Exception as exc:
        raise HTTPException(status_code=400, detail="Invalid stored embedding base64") from exc

    emb = np.frombuffer(raw, dtype=np.float32)
    if emb.size != 512:
        raise HTTPException(status_code=400, detail="Stored embedding size mismatch")
    return _normalize_embedding(emb)


def cosine_similarity(a: np.ndarray, b: np.ndarray) -> float:
    return float(np.dot(a, b) / (np.linalg.norm(a) * np.linalg.norm(b)))


@app.on_event("startup")
def startup_event() -> None:
    global face_app
    face_app = insightface.app.FaceAnalysis(name=FACE_MODEL_NAME, providers=["CPUExecutionProvider"])
    face_app.prepare(ctx_id=FACE_CTX_ID)


@app.post("/face/register")
async def register_face(request: RegisterFaceRequest):
    image_bgr = _decode_image(request.image_base64)
    embedding = _extract_embedding(image_bgr)

    return {
        "embedding_base64": _to_base64(embedding),
        "embedding_size": int(embedding.size),
        "model_version": FACE_MODEL_VERSION,
    }


@app.post("/face/verify")
async def verify_face(request: VerifyFaceRequest):
    image_bgr = _decode_image(request.image_base64)
    live_emb = _extract_embedding(image_bgr)
    stored_emb = _from_base64_embedding(request.stored_embedding_base64)

    similarity = cosine_similarity(live_emb, stored_emb)
    threshold = request.threshold if request.threshold is not None else FACE_MATCH_THRESHOLD

    return {
        "similarity_score": similarity,
        "match": similarity >= threshold,
        "threshold": threshold,
        "model_version": FACE_MODEL_VERSION,
    }
