create table sys_module_sub_group
(
	id serial not null primary key,
	code varchar(100) not null,
	short_name varchar(250) not null,
	full_name varchar(300) not null,
	created_date timestamp without time zone default now() not null
);

create unique index sys_module_sub_group_unique_index_code on sys_module_sub_group (code);