alter table sys_user 
drop column organization_id;

alter table sys_user
    add column tenant_id int not null
        references platform_tenant(id);

create index ix_sys_user_tenant_id
    on sys_user (tenant_id);

