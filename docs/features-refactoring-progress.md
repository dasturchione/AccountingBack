# Current position

Current area: Org
Current feature: Positions
Current phase: AUDIT
Last verified commit: 22a6d8f6
Last successful build: 2026-09-02 — succeeded, 0 errors, 0 warnings after Branches
Last successful test: 2026-09-02 — Department unit 5/5; Department integration 2/2

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
- `Cmn/Documents`: document type, status and currency names localized independently with base fallback and null preservation; translated search and stable no-pagination ordering verified; explicit organization scope now also protects a super-admin working in a selected organization; DTO file and filter validation normalized.
- `Cmn/PricingConditions`: all three GET contracts verified with intentional base names, SQL-side scoped list/paging and deterministic current-effective selection; list filter validation added and pricing-condition errors completed for Russian and Uzbek Cyrillic.
- `Cmn/Taxes`: four public provider-integration DTOs split without JSON contract changes; VAT list state filtering fixed, SQL search/order/paging verified, organization/effective-date resolution and inclusive/exclusive formulas covered against PostgreSQL; local errors completed for Russian and Uzbek Cyrillic while external provider text remains untouched.
- `Cmn/Manual` translations: all 19 translation-backed select lists now use requested language with base fallback inside SQL; the nine previously direct-name methods were corrected and all existing translated methods are regression-covered for ordering, active-state behavior where supported and duplicate prevention.
- `Cmn/Manual` contracts: grouped public DTO declarations split into same-named files with reflection coverage; SuperAdmin/TenantAdmin/TenantUser organization visibility, selected-organization resources, branch/bank/counterparty/product filters and deterministic module grouping verified against PostgreSQL; all 55 manual GET paths audited.
- `Cmn/Barcode`: all three controller/generator boundaries verified for PNG success and localized invalid-EAN13 ProblemDetails; no production change was required.
- Complete `Cmn` checkpoint: build 0 warnings/0 errors, all 198 tests green, no controller/service-interface/SQL-script diff from the phase baseline, and zero DTO files with multiple public DTO declarations.
- `Org/Branches`: list/detail/create/update/delete and code uniqueness explicitly scoped to the selected organization, including SuperAdmin; missing organization context now returns localized `CommonErrors`; list filters, base-name projection, search, stable order and SQL paging verified against PostgreSQL.
- `Org/Departments`: every operation and code uniqueness explicitly scoped to the selected organization; branch references must belong to that organization; list filter, base-name nested fields, branch filter, search, stable order, SQL paging and localized errors verified against PostgreSQL.

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

1. Write PostgreSQL characterization tests for `Org/Positions` selected-organization isolation.
2. Add exact `PositionListFilter` validation and fix confirmed scope defects.
3. Verify all three Org features together before moving to Organization.
