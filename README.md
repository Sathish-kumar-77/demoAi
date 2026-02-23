# UPI Fraud Detection Demo

## Version compatibility
- Node.js: `v22.15.0` ✅
- npm: `10.9.2` ✅
- Angular CLI/packages: `19.2.x` (configured in `frontend/package.json`)
- .NET SDK: 7.0+ (project targets net7.0)
- Python: 3.10+

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
dotnet restore
# create sqlite db (install dotnet-ef tool once if required: dotnet tool install --global dotnet-ef)
dotnet ef migrations add InitialCreate
dotnet ef database update
# run api
dotnet run --urls http://localhost:5000
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
- HTTPS: `UseHttpsRedirection` is disabled for this demo so Angular can call `http://localhost:5000` directly without redirect issues.
- SMTP: Update `backend/appsettings.json` with real SMTP credentials.
- ML JSON contract: backend now maps FastAPI snake_case response keys (`fraud_probability`, `is_fraud`) correctly.


## Frontend security notes (npm audit)
- If you see warnings for deprecated transitive packages like `tar@6` or `glob@10`, this repo now uses `overrides` in `frontend/package.json` to force newer versions where compatible.
- Run:
  ```bash
  cd frontend
  npm install
  npm audit
  npm audit fix
  ```
- If your corporate registry blocks package upgrades, ask your admin to allow the required Angular/npm packages.
- Avoid `npm audit fix --force` unless you are ready to retest the app for breaking changes.

- If Pay shows `API unreachable`, confirm backend is started with: `dotnet run --urls http://localhost:5000`.
