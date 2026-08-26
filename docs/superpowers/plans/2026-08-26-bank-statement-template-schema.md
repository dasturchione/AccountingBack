# Bank Statement Template Schema Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add the four global PostgreSQL tables that declaratively configure Excel bank-statement parsing.

**Architecture:** A versioned template owns header-detection rules, row-classification rules, and field mappings. All supported codes are constrained to fixed allow-lists so later parser code can interpret configuration without bank-specific methods.

**Tech Stack:** PostgreSQL SQL scripts, existing ordered script runner, .NET 10 repository verification.

**Spec:** `docs/superpowers/specs/2026-08-26-bank-statement-template-schema-design.md`

## Global Constraints

- Do not modify or stage unrelated RetailSale or cash-collection work.
- Do not add domain models, parser logic, API changes, or template seed rows in this phase.
- Do not create a commit unless the user explicitly requests it.
- Preserve the existing uncommitted `0152` and `0607` run-order entries.

---

### Task 1: Add the bank-statement template schema

**Files:**
- Create: `src/Infrastructure/Persistence/Scripts/01_cmn/0157_create_cmn_bank_statement_template.sql`
- Create: `src/Infrastructure/Persistence/Scripts/01_cmn/0158_create_cmn_bank_statement_template_header_rule.sql`
- Create: `src/Infrastructure/Persistence/Scripts/01_cmn/0159_create_cmn_bank_statement_template_row_rule.sql`
- Create: `src/Infrastructure/Persistence/Scripts/01_cmn/0160_create_cmn_bank_statement_template_field.sql`
- Modify: `src/Infrastructure/Persistence/Scripts/_run_order.txt`

**Interfaces:**
- Consumes: `cmn_bank(id)`, `cmn_state(id)`.
- Produces: the four tables and their constraints for later domain models and the universal parser.

- [ ] **Step 1: Create the parent template table**

Create `cmn_bank_statement_template` with identity PK, bank/state FKs, unique code, positive version, sheet-match allow-list, optional sheet pattern, non-negative data-row offset, and bank/state indexes.

- [ ] **Step 2: Create header-rule table**

Create `cmn_bank_statement_template_header_rule` with cascade FK, relative row offset, positive column index, match and normalization allow-lists, required flag, unique cell rule per template, and template index.

- [ ] **Step 3: Create row-rule table**

Create `cmn_bank_statement_template_row_rule` with cascade FK, positive unique priority per template, row-kind/operator/normalization allow-lists, and a constraint requiring comparison text only for textual operators.

- [ ] **Step 4: Create field-mapping table**

Create `cmn_bank_statement_template_field` with cascade FK, section/target/source/anchor/value/transform allow-lists, unique target per section and template, coordinate validation, and source-specific shape validation.

- [ ] **Step 5: Register scripts in execution order**

Insert `0157` through `0160` immediately after `0156` in `_run_order.txt`, retaining unrelated existing lines.

- [ ] **Step 6: Verify repository state**

Run:

```powershell
git diff --check
dotnet build Accounting.slnx --no-restore
dotnet test Accounting.slnx --no-restore --no-build
```

Expected: whitespace check exits 0, build exits 0, and all discovered tests pass.
