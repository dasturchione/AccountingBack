# Current position

Current area: Cmn
Current feature: Currencies
Current phase: CURRENCY_CHARACTERIZATION
Last verified commit: 5123445e
Last successful build: 2026-08-31 — succeeded, 0 errors, 2 baseline CS8629 warnings
Last successful test: 2026-08-31 — TranslationModelContract 1/1; PostgreSQLSmoke 1/1 against PostgreSQL 17

# Completed

- Baseline repository/build/test audit.
- Service-layer design approved and committed.
- Isolated worktree `codex/service-layer-refactor` created.
- Initial catalogue: 119 feature/component rows and 326 GET endpoints.
- EF metadata contract verified for 23 dedicated translation entities.
- Corrected the shadow-key mapping in `DocumentStatusTranslation` (`StatusId`, `LanguageId`).
- Reusable PostgreSQL 17 Testcontainers fixture and mutable integration user context.

# Modified but not verified

- `Cmn/Currencies` multilingual characterization tests.

# Blocked

- None.

# Translation decisions

- Current fallback: requested `IUserContext.LanguageId` translation, then base field.
- Inline projection exceptions: internal one-use/read models and common `SelectListDto` only when documented.
- SelectListDto exceptions: no dedicated builder required, but inline SQL projection must translate display fields.
- Internal DTO exceptions: internal/private state-machine and provider models stay grouped.

# Next exact action

1. Add failing list/detail translation, fallback, search, order and paging tests for `Cmn/Currencies`.
2. Refactor both currency projections to use `IUserContext.LanguageId` with base-name fallback.
3. Verify real SQL and scoped DI resolution.
