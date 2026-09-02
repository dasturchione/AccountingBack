# Current position

Current area: Cmn
Current feature: Documents
Current phase: RED_TESTS
Last verified commit: a2876666
Last successful build: 2026-09-02 — succeeded, 0 errors, 0 warnings in incremental build
Last successful test: 2026-09-02 — UnitTests 153/153; CurrencyRevaluation integration tests 2/2 against PostgreSQL 17

# Completed

- Baseline repository/build/test audit.
- Service-layer design approved and committed.
- Isolated worktree `codex/service-layer-refactor` created.
- Initial catalogue: 119 feature/component rows and 326 GET endpoints.
- EF metadata contract verified for 23 dedicated translation entities.
- Corrected the shadow-key mapping in `DocumentStatusTranslation` (`StatusId`, `LanguageId`).
- Reusable PostgreSQL 17 Testcontainers fixture and mutable integration user context.
- `Cmn/Currencies`: list/detail translation, base fallback, translated search, `Code, Id` order and paging verified against PostgreSQL.
- Phase 0 full diff/API review and full-solution verification.
- `Cmn/Banks`: all four GET contracts, localized errors and list-filter validation verified; base names retained because no translation model exists.
- `Cmn/Contracts`: list/detail ContractType translation with base fallback, translated search, organization scope, date-desc paging and filter validation verified; reconciliation request/response DTOs split without contract changes.
- `Cmn/CurrencyRates`: list/detail/latest/history currency names localized with per-currency fallback; filtering, dynamic ordering and pagination moved into SQL; latest-rate ordering corrected to `EffectiveDate desc, Id desc`; single-result repositories now honor specification result criteria and ordering instead of silently discarding them.
- `Cmn/CurrencyRevaluations`: public DTOs, validators and projections split into focused files with reflection coverage; list organization filtering, stable ordering, count and paging moved into SQL; detail line amounts/rates and organization scope verified without touching lifecycle/accounting behavior.

# Modified but not verified

- None.

# Blocked

- None.

# Translation decisions

- Current fallback: requested `IUserContext.LanguageId` translation, then base field.
- Inline projection exceptions: internal one-use/read models and common `SelectListDto` only when documented.
- SelectListDto exceptions: no dedicated builder required, but inline SQL projection must translate display fields.
- Internal DTO exceptions: internal/private state-machine and provider models stay grouped.

# Next exact action

1. Add PostgreSQL RED tests for all translated `Cmn/Documents` display fields.
2. Localize document type, status and currency with independent base fallbacks.
3. Rename the grouped DTO file mechanically and verify the unchanged no-pagination API contract.
