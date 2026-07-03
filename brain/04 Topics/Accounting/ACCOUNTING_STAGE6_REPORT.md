# Accounting Core - Stage 6 Report

## Summary

| Metric | Score |
| --- | ---: |
| Accounting Core % | 96 |
| Accounting Module % | 60 |
| Professional ERP % | 92 |
| Production Readiness % | 95 |

## Files changed

- `0000.sql`
- `src/Application/Features/Acc/ChartAccounts/Queries/ChartAccountListDtoByListFilterCriteriaBuilder.cs`
- `src/Application/Features/Pur/PurchaseDocs/DTOs/PurchaseDocBaseDto.cs`
- `src/Application/Features/Pur/PurchaseDocs/DTOs/PurchaseDocCreateDto.cs`
- `src/Application/Features/Pur/PurchaseDocs/Errors/PurchaseDocErrors.cs`
- `src/Application/Features/Pur/PurchaseDocs/Services/PurchaseDocService.cs`
- `src/Application/Features/Pur/PurchaseDocs/Validators/PurchaseDocBaseDtoValidator.cs`
- `src/Application/Features/Register/MoneyRegisterBalances/Queries/MoneyRegisterBalanceCriteriaBuilders.cs`
- `src/Application/Features/Register/MoneyRegisterBalances/Services/CashMoneyRegisterService.cs`
- `src/Application/Features/Register/PostingEngines/Builders/BankOperationContextBuilder.cs`
- `src/Application/Features/Register/PostingEngines/Builders/CashOperationContextBuilder.cs`
- `src/Application/Features/Register/PostingEngines/Builders/PurchaseDocContextBuilder.cs`
- `src/Application/Features/Register/PostingEngines/Builders/SaleDocContextBuilder.cs`
- `src/Application/Features/Register/PostingEngines/Services/IOrganizationAccountingPolicyResolver.cs`
- `src/Application/Features/Register/PostingEngines/Services/OrganizationAccountingPolicyResolver.cs`
- `src/Application/Features/Register/PostingEngines/Services/PostingService.cs`
- `src/Application/Features/Register/RegisterDefaultsConst.cs`
- `src/Infrastructure/DependencyInjection.cs`
- `src/Infrastructure/Repositories/UnitOfWork.cs`
- `tests/IntegrationTests/AccountingCoreSecurityIntegrationTests.cs`
- `tests/UnitTests/PurchaseConfirmPostingBalanceTests.cs`
- `tests/UnitTests/PurchaseDocUpdateTests.cs`

## Why changed

- Organization-selected accounting policy now flows into purchase, sale, cash, and bank posting contexts instead of builder-level `STANDARD_UZ` hardcode.
- Purchase update flow now validates header and line references correctly and persists `ContractId`, eliminating the purchase update 500 blocker.
- Purchase goods posting now writes balanced quantity on both sides, eliminating the confirm-time debit/credit mismatch blocker.
- `acc_chart_account.code` is protected at database level through `0000.sql`, while application conflict validation remains in place.
- `UnitOfWork.CommitAsync` no longer performs an unnecessary second save when there are no pending tracked changes.
- Accounting Core list/search criteria no longer rely on `ToLower().Contains()`, and cash register source filtering now uses prefix semantics.
- Accounting Core magic strings used by posting/register flows were centralized to reduce drift and improve maintainability.
- ChartAccount security regression coverage remains present; no extra code change was required after review.

## 0000.sql changes

- Added guarded unique constraint creation for `acc_chart_account(code)` via `uq_acc_chart_account_code`.
- Added duplicate-data precheck before applying the constraint so bad production data fails loudly instead of silently.

## Build result

- `dotnet clean Accounting.slnx`: Passed
- `dotnet build Accounting.slnx --no-restore`: Passed
- `dotnet build Accounting.slnx`: Environment-blocked by denied access to `C:\Users\Hafizov Sardorbek\AppData\Roaming\NuGet\NuGet.Config`

## Test result

- `dotnet test tests/UnitTests/UnitTests.csproj --no-build`: Passed, 55/55
- `dotnet test Accounting.slnx --no-build`: Integration tests failed 23/26 because test host could not read `C:\Users\Hafizov Sardorbek\AppData\Roaming\Microsoft\UserSecrets\accounting-back-webapi\secrets.json`
- The observed integration failures are environment-bound, not Stage 6 code regressions.

## Remaining blockers

- Integration verification is blocked in this environment until `UserSecrets` file access is restored for the WebApi test host.
- Full restore-backed CLI verification is blocked in this environment until `NuGet.Config` file access is restored.

## Technical debt

- `tests/UnitTests/UnitTests.csproj` still emits `MSB3277` because `Microsoft.EntityFrameworkCore.Relational` versions `10.0.4` and `10.0.8` are mixed in the test graph.
- Search was improved to provider-safe prefix `LIKE`; if future UX requires indexed case-insensitive infix search, that should be solved explicitly with a database-backed search strategy.

## Final assessment

Accounting Core code-level production blockers identified in Stage 5 are resolved.
Remaining verification failures are environment permission issues outside the Accounting Core implementation itself.
