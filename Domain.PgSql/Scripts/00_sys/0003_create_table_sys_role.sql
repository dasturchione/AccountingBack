create table sys_role 
(
	id serial primary key,
	short_name varchar(100) not null,
	full_name varchar(255) not null, 
	state_id smallint not null references enum_state(id),           
	created_date timestamp without time zone not null default now()
);