alter table sale_doc
add column customer_account_id int null references acc_chart_account(id);

alter table sale_doc
add column vat_account_id int null references acc_chart_account(id);

alter table sale_doc_product
add column inventory_account_id int null references acc_chart_account(id);

alter table sale_doc_product
add column income_account_id int null references acc_chart_account(id);

alter table sale_doc_product
add column cost_account_id int null references acc_chart_account(id);

create index idx_sale_doc_customer_account_id
on sale_doc (customer_account_id);

create index idx_sale_doc_vat_account_id
on sale_doc (vat_account_id);

create index idx_sale_doc_product_inventory_account_id
on sale_doc_product (inventory_account_id);

create index idx_sale_doc_product_income_account_id
on sale_doc_product (income_account_id);

create index idx_sale_doc_product_cost_account_id
on sale_doc_product (cost_account_id);