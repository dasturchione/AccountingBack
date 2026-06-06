create table inv_product
(
	id serial primary key,
	organization_id int not null references org_organization(id),
	product_group_id int null references inv_product_group(id),
	unit_id smallint not null references cmn_unit(id),
	code varchar(50) not null,
	barcode varchar(100) null,
	name varchar(250) not null,
	description varchar(1000) null,
	is_service boolean not null default false,
	state_id smallint not null references cmn_state(id),
	created_date timestamp without time zone default now() not null
);

create unique index idx_inv_product_org_code on inv_product (organization_id, code);
create index idx_inv_product_organization_id on inv_product (organization_id);
create index idx_inv_product_product_group_id on inv_product (product_group_id);
create index idx_inv_product_unit_id on inv_product (unit_id);
create index idx_inv_product_barcode on inv_product (barcode);
create index idx_inv_product_name on inv_product (name);
create index idx_inv_product_state_id on inv_product (state_id);
