create table pur_doc_table 
(
	id						bigserial primary key,
	owner_id				bigint not null references pur_doc_product (id),
	product_table_id		int not null references inv_product_table (id),
	amount					numeric (24, 8) not null,
	vat_rate_id				smallint null references cmn_vat_rate (id),
	vat_amount				numeric (24, 8) not null,
	total_amount			numeric (24, 8) not null
);

create index ix_pur_doc_table_owner_id_id
    on pur_doc_table (owner_id, id);

create unique index ux_pur_doc_table_owner_id_product_table_id
    on pur_doc_table (owner_id, product_table_id);
