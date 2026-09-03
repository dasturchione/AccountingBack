insert into cmn_document_registry
    (organization_id, document_type_id, document_id, doc_number, doc_date, amount, currency_id, status_id, state_id, created_date)
select organization_id, 26, id, doc_number, doc_date, amount, currency_id, status_id, state_id, created_date
from rnt_accrual_doc
on conflict (document_type_id, document_id) do update set
    organization_id = excluded.organization_id,
    doc_number = excluded.doc_number,
    doc_date = excluded.doc_date,
    amount = excluded.amount,
    currency_id = excluded.currency_id,
    status_id = excluded.status_id,
    state_id = excluded.state_id,
    updated_date = now();

drop trigger if exists trg_rnt_accrual_doc_document_registry on rnt_accrual_doc;

create trigger trg_rnt_accrual_doc_document_registry
after insert or update or delete on rnt_accrual_doc
for each row execute function cmn_sync_document_registry(
    '26', 'doc_number', 'doc_date', 'amount', 'currency_id', 'status_id', 'state_id');
