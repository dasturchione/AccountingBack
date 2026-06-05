create table cmn_state
(
	id smallint not null primary key,
	short_name varchar(250) not null,
	full_name varchar(250) not null,
	created_date timestamp without time zone default now() not null
);