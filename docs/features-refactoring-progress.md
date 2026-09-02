# Current position

Current area: Cmn
Current feature: Banks
Current phase: AUDIT
Last verified commit: d529e93b
Last successful build: 2026-09-02 — succeeded, 0 errors, 0 warnings in incremental build
Last successful test: 2026-09-02 — UnitTests 135/135; CurrencyMultilanguageQueryTests 3/3 against PostgreSQL 17

# Completed

- Baseline repository/build/test audit.
- Service-layer design approved and committed.
- Isolated worktree `codex/service-layer-refactor` created.
- Initial catalogue: 119 feature/component rows and 326 GET endpoints.
- EF metadata contract verified for 23 dedicated translation entities.
- Corrected the shadow-key mapping in `DocumentStatusTranslation` (`StatusId`, `LanguageId`).
- Reusable PostgreSQL 17 Testcontainers fixture and mutable integration user context.
- `Cmn/Currencies`: list/detail translation, base fallback, translated search, `Code, Id` order and paging verified against PostgreSQL.

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

1. Complete the Phase 0 diff and full-solution verification.
2. Create the remaining-Cmn implementation plan from the inventory.
3. Audit `Cmn/Banks` before changing production code.
