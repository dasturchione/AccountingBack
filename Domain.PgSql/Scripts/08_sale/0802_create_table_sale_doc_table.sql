create table sale_doc_table
(
	id bigserial primary key,
	owner_id bigint not null references sale_doc(id) on delete cascade,
	product_id int not null references inv_product(id),
	quantity numeric(18,3) not null,
	price numeric(18,2) not null,
	amount numeric(18,2) not null,
	vat_rate_id smallint null references cmn_vat_rate(id),
	vat_amount numeric(18,2) not null default 0,
	total_amount numeric(18,2) not null
);

create index idx_sale_doc_table_owner_id on sale_doc_table (owner_id);
create index idx_sale_doc_table_product_id on sale_doc_table (product_id);
create index idx_sale_doc_table_vat_rate_id on sale_doc_table (vat_rate_id);
