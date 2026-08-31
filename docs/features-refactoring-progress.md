# Current position

Current area: Cmn
Current feature: Currencies
Current phase: TESTCONTAINERS_HARNESS
Last verified commit: 647c5d98
Last successful build: 2026-08-31 — succeeded, 0 errors, 2 baseline CS8629 warnings
Last successful test: 2026-08-31 — UnitTests 134/134; IntegrationTests discovered 0 tests

# Completed

- Baseline repository/build/test audit.
- Service-layer design approved and committed.
- Isolated worktree `codex/service-layer-refactor` created.
- Initial catalogue: 119 feature/component rows and 326 GET endpoints.
- EF metadata contract verified for 23 dedicated translation entities.
- Corrected the shadow-key mapping in `DocumentStatusTranslation` (`StatusId`, `LanguageId`).

# Modified but not verified

- PostgreSQL integration test harness.

# Blocked

- PostgreSQL integration execution until Docker daemon is available.

# Translation decisions

- Current fallback: requested `IUserContext.LanguageId` translation, then base field.
- Inline projection exceptions: internal one-use/read models and common `SelectListDto` only when documented.
- SelectListDto exceptions: no dedicated builder required, but inline SQL projection must translate display fields.
- Internal DTO exceptions: internal/private state-machine and provider models stay grouped.

# Next exact action

1. Add the PostgreSQL Testcontainers package and shared fixture.
2. Run the database smoke test against PostgreSQL 17.
3. Add failing `Cmn/Currencies` translation behavior tests.
