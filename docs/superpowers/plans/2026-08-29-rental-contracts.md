# Rental Contracts Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add rental contracts with individual lessors, periodic draft accruals, selected accounting accounts, posting, cancellation, APIs and a midnight Quartz generator.

**Architecture:** The rental module uses normalized contract/object and accrual header/line tables. Pure policies calculate periods and amounts; application services implement CRUD, idempotent generation and lifecycle posting through the existing accounting dispatcher. Quartz invokes the same generator used by the manual API.

**Tech Stack:** .NET 10, ASP.NET Core, EF Core/Npgsql, Quartz, FluentValidation, xUnit, PostgreSQL SQL scripts.

**Spec:** `docs/superpowers/plans/2026-08-29-rental-contracts-design.md`

## Global Constraints

- Use `varchar`, `int`, `serial`/`bigserial`/`smallserial` and inline `references` in SQL.
- Generated accruals always start as `DRAFT`.
- Do not use `counterparty_card` for the lessor.
- `acc_document_account_setting` is only a recommendation source; selected accounts remain editable and are persisted in the rental document.
- Do not commit without an explicit user request.

---

### Task 1: Database schema and metadata

**Files:**
- Create: `src/Infrastructure/Persistence/Scripts/18_rnt/1801_create_rnt_rental_object_type.sql`
- Create: `src/Infrastructure/Persistence/Scripts/18_rnt/1802_create_rnt_contract.sql`
- Create: `src/Infrastructure/Persistence/Scripts/18_rnt/1803_create_rnt_accrual_doc.sql`
- Create: `src/Infrastructure/Persistence/Scripts/18_rnt/1804_seed_rnt_document_metadata.sql`
- Create: `src/Infrastructure/Persistence/Scripts/18_rnt/1805_connect_rnt_document_registry.sql`
- Create: `src/Infrastructure/Persistence/Scripts/00_sys/0021_insert_rental_permissions.sql`

**Interfaces:**
- Produces document type id `26` (`rental_accrual`) and account roles `rent_expense`, `lessor_payable`, `tax_payable`.
- Produces the unique line-period constraint used by the generator.

- [ ] Create tables using inline PK/FK syntax and all agreed checks/indexes.
- [ ] Seed four object types and uz/ru/en translations by code lookup.
- [ ] Seed document/account metadata and permissions idempotently.
- [ ] Add the `cmn_sync_document_registry` trigger for `rnt_accrual_doc.amount`.
- [ ] Inspect scripts for forbidden `integer`, `character varying`, and identity syntax.

### Task 2: Domain model and EF mapping

**Files:**
- Create: `src/Domain/Entities/Rnt/RentalObjectType.cs`
- Create: `src/Domain/Entities/Rnt/RentalObjectTypeTranslation.cs`
- Create: `src/Domain/Entities/Rnt/RentalContract.cs`
- Create: `src/Domain/Entities/Rnt/RentalContractObject.cs`
- Create: `src/Domain/Entities/Rnt/RentalAccrualDoc.cs`
- Create: `src/Domain/Entities/Rnt/RentalAccrualDocItem.cs`
- Modify: `src/Infrastructure/Persistence/AppDbContext/AppDbContext.cs`
- Modify: `src/Infrastructure/Persistence/AppDbContext/AppDbContext.AccessScope.cs`

**Interfaces:**
- Produces navigable EF entities with `OrganizationId` access filters on contracts and accrual headers.

- [ ] Add the six annotated entities matching SQL types and relationships.
- [ ] Add DbSets and organization query filters.
- [ ] Build `Domain` and `Infrastructure` to validate model compilation.

### Task 3: Calculation and schedule policies (TDD)

**Files:**
- Create: `tests/UnitTests/RentalAccrualPolicyTests.cs`
- Create: `src/Application/Features/Rnt/RentalAccruals/RentalAccrualCalculator.cs`
- Create: `src/Application/Features/Rnt/RentalAccruals/RentalAccrualSchedule.cs`

**Interfaces:**
- `RentalAccrualCalculator.Calculate(decimal contractAmount, decimal taxBaseAmount, decimal taxRate)` returns tax, payable and total amounts.
- `RentalAccrualSchedule.GetPeriod(DateTime nextDate, string unit, int value, DateTime endDate)` returns the inclusive due period and following date.

- [ ] Write a failing test expecting `5_000_000`, `6_000_000`, `12` to return `720_000`, `4_400_000`, `5_120_000`.
- [ ] Run `dotnet test tests/UnitTests/UnitTests.csproj --filter RentalAccrualPolicyTests` and observe a missing-type failure.
- [ ] Implement the minimal calculator and rerun to green.
- [ ] Add failing daily/monthly/clamped-end schedule tests and observe red.
- [ ] Implement schedule logic and rerun to green.

