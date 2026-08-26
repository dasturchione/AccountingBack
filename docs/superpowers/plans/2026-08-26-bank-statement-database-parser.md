# Database-Driven Bank Statement Parser Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Parse Trastbank and Uzsanoatqurilishbank Excel statements from SQL-seeded global templates selected by `bankId`.

**Architecture:** EF entities expose the four configuration tables as one aggregate rooted at `BankStatementTemplate`. `BankStatementParserService` loads active versions and delegates workbook interpretation to a bank-agnostic `BankStatementTemplateParser`; the controller keeps its endpoint and multipart contract while replacing the enum selector with `bankId`.

**Tech Stack:** .NET 10, C#, EF Core, ClosedXML, PostgreSQL SQL scripts, xUnit.

**Spec:** `docs/superpowers/specs/2026-08-26-bank-statement-database-parser-design.md`

## Global Constraints

- Do not add `[Index]` attributes to domain models.
- Do not include unrelated retail or cash changes.
- Do not commit the implementation unless the user explicitly requests it after review.
- Return a successful empty `BankExportDto` when no active version matches.
- Try active versions in descending `Version` order.

---

### Task 1: SQL seed and EF aggregate

**Files:**
- Create: `src/Infrastructure/Persistence/Scripts/01_cmn/0161_insert_cmn_bank_statement_template.sql`
- Modify: `src/Infrastructure/Persistence/Scripts/_run_order.txt`
- Create: `src/Domain/Entities/Cmn/BankStatementTemplate.cs`
- Create: `src/Domain/Entities/Cmn/BankStatementTemplateHeaderRule.cs`
- Create: `src/Domain/Entities/Cmn/BankStatementTemplateRowRule.cs`
- Create: `src/Domain/Entities/Cmn/BankStatementTemplateField.cs`
- Modify: `src/Domain/Entities/Cmn/Bank.cs`
- Modify: `src/Domain/Entities/Cmn/State.cs`
- Modify: `src/Infrastructure/Persistence/AppDbContext/AppDbContext.cs`
- Test: `tests/UnitTests/BankStatementTemplateModelTests.cs`

**Interfaces:**
- Produces: `BankStatementTemplate` with `HeaderRules`, `RowRules`, and `Fields` collections.
- Produces: SQL template codes `TRASTBANK_XLSX_V1` and `UZSANOATQURILISHBANK_XLSX_V1`.

- [ ] **Step 1: Write the failing EF metadata test**

Create a test that builds `AppDbContext.Model`, asserts all four table names, their foreign keys, and absence of CLR `IndexAttribute` on all four types.

- [ ] **Step 2: Run the focused test and verify RED**

Run: `dotnet test tests/UnitTests/UnitTests.csproj --no-restore --filter FullyQualifiedName~BankStatementTemplateModelTests`

Expected: compilation failure because the four domain entity types do not exist.

- [ ] **Step 3: Add the entity aggregate and DbSets**

Add annotated classes with exact table/column names and navigation properties. Add collections to `Bank` and `State`; add four `DbSet<T>` properties to the runtime `AppDbContext`. Do not use `[Index]`.

- [ ] **Step 4: Add the SQL seed**

Insert both template roots using scalar subqueries such as `(select id from cmn_bank where code = 'TRASTBANK')`. Insert all header rules, row rules, and field mappings using template IDs selected by template code. Add `0161` immediately after `0160` in `_run_order.txt` without staging or altering unrelated run-order lines.

- [ ] **Step 5: Run the focused test and verify GREEN**

Run the Task 1 focused test and require all assertions to pass.

### Task 2: Generic workbook interpreter

**Files:**
- Create: `src/Application/Features/BankParsers/Services/BankStatementTemplateParser.cs`
- Test: `tests/UnitTests/BankStatementTemplateParserTests.cs`

**Interfaces:**
- Consumes: `BankStatementTemplate` aggregate and `XLWorkbook`.
- Produces: `internal static BankExportDto Parse(XLWorkbook workbook, BankStatementTemplate template)`.

- [ ] **Step 1: Write failing parser behavior tests**

Add one test for the nine-column Uzsanoatqurilishbank mapping and one test containing two Trastbank blocks on one worksheet. Assert account metadata, dates, counterparties, organization-facing debit/credit, totals, and two returned accounts.

- [ ] **Step 2: Run focused tests and verify RED**

Run: `dotnet test tests/UnitTests/UnitTests.csproj --no-restore --filter FullyQualifiedName~BankStatementTemplateParserTests`

Expected: compilation failure because `BankStatementTemplateParser` does not exist.

