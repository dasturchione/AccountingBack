# Regulated Obligations SQL Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add the SQL schema and base catalogue data for multilingual regulated obligations and dated organization settings without switching or deleting the legacy application model.

**Architecture:** Six `cmn` tables hold categories, obligations, reporting periodicities, and their translations. One `org` table holds organization-specific rate, classifier, settlement account, and effective dates. The rollout is additive so the current `cmn_tax_type`, `org_tax_settings`, and `cmn_vat_rate` consumers continue to work until the Domain/application migration is implemented.

**Tech Stack:** PostgreSQL SQL scripts, Testcontainers PostgreSQL integration tests, xUnit, .NET 10.

**Spec:** `docs/superpowers/specs/2026-09-03-regulated-obligations-design.md`

## Global Constraints

- Use `varchar`, `int`, `smallserial`, and `serial`; do not use `character varying`, identity syntax, or table-level primary-key declarations.
- Put simple foreign-key `references` clauses directly beside their columns.
- Do not add `updated_date` to `cmn_regulated_obligation_category` or `cmn_regulated_obligation`.
- Keep `updated_date` nullable on `org_regulated_obligation_setting`.
- Do not drop or modify `cmn_tax_type`, `org_tax_settings`, or `cmn_vat_rate` in this SQL-only phase.
- Do not change Domain models, APIs, or services in this phase.
- Do not commit changes until the user explicitly requests a commit.

---

### Task 1: Define executable SQL contract tests

**Files:**
- Create: `tests/IntegrationTests/Features/Cmn/RegulatedObligations/RegulatedObligationSqlTests.cs`

**Interfaces:**
- Consumes: SQL files under `src/Infrastructure/Persistence/Scripts/01_cmn` and `02_org`, executed against the real PostgreSQL integration fixture.
- Produces: Executable schema assertions for all seven new tables, seed codes, indexes, constraints, and additive rollout safety.

- [ ] **Step 1: Write a failing executable SQL migration test**

Create an isolated PostgreSQL schema with minimal prerequisite tables (`cmn_state`, `cmn_language`, `org_organization`, `acc_chart_account`), assert that every planned script exists, execute the scripts in dependency order, and query the resulting schema. Assert real outcomes:

```sql
select count(*) from cmn_regulated_obligation_category;
select count(*) from cmn_regulated_obligation_category_translation;
select count(*) from cmn_regulated_obligation_periodicity;
select count(*) from cmn_regulated_obligation_periodicity_translation;
```

Insert a valid organization setting. Then verify PostgreSQL rejects a second active open-ended setting for the same organization and obligation, a rate above 100, an end date before the start date, a blank classifier code, and nonexistent foreign keys. Query `information_schema.columns` to verify `updated_date` is absent from the category and obligation tables and present on the organization setting.

- [ ] **Step 2: Run the focused test and verify failure**

Run:

```powershell
dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter FullyQualifiedName~RegulatedObligationSqlTests
```

Expected: FAIL with the assertion that the first regulated-obligation SQL script does not exist.

### Task 2: Add common catalogues and translations

**Files:**
- Create: `src/Infrastructure/Persistence/Scripts/01_cmn/0175_create_cmn_regulated_obligation_category.sql`
- Create: `src/Infrastructure/Persistence/Scripts/01_cmn/0176_create_cmn_regulated_obligation_category_translation.sql`
- Create: `src/Infrastructure/Persistence/Scripts/01_cmn/0177_create_cmn_regulated_obligation.sql`
- Create: `src/Infrastructure/Persistence/Scripts/01_cmn/0178_create_cmn_regulated_obligation_translation.sql`
- Create: `src/Infrastructure/Persistence/Scripts/01_cmn/0179_create_cmn_regulated_obligation_periodicity.sql`
- Create: `src/Infrastructure/Persistence/Scripts/01_cmn/0180_create_cmn_regulated_obligation_periodicity_translation.sql`
- Create: `src/Infrastructure/Persistence/Scripts/01_cmn/0181_insert_cmn_regulated_obligation_catalogues.sql`

