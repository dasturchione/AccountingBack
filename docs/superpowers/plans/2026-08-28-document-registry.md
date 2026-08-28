# Universal Document Registry Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add an automatically synchronized universal document registry and replace the cash-collection-only bank-operation reference with a universal related-document reference.

**Architecture:** Module document tables remain authoritative for detailed data. PostgreSQL triggers atomically upsert their common fields into `cmn_document_registry`; EF Core maps the registry for querying and bank-operation relationships. Cash collection retains its specialized lifecycle through the registry's document type, while other related documents remain informational references.

**Tech Stack:** PostgreSQL SQL scripts and triggers, .NET 9, C#, Entity Framework Core, ASP.NET Core, xUnit, FluentValidation.

**Spec:** `docs/superpowers/specs/2026-08-28-document-registry.md`

## Global Constraints

- Use `varchar`, not `character varying`, in new SQL.
- Use `int`, not `integer`, in new SQL.
- Use `bigserial primary key` for the registry identifier.
- Declare foreign keys inline with `references` where the relationship belongs to one column.
- Do not commit unless the user explicitly requests it.
- Preserve unrelated uncommitted files.

---

### Task 1: Define registry and database synchronization

**Files:**
- Create: `src/Infrastructure/Persistence/Scripts/01_cmn/0169_create_cmn_document_registry.sql`
- Create: `src/Infrastructure/Persistence/Scripts/17_rtl/1708_connect_cmn_document_registry.sql`

**Interfaces:**
- Consumes: `cmn_document_type`, `org_organization`, `cmn_currency`, `cmn_document_status`, `cmn_state`, and all source document tables.
- Produces: `cmn_document_registry`, `cmn_sync_document_registry()`, source-table triggers, line-amount recalculation triggers, and existing-data backfill.

- [ ] **Step 1: Write a failing database-contract test**

Add a test that executes the registry scripts against an empty PostgreSQL test database when the existing integration-test harness is available; otherwise add a schema test around EF model creation that requires the `DocumentRegistry` mapping introduced in Task 2.

- [ ] **Step 2: Run the focused test and verify it fails because the registry does not exist**

Run the focused test project command and confirm the missing table/model is the failure reason.

- [ ] **Step 3: Add the table, common trigger function, per-document triggers, line amount recalculation, and backfill**

Cover implemented types 1-5 and 7-24. Type 6 has no physical document table and is therefore not registered until such a table exists.

- [ ] **Step 4: Validate SQL syntax and rerun the focused test**

Run the database test if configured; otherwise parse and inspect every trigger/table mapping and compile the EF model in Task 2.

### Task 2: Map the registry in Domain and EF Core

**Files:**
- Create: `src/Domain/Entities/Cmn/DocumentRegistry.cs`
- Modify: `src/Domain/Entities/Cmn/DocumentType.cs`
- Modify: `src/Domain/Entities/Cmn/DocumentStatus.cs`
- Modify: `src/Domain/Entities/Cmn/Currency.cs`
- Modify: `src/Domain/Entities/Cmn/State.cs`
- Modify: `src/Domain/Entities/Organization/Organization.cs`
- Modify: `src/Infrastructure/Persistence/AppDbContext/AppDbContext.cs`
- Modify: `src/Infrastructure/Persistence/AppDbContext/AppDbContext.AccessScope.cs`
- Test: `tests/UnitTests/DocumentRegistryTests.cs`

**Interfaces:**
- Consumes: the schema from Task 1.
- Produces: `DocumentRegistry`, `AppDbContext.DocumentRegistries`, navigation collections, and organization access filtering.

- [ ] **Step 1: Write a failing EF model test**

Assert that `DocumentRegistry` maps to `cmn_document_registry`, has the unique `(DocumentTypeId, DocumentId)` index, and is organization-scoped.

- [ ] **Step 2: Run the test and verify it fails because `DocumentRegistry` is missing**

- [ ] **Step 3: Add the entity, navigations, DbSet, relationship metadata, and access filter**

- [ ] **Step 4: Run the focused test and verify it passes**

### Task 3: Replace the bank-operation database relationship

**Files:**
- Create: `src/Infrastructure/Persistence/Scripts/17_rtl/1709_replace_bank_operation_cash_collection_link.sql`
- Modify: `src/Domain/Entities/Bank/BankOperation.cs`
- Modify: `src/Domain/Entities/Cash/CashCollectionDoc.cs`
- Test: `tests/UnitTests/CashCollectionTests.cs`

**Interfaces:**
- Consumes: `DocumentRegistry.Id` and existing `cash_collection_doc_id` values.
- Produces: nullable `BankOperation.RelatedDocumentId` and `BankOperation.RelatedDocument`; removes the direct cash-collection FK/navigation.

- [ ] **Step 1: Change the projection/policy tests to require `RelatedDocumentId` and registry metadata**

- [ ] **Step 2: Run the focused tests and verify compile/test failure references the missing universal properties**

- [ ] **Step 3: Add and migrate `related_document_id`, then remove the legacy indexes, column, and Domain navigation**

- [ ] **Step 4: Compile Domain and rerun the focused tests**

### Task 4: Generalize bank-operation linking while preserving cash collection

