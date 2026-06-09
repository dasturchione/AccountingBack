create table sys_user
(
	id serial not null primary key,
	user_name varchar(250) not null,
	password_hash varchar(250) not null,
	password_salt varchar(250) not null,
	phone_number varchar(50) not null,
	email varchar(200),
	first_name varchar(100) not null,
	last_name varchar(100) not null,
	role_id int not null references sys_role (id),
	last_access_time timestamp without time zone null,
	state_id smallint not null references cmn_state (id),
	created_date timestamp without time zone default now() not null
);

create unique index uidx_sys_user_user_name on sys_user (user_name);
create index idx_sys_user_phone on sys_user (phone_number);
create index idx_sys_user_role_id on sys_user (role_id);