create table inv_product_table
(
	id              serial          primary key,
	product_id      int             not null references inv_product(id),
	organization_id int             not null references org_organization(id),
	name            varchar(300)    not null,
	code            varchar(100)    null,
	barcode         varchar(100)    null,
	state_id        smallint        not null references cmn_state(id),
	created_date    timestamp without time zone not null default now()
);

create index idx_inv_product_table_product_id      on inv_product_table (product_id);
create index idx_inv_product_table_organization_id on inv_product_table (organization_id);
create index idx_inv_product_table_state_id        on inv_product_table (state_id);
