# Accounting Core Final Production Audit

Date: 2026-07-03
Module Stage: 5/10 Accounting Core Final Production Audit

## Accounting Core Score
8.1/10

## Professional ERP Score
7.7/10

## Production Readiness
Not ready for final production sign-off.

## Architecture Score
8.0/10

Strengths:
- Posting, register, lifecycle and period validation responsibilities are separated reasonably well.
- CQRS-style read/write separation exists through `IQueryRepository` / `ICommandRepository`.
- Transaction boundaries and document-level advisory locks are present in lifecycle services.
- Optimistic concurrency was added for key accounting entities through `xmin`.

Gaps:
- `CommandRepository` persists inside each method, so UnitOfWork is not a pure deferred commit boundary.
- Organization accounting policy is configurable in setup, but posting builders still hardcode `STANDARD_UZ`.

## Security Score
7.4/10

Confirmed finding:
- `src/Presentation/WebApi/Controllers/Acc/ChartAccountController.cs`
  `acc_chart_account` is global master data and is not organization-scoped in EF filters or save guards. Mutation endpoints were protected only by module permissions, so an organization user with `CHART_ACCOUNT_CREATE/UPDATE/DELETE` could modify shared chart-of-accounts data across all tenants. This is a real cross-tenant privilege-escalation and data-isolation risk.

Fix applied:
- Added `GlobalAccessAuthorize` on `POST`, `PUT`, `DELETE` while keeping `ModuleAuthorize(...)`.
- Added regression coverage in `tests/IntegrationTests/AccountingCoreSecurityIntegrationTests.cs`.

Residual security risks:
- `src/Application/Features/Register/PostingEngines/Builders/BankOperationContextBuilder.cs:109`
- `src/Application/Features/Register/PostingEngines/Builders/CashOperationContextBuilder.cs:32`
- `src/Application/Features/Register/PostingEngines/Builders/PurchaseDocContextBuilder.cs:51`
- `src/Application/Features/Register/PostingEngines/Builders/SaleDocContextBuilder.cs:115`
  Posting still ignores organization-selected accounting policy and always resolves accounts with `STANDARD_UZ`.

## Performance Score
7.8/10

Assessment:
- Core accounting reads mostly use projections and avoid obvious N+1 patterns in the reviewed flows.
- Lifecycle services use document locks and targeted queries appropriately.
- Search builders such as `src/Application/Features/Acc/ChartAccounts/Queries/ChartAccountListDtoByListFilterCriteriaBuilder.cs` still use `ToLower().Contains(...)`, which is index-unfriendly.

## Maintainability Score
7.6/10

Findings:
- `src/Infrastructure/BackgroundServices/AdjustBalanceJob.cs:25` still contains `TODO`.
- `src/Infrastructure/Persistence/AppDbContext/AppDbContext.OrganizationScope.cs:54` contains commented tenant-filter code for `ChartAccount`, which is confusing even if chart accounts are currently global master data.
- `acc_chart_account.code` uniqueness is enforced only at application level, not at database level.

## Accounting Logic
Status:
- Chart of accounts structure, posting rules, posting batches, reversal linking and register storage are implemented.
- Group-account posting protection exists through `AccountingDispatcher.EnsureNoGroupAccountsAsync`.

Blocking logic gap:
- `src/Application/Features/Organization/Setup/Services/OrganizationSetupService.cs:206`
  Accounting policy can be configured per organization.
- But posting builders still hardcode `AccountingPolicyIdConst.STANDARD_UZ`, so the selected policy does not affect postings.

## Transaction
Status:
- Lifecycle services use `ExecuteInTransactionAsync(...)`.
- Advisory transaction locks exist via `DocumentPostingLock`.
- Reversal logic links back to original entries with `ReversalEntryId`.

Concern:
- Repository methods call `SaveChangesAsync()` immediately, which weakens strict UnitOfWork purity even though the surrounding database transaction still protects atomicity.

## Files Changed
- `src/Presentation/WebApi/Controllers/Acc/ChartAccountController.cs`
- `tests/IntegrationTests/AccountingCoreSecurityIntegrationTests.cs`
- `ACCOUNTING_FINAL_AUDIT_REPORT.md`
- `brain/04 Topics/Accounting/ACCOUNTING_FINAL_AUDIT_REPORT.md`

## Why Changed
- Closed a real global master-data mutation gap on chart-account mutations.
- Added regression coverage for the access-control gap.
- Wrote the final audit report in both requested locations.

## Build
- `dotnet clean Accounting.slnx`: Success
- `dotnet restore Accounting.slnx`: Success after unsandboxed restore approval because sandbox blocked global NuGet config/user profile access
- `dotnet build Accounting.slnx --no-restore`: Success
- Build warning observed:
  `tests/UnitTests/UnitTests.csproj` has an EF Core Relational version conflict (`10.0.4` vs `10.0.8`)

## Test
- `dotnet test Accounting.slnx --no-build --no-restore`: Failed
- Unit tests: 51 passed, 1 failed, 52 total
- Integration tests: 0 passed, 26 failed, 26 total

Observed failure mode:
- The dominant failures were environment/host-policy related, not business-assertion failures:
  - `Application Control policy has blocked ... Infrastructure.dll`
  - sandbox/user-profile restrictions around secrets on non-escalated runs

Impact:
- Full automated verification is not green, so final production sign-off is not justified.

## 0000.sql Changes
None.

## Remaining Risks
- Organization-selected accounting policy is not honored during posting.
- `acc_chart_account.code` is not protected by a database uniqueness constraint.
- Full test suite is not green in the current environment, so regression confidence is incomplete.
- EF Core package-version mismatch warning exists in unit-test build output.

## Technical Debt
- Repository pattern and UnitOfWork semantics are mixed because command repositories save immediately.
- Search filters rely on `ToLower().Contains(...)` rather than index-friendly normalized search.
- Test infrastructure is sensitive to host machine policy and environment-specific secrets loading.

## Final Conclusion
❌ Production Ready emas

Sabablari:
- Real security issue topildi va minimal fix qilindi, lekin accounting policy selection hali posting engine tomonidan ishlatilmayapti.
- Full verification green emas: `dotnet test` host policy/environment sabab yakuniy sign-off darajasiga yetmagan.
- Chart account code integrity hali database darajasida mustahkamlanmagan.
