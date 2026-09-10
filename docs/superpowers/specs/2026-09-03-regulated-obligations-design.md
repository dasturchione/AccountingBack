# Regulated obligations design

## Scope

Introduce a system-wide multilingual catalogue for taxes and contributions, a multilingual reporting-periodicity catalogue, and dated organization-specific obligation settings. Replace the legacy `cmn_tax_type` and `org_tax_settings` model after the new data has been configured. Keep `cmn_vat_rate`, because purchase and sale document lines continue to reference it.

## Terminology

`regulated_obligation` is the shared term for a tax or contribution. It describes a configured obligation, not an actual payment transaction.

The initial categories are:

- `TAX` — tax;
- `CONTRIBUTION` — contribution or deduction.

The initial reporting periodicities are:

- `NOT_REQUIRED` — no report is submitted;
- `TEN_DAY` — ten-day period;
- `MONTHLY` — month;
- `QUARTERLY` — quarter;
- `SEMI_ANNUAL` — half-year;
- `ANNUAL` — year.

## Database model

### `cmn_regulated_obligation_category`

System catalogue that distinguishes taxes from contributions.

| Column | Type | Rules |
|---|---|---|
| `id` | `smallserial` | Primary key |
| `code` | `varchar(50)` | Required, unique, uppercase code |
| `name` | `varchar(150)` | Required base name |
| `state_id` | `smallint` | Required, defaults to active, references `cmn_state(id)` |
| `created_date` | `timestamp` | Required, defaults to `now()` |

The table intentionally has no `updated_date`. Catalogue values are delivered by SQL scripts; existing values are disabled through `state_id` instead of being edited as business data.

### `cmn_regulated_obligation_category_translation`

| Column | Type | Rules |
|---|---|---|
| `category_id` | `smallint` | Required, references the category |
| `language_id` | `smallint` | Required, references `cmn_language(id)` |
| `name` | `varchar(150)` | Required |

Primary key: `(category_id, language_id)`.

### `cmn_regulated_obligation`

System catalogue containing VAT, profit tax, personal income tax, social tax, pension contributions, and other taxes or contributions.

| Column | Type | Rules |
|---|---|---|
| `id` | `smallserial` | Primary key |
| `category_id` | `smallint` | Required, references the category |
| `code` | `varchar(50)` | Required, unique, uppercase code |
| `name` | `varchar(250)` | Required base name |
| `state_id` | `smallint` | Required, defaults to active, references `cmn_state(id)` |
| `created_date` | `timestamp` | Required, defaults to `now()` |

The table intentionally has no `updated_date`. Legal or classifier changes that must preserve history belong to the dated organization setting, not to destructive edits of the system catalogue.

### `cmn_regulated_obligation_translation`

| Column | Type | Rules |
|---|---|---|
| `regulated_obligation_id` | `smallint` | Required, references the obligation |
| `language_id` | `smallint` | Required, references `cmn_language(id)` |
| `name` | `varchar(250)` | Required |

Primary key: `(regulated_obligation_id, language_id)`.

### `cmn_regulated_obligation_periodicity`

System catalogue describing how frequently the corresponding report is submitted.

| Column | Type | Rules |
|---|---|---|
| `id` | `smallserial` | Primary key |
| `code` | `varchar(50)` | Required, unique, uppercase code |
| `name` | `varchar(150)` | Required base name |
| `state_id` | `smallint` | Required, defaults to active, references `cmn_state(id)` |
| `created_date` | `timestamp` | Required, defaults to `now()` |

### `cmn_regulated_obligation_periodicity_translation`

| Column | Type | Rules |
|---|---|---|
| `periodicity_id` | `smallint` | Required, references the periodicity |
| `language_id` | `smallint` | Required, references `cmn_language(id)` |
| `name` | `varchar(150)` | Required |

Primary key: `(periodicity_id, language_id)`.

### `org_regulated_obligation_setting`

Historical configuration of an obligation for one organization.

| Column | Type | Rules |
|---|---|---|
| `id` | `serial` | Primary key |
| `organization_id` | `int` | Required, references `org_organization(id)` |
| `regulated_obligation_id` | `smallint` | Required, references `cmn_regulated_obligation(id)` |
| `periodicity_id` | `smallint` | Required, references `cmn_regulated_obligation_periodicity(id)` |
| `classifier_code` | `varchar(30)` | Optional; blank strings are rejected |
| `rate` | `numeric(9,6)` | Optional; when supplied, from 0 through 100 |
| `chart_account_id` | `int` | Required, references `acc_chart_account(id)` |
| `effective_from` | `date` | Required |
| `effective_to` | `date` | Optional and not earlier than `effective_from` |
| `state_id` | `smallint` | Required, defaults to active, references `cmn_state(id)` |
| `created_date` | `timestamp` | Required, defaults to `now()` |
| `updated_date` | `timestamp` | Optional |

Unique key: `(organization_id, regulated_obligation_id, effective_from)`.

A filtered unique index allows only one active open-ended row for an organization and obligation. Service validation additionally rejects any overlapping finite or open-ended effective periods and verifies that `chart_account_id` belongs to the same organization.

`chart_account_id` is the settlement/liability account for the amount owed to the budget or fund. The expense or withholding side is selected by the source document or payroll component and is not stored here.

## Data lifecycle

An organization can have several obligations active simultaneously. To change a rate, classifier code, reporting periodicity, or settlement account, the service closes the previous row with `effective_to` and creates a new row with a later `effective_from`. Historical documents resolve the row that covers their document date.

No `NONE` obligation is created. The absence of an active obligation row represents non-application. Consequently, an active organization setting for obligation code `VAT` replaces the legacy `is_vat_payer` flag.

## Migration

The rollout is staged to avoid breaking existing databases:

1. Create and seed the six common catalogue and translation tables.
2. Create `org_regulated_obligation_setting` and its constraints and indexes.
3. Migrate the legacy `VAT` catalogue entry to regulated obligation code `VAT`; do not migrate legacy `NONE`.
4. Configure complete organization rows with periodicity, classifier code, rate, and a settlement account. Legacy organization rows cannot be safely transformed automatically because they do not contain those values.
5. Switch Domain models, manuals, setup, resolution, and calculation code to the new model.
6. Remove `cmn_tax_type`, `org_tax_settings`, their Domain models, and obsolete `TaxTypeIdConst` usage only after the new organization settings exist.
7. Retain `cmn_vat_rate` and all document foreign keys to it.

The destructive legacy-table removal is kept in a separate SQL script so it is run only after application deployment and data configuration have succeeded.

## Error handling

Feature-specific multilingual errors cover missing obligation/category/periodicity/account, an account from another organization, duplicate effective start dates, overlapping periods, invalid date ranges, and invalid rates. Catalogue rows referenced by settings are deactivated rather than physically deleted.

## Verification

Tests must verify:

- category, obligation, and periodicity manuals return the requested translation with base-name fallback;
- an organization can hold multiple simultaneous different obligations;
- duplicate and overlapping periods for the same organization and obligation are rejected;
- a chart account from another organization is rejected;
- a document date resolves the correct historical setting;
- a rate change preserves the old setting and creates a new interval;
- `rate = null` and `classifier_code = null` are accepted;
- `cmn_vat_rate` document behavior remains unchanged;
- the solution builds after legacy model removal.
