# Database-driven bank operation classification

## Goal

Classify normalized bank-statement transactions by bank-specific, priority-ordered SQL rules. The first matching active rule wins. Rules are system-wide, seeded only by SQL, and scoped to `cmn_bank.id`; organization-specific values such as the taxpayer number are resolved at runtime from `org_organization.inn`.

The first supported rule set is for `UZSANOATQURILISHBANK`. Classification runs after the Excel template parser has converted bank-relative debit/credit columns into organization-relative `IN`/`OUT` values.

## Decisions

- Categories are common across the system; matching rules are bank-specific.
- Rules operate only on normalized `TransactionDto` fields, never on raw Excel coordinates.
- Rule sets are versioned. The newest active version for the selected `bank_id` is used.
- Rules are evaluated by ascending `priority`; evaluation stops on the first match.
- Conditions inside one rule are joined with logical `AND`.
- `CONTAINS_ANY` and `NOT_CONTAINS_ANY` use child values joined with logical `OR`.
- No JSON expressions are stored. Supported fields and operators are constrained by SQL checks and application constants.
- `ORGANIZATION_INN` is a dynamic value source. It resolves to the current `org_organization.inn`; no taxpayer number is stored in rule rows.
- The statement-level `CompanyInn` must match the current organization's normalized INN before classification is allowed.
- Unknown or unsafe matches return `REVIEW_REQUIRED` rather than selecting accounting behavior by guesswork.
- The result is persisted on `bank_operation`: category records the business meaning, while matched rule records why the automatic result was chosen.
- Entity models use mapping attributes but do not add `[Index]` attributes; indexes remain SQL-owned.

## Alternatives considered

### Normalized rule and condition tables — selected

This uses explicit rule, condition, and multi-value tables. It provides database constraints, stable foreign keys, auditable seed data, and enough flexibility for the currently known rules without implementing a general expression language.

### JSONB condition expression

This reduces the number of tables but moves validation into application code, makes SQL review difficult, and permits invalid field/operator combinations. It is rejected because rules are deployed and audited through SQL scripts.

### Dedicated columns for every known condition

This is initially simple but requires schema changes whenever a new parsed field or operator is introduced. It is rejected because different banks use different combinations of fields and phrases.

## Database schema

SQL follows project conventions: `serial`/`smallserial primary key`, `not null` before `default`, and inline `references` on FK columns.

### `cmn_bank_operation_category`

- `id smallserial primary key`
- `code varchar(50) not null`
- `name varchar(250) not null`
- `state_id smallint not null default 1 references cmn_state(id)`
- `created_date timestamp without time zone not null default now()`
- unique `code`
- code format: `^[A-Z0-9_]+$`

Initial codes:

- `BANK_COMMISSION`
- `BANK_SERVICE`
- `ACQUIRING`
- `CASH_COLLECTION`
- `CAPITAL_CONTRIBUTION`
- `PERSONAL_CARD`
- `COUNTERPARTY`
- `REVIEW_REQUIRED`

### `cmn_bank_operation_category_translation`

- `category_id smallint not null references cmn_bank_operation_category(id)`
- `language_id smallint not null references cmn_language(id)`
- `name varchar(250) not null`
- primary key `(category_id, language_id)`
- index on `language_id`

Translations are seeded for `uz`, `ru`, and `en` by category and language codes rather than fixed IDs.

### `cmn_bank_operation_classification_rule_set`

- `id serial primary key`
- `bank_id integer not null references cmn_bank(id)`
- `code varchar(100) not null`
- `name varchar(250) not null`
- `version smallint not null`
- `state_id smallint not null default 1 references cmn_state(id)`
- `created_date timestamp without time zone not null default now()`
- unique `code`
- unique `(bank_id, version)`
- positive version and uppercase code checks
- indexes on `bank_id` and `state_id`

### `cmn_bank_operation_classification_rule`

