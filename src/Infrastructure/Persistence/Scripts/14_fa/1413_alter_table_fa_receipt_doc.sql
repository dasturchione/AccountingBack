alter table fa_receipt_doc
    add column supplier_account_id integer
        references acc_chart_account (id)
        on delete restrict;

create index ix_fa_receipt_doc_supplier_account
    on fa_receipt_doc (supplier_account_id);
