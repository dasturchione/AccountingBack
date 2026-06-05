create table ref_district
(
	id serial not null primary key,
	short_name varchar(250) not null,
	full_name varchar(250) not null,
	region_id int not null references info_region (id),
	state_id smallint not null references enum_state (id),
	created_date timestamp without time zone default now() not null
);

create index idx_info_district_region_id on info_district (region_id);