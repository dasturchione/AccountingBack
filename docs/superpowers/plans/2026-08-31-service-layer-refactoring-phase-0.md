# Service Layer Refactoring Phase 0 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Создать воспроизводимую инфраструктуру аудита и PostgreSQL integration-тестов, полностью инвентаризировать сервисный слой и довести первый translation-aware feature `Cmn/Currencies` до `VERIFIED` без изменения API.

**Architecture:** Общая задача разделяется на последовательные area plans. Phase 0 создаёт три обязательных рабочих документа, metadata contracts для translation entities, Testcontainers PostgreSQL harness и первый вертикальный feature slice. После Phase 0 каждая область получает отдельный план с тем же циклом audit → failing characterization test → minimal refactor → verification.

**Tech Stack:** .NET 10, C# 14, EF Core 10, Npgsql, PostgreSQL 17, Testcontainers.PostgreSql 4.14.0, xUnit 2.9.3, FluentValidation, Scrutor, existing Result/QueryBuilder/repository abstractions.

**Spec:** `docs/superpowers/specs/2026-08-31-service-layer-refactoring-design.md`

## Global Constraints

- Preserve routes, HTTP verbs, request/response JSON, nullable contracts, IDs, status codes and public behavior.
- Preserve accounting, inventory, money, counterparty, VAT, totals, numbering, status and transaction semantics.
- Do not overwrite the existing untracked `docs/service-layer-deep-analysis.md`.
- Use `IUserContext.LanguageId`; fallback is requested translation then the base field.
- Keep filtering, translation projection, ordering and paging SQL-side.
- One public API DTO/filter per same-named file; internal state models remain grouped when justified.
- No AutoMapper, Mapster, MediatR, mass formatting or global QueryBuilder semantic change.
- A feature becomes `VERIFIED` only after targeted tests, PostgreSQL query tests, build, DI/API review and documentation update.
- Existing baseline warnings are `CS8629` in `EdoUnifiedImportPlanRules.cs:33` and `EdoOutboxProviderDocumentDetailMapper.cs:145`.

---

### Task 1: Persistent working documents and full feature catalogue

**Files:**
- Create: `docs/features-refactoring-plan.md`
- Create: `docs/features-refactoring-progress.md`
- Create: `docs/features-multilanguage-audit.md`
- Read: `src/Application/Features/**/*.cs`
- Read: `src/Presentation/WebApi/Controllers/**/*.cs`

**Interfaces:**
- Consumes: file layout, service/controller declarations and the status vocabulary from the spec.
- Produces: one plan row for every actual feature directory, one audit row for every current `[HttpGet]`/GET service method, and a resumable current-position record.

- [ ] **Step 1: Capture the baseline in progress documentation**

Create `docs/features-refactoring-progress.md` with the exact baseline:

```markdown
# Current position

Current area: Cmn
Current feature: Currencies
Current phase: INVENTORY
Last verified commit: ff7dd8e0
Last successful build: 2026-08-31 — succeeded, 0 errors, 2 baseline CS8629 warnings
Last successful test: 2026-08-31 — UnitTests 134/134; IntegrationTests discovered 0 tests

# Completed

- Baseline repository/build/test audit.
- Service-layer design approved and committed.

# Modified but not verified

- None.

# Blocked

- PostgreSQL integration execution until Docker daemon is available.

# Translation decisions

- Current fallback: requested IUserContext.LanguageId translation, then base field.
- Inline projection exceptions: internal one-use/read models and common SelectListDto only when documented.
- SelectListDto exceptions: no dedicated builder required, but inline SQL projection must translate display fields.
- Internal DTO exceptions: internal/private state-machine and provider models stay grouped.

# Next exact action

1. Create the complete feature catalogue.
2. Create the translation entity/key/navigation catalogue.
3. Add PostgreSQL integration harness.
```

- [ ] **Step 2: Build the complete feature inventory**

