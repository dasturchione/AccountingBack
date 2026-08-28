# Bank Operation Classification Rules Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add SQL-configured, bank-specific transaction classification and return/persist its result through the bank-statement parser and bank-operation CRUD.

**Architecture:** SQL owns versioned categories, rule sets, ordered rules, conditions, and multi-values. Application code loads the newest active rule set for `bankId`, evaluates normalized transactions using the current organization's INN, and stores the matched category/rule separately from movement direction.

**Tech Stack:** PostgreSQL SQL scripts, .NET 10, Entity Framework Core, repository/query-builder abstractions, ClosedXML parser DTOs, xUnit, FluentValidation.

**Spec:** `docs/superpowers/specs/2026-08-26-bank-operation-classification-rules-design.md`

## Global Constraints

- Rules are global SQL seed data and scoped to `cmn_bank.id`.
- `ORGANIZATION_INN` resolves from `org_organization.inn`; no organization INN is stored in rules.
- First matching active rule by ascending priority wins.
- SQL uses `serial`/`smallserial primary key`, inline `references`, and `not null` before `default`.
- New entity classes do not use `[Index]`; SQL owns indexes.
- Existing parse request remains multipart `File + BankId`; response changes are additive.
- Existing cash/retail working-tree changes are out of scope.

---

### Task 1: SQL schema and Uzsanoatqurilishbank seed

**Files:**
- Create: `src/Infrastructure/Persistence/Scripts/01_cmn/0162_create_cmn_bank_operation_category.sql`
- Create: `src/Infrastructure/Persistence/Scripts/01_cmn/0163_create_cmn_bank_operation_category_translation.sql`
- Create: `src/Infrastructure/Persistence/Scripts/01_cmn/0164_create_cmn_bank_operation_classification_rule_set.sql`
- Create: `src/Infrastructure/Persistence/Scripts/01_cmn/0165_create_cmn_bank_operation_classification_rule.sql`
- Create: `src/Infrastructure/Persistence/Scripts/01_cmn/0166_create_cmn_bank_operation_classification_condition.sql`
- Create: `src/Infrastructure/Persistence/Scripts/01_cmn/0167_create_cmn_bank_operation_classification_condition_value.sql`
- Create: `src/Infrastructure/Persistence/Scripts/01_cmn/0168_insert_cmn_bank_operation_classification.sql`
- Create: `src/Infrastructure/Persistence/Scripts/05_bank/0507_add_classification_to_bank_operation.sql`
- Modify: `src/Infrastructure/Persistence/Scripts/_run_order.txt`

**Interfaces:**
- Consumes: existing `cmn_bank`, `cmn_state`, `cmn_language`, `cmn_movement_direction`, and `bank_operation` tables.
- Produces: six classification tables, V1 seed data for bank code `UZSANOATQURILISHBANK`, and nullable bank-operation result FKs.

- [ ] Create the six tables with constraints and SQL-owned indexes from the spec.
- [ ] Seed eight categories and `uz`/`ru`/`en` translations using code-based subqueries.
- [ ] Seed rule-set code `UZSANOATQURILISHBANK_CLASSIFICATION_V1`, eight ordered rules, all conditions, and ANY values using parent codes rather than fixed IDs.
- [ ] Add nullable `classification_category_id` and `classification_rule_id` to `bank_operation` with FKs and indexes.
- [ ] Add only lines `0162`–`0168` and `0507` to `_run_order.txt`, preserving unrelated working-tree lines.
- [ ] Parse all new SQL with `pglast`; expected result is zero syntax errors.

### Task 2: EF models and relationships

**Files:**
- Create: `src/Domain/Entities/Cmn/BankOperationCategory.cs`
- Create: `src/Domain/Entities/Cmn/BankOperationCategoryTranslation.cs`
- Create: `src/Domain/Entities/Cmn/BankOperationClassificationRuleSet.cs`
- Create: `src/Domain/Entities/Cmn/BankOperationClassificationRule.cs`
- Create: `src/Domain/Entities/Cmn/BankOperationClassificationCondition.cs`
- Create: `src/Domain/Entities/Cmn/BankOperationClassificationConditionValue.cs`
- Modify: `src/Domain/Entities/Cmn/Bank.cs`
- Modify: `src/Domain/Entities/Cmn/State.cs`
- Modify: `src/Domain/Entities/Cmn/Language.cs`
- Modify: `src/Domain/Entities/Cmn/MovementDirection.cs`
- Modify: `src/Domain/Entities/Bank/BankOperation.cs`
- Modify: `src/Infrastructure/Persistence/AppDbContext/AppDbContext.cs`
- Test: `tests/UnitTests/BankOperationClassificationModelTests.cs`

**Interfaces:**
- Produces: navigable EF graph from rule set to rules, conditions, values, category/translations, plus bank-operation result relations.

- [ ] Write a reflection/EF metadata test asserting exact table/column/FK mappings and no `[Index]` attributes on the six new entity types.
- [ ] Run `dotnet test tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~BankOperationClassificationModelTests`; expected RED because the types do not exist.
- [ ] Add six mapped entities, inverse collections, bank-operation nullable result properties, and six `DbSet` properties.
- [ ] Re-run the focused test; expected GREEN.

