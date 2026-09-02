# Current position

Current area: Cmn
Current feature: Contracts
Current phase: AUDIT
Last verified commit: 0f21a1f2
Last successful build: 2026-09-02 — succeeded, 0 errors, 0 warnings in incremental build
Last successful test: 2026-09-02 — UnitTests 140/140; Bank + Currency integration tests 5/5 against PostgreSQL 17

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

1. Audit `Cmn/Contracts` list/detail projections, filter and public DTO layout.
2. Add failing Contract filter and translated ContractType tests.
3. Apply only the translation, validation and mechanical DTO split required by those tests.
