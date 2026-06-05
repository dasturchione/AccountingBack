create table sys_module 
(
	id serial not null primary key,
	code varchar(100) not null,
	short_name varchar(250) not null,
	full_name varchar(300) not null,
	sub_group_id int not null references sys_module_sub_group(id),
	state_id smallint not null references enum_state(id),
	created_date timestamp without time zone default now() not null         
);

create unique index sys_module_unique_index_code on sys_module (code);
create index sys_module_unique_index_sub_group_id on sys_module (sub_group_id);