Enumerate every first- and second-level feature location under `src/Application/Features`. For areas containing service files directly rather than a child feature directory, add the area itself as a feature row. Record:

```markdown
| Area | Feature | Entity | GET methods | Translation entity | LanguageId | Projection | DTO split | Complexity | Status |
|---|---|---|---|---|---|---|---|---|---|
```

Every discovered row starts `NOT_STARTED`, except `Cmn/Currencies`, which starts `AUDITED` after Task 4 Step 1. Do not merge physically separate features just because they share a namespace.

- [ ] **Step 3: Build the GET audit catalogue**

Parse all 326 baseline `[HttpGet]` declarations and match each action to its called service method. Add a row to `docs/features-multilanguage-audit.md`:

```markdown
| Endpoint/method | Entity | Translation | Fields | Language selection | Fallback | Projection | Search/order | Tests | Status |
|---|---|---|---|---|---|---|---|---|---|
```

Rows start `NOT_STARTED`. Where one controller GET calls multiple service reads, list each service method in the first column.

- [ ] **Step 4: Verify document structure**

Run:

```powershell
$plan = Get-Content docs/features-refactoring-plan.md -Raw
$audit = Get-Content docs/features-multilanguage-audit.md -Raw
if ($plan -notmatch '\| Area \| Feature \|') { throw 'Feature table missing' }
if ($audit -notmatch '\| Endpoint/method \| Entity \|') { throw 'GET audit table missing' }
if ((rg -n '\[HttpGet' src/Presentation/WebApi/Controllers -g '*.cs').Count -ne 326) { throw 'GET baseline changed' }
```

Expected: exit 0.

- [ ] **Step 5: Commit the documentation catalogue**

```bash
git add docs/features-refactoring-plan.md docs/features-refactoring-progress.md docs/features-multilanguage-audit.md
git commit -m "docs: inventory application features and GET endpoints"
```

---

### Task 2: Translation model metadata contract

**Files:**
- Create: `tests/UnitTests/TranslationModelContractTests.cs`
- Modify: `docs/features-multilanguage-audit.md`
- Read: `src/Domain/Entities/**/*Translation.cs`
- Read: `src/Infrastructure/Persistence/AppDbContext/AppDbContext.cs`
- Read: `src/Infrastructure/Persistence/AppDbContext/AppDbContext.cs` model configuration partials

**Interfaces:**
- Consumes: `Infrastructure.Persistence.AppDbContext` EF metadata.
- Produces: `TranslationModelContractTests.DedicatedTranslationTablesHaveCompositeLanguageKeysAndNavigations` and a complete table/entity/fields/key/FK/navigation audit.

- [ ] **Step 1: Write the failing metadata test**

Create a test that constructs `AppDbContext` with Npgsql metadata-only options and enumerates `context.Model.GetEntityTypes()` whose table name ends with `_translation`. For every dedicated translation entity assert:

```csharp
var primaryKey = entityType.FindPrimaryKey();
Assert.NotNull(primaryKey);
Assert.Equal(2, primaryKey.Properties.Count);
Assert.Contains(primaryKey.Properties, property => property.Name == "LanguageId");
Assert.Contains(entityType.GetForeignKeys(), foreignKey =>
    foreignKey.Properties.Any(property => property.Name == "LanguageId"));
Assert.Contains(entityType.GetForeignKeys(), foreignKey =>
    foreignKey.PrincipalEntityType != entityType &&
    foreignKey.Properties.All(property => property.Name != "LanguageId"));
```

Also assert the baseline dedicated translation count equals the actual discovered count captured by the test, not a hardcoded filename count. Emit the table/entity/property list into assertion diagnostics.

- [ ] **Step 2: Run the test to expose model exceptions**

Run:

```bash
dotnet test tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~TranslationModelContractTests --no-restore
```

Expected before final implementation: FAIL if a `_translation` mapping lacks the composite language key/FKs, or compile failure until helper code is complete.