### Task 3: Pure priority rule evaluator

**Files:**
- Create: `src/Application/Features/BankParsers/Services/BankOperationClassificationEvaluator.cs`
- Create: `src/Application/Features/BankParsers/Services/BankOperationClassificationConstants.cs`
- Test: `tests/UnitTests/BankOperationClassificationEvaluatorTests.cs`

**Interfaces:**
- Consumes: hydrated `BankOperationClassificationRuleSet`, organization INN, `AccountStatementDto`, and `TransactionDto`.
- Produces: `BankOperationClassificationResult(short CategoryId, string CategoryCode, string CategoryName, int? RuleId, string? RuleCode, bool RequiresReview)`.

- [ ] Write tests for priority-first behavior, `ORGANIZATION_INN`, IN/OUT filtering, scalar operators, ANY operators, normalization, fallback, unknown-without-fallback, and invalid rule shapes.
- [ ] Run focused tests; expected RED because evaluator/constants do not exist.
- [ ] Implement supported-field selection, normalization, ordinal case-insensitive operators, AND/ANY behavior, and first-match selection.
- [ ] Return `REVIEW_REQUIRED` without a rule when no fallback matches; reject statement/company INN mismatch as a classification error.
- [ ] Re-run focused tests; expected GREEN.

### Task 4: Load rules and classify parser response

**Files:**
- Create: `src/Application/Features/BankParsers/Services/IBankOperationClassifier.cs`
- Create: `src/Application/Features/BankParsers/Services/BankOperationClassifier.cs`
- Modify: `src/Application/Features/BankParsers/DTOs/BankExportDto.cs`
- Modify: `src/Application/Features/BankParsers/Services/BankStatementParserService.cs`
- Modify: `tests/UnitTests/BankStatementTemplateVersionTests.cs`
- Test: `tests/UnitTests/BankStatementClassificationTests.cs`

**Interfaces:**
- `Task<Result> ClassifyAsync(BankExportDto export, int bankId, CancellationToken ct)` mutates additive classification fields on transactions.
- Loads the newest active `BankOperationClassificationRuleSet` by `bankId` with rules, conditions, values, category translations, and current `Organization.Inn`.

- [ ] Write tests showing parser output gets category/rule fields and that the request remains `File + BankId`.
- [ ] Run focused tests; expected RED because response fields/classifier are absent.
- [ ] Add JSON fields `classificationCategoryId`, `classificationCode`, `classificationName`, `classificationRuleId`, `classificationRuleCode`, and `requiresReview`.
- [ ] Implement repository/query-builder loading, language translation fallback, organization INN lookup, and evaluator invocation for every account transaction.
- [ ] Call classification after current bank/counterparty enrichment and propagate configuration/ownership failures.
- [ ] Re-run focused tests; expected GREEN.

### Task 5: Persist classification through bank-operation CRUD

**Files:**
- Modify: `src/Application/Features/Bank/BankOperations/DTOs/BankOperationBaseDto.cs`
- Modify: `src/Application/Features/Bank/BankOperations/DTOs/BankOperationDto.cs`
- Modify: `src/Application/Features/Bank/BankOperations/DTOs/BankOperationListDto.cs`
- Modify: `src/Application/Features/Bank/BankOperations/Projections/BankOperationDtoProjection.cs`
- Modify: `src/Application/Features/Bank/BankOperations/Projections/BankOperationListDtoProjection.cs`
- Modify: `src/Application/Features/Bank/BankOperations/Services/BankOperationService.cs`
- Modify: `src/Application/Features/Bank/BankOperations/Validators/BankOperationBaseDtoValidator.cs`
- Test: `tests/UnitTests/BankOperationClassificationCrudTests.cs`

**Interfaces:**
- Create/update accepts nullable `ClassificationCategoryId` and `ClassificationRuleId`.
- Detail/list returns category ID/code/name and matched rule ID/code.

- [ ] Write tests for DTO shape, projection mappings, nullable/manual category behavior, and rule/category/bank consistency validation.
- [ ] Run focused tests; expected RED because CRUD fields are absent.
- [ ] Add DTO/projection fields and service validation that rule category equals supplied category and rule-set bank equals the selected bank account's bank.
- [ ] Store both IDs on create/update; allow category with null rule for manual classification.
- [ ] Re-run focused tests; expected GREEN.

### Task 6: Verification and handoff

**Files:**
- Verify all files from Tasks 1–5.

**Interfaces:**
- Produces: buildable, tested implementation and exact SQL run list.

- [ ] Run `git diff --check`; expected no whitespace errors.
- [ ] Run `dotnet build Accounting.slnx --no-restore`; expected zero errors and zero warnings.
- [ ] Run `dotnet test Accounting.slnx --no-restore --no-build`; expected zero failed tests.
- [ ] Re-run PostgreSQL syntax parsing for SQL `0162`–`0168` and `0507`.
- [ ] Confirm staged/working changes exclude unrelated retail and cash files.
- [ ] Report scripts to execute in `_run_order.txt` order and leave implementation uncommitted unless the user explicitly asks for a commit.
