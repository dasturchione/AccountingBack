# Bank Statement Template Schema Design

## Scope

Create a global, SQL-seeded configuration schema for Excel bank-statement templates. This phase creates schema only: no domain models, parser changes, API changes, or bank-specific seed data.

## Decisions

- Templates are global and belong to `cmn_bank`; they are not organization-specific.
- Templates are created and changed only by SQL scripts. No runtime CRUD is exposed.
- A template is selected later by a stable string `code`, not by a C# enum.
- The parser will scan every used row on every eligible sheet and recognize every matching account block. This supports several Trastbank accounts on one sheet.
- Configuration is normalized into four tables: template, header rules, row rules, and field mappings.
- Configuration stores only declarative operations from fixed allow-lists. It never stores executable C# or SQL expressions.

## Tables

1. `cmn_bank_statement_template` identifies a versioned active template and its sheet-selection behavior.
2. `cmn_bank_statement_template_header_rule` identifies every account-block header by matching cells relative to a candidate row.
3. `cmn_bank_statement_template_row_rule` classifies rows inside a block as `DATA`, `TOTAL`, `STOP`, or `SKIP` in priority order.
4. `cmn_bank_statement_template_field` maps cells and extracted values to the fixed statement and transaction DTO fields.

All child tables reference the template with `ON DELETE CASCADE`. Codes and configuration operators are constrained with PostgreSQL `CHECK` constraints. Child tables receive indexes on `template_id`; the template receives indexes on `bank_id` and `state_id`.

## Migration order

- `0157_create_cmn_bank_statement_template.sql`
- `0158_create_cmn_bank_statement_template_header_rule.sql`
- `0159_create_cmn_bank_statement_template_row_rule.sql`
- `0160_create_cmn_bank_statement_template_field.sql`

These scripts run after the existing `0155`/`0156` bank and branch scripts.

## Verification

- Review every foreign key, uniqueness rule, value allow-list, and source-shape constraint.
- Run `git diff --check`.
- Run the solution build and existing test suite to ensure repository integrity; SQL execution against a database is outside this schema-only phase because no disposable PostgreSQL test environment is configured in the repository.