- [ ] **Step 3: Complete the metadata test against the actual model**

Use:

```csharp
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseNpgsql("Host=localhost;Database=model_metadata_only")
    .Options;
await using var context = new AppDbContext(options);
```

Do not open the connection. Separate the generic `Translation` entity from dedicated `*_translation` tables in diagnostics because it may not follow the dedicated two-column pattern.

- [ ] **Step 4: Record actual translation mappings**

Add a section to `docs/features-multilanguage-audit.md`:

```markdown
## Translation model inventory

| Table | Base entity | Translation entity | Translated fields | Composite PK | Base FK/navigation | Language FK/navigation | Status |
|---|---|---|---|---|---|---|---|
```

Populate from actual EF metadata and Domain classes. Mark `MODEL_VERIFIED` only when the test passes.

- [ ] **Step 5: Run and commit**

Run:

```bash
dotnet test tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~TranslationModelContractTests --no-restore
```

Expected: PASS.

```bash
git add tests/UnitTests/TranslationModelContractTests.cs docs/features-multilanguage-audit.md
git commit -m "test(i18n): verify translation model contracts"
```

---

### Task 3: PostgreSQL Testcontainers integration harness

**Files:**
- Modify: `tests/IntegrationTests/IntegrationTests.csproj`
- Create: `tests/IntegrationTests/Infrastructure/PostgreSqlIntegrationFixture.cs`
- Create: `tests/IntegrationTests/Infrastructure/IntegrationTestUserContext.cs`
- Create: `tests/IntegrationTests/Infrastructure/PostgreSqlSmokeTests.cs`
- Modify: `docs/features-refactoring-progress.md`

**Interfaces:**
- Consumes: `AppDbContext(DbContextOptions<AppDbContext>, IBackgroundOrganizationScope?)` and `AppDbContext.SetUserContext(IUserContext)`.
- Produces: shared xUnit collection `PostgreSqlIntegration`, `PostgreSqlIntegrationFixture.CreateDbContext(IUserContext?)`, and an ephemeral PostgreSQL connection string.

- [ ] **Step 1: Add the test-only package**

Add to `tests/IntegrationTests/IntegrationTests.csproj`:

```xml
<PackageReference Include="Testcontainers.PostgreSql" Version="4.14.0" />
```

Run `dotnet restore tests/IntegrationTests/IntegrationTests.csproj`. Expected: restore succeeds.

- [ ] **Step 2: Write the failing smoke test**

Create `PostgreSqlSmokeTests` in collection `PostgreSqlIntegration`:

```csharp
[Collection(PostgreSqlIntegrationFixture.CollectionName)]
public sealed class PostgreSqlSmokeTests(PostgreSqlIntegrationFixture fixture)
{
    [Fact]
    public async Task AppDbContextExecutesAgainstPostgreSql()
    {
        await using var context = fixture.CreateDbContext();
        Assert.True(await context.Database.CanConnectAsync());
        Assert.Contains("Npgsql", context.Database.ProviderName);
    }
}
```

- [ ] **Step 3: Run to verify the missing fixture failure**

Run:

```bash
dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter FullyQualifiedName~PostgreSqlSmokeTests
```

Expected: FAIL to compile because `PostgreSqlIntegrationFixture` is not defined.

- [ ] **Step 4: Implement the fixture**

Implement `IAsyncLifetime` with:

```csharp
private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
    .WithImage("postgres:17-alpine")
    .WithDatabase("accounting_tests")
    .WithUsername("postgres")
    .WithPassword("postgres")
    .Build();

public async Task InitializeAsync()
{
    await _container.StartAsync();
    await using var context = CreateDbContext();
    await context.Database.EnsureCreatedAsync();
}

public Task DisposeAsync() => _container.DisposeAsync().AsTask();
```

