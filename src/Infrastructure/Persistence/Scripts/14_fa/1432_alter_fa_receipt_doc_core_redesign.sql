begin;

drop index if exists idx_fa_receipt_doc_warehouse_id;
drop index if exists idx_fa_receipt_doc_currency_id;
drop index if exists idx_fa_receipt_doc_state_id;
drop index if exists idx_fa_receipt_doc_doc_date;

alter table fa_receipt_doc
    drop column warehouse_id,
    alter column doc_number type varchar(50);

alter table fa_receipt_doc
    drop constraint if exists fa_receipt_doc_supplier_account_id_fkey,
    drop constraint if exists fa_receipt_doc_posted_by_user_id_fkey,
    drop constraint if exists fa_receipt_doc_cancelled_by_user_id_fkey,
    add constraint fa_receipt_doc_supplier_account_id_fkey
        foreign key (supplier_account_id) references acc_chart_account (id),
    add constraint fa_receipt_doc_posted_by_user_id_fkey
        foreign key (posted_by_user_id) references sys_user (id),
    add constraint fa_receipt_doc_cancelled_by_user_id_fkey
        foreign key (cancelled_by_user_id) references sys_user (id),
    add constraint ck_fa_receipt_doc_total_amount check (total_amount >= 0),
    add constraint ck_fa_receipt_doc_vat_amount check (vat_amount >= 0),
    add constraint ck_fa_receipt_doc_final_amount check (final_amount >= 0),
    add constraint ck_fa_receipt_doc_amounts check (final_amount = total_amount + vat_amount);

alter table fa_receipt_doc
    rename constraint fa_receipt_doc_pkey to pk_fa_receipt_doc;

alter index ux_fa_receipt_doc_org_doc_number
    rename to uq_fa_receipt_doc_org_number;

alter table fa_receipt_doc
    add constraint uq_fa_receipt_doc_org_number
        unique using index uq_fa_receipt_doc_org_number;

alter index idx_fa_receipt_doc_counterparty_id rename to ix_fa_receipt_doc_counterparty_id;
alter index idx_fa_receipt_doc_status_id rename to ix_fa_receipt_doc_status_id;
alter index idx_fa_receipt_doc_receipt_type_id rename to ix_fa_receipt_doc_receipt_type_id;
alter index ix_fa_receipt_doc_supplier_account rename to ix_fa_receipt_doc_supplier_account_id;

create index ix_fa_receipt_doc_org_date
    on fa_receipt_doc (organization_id, doc_date);

commit;
