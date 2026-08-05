alter table sys_user 
	add column user_kind_id smallint not null references sys_user_kind(id) default 3; 

create index if not exists ix_sys_user_user_kind_id
    on sys_user(user_kind_id);

alter table sys_user
	drop column role_id;

alter table sys_user
	drop column is_platform_admin;