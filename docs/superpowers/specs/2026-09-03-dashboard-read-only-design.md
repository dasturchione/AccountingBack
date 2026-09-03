# Dashboard Read-Only Aggregate APIs Design

**Goal:** Add organization-scoped, read-only dashboard aggregate APIs without changing existing import, inventory, provider, or database schemas.

## Scope

Add `GET /api/dashboard/overview`, `/cash`, `/receivables-payables`, `/electronic-documents`, `/tax-summary`, and `GET /api/tasks/calendar`.

The task endpoint returns `sourceStatus=NOT_AVAILABLE` with zero counts and an empty item list. HR and payroll calendars are not task data.

## Architecture

Use a new business-dashboard controller and application service layer; leave the existing SuperAdmin-only `DashboardController` and `IDashboardService` unchanged. All dashboard queries use `IUserContext.OrganizationId` and `IQueryRepository` with no internal HTTP calls and no provider detail calls.

Cash, receivable/payable, electronic-document, and tax aggregates are built from existing local entities. Missing source dimensions are represented by `sourceStatus`, `null`, zero, or empty arrays rather than inferred values.

## Security and data safety

- Require authentication and the existing `DASHBOARD_VIEW` permission.
- Enforce organization isolation from middleware/user context; ignore any organization value supplied in query input.
- Use `AsNoTracking` query projections through existing query infrastructure.
- Do not return raw marking values, credentials, tokens, passwords, or connection strings.
- Do not add migrations, indexes, provider calls, or write operations.

## Response behavior

All aggregate sections expose a source status. Successful local data uses `AVAILABLE`; missing or unavailable local source data uses `NOT_AVAILABLE` or `PARTIAL`. Overview remains a valid response if one section has no data.

## Filters

The shared filter accepts `dateFrom`, `dateTo`, `currencyIds[]`, `warehouseIds[]`, `documentTypes[]`, and `statusIds[]`. Organization is selected by `X-OrganizationId` middleware context, not by a client-supplied organization filter.

## Testing

Add unit tests for each aggregate and task response, scope/permission behavior, every relevant filter, empty data, and no provider/write access. Add WebApi contract tests for routes and response shapes, plus existing regression tests.
