create table inv_reg_balance
(
	id bigserial primary key,
	organization_id int not null references org_organization(id),
	document_type_id smallint not null references cmn_document_type(id),
	document_id bigint not null,
	warehouse_id int not null references inv_warehouse(id),
	product_id int not null references inv_product(id),
	operation_type_id smallint not null references cmn_operation_type(id),
	quantity numeric(18,3) not null,
	amount numeric(18,2) not null,
	doc_date timestamp without time zone not null,
	created_date timestamp without time zone default now() not null
);

create index idx_inv_reg_balance_organization_id on inv_reg_balance (organization_id);
create index idx_inv_reg_balance_document on inv_reg_balance (document_type_id, document_id);
create index idx_inv_reg_balance_warehouse_id on inv_reg_balance (warehouse_id);
create index idx_inv_reg_balance_product_id on inv_reg_balance (product_id);
create index idx_inv_reg_balance_doc_date on inv_reg_balance (doc_date);
