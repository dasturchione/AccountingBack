create table org_department
(
	id serial primary key,
	organization_id int not null references org_organization(id),
	branch_id int null references org_branch(id),
	code varchar(50) not null,
	name varchar(250) not null,
	state_id smallint not null references cmn_state(id),
	created_date timestamp without time zone default now() not null
);

create unique index idx_org_department_org_code on org_department (organization_id, code);
create index idx_org_department_organization_id on org_department (organization_id);
create index idx_org_department_branch_id on org_department (branch_id);
create index idx_org_department_state_id on org_department (state_id);