- [ ] **Step 3: Implement header, row, and source resolution**

Implement sheet matching, required header rules, next-header boundaries, priority row classification, `ABSOLUTE_CELL`, `RELATIVE_CELL`, `SEARCH_CELL`, `CONSTANT`, and `SUM_TRANSACTIONS`. Match text case-insensitively and support configured regular expressions with named or numbered extraction groups.

- [ ] **Step 4: Implement value conversion and DTO mapping**

Implement `NONE`, `TRIM`, `NORMALIZE_WHITESPACE`, `NORMALIZE_KEY`, `NORMALIZE_MFO`, and `AMOUNT_FROM_TEXT`; parse `STRING`, `DATE`, and `DECIMAL`; map fixed statement, transaction, and total target codes; derive `Direction` and `Amount`; sum totals when explicit totals are absent or zero.

- [ ] **Step 5: Run focused tests and verify GREEN**

Run the Task 2 focused tests and require both bank formats to pass.

### Task 3: Version fallback and API bankId

**Files:**
- Modify: `src/Application/Features/BankParsers/Services/BankStatementParserService.cs`
- Modify: `src/Application/Features/BankParsers/Services/IBankStatementParserService.cs`
- Modify: `src/Presentation/WebApi/Controllers/Bank/BankStatementParserController.cs`
- Modify: `tests/UnitTests/UzsanoatqurilishbankStatementParserTests.cs`
- Modify: `tests/UnitTests/BankBranchAssociationTests.cs`
- Create: `tests/UnitTests/BankStatementTemplateVersionTests.cs`

**Interfaces:**
- Produces: `ParseAsync(Stream stream, int bankId, CancellationToken ct = default)`.
- Produces: `ParseExcelAsync(Stream stream, int bankId, CancellationToken ct = default)`.
- Produces: multipart request record `BankStatementParseRequest(IFormFile File, int BankId)`.

- [ ] **Step 1: Write failing version and empty-result tests**

Test that version 2 is attempted before version 1 and the service falls back to version 1 when version 2 does not match. Test that zero matching versions returns success with an empty `Accounts` collection.

- [ ] **Step 2: Run focused tests and verify RED**

Run: `dotnet test tests/UnitTests/UnitTests.csproj --no-restore --filter FullyQualifiedName~BankStatementTemplateVersionTests`

Expected: compilation failure because the service still accepts `BankStatementBankType` and has no template repository.

- [ ] **Step 3: Load and try active template versions**

Inject `IQueryRepository<BankStatementTemplate>`, query by `BankId` and active `StateId`, include all three child collections, order results by `Version` descending in memory, open the workbook once, and return the first non-empty parse. Return `Result.Success(new BankExportDto())` if none matches.

- [ ] **Step 4: Update the controller and dependent tests**

Replace `BankType` with `BankId` in the multipart record and pass it to the service. Update existing test constructors with an in-memory template repository while leaving route, authorization, file checks, and response DTO unchanged.

- [ ] **Step 5: Run focused tests and verify GREEN**

Run all bank statement and branch-association unit tests.

### Task 4: Remove legacy parser and verify the complete change

**Files:**
- Delete: `src/Application/Features/BankParsers/DTOs/BankStatementBankType.cs`
- Delete: `src/Application/Features/BankParsers/Services/BankStatementParserService.Uzsanoatqurilishbank.cs`
- Modify: `src/Application/Features/BankParsers/Services/BankStatementParserService.cs`
- Delete or simplify: `src/Application/Features/BankParsers/Errors/BankStatementParserErrors.cs`

**Interfaces:**
- Removes: enum/switch and all bank-specific parsing methods.

- [ ] **Step 1: Remove old enum and hardcoded parsers**

Delete the enum and Uzsanoatqurilishbank partial file. Remove Trastbank-specific parsing, regexes, and switch logic from the service; retain only generic orchestration and ID enrichment.

- [ ] **Step 2: Confirm no legacy references remain**

Run: `rg -n "BankStatementBankType|ParseTrastbank|ParseUzsanoatqurilishbank" src tests`

Expected: no matches.

- [ ] **Step 3: Validate SQL syntax and repository cleanliness**

Parse scripts `0157` through `0161` with a PostgreSQL parser, run `git diff --check`, and inspect `git status --short` to confirm unrelated cash/retail files remain untouched.

- [ ] **Step 4: Run full verification**

Run: `dotnet build Accounting.slnx --no-restore`

Run: `dotnet test Accounting.slnx --no-restore --no-build`

Expected: zero build errors and all discovered tests passing.