`CreateDbContext` uses `UseNpgsql(_container.GetConnectionString())`; when a user context is supplied, call `context.SetUserContext(userContext)` before returning it. Define an xUnit `CollectionDefinition` implementing `ICollectionFixture<PostgreSqlIntegrationFixture>`.

- [ ] **Step 5: Implement mutable test user context**

`IntegrationTestUserContext` implements every `IUserContext` property and allows test setup of `Id`, `UserKind`, `LanguageId`, `TenantId`, `OrganizationId`, `AllowedOrganizationIds` and `BranchId` without production changes.

- [ ] **Step 6: Start Docker and run smoke test**

Ensure Docker Desktop daemon is running, then run:

```bash
dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter FullyQualifiedName~PostgreSqlSmokeTests
```

Expected: 1 passed, provider name contains `Npgsql`.

- [ ] **Step 7: Record and commit harness**

Update progress `Last successful test` with smoke-test timestamp and remove the Docker blocker only after PASS.

```bash
git add tests/IntegrationTests/IntegrationTests.csproj tests/IntegrationTests/Infrastructure docs/features-refactoring-progress.md
git commit -m "test(i18n): add PostgreSQL integration harness"
```

---

### Task 4: Cmn/Currencies multilingual characterization and fix

**Files:**
- Modify: `src/Application/Features/Cmn/Currencies/Projections/CurrencyDtoProjection.cs`
- Modify: `src/Application/Features/Cmn/Currencies/Projections/CurrencyListDtoProjection.cs`
- Read and preserve: `src/Application/Features/Cmn/Currencies/Queries/CurrencyListDtoByListFilterCriteriaBuilder.cs`
- Read and preserve: `src/Application/Features/Cmn/Currencies/OrderBy/CurrencyListDtoOrderByBuilder.cs`
- Create: `tests/IntegrationTests/Features/Cmn/Currencies/CurrencyMultilanguageQueryTests.cs`
- Modify: `docs/features-refactoring-plan.md`
- Modify: `docs/features-multilanguage-audit.md`
- Modify: `docs/features-refactoring-progress.md`

**Interfaces:**
- Consumes: `IProjectionBuilder<Currency,CurrencyDto>`, `IProjectionBuilder<Currency,CurrencyListDto>`, `IUserContext.LanguageId`, `Currency.CurrencyTranslations`, `CurrencyTranslation` composite key.
- Produces: list/detail projections whose `Name` is requested translation with base fallback; no public DTO/API changes.

- [ ] **Step 1: Audit the actual feature**

Verify `Currency`, `CurrencyTranslation`, EF composite key `(CurrencyId, LanguageId)`, `Currency.CurrencyTranslations`, `CurrencyTranslation.Currency/Language`, controller GET routes, service GET methods, current projections, criteria and order. Record exact endpoint rows and set feature `IN_PROGRESS`.

- [ ] **Step 2: Write failing PostgreSQL list/detail tests**

Seed two currencies and translations for language IDs 1 and 3. Test through actual SQL projections:

```csharp
var user = new IntegrationTestUserContext { LanguageId = 3 };
var listProjection = new CurrencyListDtoProjection(user).Build();
var detailProjection = new CurrencyDtoProjection(user).Build();

var list = await context.Currencies
    .OrderBy(x => x.Id)
    .Select(listProjection)
    .ToListAsync();
var detail = await context.Currencies
    .Where(x => x.Id == translatedCurrencyId)
    .Select(detailProjection)
    .SingleAsync();

Assert.Equal("Русский сум", detail.Name);
Assert.Equal("Русский сум", list.Single(x => x.Id == translatedCurrencyId).Name);
Assert.Equal("Base dollar", list.Single(x => x.Id == fallbackCurrencyId).Name);
```

Add a second test using language ID 1 and assert a different translated value. Add `ToQueryString()` assertions that SQL contains `cmn_currency_translation` and no client-side method appears.

- [ ] **Step 3: Run tests to verify the current bug**

Run:

```bash
dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter FullyQualifiedName~CurrencyMultilanguageQueryTests
```

