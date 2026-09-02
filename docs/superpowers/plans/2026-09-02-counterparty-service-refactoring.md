# Counterparty Service Refactoring Implementation Plan

> **Execution rule:** implement one small verified batch at a time, using PostgreSQL characterization tests before production changes.

**Goal:** довести `Counterparty/CounterpartyCards`, `Counterparty/CounterpartyBankAccounts` и `Counterparty/CounterpartyContacts` до `VERIFIED`, сохранив routes, HTTP verbs, JSON-контракты и бизнес-семантику.

**Architecture:** CRUD lists/details continue to use the existing QueryBuilder criteria/projection/order pipeline. Every organization-owned query receives an explicit selected-organization predicate because the global EF filter intentionally allows SuperAdmin access. Child records additionally validate that their counterparty belongs to the selected organization. Display fields use base entity names unless a dedicated translation entity exists; `Currency` uses `CurrencyTranslation` with base fallback.

**Tech Stack:** .NET 10, EF Core 10, Npgsql/PostgreSQL 17, xUnit, Testcontainers, FluentValidation, Scrutor, existing Result/QueryBuilder/repository abstractions.

**Spec:** `docs/superpowers/specs/2026-08-31-service-layer-refactoring-design.md`

## Constraints

- Preserve all controller and service-interface contracts.
- Keep search, order and paging SQL-side with stable `ThenBy(Id)`.
- Reject missing selected-organization context through localized `CommonErrors`, never through nullable `.Value` exceptions.
- Treat counterparty short-name and bank-account number uniqueness as organization-local, matching tenant isolation.
- Do not add translation tables. Counterparty, bank and state names remain base fields because they have no dedicated translation model.
- Validate child ownership before creating or moving bank accounts and contacts.
- Align request length validation with Domain/schema constraints without changing public JSON.

---

### Task 1: CounterpartyCards scoped CRUD and GET contract

**Files:**
- Create `tests/IntegrationTests/Features/Counterparty/CounterpartyCards/CounterpartyCardQueryContractTests.cs`.
- Create `tests/UnitTests/CounterpartyCardValidatorTests.cs`.
- Create `src/Application/Features/Counterparty/CounterpartyCards/Validators/CounterpartyCardListFilterValidator.cs`.
- Create the entity list-filter criteria builder and modify filter/service/base validator.
- Modify the three progress/audit documents.

- [x] RED: prove selected-organization SuperAdmin leakage in list/detail and cross-organization short-name conflict.
- [x] Add explicit organization scope to every list/detail/create/update/delete query and conflict check.
- [x] Return localized `CommonErrors.UserHasNoOrganization` when organization context is absent.
- [x] Align Code, Email, Address, Oked and ExternalId validation with entity limits; validate optional IDs.
- [x] Verify search, `ShortName, Id` order, SQL paging, base nested names and localized errors.

### Task 2: CounterpartyBankAccounts scope, ownership and translation contract

**Files:**
- Create PostgreSQL query/command contract tests.
- Create list-filter and request validator tests.
- Modify filter, criteria, projections, service, errors and validators as confirmed by RED tests.
- Modify the three progress/audit documents.

- [x] RED: prove selected-organization leakage and acceptance of a counterparty from another organization.
- [x] Explicitly scope all operations and account-number uniqueness to the selected organization.
- [x] Validate Counterparty ownership and preserve existing bank-branch/bank compatibility validation.
- [x] Localize `CurrencyName` by requested language with independent base fallback in detail/list SQL projections.
- [x] Verify counterparty filter, search, `IsMain desc, CounterpartyName, Id` order, paging and localized failures.

### Task 3: CounterpartyContacts scope and ownership contract

**Files:**
- Create PostgreSQL query/command contract tests.
- Create list-filter and request validator tests.
- Modify filter, criteria, service, errors and validators as confirmed by RED tests.
- Modify the three progress/audit documents.

- [ ] RED: prove selected-organization leakage and acceptance of a counterparty from another organization.
- [ ] Explicitly scope all list/detail/update/delete operations and validate counterparty ownership on create/update.
- [ ] Align Email validation with the entity/schema length.
- [ ] Verify counterparty filter, search, stable order, paging, base nested names and localized failures.

### Task 4: Counterparty checkpoint

- [ ] Run all targeted Counterparty integration and unit tests.
- [ ] Run full UnitTests, full IntegrationTests, full solution build and full solution tests.
- [ ] Validate DI scopes and resolution for every modified service/projection/validator.
- [ ] Confirm no controller/service-interface/SQL-script diff and no API DTO drift.
- [ ] Update every Counterparty feature and GET audit row to `VERIFIED`.
- [ ] Record the exact next area in progress documentation.
