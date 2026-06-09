alter table sys_user
add column if not exists organization_id int null references org_organization(id);

create index if not exists idx_sys_user_organization_id
on sys_user (organization_id);
