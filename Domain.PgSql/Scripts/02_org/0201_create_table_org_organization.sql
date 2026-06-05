create table org_organization
(
	id serial not null primary key,
	short_name varchar(250) not null,
	full_name varchar(500) not null,
	inn varchar(20) not null,
	phone_number varchar(50) null,
	region_id int not null references cmn_region (id),
	district_id int null references cmn_district (id),
	address varchar(1000) null,
	director varchar(250) null,
	is_parent boolean not null default false,
	state_id smallint not null references cmn_state (id),
	created_date timestamp without time zone default now() not null
);

create index idx_org_organization_short_name on org_organization (short_name);
create index idx_org_organization_full_name on org_organization (full_name);
create index idx_org_organization_inn on org_organization (inn);
create index idx_org_organization_region_id on org_organization (region_id);
create index idx_org_organization_district_id on org_organization (district_id);
create index idx_org_organization_state_id on org_organization (state_id);
