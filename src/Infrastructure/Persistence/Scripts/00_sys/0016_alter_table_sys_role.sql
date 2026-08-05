alter table sys_role 
	drop column has_global_access; 

alter table sys_role 
	drop column is_owner_role;

alter table sys_role
    add constraint ck_sys_role_organization_system
    check (
        (is_system = true and organization_id is null)
        or
        (is_system = false and organization_id is not null)
    );

create unique index if not exists ux_sys_role_system_code
    on sys_role (code)
    where is_system = true;

create unique index if not exists ux_sys_role_organization_code
    on sys_role (organization_id, code)
    where is_system = false;