### Task 4: Contracts CRUD and manual

**Files:**
- Create files under `src/Application/Features/Rnt/RentalContracts/{DTOs,Errors,Filters,Projections,Queries,Services,Validators}`.
- Modify: `src/Application/Features/Cmn/Manual/Services/IManualService.cs`
- Modify: `src/Application/Features/Cmn/Manual/Services/ManualService.cs`
- Modify: `src/Presentation/WebApi/Controllers/Cmn/ManualController.cs`
- Create: `src/Presentation/WebApi/Controllers/Rnt/RentalContractController.cs`
- Modify: `src/SharedKernel/Constants/PermissionCodeConst.cs`
- Modify: `src/Infrastructure/DependencyInjection.cs`

**Interfaces:**
- `IRentalContractService` exposes paged list, detail, create, update and draft delete.
- Create/update requests contain the lessor fields, accounts and complete object list.

- [ ] Add validator tests for missing identity, invalid dates/rates/periods and run red.
- [ ] Implement DTOs and validators and run green.
- [ ] Implement query builders/projections/services following existing QueryBuilder and Result patterns.
- [ ] Validate referenced currency, type and accounts against the current organization.
- [ ] Add controller and `GET /api/manuals/rental-object-types`.

### Task 5: Accrual generation and CRUD (TDD)

**Files:**
- Create files under `src/Application/Features/Rnt/RentalAccruals/{DTOs,Errors,Filters,Projections,Queries,Services,Validators}`.
- Create: `src/Presentation/WebApi/Controllers/Rnt/RentalAccrualController.cs`

**Interfaces:**
- `IRentalAccrualGenerationService.GenerateDueAsync(DateTime asOfDate, int? organizationId, CancellationToken)` returns created document/line counts.
- `IRentalAccrualService` exposes list, detail, draft account update, post, cancel and delete.

- [ ] Add failing factory tests proving generated status is DRAFT, formulas are frozen, account defaults are copied and duplicate periods are rejected.
- [ ] Implement pure draft factory and run tests green.
- [ ] Implement organization-scoped due generation, grouping all periods caught up in the current run by contract.
- [ ] Generate document numbers with `IDocumentNumberService` and persist header/lines transactionally.
- [ ] Add list/detail/update/delete API methods and validators.

### Task 6: Accounting lifecycle (TDD)

**Files:**
- Create: `src/Application/Features/Register/PostingEngines/Builders/RentalAccrualContextBuilder.cs`
- Create lifecycle implementation under `src/Application/Features/Rnt/RentalAccruals/Services`.
- Modify: `src/Infrastructure/DependencyInjection.cs`
- Modify: `src/SharedKernel/Constants/DocumentTypeIdConst.cs`

**Interfaces:**
- Posting builder returns two balanced entries per line using persisted accounts.
- Post is DRAFT -> POSTED; cancel is DRAFT/POSTED -> CANCELLED, reversing an active posting batch when necessary.

- [ ] Add a failing posting-builder test with literal expected accounts and amounts.
- [ ] Implement builder and run green.
- [ ] Implement account ownership/status and accounting-period validation.
- [ ] Post through `IAccountingDispatcher` and persist `PostingBatch` metadata.
- [ ] Implement reversal on cancellation and idempotent lifecycle behavior.

### Task 7: Midnight automation

**Files:**
- Create: `src/Infrastructure/BackgroundServices/RentalAccrualJob.cs`
- Modify: `src/Presentation/WebApi/Configuration/HostConfiguration.Extensions.cs`

**Interfaces:**
- Quartz runs once daily at 00:00 in `TashkentTime.Zone` and invokes the same generation service as the API.

- [ ] Write a failing job test that exercises the generator boundary with a fixed date.
- [ ] Implement the non-concurrent Quartz job.
- [ ] Register the Tashkent-time midnight trigger.
- [ ] Ensure failures are logged per run and cancellation is rethrown.

### Task 8: Verification and API handoff

**Files:**
- Modify tests only if a real regression is identified.

- [ ] Run focused rental tests.
- [ ] Run `dotnet build Accounting.slnx --no-restore`.
- [ ] Run `dotnet test Accounting.slnx --no-restore`.
- [ ] Review `git diff --check` and `git status --short`.
- [ ] Report SQL execution order, routes, request shapes, existing warnings and uncommitted files.
