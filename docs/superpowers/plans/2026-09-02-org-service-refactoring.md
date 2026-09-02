# Org and Organization Service Refactoring Implementation Plan

> **Execution rule:** implement one small verified batch at a time, using PostgreSQL characterization tests before production changes.

**Goal:** довести `Org/Branches`, `Org/Departments`, `Org/Positions`, `Organization/Organizations` и `Organization/Setup` до `VERIFIED`, сохранив routes, HTTP verbs, JSON-контракты и бизнес-семантику.

**Architecture:** CRUD lists/details continue to use the existing QueryBuilder criteria/projection/order pipeline. Organization-owned records receive explicit selected-organization predicates so that a super-admin working under `X-OrganizationId` cannot bypass isolation through the global EF filter. Entities without dedicated translation models keep their base display fields. Public setup DTOs are split mechanically into same-named files; database-state validation stays in services.

**Tech Stack:** .NET 10, EF Core 10, Npgsql/PostgreSQL 17, xUnit, Testcontainers, FluentValidation, Scrutor, existing Result/QueryBuilder/repository abstractions.

**Spec:** `docs/superpowers/specs/2026-08-31-service-layer-refactoring-design.md`

## Constraints

- Do not add translation tables. `Branch`, `Department`, `Position`, `Organization`, `Region`, `District` and `State` currently have no dedicated translation entity.
- Preserve all controller and service-interface contracts.
- Keep search, order and paging SQL-side with stable `ThenBy(Id)`.
- Reject missing selected-organization context through localized `CommonErrors`, never through nullable `.Value` exceptions.
- Preserve Organization visibility rules: SuperAdmin sees all organizations, TenantAdmin sees its tenant, TenantUser sees allowed organizations.
- Treat Faktura company lookup text as external provider data.
- Do not change setup transaction ownership or completion semantics without a failing characterization test.

---

### Task 1: Org/Branches scoped CRUD and GET contract

**Files:**
- Create `tests/IntegrationTests/Features/Org/Branches/BranchQueryContractTests.cs`
- Create `tests/UnitTests/BranchListFilterValidatorTests.cs`
- Create `src/Application/Features/Org/Branches/Validators/BranchListFilterValidator.cs`
- Modify Branch filter, criteria builder and service.
- Modify the three progress/audit documents.

- [ ] RED: prove a selected-organization SuperAdmin currently receives another organization's branch in list/detail and that cross-organization code affects duplicate detection.
- [ ] Add an internal `OrganizationId` query option, set it from `IUserContext`, and explicitly scope list/detail/create/update/delete and duplicate checks.
- [ ] Return localized `CommonErrors.UserHasNoOrganization` when organization context is absent.
- [ ] Validate optional `RegionId`, `Search`, `Page` and `PageSize` without changing the query contract.
- [ ] Verify base names, region filter, projected search, `Name, Id` order, SQL paging and localized not-found.

### Task 2: Org/Departments scoped CRUD and GET contract

**Files:**
- Create `tests/IntegrationTests/Features/Org/Departments/DepartmentQueryContractTests.cs`
- Create `tests/UnitTests/DepartmentListFilterValidatorTests.cs`
- Create `src/Application/Features/Org/Departments/Validators/DepartmentListFilterValidator.cs`
- Modify Department filter, criteria builder and service.
- Modify the three progress/audit documents.

- [ ] RED: prove selected-organization leakage for SuperAdmin.
- [ ] Scope list/detail/create/update/delete and code conflict checks explicitly to current organization.
- [ ] Keep optional branch filtering, base Branch/State names, `Name, Id` order and paging in PostgreSQL.
- [ ] Add exact list-filter validation and localized missing-context behavior.

### Task 3: Org/Positions scoped CRUD and GET contract

**Files:**
- Create `tests/IntegrationTests/Features/Org/Positions/PositionQueryContractTests.cs`
- Create `tests/UnitTests/PositionListFilterValidatorTests.cs`
- Create `src/Application/Features/Org/Positions/Validators/PositionListFilterValidator.cs`
- Modify Position filter, criteria builder and service.
- Modify the three progress/audit documents.

- [ ] RED: prove selected-organization leakage for SuperAdmin.
- [ ] Add explicit organization predicates to every database operation and organization-local code uniqueness.
- [ ] Verify base names, projected search, stable order, SQL paging, detail/not-found and exact filter validation.

### Task 4: Organization/Organizations query, access and validation contract

**Files:**
- Create `tests/IntegrationTests/Features/Organization/Organizations/OrganizationQueryContractTests.cs`
- Create `tests/UnitTests/OrganizationListFilterValidatorTests.cs`
- Create `src/Application/Features/Organization/Organizations/Validators/OrganizationListFilterValidator.cs`
- Modify Organization list/detail implementation only if tests expose a contract issue.
- Modify the three progress/audit documents.

- [ ] Characterize SuperAdmin, TenantAdmin and TenantUser list/detail visibility through real EF query filters.
- [ ] Verify region/is-parent filters, projected search, `ShortName, Id` order and SQL paging.
- [ ] Verify Region/District/State/Language fields intentionally use base names because no corresponding dedicated translation entity exists.
- [ ] Verify localized detail errors and mark `by-inn` as external provider output.
- [ ] Add exact list-filter bounds; preserve all public DTO properties and management-core scope rules.

### Task 5: Organization/Setup public DTO split and request validation

**Files:**
- Split the nine public types currently in `OrganizationSetupDtos.cs` into same-named files.
- Create exact validators for the four public setup request DTOs.
- Create reflection/validator tests in UnitTests.
- Create PostgreSQL setup query/lifecycle tests in IntegrationTests.
- Modify setup service/core only for confirmed defects.
- Modify the three progress/audit documents.

- [ ] Capture every public property/type/default before moving files.
- [ ] Split DTOs mechanically without namespace, nullability or JSON changes.
- [ ] Validate company-profile shape, tax dates/state, accounting method/month and nullable default IDs; leave FK ownership/existence checks in the service.
- [ ] Verify organization context/membership, setup aggregate mapping, current tax/pricing selection, defaults, localized errors and no cross-organization data.
- [ ] Characterize update/complete transaction behavior before any lifecycle correction.

### Task 6: Org/Organization checkpoint

- [ ] Run all targeted Org/Organization integration and unit tests.
- [ ] Run full UnitTests, full IntegrationTests, full solution build and full solution tests.
- [ ] Validate DI scopes and resolution for every modified service/projection/validator.
- [ ] Confirm no controller/service-interface/SQL-script diff and no API DTO drift.
- [ ] Update all Org/Organization feature and GET audit rows to `VERIFIED`.
- [ ] Record the exact next area (`Counterparty`) in progress documentation.
