create table cmn_region
(
	id serial not null primary key,
	short_name varchar(250) not null,
	full_name varchar(250) not null,
	state_id smallint not null references cmn_state (id),
	created_date timestamp without time zone default now() not null
);