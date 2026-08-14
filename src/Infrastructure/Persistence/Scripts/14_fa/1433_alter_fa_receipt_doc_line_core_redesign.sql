begin;

drop index if exists idx_fa_receipt_doc_line_source_product_id;

alter table fa_receipt_doc_line
    drop column source_product_id;

alter table fa_receipt_doc_line
    rename column owner_id to receipt_doc_id;

alter table fa_receipt_doc_line
    drop constraint fa_receipt_doc_line_owner_id_fkey,
    drop constraint fa_receipt_doc_line_capital_investment_account_id_fkey,
    drop constraint fa_receipt_doc_line_vat_account_id_fkey,
    add constraint fa_receipt_doc_line_receipt_doc_id_fkey
        foreign key (receipt_doc_id) references fa_receipt_doc (id),
    add constraint fa_receipt_doc_line_capital_investment_account_id_fkey
        foreign key (capital_investment_account_id) references acc_chart_account (id),
    add constraint fa_receipt_doc_line_vat_account_id_fkey
        foreign key (vat_account_id) references acc_chart_account (id),
    alter column name type varchar(500),
    alter column quantity type integer using quantity::integer,
    alter column amount drop default,
    alter column total_amount drop default;

alter table fa_receipt_doc_line
    rename constraint fa_receipt_doc_line_pkey to pk_fa_receipt_doc_line;

alter table fa_receipt_doc_line
    rename constraint ck_fa_receipt_doc_line_quantity_positive to ck_fa_receipt_doc_line_quantity;

alter table fa_receipt_doc_line
    rename constraint ck_fa_receipt_doc_line_price_nonnegative to ck_fa_receipt_doc_line_price;

alter table fa_receipt_doc_line
    add constraint ck_fa_receipt_doc_line_amount check (amount >= 0),
    add constraint ck_fa_receipt_doc_line_vat_amount check (vat_amount >= 0),
    add constraint ck_fa_receipt_doc_line_total_amount check (total_amount >= 0),
    add constraint ck_fa_receipt_doc_line_amount_formula check (amount = price * quantity),
    add constraint ck_fa_receipt_doc_line_total_formula check (total_amount = amount + vat_amount);

alter index idx_fa_receipt_doc_line_owner_id rename to ix_fa_receipt_doc_line_receipt_doc_id;
alter index idx_fa_receipt_doc_line_vat_rate_id rename to ix_fa_receipt_doc_line_vat_rate_id;
alter index ix_fa_receipt_line_capital_account rename to ix_fa_receipt_doc_line_capital_investment_account_id;
alter index ix_fa_receipt_line_vat_account rename to ix_fa_receipt_doc_line_vat_account_id;

commit;
