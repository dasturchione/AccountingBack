
create table sys_role_module 
(
    role_id integer not null,
    module_id integer not null,
    created_date timestamp with time zone default now() not null,
    constraint sys_role_module_pkey primary key (role_id, module_id),
    constraint sys_role_module_module_id_fkey foreign key (module_id) references sys_module(id),
    constraint sys_role_module_role_id_fkey foreign key (role_id) references sys_role(id)
);