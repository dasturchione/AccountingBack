# Remaining Cmn Service Refactoring Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Довести все оставшиеся feature области `src/Application/Features/Cmn` до статуса `VERIFIED`, сохранив публичный API и бизнес-поведение.

**Architecture:** Каждый feature проходит один вертикальный цикл: инвентаризация controller/service/query contracts, PostgreSQL characterization tests, минимальная правка projection/validation/DTO layout и полная проверка. Переводимые display-поля выбираются внутри EF expression по `IUserContext.LanguageId` с fallback на базовое поле; criteria, order и paging остаются SQL-side.

**Tech Stack:** .NET 10, C# 14, EF Core 10, Npgsql, PostgreSQL 17, Testcontainers.PostgreSql 4.14.0, xUnit 2.9.3, FluentValidation, Scrutor, existing Result/QueryBuilder/repository abstractions.

**Spec:** `docs/superpowers/specs/2026-08-31-service-layer-refactoring-design.md`

## Global Constraints

- Preserve routes, HTTP verbs, request/response JSON, nullable contracts, IDs, status codes and public behavior.
- Preserve accounting, inventory, money, counterparty, VAT, totals, numbering, status and transaction semantics.
- Do not modify `docs/service-layer-deep-analysis.md` in the main checkout.
- Use requested `IUserContext.LanguageId`; fallback is requested translation then the existing base field.
- Keep projection, translated search, ordering and paging inside PostgreSQL-translatable expressions.
- Keep current intentional order rules, especially document dates descending and reference codes/names ascending.
- Put each public DTO/filter in its same-named file; private/internal helper models may remain nested.
- Do not introduce new translation tables or infer translated fields when the Domain model has no such table.
- Do not use InMemory EF for query semantics; use the shared `PostgreSqlIntegrationFixture`.
- A feature becomes `VERIFIED` only after targeted tests, scoped DI resolution, API diff review, full unit tests and build.
- Existing accepted warnings are CS8629 in `EdoUnifiedImportPlanRules.cs:33` and `EdoOutboxProviderDocumentDetailMapper.cs:145`.

---

### Task 1: Cmn/Banks query and validation contract

**Files:**
- Create: `tests/IntegrationTests/Features/Cmn/Banks/BankQueryContractTests.cs`
- Create: `tests/UnitTests/BankListFilterValidatorTests.cs`
- Create: `src/Application/Features/Cmn/Banks/Validation/BankListFilterValidator.cs`
- Modify: `docs/features-refactoring-plan.md`
- Modify: `docs/features-multilanguage-audit.md`
- Modify: `docs/features-refactoring-progress.md`

**Interfaces:**
- Consumes: `IBankService.GetAllAsync`, `GetByIdAsync`, `GetBranchesAsync`, `GetBranchByMfoAsync`; `BankListFilter`; the three existing Bank projections.
- Produces: validated paged bank list and PostgreSQL coverage for all four unchanged GET contracts.

- [ ] **Step 1: Write failing filter validation tests**

Assert literal failures for `Page = 0`, `PageSize = 0`, and `Search` longer than 100 characters, plus a valid default filter. The production change that makes these tests pass is registration of a real `AbstractValidator<BankListFilter>`.

- [ ] **Step 2: Run RED**

Run `dotnet test tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~BankListFilterValidatorTests --no-restore`. Expected: compile failure because `BankListFilterValidator` is absent.

- [ ] **Step 3: Add the exact validator**

Implement rules: optional `StateId > 0`, optional nonblank `Search.MaximumLength(100)`, `Page > 0`, and optional `PageSize > 0`. Do not add a maximum page size because the current API has none.

- [ ] **Step 4: Characterize all Bank GETs on PostgreSQL**

Seed two banks and two branches. Execute `BankService` through scoped `QueryBuilderResolver` and assert: result names remain base `Bank.Name`/`BankBranch.Name` because no bank translation entity exists; search is applied before page count; order remains `Name, Id`; MFO lookup trims and finds one branch; a missing bank/MFO returns existing localized `BankErrors`.

- [ ] **Step 5: Verify and commit**

Run targeted Bank tests, `dotnet test tests/UnitTests/UnitTests.csproj --no-restore`, and `dotnet build Accounting.slnx --no-restore`. Update both Bank GET rows plus feature status to `VERIFIED`. Commit as `test(cmn): verify bank query contracts`.

