alter table bank_operation
    add column cash_collection_doc_id bigint references cash_collection_doc(id);

create index idx_bank_operation_cash_collection_doc_id
    on bank_operation using btree (cash_collection_doc_id);

create unique index ux_bank_operation_active_cash_collection_doc_id
    on bank_operation (cash_collection_doc_id)
    where cash_collection_doc_id is not null
      and state_id = 1
      and status_id <> 3;