**Interfaces:**
- Consumes: `cmn_state(id)` and `cmn_language(id)`.
- Produces: `cmn_regulated_obligation_category(id)`, `cmn_regulated_obligation(id)`, and `cmn_regulated_obligation_periodicity(id)` for later organization settings and Domain models.

- [ ] **Step 1: Create category and category translation tables**

Use `smallserial primary key`, uppercase-code and nonblank-name checks, a unique code constraint, `state_id smallint not null default 1 references cmn_state(id)`, and a composite translation primary key `(category_id, language_id)`. Do not include `updated_date` in the category table.

- [ ] **Step 2: Create obligation and obligation translation tables**

Use `category_id smallint not null references cmn_regulated_obligation_category(id)`, a unique uppercase `code`, a required base `name`, active state metadata, and a composite translation primary key `(regulated_obligation_id, language_id)`. Do not include `updated_date` in the obligation table.

- [ ] **Step 3: Create periodicity and periodicity translation tables**

Use a unique uppercase `code`, required base `name`, active state metadata, and composite translation primary key `(periodicity_id, language_id)`.

- [ ] **Step 4: Seed categories and reporting periodicities by code**

Insert base rows idempotently with `on conflict (code) do update`, then insert translations by resolving parent and language IDs through their codes rather than hard-coding foreign-key IDs. Seed exactly the category and periodicity codes declared in the design spec.

- [ ] **Step 5: Run the focused tests**

Run:

```powershell
dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter FullyQualifiedName~RegulatedObligationSqlTests
```

Expected: category and periodicity assertions pass; organization-setting assertions still fail.

### Task 3: Add dated organization settings

**Files:**
- Create: `src/Infrastructure/Persistence/Scripts/02_org/0211_create_org_regulated_obligation_setting.sql`
- Modify: `src/Infrastructure/Persistence/Scripts/_run_order.txt`

**Interfaces:**
- Consumes: organization, obligation, periodicity, chart-account, and state primary keys.
- Produces: `org_regulated_obligation_setting` for the later application feature.

- [ ] **Step 1: Create the organization setting table**

Use the approved columns and types. Add checks for rate range, date order, and nonblank optional classifier code. Add a unique constraint on `(organization_id, regulated_obligation_id, effective_from)`.

- [ ] **Step 2: Add lookup and current-row indexes**

Add indexes for organization, obligation, periodicity, chart account, and effective dates. Add a filtered unique index on `(organization_id, regulated_obligation_id)` where `effective_to is null and state_id = 1`.

- [ ] **Step 3: Register scripts in execution order**

Append the seven `01_cmn` scripts in dependency order, followed by `02_org/0211_create_org_regulated_obligation_setting.sql`, after all existing entries so `acc_chart_account` already exists when the organization setting script runs.

- [ ] **Step 4: Run focused tests**

Run:

```powershell
dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter FullyQualifiedName~RegulatedObligationSqlTests
```

Expected: PASS.

### Task 4: Verify the additive SQL phase

**Files:**
- Verify all files created or modified in Tasks 1–3.

**Interfaces:**
- Consumes: completed SQL schema and tests.
- Produces: verified SQL-only deliverable ready for user review.

- [ ] **Step 1: Run the complete integration test project**

Run:

```powershell
dotnet test tests/IntegrationTests/IntegrationTests.csproj
```

Expected: PASS.

- [ ] **Step 2: Run repository formatting checks**

Run:

```powershell
git diff --check
git status --short
```

Expected: no whitespace errors; only the design/plan, SQL scripts, run-order file, and focused test are changed.

- [ ] **Step 3: Report deployment order**

Report the exact SQL scripts to run and explicitly state that no legacy table is dropped and no commit was created.