---

### Task 2: Cmn/Contracts translated contract type and DTO split

**Files:**
- Create: `tests/IntegrationTests/Features/Cmn/Contracts/ContractMultilanguageQueryTests.cs`
- Create: `tests/UnitTests/ContractListFilterValidatorTests.cs`
- Create: `src/Application/Features/Cmn/Contracts/Validation/ContractListFilterValidator.cs`
- Create: `src/Application/Features/Cmn/Contracts/DTOs/ProviderContractReconciliationCreateDto.cs`
- Create: `src/Application/Features/Cmn/Contracts/DTOs/ProviderContractReconciliationResultDto.cs`
- Delete: `src/Application/Features/Cmn/Contracts/DTOs/ProviderContractReconciliationDtos.cs`
- Modify: `src/Application/Features/Cmn/Contracts/Projections/ContractDtoProjection.cs`
- Modify: `src/Application/Features/Cmn/Contracts/Projections/ContractListDtoProjection.cs`
- Modify: the three working documentation files.

**Interfaces:**
- Consumes: `ContractType.ContractTypeTranslations`, `IUserContext.LanguageId`, existing Contract list/detail DTO properties.
- Produces: translated `ContractTypeName` with base fallback and unchanged provider-reconciliation JSON contracts.

- [ ] **Step 1: Write RED tests**

Seed organizations, counterparties, contract types, translations for two languages, and contracts. Assert list/detail return the requested `ContractTypeTranslation.Name`, fallback to `ContractType.Name`, translated search still follows the current projected criteria, and order remains current `ContractDate desc, Id desc`. Add filter tests derived from `ContractListFilter` properties.

- [ ] **Step 2: Verify RED**

Run the targeted integration test. Expected: actual `ContractTypeName` equals the base name. Run validator test. Expected: compile failure until the filter validator exists.

- [ ] **Step 3: Implement translation and validation**

Inject `IUserContext` into both projections and replace only `ContractTypeName` with `ContractTypeTranslations.Where(t => t.LanguageId == languageId).Select(t => t.Name).FirstOrDefault() ?? ContractType.Name`. Implement only bounds already implied by DTO field types and existing validators.

- [ ] **Step 4: Split reconciliation DTOs mechanically**

Move each public type unchanged into its same-named file. Compare reflected public property names/types before and after; namespaces and JSON property names must remain identical.

- [ ] **Step 5: Verify and commit**

Run Contract tests, full UnitTests and build; confirm controller/interface files are unchanged. Update docs and commit `feat(i18n): localize contract type queries`.

---

### Task 3: Cmn/CurrencyRates translated currency names

**Files:**
- Create: `tests/IntegrationTests/Features/Cmn/CurrencyRates/CurrencyRateMultilanguageQueryTests.cs`
- Modify: `src/Application/Features/Cmn/CurrencyRates/Projections/CurrencyRateDtoProjection.cs`
- Modify: `src/Application/Features/Cmn/CurrencyRates/Projections/CurrencyRateListDtoProjection.cs`
- Modify: the three working documentation files.

**Interfaces:**
- Consumes: `BaseCurrency.CurrencyTranslations`, `TargetCurrency.CurrencyTranslations`, `IUserContext.LanguageId`.
- Produces: translated `BaseCurrencyName` and `TargetCurrencyName` for list/detail/latest/history without changing rate/date/source fields.

- [ ] **Step 1: Write and run RED PostgreSQL tests**

Seed base/target currencies with language 1 and 3 translations and dated rates. Through `ICurrencyRateService`, assert list, detail, latest and history display requested translations, use base fallback when missing, keep history date-desc ordering, apply filters/paging without duplicate rate IDs, and emit SQL containing `cmn_currency_translation`. Expected RED: both names are base names.

- [ ] **Step 2: Implement minimal projection changes**

Inject `IUserContext` into both projections. Map both display names with the same language-filtered scalar subquery and `?? currency.Name`. Keep codes, numeric rates, `EffectiveDate`, `RateSource`, `StateName` and order builder unchanged.

- [ ] **Step 3: Verify and commit**

