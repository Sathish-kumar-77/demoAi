# UPI Fraud Detection Demo

## Folder structure
```
frontend/           # Angular app
backend/            # ASP.NET Core Web API
ml_service/         # FastAPI ML service
```

## Run instructions

### 1) ML Service (FastAPI)
```bash
cd ml_service
python -m venv .venv
source .venv/bin/activate
pip install -r requirements.txt
python train.py
uvicorn app:app --host 0.0.0.0 --port 8000
```

### 2) Backend (.NET API)
```bash
cd backend
# restore packages
DOTNET_ENVIRONMENT=Development dotnet restore
# create sqlite db
DOTNET_ENVIRONMENT=Development dotnet ef migrations add InitialCreate
DOTNET_ENVIRONMENT=Development dotnet ef database update
# run api
DOTNET_ENVIRONMENT=Development dotnet run --urls http://localhost:5000
```

### 3) Frontend (Angular)
```bash
cd frontend
npm install
npm start
```

## Example requests

### Register
```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"Pass@123"}'
```

### Login
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"Pass@123"}'
```
Response:
```json
{"token":"<jwt>"}
```

### Pay transaction
```bash
curl -X POST http://localhost:5000/api/transactions/pay \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <jwt>" \
  -d '{"upiId":"name@bank","amount":4200,"note":"groceries","deviceId":"pixel-7","city":"Mumbai"}'
```
Response:
```json
{
  "transactionId": 1,
  "isFraud": false,
  "fraudProbability": 0.12,
  "reasons": ["Behavior within normal range"]
}
```

### History
```bash
curl -X GET http://localhost:5000/api/transactions/history \
  -H "Authorization: Bearer <jwt>"
```

## Troubleshooting
- CORS: Backend allows `http://localhost:4200`. Update in `Program.cs` if needed.
- HTTPS: `dotnet run` uses HTTP. If HTTPS is enforced in your setup, update Angular API base URL.
- SMTP: Update `backend/appsettings.json` with real SMTP credentials.
