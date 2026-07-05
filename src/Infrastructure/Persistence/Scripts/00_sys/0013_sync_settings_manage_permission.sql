insert into sys_module (
    id,
    code,
    short_name,
    full_name,
    sub_group_id,
    state_id,
    created_date,
    route,
    icon,
    sort_order,
    is_visible
)
values (
    1140,
    'SETTINGS_MANAGE',
    'Tizim sozlamalari',
    'Tizim sozlamalarini boshqarish',
    1,
    1,
    '2026-07-05 00:00:00',
    '/settings',
    'settings',
    92,
    true
)
on conflict (code) do nothing;

insert into sys_role_module (role_id, module_id, created_date)
select
    role.id,
    module.id,
    now()
from sys_role role
cross join sys_module module
where role.has_global_access = true
  and module.code = 'SETTINGS_MANAGE'
on conflict do nothing;

select setval(
    'sys_module_id_seq',
    greatest((select coalesce(max(id), 0) from sys_module), 1140),
    true
);
