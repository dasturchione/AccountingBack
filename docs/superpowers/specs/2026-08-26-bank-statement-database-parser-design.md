# Database-Driven Bank Statement Parser Design

## Scope

Replace the hardcoded Trastbank and Uzsanoatqurilishbank Excel parsing switch with global SQL-seeded templates stored in the four `cmn_bank_statement_template*` tables.

## Decisions

- The multipart endpoint and route stay unchanged; its template selector becomes integer `bankId` instead of `BankStatementBankType`.
- Active templates are loaded by `bank_id` and tried from the highest version to the lowest version. The first template that recognizes at least one statement block wins.
- If the bank has no active templates or none matches the workbook, parsing returns HTTP 200 with `{ "accounts": [] }`.
- The parser scans every used row of every eligible worksheet. A template may recognize several statement blocks on the same worksheet.
- Header, row, and field behavior comes only from the database configuration. Bank-specific C# parsing methods and the enum are removed.
- Field mappings use a fixed allow-list of source types, match types, transformations, value types, anchors, and target codes. Database content is data, never executable SQL or C#.
- Amount columns are mapped to the organization perspective. For Uzsanoatqurilishbank response `DEBIT` reads source column 9 and response `CREDIT` reads source column 8. For Trastbank response `DEBIT` reads source column 7 and response `CREDIT` reads source column 6, preserving incoming/outgoing semantics while keeping `direction` and `directionId` consistent.
- Domain models use data annotations for table, column, length, and navigation mapping. No `[Index]` attributes are added.

## Data flow

1. Controller receives `file` and `bankId` from the existing multipart endpoint.
2. Service loads active templates for the bank, including all three child collections.
3. Workbook is opened once and passed to each template in descending version order.
4. The generic parser filters sheets, detects all header rows, classifies subsequent rows, maps statement/transaction/total fields, and returns account statements.
5. The first non-empty parse result is enriched with bank, branch, organization bank account, counterparty, and counterparty bank account IDs.
6. No match returns a successful empty export.

## Seed templates

- `TRASTBANK_XLSX_V1`: recognizes the bank metadata row and uses relative offsets for account metadata, transaction columns 1–8, and organization-facing totals derived from source columns 7/6. The scanner supports multiple account blocks per sheet.
- `UZSANOATQURILISHBANK_XLSX_V1`: recognizes the nine-column transaction header, searches up to ten rows for account/period/balances, and swaps source debit/credit columns for the organization-facing response.

All `bank_id` values are selected from `cmn_bank.id` by `cmn_bank.code`. Child rows select `template_id` by stable template code.

## Verification

- Unit tests cover template fallback, empty successful response, Uzsanoatqurilishbank mapping, and multiple Trastbank blocks.
- EF model metadata is checked for the four tables and relations without index attributes.
- SQL scripts are parsed as PostgreSQL syntax.
- The full solution is built and all available tests are run.