Run targeted integration tests, existing validators, full UnitTests and build. Update all six CurrencyRates GET audit rows, explicitly marking provider/status responses as external/non-translation. Commit `feat(i18n): localize currency rate queries`.

---

### Task 4: Cmn/CurrencyRevaluations focused file layout and query contract

**Files:**
- Create: six same-named DTO files for `CurrencyRevaluationBaseDto`, `CurrencyRevaluationCreateDto`, `CurrencyRevaluationPreviewDto`, `CurrencyRevaluationLineDto`, `CurrencyRevaluationDto`, `CurrencyRevaluationListDto`.
- Delete: `src/Application/Features/Cmn/CurrencyRevaluations/DTOs/CurrencyRevaluationDtos.cs`
- Create: three same-named validator files for `CurrencyRevaluationBaseDtoValidator`, `CurrencyRevaluationPreviewDtoValidator`, `CurrencyRevaluationCreateDtoValidator`.
- Delete: `src/Application/Features/Cmn/CurrencyRevaluations/Validators/CurrencyRevaluationValidators.cs`
- Create: `src/Application/Features/Cmn/CurrencyRevaluations/Projections/CurrencyRevaluationDtoProjection.cs`
- Create: `src/Application/Features/Cmn/CurrencyRevaluations/Projections/CurrencyRevaluationListDtoProjection.cs`
- Delete: `src/Application/Features/Cmn/CurrencyRevaluations/Projections/CurrencyRevaluationProjections.cs`
- Create: `tests/IntegrationTests/Features/Cmn/CurrencyRevaluations/CurrencyRevaluationQueryContractTests.cs`
- Modify: the three working documentation files.

**Interfaces:**
- Consumes: current revaluation service/lifecycle and DTO signatures.
- Produces: identical JSON/API/accounting behavior with one public DTO/projection/validator per focused file.

- [ ] **Step 1: Capture contract before moving files**

Add reflection assertions for every public DTO property and FluentValidation assertions for create/preview inputs. Add PostgreSQL assertions for list/detail line totals, target currency code, date-desc order, page count and organization query filter.

- [ ] **Step 2: Run baseline tests**

The characterization tests must pass before the mechanical split. If a test exposes existing behavior, preserve the observed literal value unless it conflicts with the approved specification.

- [ ] **Step 3: Move types without semantic edits**

Move each class body verbatim to the exact same-named file. Do not alter `PreviewAsync`, posting, balance calculations, status transitions or transaction boundaries.

- [ ] **Step 4: Verify and commit**

Run targeted revaluation tests, full UnitTests and build; ensure the diff contains no service/lifecycle accounting change. Update docs and commit `refactor(cmn): split currency revaluation contracts`.

---

### Task 5: Cmn/Documents translated registry display fields

**Files:**
- Rename: `src/Application/Features/Cmn/Documents/DTOs/DocumentRegistryDtos.cs` to `DocumentRegistryDto.cs`
- Create: `tests/IntegrationTests/Features/Cmn/Documents/DocumentRegistryMultilanguageQueryTests.cs`
- Modify: `src/Application/Features/Cmn/Documents/Projections/DocumentRegistryProjections.cs`
- Modify: the three working documentation files.

**Interfaces:**
- Consumes: `DocumentTypeTranslations`, `DocumentStatusTranslations`, `CurrencyTranslations`, `IUserContext.LanguageId`.
- Produces: translated `DocumentTypeName`, nullable `StatusName`, nullable `CurrencyName`, each with its existing base-name fallback.

- [ ] **Step 1: Write and run RED tests**

Seed two registry rows and translations for requested language. Call list by `DocumentTypeCode` and detail by registry ID; assert all three display names, nullable navigation handling, date filtering, date-desc order, no paging wrapper, and SQL translation. Expected RED: base names are returned.

- [ ] **Step 2: Implement minimal projection change**

Inject `IUserContext` and map the three names with language-filtered subqueries. Retain current code fields, IDs, amount, dates, status nullability and `State.FullName` because State has no dedicated translation entity.

- [ ] **Step 3: Rename DTO file mechanically**

Move the unchanged `DocumentRegistryDto` class to its same-named file. Do not modify `IDocumentRegistryService`, `DocumentsController`, `DocumentRegistryListFilter` or JSON types.

- [ ] **Step 4: Verify and commit**

