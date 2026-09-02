# Inventory Service Refactoring Implementation Plan

> **Execution rule:** complete the low-risk reference features first, then document lifecycles and stock calculations in small independently verified batches.

**Goal:** довести все 11 feature области `Inv` до `VERIFIED`, сохранив публичный API, складские остатки, партии/маркировки, статусы документов, проводки и транзакционные границы.

**Architecture:** CRUD GET paths retain the QueryBuilder criteria/projection/order pipeline with explicit selected-organization predicates. Translation-backed display fields are projected SQL-side by requested language with base fallback. Complex stock and document services are characterized before extraction or query changes; lifecycle services remain transaction owners.

**Tech Stack:** .NET 10, EF Core 10, Npgsql/PostgreSQL 17, xUnit, Testcontainers, FluentValidation, Scrutor, existing Result/QueryBuilder/repository abstractions.

**Spec:** `docs/superpowers/specs/2026-08-31-service-layer-refactoring-design.md`

## Constraints

- Preserve routes, verbs, public signatures, JSON shapes and document accounting semantics.
- Keep search/order/count/paging SQL-side with deterministic final `Id` ordering.
- No new translation table; only use translation entities already present in Domain/EF metadata.
- Reject missing organization context with localized Result errors rather than nullable-value exceptions.
- Do not change stock quantities, batch/marking rules, movement direction, posting, confirmation/cancellation or transaction ownership without a failing characterization test.
- Split only public API DTO declarations; internal lifecycle/read models may remain grouped when documented.

---

### Task 1: ProductGroups

- [x] Characterize organization scope, requested translation/base fallback, translated search/order/paging and detail errors in PostgreSQL.
- [x] Split grouped public DTO declarations without changing their reflection/JSON contract.
- [x] Add exact list/request validation and correct only test-confirmed query/service defects.
- [x] Verify global group code uniqueness and selected-organization ownership of nested products (the group itself is intentionally global).

### Task 2: Products

- [ ] Characterize list/detail organization isolation, nested display fields, search/order/paging and errors.
- [ ] Split grouped public DTO declarations and add exact filter/request validation.
- [ ] Verify product-group/unit/reference ownership without changing product semantics.

### Task 3: Warehouses

- [ ] Characterize list/detail organization scope, base display fields, search/order/paging and errors.
- [ ] Add exact list/request validation and fix only confirmed scope/contract defects.

### Task 4: ProductPrices

- [ ] Inventory public and internal price DTOs, query paths and costing/pricing dependencies.
- [ ] Characterize current pricing-condition selection, cost/sale price maps and SQL materialization boundaries.
- [ ] Preserve costing formulas and effective-date behavior while removing only proven query/service defects.

### Task 5: ProductStocks and WarehouseProducts

- [ ] Characterize current/historical stock, product/group/table aggregation, marking lookup and warehouse scope.
- [ ] Prove quantity/cost totals and organization/warehouse isolation against PostgreSQL.
- [ ] Split only public API DTOs and avoid changing batch allocation or historical valuation semantics.

### Task 6: InventoryMovements

- [ ] Audit the movement query endpoint and its direction/document/source fields.
- [ ] Characterize scope, filtering, ordering and output contract; add exact validation where applicable.

### Task 7: OpeningInventories

- [ ] Characterize CRUD, effects, number generation, currency display, quantities/costs and transaction rollback.
- [ ] Split grouped public DTO declarations without changing JSON.
- [ ] Preserve opening-balance/register and batch/marking behavior.

### Task 8: WarehouseTransfers

- [ ] Characterize CRUD, source/destination ownership, availability validation, numbering and lifecycle.
- [ ] Split grouped public DTO declarations and verify movement symmetry and rollback.
- [ ] Preserve posting batches and warehouse movement semantics.

### Task 9: InventoryCounts

- [ ] Characterize count/difference calculations, generated adjustments, status transitions and rollback.
- [ ] Split grouped public DTO declarations and localize MovementDirection with fallback.
- [ ] Preserve active-count concurrency and batch/marking behavior.

### Task 10: InventoryAdjustments

- [ ] Characterize CRUD/lifecycle, numbering, direction translation, quantities, posting and rollback.
- [ ] Split grouped public DTO declarations without changing JSON.
- [ ] Preserve generated-vs-manual adjustment semantics and stock movement effects.

### Task 11: Inventory checkpoint

- [ ] Run all targeted inventory tests, full build and full solution tests.
- [ ] Validate DI, translations, SQL-side paging and every Inv GET audit row.
- [ ] Confirm no unintended controller/interface/script/API/accounting diff.
- [ ] Mark all Inv features and endpoints `VERIFIED` or record a concrete blocker.
- [ ] Record the next master-plan area (`Acc`).
