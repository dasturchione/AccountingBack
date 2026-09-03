# Dashboard Read-Only Aggregate APIs Implementation Plan

> **For agentic workers:** Execute this plan task-by-task with TDD checkpoints.

**Goal:** Add six authenticated organization-scoped read-only dashboard endpoints using existing local query sources.

**Architecture:** Add a business dashboard controller and focused read-only aggregate services under the Application layer. The existing platform dashboard remains unchanged; the task endpoint is an explicit empty/unavailable contract.

**Tech Stack:** ASP.NET Core controllers, application services, EF-backed `IQueryRepository`, existing authorization and `IUserContext` infrastructure, xUnit test projects.

**Spec:** `docs/superpowers/specs/2026-09-03-dashboard-read-only-design.md`

## Global Constraints

- Only existing database entities and read-only query repositories.
- No migration, schema/index change, provider call, import call, inventory write, or existing-flow change.
- Organization comes from `IUserContext.OrganizationId`/middleware.
- Do not expose raw marking values or secrets.
- Unavailable sources return explicit status plus null/zero/empty data.

---

### Task 1: Add failing application and controller contract tests

**Files:**
- Create: `tests/UnitTests/DashboardReadOnlyApiTests.cs`
- Create: `tests/IntegrationTests/DashboardReadOnlyApiContractTests.cs`

**Interfaces:**
- Tests define the required DTO names, route paths, task unavailable response, organization filter behavior, and aggregate response sections before production types exist.

- [ ] Write tests for the six GET routes and exact task response.
- [ ] Write tests for organization scope, permission/auth rejection, date/currency/warehouse/document/status filters, empty source data, and partial source status.
- [ ] Write a query-count test fixture that fails if an aggregate invokes per-document provider/detail calls.
- [ ] Run the focused tests and confirm they fail because the new controller/services do not exist.

### Task 2: Add shared dashboard contracts and unavailable task contract

**Files:**
- Create: `src/Application/Features/Dashboard/DTOs/DashboardFilterDto.cs`
- Create: `src/Application/Features/Dashboard/DTOs/DashboardResponseDtos.cs`
- Create: `src/Application/Features/Dashboard/DTOs/TaskCalendarDto.cs`

**Interfaces:**
- `DashboardFilterDto` exposes date range and collection filters.
- `TaskCalendarDto` exposes `SourceStatus`, `Total`, `Completed`, `Pending`, `Overdue`, and `Items`.
- Aggregate DTOs expose section-level `SourceStatus` and the requested cash, relationship, task, debt, tax, and electronic-document fields.

- [ ] Implement only the DTOs required by the failing tests.
- [ ] Run focused tests and confirm they advance to missing service/controller failures.

### Task 3: Implement focused read-only aggregate services

**Files:**
- Create: `src/Application/Features/Dashboard/Services/IBusinessDashboardService.cs`
- Create: `src/Application/Features/Dashboard/Services/BusinessDashboardService.cs`
- Create: `src/Application/Features/Dashboard/Services/ITaskCalendarService.cs`
- Create: `src/Application/Features/Dashboard/Services/TaskCalendarService.cs`

**Interfaces:**
- `IBusinessDashboardService.GetOverviewAsync(DashboardFilterDto, CancellationToken)`
- `IBusinessDashboardService.GetCashAsync(DashboardFilterDto, CancellationToken)`
- `IBusinessDashboardService.GetReceivablesPayablesAsync(DashboardFilterDto, CancellationToken)`
- `IBusinessDashboardService.GetElectronicDocumentsAsync(DashboardFilterDto, CancellationToken)`
- `IBusinessDashboardService.GetTaxSummaryAsync(DashboardFilterDto, CancellationToken)`
- `ITaskCalendarService.GetAsync(DashboardFilterDto, CancellationToken)`

- [ ] Implement organization-scoped `AsNoTracking` projections over existing cash/bank/register, sale/purchase, local EDO, and organization tax entities.
- [ ] Apply all applicable filters in the query predicates; do not use query organization input.
- [ ] Return `NOT_AVAILABLE`/`PARTIAL` where source data cannot provide a requested dimension, without invented values.
- [ ] Return the fixed empty task response.
- [ ] Run focused unit tests and correct implementation failures without changing test expectations.

### Task 4: Register services and expose GET routes

**Files:**
- Create: `src/Presentation/WebApi/Controllers/Dashboard/BusinessDashboardController.cs`
- Modify: `src/Application/DependencyInjection.cs`
- Modify: `src/Infrastructure/DependencyInjection.cs`

**Interfaces:**
- `GET /api/dashboard/overview`
- `GET /api/dashboard/cash`
- `GET /api/dashboard/receivables-payables`
- `GET /api/dashboard/electronic-documents`
- `GET /api/dashboard/tax-summary`
- `GET /api/tasks/calendar`

- [ ] Register services as scoped.
- [ ] Protect routes with authentication and existing `DASHBOARD_VIEW` permission without adding a permission or migration.
- [ ] Keep existing `/api/dashboard/stats` controller unchanged.
- [ ] Run route contract tests and confirm all six GET routes bind the shared filters.

### Task 5: Verify and regression-check

**Files:**
- Modify only tests if a verified contract assertion needs correction.

- [ ] Run all unit tests.
- [ ] Run read-only integration/contract tests.
- [ ] Run Release build.
- [ ] Run `git diff --check`.
- [ ] Inspect the final diff for unrelated changes, provider calls, write repositories, migrations, secrets, and raw marking fields.
