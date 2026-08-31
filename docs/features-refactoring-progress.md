# Current position

Current area: Cmn
Current feature: Currencies
Current phase: INVENTORY
Last verified commit: f21b8dd0
Last successful build: 2026-08-31 — succeeded, 0 errors, 2 baseline CS8629 warnings
Last successful test: 2026-08-31 — UnitTests 134/134; IntegrationTests discovered 0 tests

# Completed

- Baseline repository/build/test audit.
- Service-layer design approved and committed.
- Isolated worktree `codex/service-layer-refactor` created.
- Initial catalogue: 119 feature/component rows and 326 GET endpoints.

# Modified but not verified

- Working inventory documents.

# Blocked

- PostgreSQL integration execution until Docker daemon is available.

# Translation decisions

- Current fallback: requested `IUserContext.LanguageId` translation, then base field.
- Inline projection exceptions: internal one-use/read models and common `SelectListDto` only when documented.
- SelectListDto exceptions: no dedicated builder required, but inline SQL projection must translate display fields.
- Internal DTO exceptions: internal/private state-machine and provider models stay grouped.

# Next exact action

1. Verify dedicated translation entity composite keys and navigations through EF metadata.
2. Populate the translation model inventory.
3. Add PostgreSQL integration harness.
