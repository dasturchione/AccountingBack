create table sale_doc_product
(
	id bigserial primary key,
	owner_id bigint not null references sale_doc(id) on delete cascade,
	product_id int not null references inv_product(id),
	quantity numeric(18,3) not null,
	unit_price numeric(18,2) not null,
	cost_price numeric(18, 2) not null default 0,
	amount numeric(18,2) not null,
	vat_rate_id smallint null references cmn_vat_rate(id),
	vat_amount numeric(18,2) not null default 0,
	total_amount numeric(18,2) not null
);

create index ix_sale_doc_product_owner_id
	on sale_doc_product(owner_id);

create index ix_sale_doc_product_product_id
	on sale_doc_product(product_id);
