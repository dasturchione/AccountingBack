create table org_branch
(
	id serial primary key,
	organization_id int not null references org_organization(id),
	code varchar(50) not null,
	name varchar(250) not null,
	region_id int null references cmn_region(id),
	district_id int null references cmn_district(id),
	address varchar(1000) null,
	phone_number varchar(50) null,
	state_id smallint not null references cmn_state(id),
	created_date timestamp without time zone default now() not null
);

create unique index idx_org_branch_org_code on org_branch (organization_id, code);
create index idx_org_branch_organization_id on org_branch (organization_id);
create index idx_org_branch_region_id on org_branch (region_id);
create index idx_org_branch_district_id on org_branch (district_id);
create index idx_org_branch_state_id on org_branch (state_id);
