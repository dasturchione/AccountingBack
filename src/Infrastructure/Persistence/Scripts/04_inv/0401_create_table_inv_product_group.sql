create table inv_product_group
(
	id serial primary key,
	organization_id int not null references org_organization(id),
	name varchar(250) not null,
	state_id smallint not null references cmn_state(id),
	created_date timestamp without time zone default now() not null
);

create index idx_inv_product_group_organization_id on inv_product_group (organization_id);
create index idx_inv_product_group_state_id on inv_product_group (state_id);