Expected: FAIL because both current projections return `Currency.Name` and ignore `LanguageId`.

- [ ] **Step 4: Implement minimal projection change**

Inject `IUserContext` into both projection constructors. Capture `var languageId = _userContext.LanguageId` before returning the expression and map:

```csharp
Name = x.CurrencyTranslations
    .Where(translation => translation.LanguageId == languageId)
    .Select(translation => translation.Name)
    .FirstOrDefault()
    ?? x.Name
```

Leave `Code`, `Symbol`, IDs and `StateName` unchanged because current `State` model has no dedicated `StateTranslation` navigation. Do not add one without a real table/model.

- [ ] **Step 5: Verify translated search and paging**

Resolve the actual `CurrencyListDtoByListFilterCriteriaBuilder` and `CurrencyListDtoOrderByBuilder` through `QueryBuilderResolver`, execute `BuildPaged<Currency,CurrencyListDto,CurrencyListFilter>` against PostgreSQL, and assert:

- search by the displayed translated name returns the row;
- base name remains searchable only if the chosen criteria intentionally includes it;
- page size is applied after result criteria;
- no duplicate currency IDs;
- order remains `Code, Id`, matching the current public behavior.

Do not change ordering to translated `Name`, because Currency currently intentionally orders by code.

- [ ] **Step 6: Run feature and full build tests**

```bash
dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter FullyQualifiedName~CurrencyMultilanguageQueryTests
dotnet test tests/UnitTests/UnitTests.csproj --no-restore
dotnet build Accounting.slnx --no-restore
```

Expected: multilingual tests PASS, UnitTests 134+ PASS, build 0 errors with only the two baseline warnings.

- [ ] **Step 7: DI and API compatibility review**

Resolve both projections from a scoped service provider containing scoped `IUserContext`; assert different scopes/languages produce different expression constants. Compare `CurrencyController` routes and DTO public properties before/after; expected: no changes.

- [ ] **Step 8: Mark verified and commit**

Update all three working documents: `Cmn/Currencies = VERIFIED`, GET rows `VERIFIED`, tests/build timestamps, fallback decision and next feature `Cmn/Banks`.

```bash
git add src/Application/Features/Cmn/Currencies tests/IntegrationTests/Features/Cmn/Currencies docs/features-refactoring-plan.md docs/features-multilanguage-audit.md docs/features-refactoring-progress.md
git commit -m "feat(i18n): localize currency query projections"
```

---

### Task 5: Phase 0 verification and handoff

**Files:**
- Modify: `docs/features-refactoring-progress.md`
- Review: all files changed by Tasks 1–4

**Interfaces:**
- Consumes: verified harness, complete inventories and the Currencies vertical slice.
- Produces: verified Phase 0 baseline and an exact next position for the separate remaining-Cmn plan.

- [ ] **Step 1: Review Phase 0 diff**

Run:

```bash
git status --short
git diff --check HEAD~3..HEAD
git diff --stat HEAD~3..HEAD
```

Confirm no route/DTO property/accounting files changed and `docs/service-layer-deep-analysis.md` remains untouched/untracked.

- [ ] **Step 2: Run the full verification suite**

```bash
dotnet build Accounting.slnx --no-restore
dotnet test Accounting.slnx --no-build
```

Expected: 0 build errors, only the two baseline warnings; all UnitTests and newly discoverable IntegrationTests pass.

- [ ] **Step 3: Update resumable position**

Set:

```markdown
Current area: Cmn
Current feature: Banks
Current phase: AUDIT
```

Record all successful command outputs. Set `Next exact action` to audit `Cmn/Banks`, then create the separate remaining-Cmn implementation plan from the now-complete inventory before changing Banks production code.

- [ ] **Step 4: Commit the Phase 0 verification record**

```bash
git add docs/features-refactoring-progress.md
git commit -m "docs: record service refactoring phase zero"
```
