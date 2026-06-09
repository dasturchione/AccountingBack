create table inv_warehouse
(
	id serial primary key,
	organization_id int not null references org_organization(id),
	branch_id int null references org_branch(id),
	code varchar(50) not null,
	name varchar(250) not null,
	responsible_user_id int null references sys_user(id),
	state_id smallint not null references cmn_state(id),
	created_date timestamp without time zone default now() not null
);

create unique index idx_inv_warehouse_org_code on inv_warehouse (organization_id, code);
create index idx_inv_warehouse_organization_id on inv_warehouse (organization_id);
create index idx_inv_warehouse_branch_id on inv_warehouse (branch_id);
create index idx_inv_warehouse_responsible_user_id on inv_warehouse (responsible_user_id);
create index idx_inv_warehouse_state_id on inv_warehouse (state_id);
