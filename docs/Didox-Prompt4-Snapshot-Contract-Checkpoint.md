# Didox Prompt 4 — Smoke + Contract Proof Checkpoint

Base URL: `http://localhost:5017`

## 1) Scope lock
- Endpoints:
  - `POST /api/taxes/didox/submit`
  - `POST /api/taxes/didox/status`
  - `POST /api/taxes/didox/cancel`
  - Compatibility alias added: `POST /api/taxes/didox/status/{externalDocumentId}`.
- Auth: `Authorization: Bearer <JWT>` required.
- Provider config: `TaxIntegration.Didox` in `appsettings.json` now includes:
  - `BaseUrl`, `SubmitPath`, `StatusQueryPath`, `CancelPath`
  - `AuthKeyHeaderName`, `AuthSignatureHeaderName`, `AuthTimestampHeaderName`, `AuthNonceHeaderName`, `AuthDateFormat`
- Contract model lock:
  - Request DTOs: `SubmitDidoxRequest`, `StatusDidoxRequest`, `CancelDidoxRequest`
  - Response DTOs: `SubmitDidoxResponse`, `StatusDidoxResponse`, `CancelDidoxResponse`
  - Mapping fallback from `status/message/errors/data/items` supported.

## 2) Runtime proof (snapshot log)

### Collected on environment
- Host check: `http://localhost:5017/api/taxes/didox/submit` with payload and bearer -> `Unable to connect to remote server`.
- Fail-path checks (submit/status/cancel without token and with bad token) also returned `Unable to connect to remote server` at this time because API was not running.
- `Partner-Authorization` string audit:
  - Search result in `src` for `"Partner-Authorization"` / `PartnerAuthorization`: **none found**.

### Required re-run when app is running
Use the following command set and save outputs exactly in JSON + status format:

1. Submit (success payload)
```powershell
$body = '{ "providerCode": "DIDOX", "organizationId": "307797292", "documentNumber": "INV-2026-0001", "externalDocumentId": "ext-0001", "payload": {} }'
$headers = @{ Authorization = "Bearer $env:DIDOX_TOKEN"; "Content-Type" = "application/json" }
Invoke-RestMethod -Method Post -Uri "http://localhost:5017/api/taxes/didox/submit" -Headers $headers -Body $body -ContentType "application/json"
```

2. Status (body payload + by body route)
```powershell
$body = '{ "providerCode": "DIDOX", "organizationId": "307797292", "documentNumber": "INV-2026-0001", "externalDocumentId": "ext-0001", "payload": { "invoiceId": "ext-0001" } }'
Invoke-RestMethod -Method Post -Uri "http://localhost:5017/api/taxes/didox/status" -Headers $headers -Body $body -ContentType "application/json"
```

3. Cancel
```powershell
$body = '{ "providerCode": "DIDOX", "organizationId": "307797292", "documentNumber": "INV-2026-0001", "externalDocumentId": "ext-0001", "payload": { "cancelReason": "test-close" } }'
Invoke-RestMethod -Method Post -Uri "http://localhost:5017/api/taxes/didox/cancel" -Headers $headers -Body $body -ContentType "application/json"
```

4. Failures:
```powershell
# validation example
$body = '{ "providerCode": "", "organizationId": "", "payload": null }'
Invoke-RestMethod -Method Post -Uri "http://localhost:5017/api/taxes/didox/submit" -Body $body -ContentType "application/json"

# unauthorized example
Invoke-RestMethod -Method Post -Uri "http://localhost:5017/api/taxes/didox/submit" -Body $body -ContentType "application/json"
```

## 3) E-IMZO header verification
- Config keys are now fully parametric (`Auth*HeaderName`).
- Provider logs auth header preparation in debug; it prints:
  - header names: `Key`, `Signature`, `Timestamp`, `Nonce` from config
  - signature length and timestamp/nonce values.

## 4) Global exception mapping check
- `401/403/4xx/5xx` handled through `GlobalExceptionHandler` (`IntegrationHttpException` included).
- `401` for missing/invalid bearer is expected for `/api/taxes/didox/*` endpoints.
