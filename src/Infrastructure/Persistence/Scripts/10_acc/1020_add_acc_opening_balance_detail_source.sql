alter table acc_opening_balance_account_detail
    add column if not exists source_document_type_id smallint,
    add column if not exists source_document_id bigint,
    add column if not exists source_line_id bigint;

create index if not exists ix_acc_opening_balance_detail_source_document
    on acc_opening_balance_account_detail(source_document_type_id, source_document_id)
    where source_document_type_id is not null and source_document_id is not null;
