# Universal Document Registry Design

## Goal

Add one organization-scoped registry that exposes the common identity and display fields of every business document, while keeping each module table as the source of its detailed data.

## Registry

`cmn_document_registry` stores:

- `id bigserial primary key` as the universal document identifier;
- `organization_id`, `document_type_id`, and the source table `document_id`;
- `doc_number`, `doc_date`, and normalized document `amount`;
- optional `currency_id` and `status_id`;
- `state_id`, `created_date`, and `updated_date`.

The pair `(document_type_id, document_id)` is unique. `document_id` cannot have a direct foreign key because it points to different physical tables according to `document_type_id`.

## Synchronization

PostgreSQL triggers synchronize registry rows after insert, update, and delete of every implemented document table. This keeps application writes, imports, background jobs, and direct SQL writes consistent. A backfill in the trigger script registers existing documents.

For document types whose amount is stored only in lines, line triggers recalculate the registry amount. Non-monetary documents use `0`. A missing source document number uses its source identifier as the stable fallback so the registry's `doc_number` remains non-null.

## Bank operation link

Replace `bank_operation.cash_collection_doc_id` with `bank_operation.related_document_id`, a nullable foreign key to `cmn_document_registry(id)`.

The bank-operation API accepts and returns `relatedDocumentId` and returns the linked registry metadata. Any active document belonging to the same organization can be linked. When the linked registry row represents a cash-collection document, the existing cash-collection validation, account assignment, completion, cancellation, and single-active-bank-operation rules still apply. Other document types are informational links and do not inherit cash-collection rules.

Existing cash-collection links are migrated to the corresponding registry row before the old column is removed.

## Constraints

- SQL uses `varchar`, `int`, inline `references`, and `bigserial primary key`.
- No commit is created unless explicitly requested by the user.
- Existing unrelated working-tree changes are preserved.
