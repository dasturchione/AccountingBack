create table inv_product_group
(
	id serial primary key,
	organization_id int not null references org_organization(id),
	parent_id int null references inv_product_group(id),
	code varchar(50) not null,
	name varchar(250) not null,
	state_id smallint not null references cmn_state(id),
	created_date timestamp without time zone default now() not null
);

create unique index idx_inv_product_group_org_code on inv_product_group (organization_id, code);
create index idx_inv_product_group_organization_id on inv_product_group (organization_id);
create index idx_inv_product_group_parent_id on inv_product_group (parent_id);
create index idx_inv_product_group_state_id on inv_product_group (state_id);
