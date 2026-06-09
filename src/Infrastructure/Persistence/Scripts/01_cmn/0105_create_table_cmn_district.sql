create table cmn_district
(
	id serial not null primary key,
	short_name varchar(250) not null,
	full_name varchar(250) not null,
	region_id int not null references cmn_region (id),
	state_id smallint not null references cmn_state (id),
	created_date timestamp without time zone default now() not null
);

create index idx_cmn_district_region_id on cmn_district (region_id);