Run targeted tests, full UnitTests and build. Update both Documents GET rows and commit `feat(i18n): localize document registry queries`.

---

### Task 6: Cmn/PricingConditions query and filter contract

**Files:**
- Create: `tests/IntegrationTests/Features/Cmn/PricingConditions/PricingConditionQueryContractTests.cs`
- Create: `tests/UnitTests/PricingConditionListFilterValidatorTests.cs`
- Create: `src/Application/Features/Cmn/PricingConditions/Validation/PricingConditionListFilterValidator.cs`
- Modify: the three working documentation files.

**Interfaces:**
- Consumes: existing PricingCondition list/detail/now queries and order builder.
- Produces: validated filters and evidence that base names are intentional because no PricingCondition translation entity exists.

- [ ] **Step 1: Write RED validator tests and PostgreSQL characterization**

Assert invalid pagination/search bounds fail. Seed overlapping effective periods and assert `GetNowAsync` selects exactly the current active row according to existing priority, list filtering stays SQL-side, order remains current builder order, and detail errors remain localized.

- [ ] **Step 2: Implement only the missing filter validator**

Mirror the concrete `PricingConditionListFilter` optional IDs/dates and pagination rules without inventing cross-field business constraints. Leave projections unchanged after the test proves no translation model exists.

- [ ] **Step 3: Verify and commit**

Run targeted tests, full UnitTests and build; update all three endpoint rows and commit `test(cmn): verify pricing condition queries`.

---

### Task 7: Cmn/Taxes public contract split and behavior verification

**Files:**
- Create: four same-named integration DTO files for `TaxLookupRequestDto`, `TaxDocumentRequestDto`, `TaxLookupItemDto`, `TaxDocumentResultDto`.
- Delete: `src/Application/Features/Cmn/Taxes/Integration/DTOs/TaxIntegrationRequests.cs`
- Delete: `src/Application/Features/Cmn/Taxes/Integration/DTOs/TaxIntegrationResponses.cs`
- Create: `tests/UnitTests/TaxPublicContractTests.cs`
- Create: `tests/IntegrationTests/Features/Cmn/Taxes/TaxQueryContractTests.cs`
- Modify: the three working documentation files.

**Interfaces:**
- Consumes: `ITaxService`, `ITaxResolverService`, `ITaxCalculationService`, `ITaxIntegrationService`.
- Produces: unchanged tax CRUD/calculation/provider JSON contracts with one public DTO per file.

- [ ] **Step 1: Characterize before split**

Reflect exact property names/types/nullability for all four integration DTOs. On PostgreSQL assert organization scoping, effective-date resolution, rate calculation literals, list paging/order and existing multilingual errors. Provider/MXIK responses are external provider text and must not be rewritten through local translation tables.

- [ ] **Step 2: Move DTOs verbatim**

Create the four same-named files with identical namespaces, properties and defaults, then delete the two grouped files. Do not change tax formulas, rounding, effective-date selection or provider calls.

- [ ] **Step 3: Verify and commit**

Run tax unit/integration tests, full UnitTests and build. Mark local GETs and external GETs separately in the audit. Commit `refactor(cmn): split tax integration contracts`.

---

### Task 8: Cmn/Manual translation-backed select lists

**Files:**
- Create: `tests/IntegrationTests/Features/Cmn/Manual/ManualMultilanguageQueryTests.cs`
- Modify: `src/Application/Features/Cmn/Manual/Services/ManualService.cs`
- Modify: the three working documentation files.

**Interfaces:**
- Consumes: the existing `IManualService` methods and 19 dedicated translation navigations used by manual entities.
- Produces: SQL-side requested-language names with base fallback for every translation-backed manual endpoint.

- [ ] **Step 1: Write grouped RED tests from literal fixtures**

Seed requested-language and fallback rows for Currency, DocumentStatus, PaymentType, CostingMethod, DocumentType, OperationType, MovementDirection, ContractType and AccountType. Existing translated methods (UserKind, BankOperationCategory, FaReceiptType, FaDisposalType, RentalObjectType, ProductGroup, PaymentAcceptancePointType, PaymentMethod, FiscalCashRegisterType, SubkontoType) are regression controls. For every method assert translated name, base fallback, name order, active-state filter and no duplicate IDs.

