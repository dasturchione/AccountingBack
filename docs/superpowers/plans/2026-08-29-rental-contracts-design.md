# Rental Contracts Design

## Scope

The organization rents one or more assets from an individual who does not issue an invoice. The rental contract stores only the lessor's full name, INN and/or PINFL; it does not reference `counterparty_card`.

## Data model

- `rnt_rental_object_type` and translation: fixed system-wide rental object kinds.
- `rnt_contract`: organization, lessor identity, term, currency, payable/tax account defaults, status.
- `rnt_contract_object`: one rented asset, recurrence, next accrual date, gross contract amount, tax base, tax rate and expense account default.
- `rnt_accrual_doc`: generated or manual document header with frozen totals and payable/tax accounts.
- `rnt_accrual_doc_item`: frozen object period, amounts and expense account.

## Calculation

For each line:

```text
taxAmount     = taxBaseAmount * taxRate / 100
payableAmount = contractAmount - contractAmount * taxRate / 100
amount        = payableAmount + taxAmount
```

Header totals are sums of lines. `cmn_document_registry.amount` is the header `amount`.

## Lifecycle and accounting

Generated documents start in `DRAFT`. Posting requires all three selected accounts, validates that they are active accounts of the organization, and creates two entries per line:

```text
Dr expenseAccount / Cr lessorPayableAccount = payableAmount
Dr expenseAccount / Cr taxPayableAccount    = taxAmount
```

Cancellation reverses posted entries. Draft deletion physically removes the draft lines and rewinds each affected object's `next_accrual_date`, so an intentionally deleted period can be generated again without violating idempotency.

## Automation

Quartz runs `RentalAccrualJob` every day at 00:00 Asia/Tashkent. The generator finds active posted contracts with due active objects, creates missing draft lines and advances `next_accrual_date`. A unique `(contract_object_id, period_from, period_to)` constraint makes retries idempotent.

## API

- `GET /api/manuals/rental-object-types`
- CRUD `/api/rental-contracts`
- `GET /api/rental-accrual-docs`
- `GET /api/rental-accrual-docs/{id}`
- `PUT /api/rental-accrual-docs/{id}` for draft account correction
- `POST /api/rental-accrual-docs/generate-due`
- `PUT /api/rental-accrual-docs/{id}/post`
- `PUT /api/rental-accrual-docs/{id}/cancel`
- `DELETE /api/rental-accrual-docs/{id}`
