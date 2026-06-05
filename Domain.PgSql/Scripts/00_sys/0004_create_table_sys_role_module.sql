create table sys_role_module
(                   
	role_id int not null references sys_role(id),                      
	module_id int not null references sys_module(id), 
	created_date timestamp with time zone not null default now(),           
	primary key (role_id, module_id)
);