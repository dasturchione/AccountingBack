alter table sys_role
add column if not exists organization_id int null references org_organization(id);

create index if not exists idx_sys_role_organization_id
on sys_role (organization_id);