**Files:**
- Replace: `src/Application/Features/Cash/CashCollections/Services/CashCollectionBankLinkService.cs`
- Modify: `src/Application/Features/Cash/CashCollections/Services/CashCollectionBankLinkPolicy.cs`
- Modify: `src/Application/Features/Bank/BankOperations/Services/BankOperationService.cs`
- Modify: `src/Application/Features/Bank/BankOperations/Services/BankLifecycleService.cs`
- Modify: `src/Application/Features/Cash/CashCollections/Services/CashCollectionService.cs`
- Modify: `src/Application/Features/Cash/CashCollections/Services/CashCollectionLifecycleService.cs`
- Modify: `src/Application/Features/Cash/CashCollections/Projections/CashCollectionProjections.cs`
- Test: `tests/UnitTests/CashCollectionTests.cs`

**Interfaces:**
- Consumes: `BankOperation.RelatedDocumentId`, `DocumentRegistry.DocumentTypeId`, and `DocumentRegistry.DocumentId`.
- Produces: a related-document resolver that validates organization/state for every type and invokes cash-collection rules only for document type 24.

- [ ] **Step 1: Add failing tests for a generic related document and for cash-collection special handling through its registry row**

- [ ] **Step 2: Run tests and confirm expected failures**

- [ ] **Step 3: Implement registry resolution, generic validation, and cash-collection branching**

- [ ] **Step 4: Replace cash-collection navigation queries with registry-based bank-operation queries**

- [ ] **Step 5: Run focused tests and verify they pass**

### Task 5: Update bank-operation API contracts and projections

**Files:**
- Modify: `src/Application/Features/Bank/BankOperations/DTOs/BankOperationBaseDto.cs`
- Modify: `src/Application/Features/Bank/BankOperations/DTOs/BankOperationDto.cs`
- Modify: `src/Application/Features/Bank/BankOperations/DTOs/BankOperationListDto.cs`
- Modify: `src/Application/Features/Bank/BankOperations/Filters/BankOperationListFilter.cs`
- Modify: `src/Application/Features/Bank/BankOperations/Validators/BankOperationBaseDtoValidator.cs`
- Modify: `src/Application/Features/Bank/BankOperations/Projections/BankOperationDtoProjection.cs`
- Modify: `src/Application/Features/Bank/BankOperations/Projections/BankOperationListDtoProjection.cs`
- Modify: `src/Application/Features/Bank/BankOperations/Queries/BankOperationByListFilterCriteriaBuilder.cs`
- Modify: `src/Application/Features/Bank/BankOperations/Queries/BankOperationListDtoByListFilterCriteriaBuilder.cs`
- Test: `tests/UnitTests/CashCollectionTests.cs`

**Interfaces:**
- Consumes: universal relation from Tasks 3-4.
- Produces: request `relatedDocumentId`; response fields `relatedDocumentId`, source `relatedDocumentTypeId`, source `relatedDocumentEntityId`, `relatedDocumentNumber`, and `relatedDocumentDate`; filter `relatedDocumentId`.

- [ ] **Step 1: Add failing DTO/projection tests with hand-authored expected values**

- [ ] **Step 2: Run tests and verify the missing fields cause failure**

- [ ] **Step 3: Replace legacy request, response, search, validation, and filter fields**

- [ ] **Step 4: Run focused tests and verify they pass**

### Task 6: Add a read-only registry API for document selection

**Files:**
- Create: `src/Application/Features/Cmn/Documents/DTOs/DocumentRegistryDtos.cs`
- Create: `src/Application/Features/Cmn/Documents/Filters/DocumentRegistryListFilter.cs`
- Create: `src/Application/Features/Cmn/Documents/Projections/DocumentRegistryProjections.cs`
- Create: `src/Application/Features/Cmn/Documents/Queries/DocumentRegistryQueryBuilders.cs`
- Create: `src/Application/Features/Cmn/Documents/Services/IDocumentRegistryService.cs`
- Create: `src/Application/Features/Cmn/Documents/Services/DocumentRegistryService.cs`
- Create: `src/Presentation/WebApi/Controllers/Cmn/DocumentsController.cs`
- Test: `tests/UnitTests/DocumentRegistryTests.cs`

**Interfaces:**
- Consumes: `DocumentRegistry` and standard QueryBuilder/repository abstractions.
- Produces: organization-scoped `GET /api/documents` and `GET /api/documents/{id}` for selecting `relatedDocumentId`.

- [ ] **Step 1: Write failing projection and filtering tests**

- [ ] **Step 2: Run focused tests and verify failures are caused by missing registry API types**

- [ ] **Step 3: Implement DTOs, query builders, service, and controller following existing feature conventions**

- [ ] **Step 4: Run focused tests and verify they pass**

### Task 7: Final verification

**Files:**
- Review all files changed by Tasks 1-6.

**Interfaces:**
- Consumes: all preceding tasks.
- Produces: verified build, tests, SQL execution order, and a scoped working-tree report.

- [ ] **Step 1: Search for remaining production references to `CashCollectionDocId` and `cash_collection_doc_id`**

Expected: only migration statements that copy/drop the legacy column may remain.

- [ ] **Step 2: Run focused unit tests**

Run: `dotnet test tests/UnitTests/UnitTests.csproj --no-restore --filter "FullyQualifiedName~DocumentRegistryTests|FullyQualifiedName~CashCollectionTests"`

- [ ] **Step 3: Run the full solution build**

Run: `dotnet build Accounting.slnx --no-restore`

- [ ] **Step 4: Run the full unit-test project**

Run: `dotnet test tests/UnitTests/UnitTests.csproj --no-build`

- [ ] **Step 5: Inspect `git diff` and confirm unrelated files are untouched**
