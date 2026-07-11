alter table pur_doc
add column supplier_account_id int null references acc_chart_account (id);

alter table pur_doc_product
add column debit_account_id int null references acc_chart_account (id);

alter table pur_doc_product
add column vat_account_id int null references acc_chart_account (id);

create index idx_pur_doc_supplier_account_id
on pur_doc (supplier_account_id);

create index idx_pur_doc_product_debit_account_id
on pur_doc_product (debit_account_id);

create index idx_pur_doc_product_vat_account_id
on pur_doc_product (vat_account_id);