create table org_position
(
	id serial primary key,
	organization_id int not null references org_organization(id),
	code varchar(50) not null,
	name varchar(250) not null,
	state_id smallint not null references cmn_state(id),
	created_date timestamp without time zone default now() not null
);

create unique index idx_org_position_org_code on org_position (organization_id, code);
create index idx_org_position_organization_id on org_position (organization_id);
create index idx_org_position_state_id on org_position (state_id);