- [ ] **Step 2: Verify RED**

Run `ManualMultilanguageQueryTests`. Expected: the nine methods currently projecting `.Name` directly fail; existing translated methods remain green.

- [ ] **Step 3: Replace only nine direct-name projections**

Capture `var languageId = _userContext.LanguageId` and map each of the nine entities through its real translation navigation. Do not add translation logic to State, Region, District, Unit, InventoryAdjustmentType, FaGroup, FaOkof, FaDepreciationMethod, TaxType, VatRate or other entities without a dedicated translation model.

- [ ] **Step 4: Verify SQL behavior and commit**

Assert generated SQL contains the corresponding `*_translation` table and sorting occurs after the translated projection. Run full UnitTests/build, update the affected manual endpoint rows, and commit `feat(i18n): localize manual select lists`.

---

### Task 9: Cmn/Manual DTO layout and remaining select-list contracts

**Files:**
- Create: `BankBranchSelectListDto.cs`, `ProductSelectListDto.cs`, `ModuleSelectListDto.cs` in the existing Manual DTO folder.
- Modify: `SelectListDto.cs` so it contains only `SelectListDto`.
- Modify: `ModuleSubGroupSelectListDto.cs` so it contains only `ModuleSubGroupSelectListDto`.
- Create: `tests/UnitTests/ManualDtoContractTests.cs`
- Create: `tests/IntegrationTests/Features/Cmn/Manual/ManualScopeAndFilterTests.cs`
- Modify: the three working documentation files.

**Interfaces:**
- Consumes: all remaining `IManualService` selectors, organization/user scope and existing DTO properties.
- Produces: unchanged manual JSON with one public DTO per file and PostgreSQL coverage of scope/filter behavior.

- [ ] **Step 1: Capture DTO and scope behavior**

Reflect exact public properties for all five DTOs. Test SuperAdmin/TenantAdmin/TenantUser organization selection, organization-scoped FA groups/products/warehouses/accounts/cashes, optional branch/bank/counterparty filters, module grouping and deterministic ordering.

- [ ] **Step 2: Split DTOs mechanically**

Move the four secondary public DTO declarations unchanged into their same-named files. Keep private `ModuleFlatDto` nested in `ManualService` because it is an internal SQL projection only.

- [ ] **Step 3: Verify and commit**

Run all Manual tests, full UnitTests and build; compare `ManualController` and `IManualService` signatures before/after. Mark all remaining manual GET rows `VERIFIED` and commit `refactor(cmn): split manual DTO contracts`.

---

### Task 10: Cmn/Barcode and complete Cmn verification

**Files:**
- Create: `tests/UnitTests/BarcodeControllerContractTests.cs`
- Modify: `docs/features-refactoring-plan.md`
- Modify: `docs/features-refactoring-progress.md`
- Modify: `docs/features-multilanguage-audit.md`

**Interfaces:**
- Consumes: current QR, Code128 and EAN13 GET routes plus `BarcodeErrors`.
- Produces: a complete `Cmn` verification checkpoint and exact next area `Org`.

- [ ] **Step 1: Characterize barcode GETs**

Execute the real barcode generation boundary for valid content and invalid EAN13 input. Assert content types/status/error codes rather than image bytes or source text. Preserve the existing localized `BarcodeErrors` and do not introduce database abstractions.

- [ ] **Step 2: Run the complete verification suite**

Run `dotnet build Accounting.slnx --no-restore` and `dotnet test Accounting.slnx --no-build`. Require zero failures and no warnings beyond the two accepted baseline CS8629 warnings.

- [ ] **Step 3: Review API and architecture diff**

Diff from commit `9a7d6a46`. Confirm no controller route, HTTP verb, service interface, public property type, posting/calculation/transaction or SQL schema changed. Run the public DTO one-file audit for `src/Application/Features/Cmn` and require zero grouped public DTO files.

- [ ] **Step 4: Record and commit the Cmn checkpoint**

Set every Cmn row and all its GET audit rows to `VERIFIED`, except any explicit external-provider row which is marked `VERIFIED_EXTERNAL_TEXT`. Set progress to `Current area: Org`, `Current feature: Branches`, `Current phase: AUDIT`, and record exact build/test counts. Commit `docs: record completed cmn service refactoring`.
