# Current position

Current area: Cmn
Current feature: Banks
Current phase: AUDIT
Last verified commit: 1fb04129
Last successful build: 2026-09-02 — full solution succeeded, 0 errors, 2 baseline CS8629 warnings
Last successful test: 2026-09-02 — UnitTests 135/135; IntegrationTests 4/4 against PostgreSQL 17

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

1. Create and review the remaining-`Cmn` implementation plan from the inventory.
2. Audit `Cmn/Banks` before changing production code.
3. Add failing PostgreSQL characterization tests for the first incorrect `Cmn/Banks` GET behavior.