- `id serial primary key`
- `rule_set_id integer not null references cmn_bank_operation_classification_rule_set(id) on delete cascade`
- `category_id smallint not null references cmn_bank_operation_category(id)`
- `code varchar(100) not null`
- `name varchar(250) not null`
- `priority smallint not null`
- `direction_id smallint references cmn_movement_direction(id)`
- `is_fallback boolean not null default false`
- `state_id smallint not null default 1 references cmn_state(id)`
- unique `(rule_set_id, code)`
- unique `(rule_set_id, priority)`
- positive priority and uppercase code checks
- fallback rules must have no direction and no conditions
- indexes on `rule_set_id`, `category_id`, and `state_id`

The no-conditions requirement for a fallback is enforced by the classifier and covered by tests because a row-level SQL check cannot inspect child rows.

### `cmn_bank_operation_classification_condition`

- `id serial primary key`
- `rule_id integer not null references cmn_bank_operation_classification_rule(id) on delete cascade`
- `condition_order smallint not null`
- `field_code varchar(50) not null`
- `operator_code varchar(30) not null`
- `value_source_code varchar(30) not null default 'LITERAL'`
- `compare_value varchar(1000)`
- `normalization_code varchar(30) not null default 'NORMALIZE_WHITESPACE'`
- unique `(rule_id, condition_order)`
- indexes on `rule_id`

Supported `field_code` values:

- `COUNTERPARTY_INN`
- `COUNTERPARTY_NAME`
- `COUNTERPARTY_ACCOUNT`
- `PURPOSE`
- `BANK_DOCUMENT_NUMBER`
- `OPERATION_CODE`

Supported `operator_code` values:

- `EQUALS`
- `NOT_EQUALS`
- `STARTS_WITH`
- `NOT_STARTS_WITH`
- `CONTAINS`
- `NOT_CONTAINS`
- `CONTAINS_ANY`
- `NOT_CONTAINS_ANY`

Supported `value_source_code` values:

- `LITERAL`
- `ORGANIZATION_INN`

Supported normalizations:

- `NONE`
- `TRIM`
- `NORMALIZE_WHITESPACE`
- `NORMALIZE_KEY`
- `LOWER`

Shape constraints:

- `ORGANIZATION_INN` requires `field_code = 'COUNTERPARTY_INN'`, `operator_code` in `EQUALS`/`NOT_EQUALS`, and null `compare_value`.
- Literal scalar operators require a nonblank `compare_value`.
- `CONTAINS_ANY`/`NOT_CONTAINS_ANY` require null `compare_value` and at least one child value; the latter is enforced by the classifier and tests.

### `cmn_bank_operation_classification_condition_value`

- `id serial primary key`
- `condition_id integer not null references cmn_bank_operation_classification_condition(id) on delete cascade`
- `value_order smallint not null`
- `compare_value varchar(500) not null`
- unique `(condition_id, value_order)`
- unique `(condition_id, compare_value)`
- index on `condition_id`

### `bank_operation` extension

- `classification_category_id smallint references cmn_bank_operation_category(id)`
- `classification_rule_id integer references cmn_bank_operation_classification_rule(id)`

Both columns are nullable so manually created and historical operations remain valid. A manually selected category has a null rule. An automatically classified operation stores both values. Indexes are added for both FKs.

## Uzsanoatqurilishbank V1 seed

Rules are selected through `bank_id = (select id from cmn_bank where code = 'UZSANOATQURILISHBANK')`; all referenced categories, languages, directions, and parent rows are selected by code.

Priority order:

1. `10 BANK_COMMISSION`, direction `OUT`: own INN; name starts with `Начисленные`; purpose contains `от суммы`.
2. `20 BANK_SERVICE`, direction `OUT`: own INN; name starts with `Начисленные`; purpose does not contain `от суммы`; purpose contains any of `выпуск`, `карт`, `терминал`, `terminal`, `тариф`, `абонент`.
3. `30 REVIEW_BANK_ACCRUAL`, direction `OUT`: own INN; name starts with `Начисленные`. This catches unknown bank accrual wording after the two safe rules.
4. `40 CAPITAL_CONTRIBUTION`, direction `IN`: normalized name equals `Айланма кассадаги накд пуллар`; normalized account equals `10101000800010900101`; purpose contains any of `устав`, `ustav`, `капитал`, `kapital`.
5. `50 CASH_COLLECTION`, direction `IN`: the same normalized name and account without capital-purpose terms.
6. `60 ACQUIRING`, direction `IN`: own INN; name does not start with `Начисленные`.
7. `70 PERSONAL_CARD`, any direction: purpose contains any of `kartasiga`, `картасига`.
8. `999 COUNTERPARTY`, fallback with no conditions or direction.

This seed reproduces the reviewed workbook result while separating three capital contributions from eight trade-cash deposits and routing the one unknown `погашение задолженности` row to review.

## Application model

New entities:

- `BankOperationCategory`
- `BankOperationCategoryTranslation`
- `BankOperationClassificationRuleSet`
- `BankOperationClassificationRule`
- `BankOperationClassificationCondition`
- `BankOperationClassificationConditionValue`

Existing entities gain inverse collections where needed: `Bank`, `State`, `Language`, `MovementDirection`, and `BankOperation`. `AppDbContext` gains six `DbSet` properties. Index attributes are not added to the new entities.

`BankOperation` gains nullable `ClassificationCategoryId`, `ClassificationRuleId`, and their navigation properties.

## Classification service

`IBankOperationClassifier` receives:

- selected `bankId`;
- current organization ID/INN;
- parsed `AccountStatementDto` and `TransactionDto`.

The service loads the newest active rule-set version with rules, conditions, values, categories, and requested-language translations. It validates statement `CompanyInn` against `org_organization.inn`, then evaluates active rules by priority.

Direction is evaluated from the normalized parser result. Conditions use normalized string values and ordinal case-insensitive comparison after the selected normalization. Invalid rule shapes are not silently ignored; the parser request fails with a configuration error identifying the rule.

The first match returns:

- category ID/code/name;
- matched rule ID/code;
- `requiresReview`, derived from category code `REVIEW_REQUIRED`.

If no rule matches and no fallback exists, the result is `REVIEW_REQUIRED` with no matched rule rather than an exception for the whole workbook.

## API integration

`TransactionDto` gains:

- `classificationCategoryId`
- `classificationCode`
- `classificationName`
- `classificationRuleId`
- `classificationRuleCode`
- `requiresReview`

The existing parse request remains `File + BankId`. Classification is applied after parsing and existing bank/counterparty enrichment.

Bank-operation create/update DTOs gain nullable `ClassificationCategoryId` and `ClassificationRuleId`. Create/update validates that a supplied rule belongs to the selected category and to the bank of `BankAccountId`. Detail/list DTOs and projections return both IDs plus category code/name and rule code. Classification is descriptive and does not automatically select chart accounts or post the operation.

## Tests and verification

- EF metadata tests cover table names, column names, FKs, composite key, and absence of `[Index]` on new models.
- Rule evaluator tests cover priority-first behavior, dynamic organization INN, direction, scalar operators, ANY operators, fallback, normalization, and invalid configuration.
- Workbook regression tests cover the reviewed Uzsanoatqurilishbank examples: commission, bank service, review-required accrual, acquiring, capital contribution, trade cash collection, personal card, and counterparty.
- API/service tests cover parse-response classification and create/update persistence validation.
- SQL scripts are parsed with PostgreSQL syntax tooling.
- Full solution build and test suite must pass before completion.

## Deployment order

1. Create category and translation tables.
2. Create rule-set, rule, condition, and condition-value tables.
3. Seed categories and translations.
4. Seed Uzsanoatqurilishbank V1 rules by codes.
5. Alter `bank_operation` with nullable result FKs.
6. Deploy models and classifier code.

Existing bank operations remain valid because new result columns are nullable. Existing parser clients remain compatible because the request is unchanged and response fields are additive.
