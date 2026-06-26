create table pur_doc_product
(
	id						bigserial primary key,
	owner_id				bigint not null references pur_doc (id),
	item_type_id			smallint not null references cmn_purchase_item_type (id), 
	product_id				int not null references inv_product (id),
	quantity				numeric (19, 6) not null,
	unit_id					smallint not null references cmn_unit (id),
	unit_price				numeric (24, 8) not null,
	amount					numeric (24, 8) not null,
	vat_rate_id				smallint null references cmn_vat_rate (id),
	vat_amount				numeric (24, 8) not null,
	total_amount			numeric (24, 8) not null
);

create index ix_pur_doc_product_owner_id
    on pur_doc_product (owner_id);

create index ix_pur_doc_product_product_id
    on pur_doc_product (product_id);
	
create index ix_pur_doc_product_item_type_id
    on pur_doc_product (item_type_id);
