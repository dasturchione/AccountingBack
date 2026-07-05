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
    1139,
    'AUDIT_LOG_VIEW',
    'Audit log',
    'Audit loglarni ko''rish',
    1,
    1,
    '2026-07-05 00:00:00',
    '/audit-logs',
    'history',
    90,
    true
)
on conflict (code) do nothing;

select setval(
    'sys_module_id_seq',
    greatest((select coalesce(max(id), 0) from sys_module), 1139),
    true
);
