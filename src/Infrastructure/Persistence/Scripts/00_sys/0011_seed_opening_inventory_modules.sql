insert into sys_module_sub_group (id, code, short_name, full_name, created_date)
values (21, 'OPENING_INVENTORY', 'Boshlang''ich tovar qoldig''i', 'Boshlang''ich tovar qoldiqlari', now())
on conflict (id) do update set
    code = excluded.code,
    short_name = excluded.short_name,
    full_name = excluded.full_name;

insert into sys_module
    (id, code, short_name, full_name, sub_group_id, state_id, created_date, sort_order, is_visible)
values
    (1500, 'OPENING_INVENTORY_VIEW', 'Boshlang''ich qoldiqlar', 'Boshlang''ich tovar qoldiqlarini ko''rish', 21, 1, now(), 0, false),
    (1501, 'OPENING_INVENTORY_VIEW_DETAIL', 'Boshlang''ich qoldiq detail', 'Boshlang''ich tovar qoldig''ini ko''rish', 21, 1, now(), 0, false),
    (1502, 'OPENING_INVENTORY_CREATE', 'Boshlang''ich qoldiq yaratish', 'Boshlang''ich tovar qoldig''ini yaratish', 21, 1, now(), 0, false),
    (1503, 'OPENING_INVENTORY_UPDATE', 'Boshlang''ich qoldiq tahrirlash', 'Boshlang''ich tovar qoldig''ini tahrirlash', 21, 1, now(), 0, false),
    (1504, 'OPENING_INVENTORY_DELETE', 'Boshlang''ich qoldiq o''chirish', 'Boshlang''ich tovar qoldig''ini o''chirish', 21, 1, now(), 0, false)
on conflict (id) do update set
    code = excluded.code,
    short_name = excluded.short_name,
    full_name = excluded.full_name,
    sub_group_id = excluded.sub_group_id,
    state_id = excluded.state_id;

insert into sys_role_module (role_id, module_id)
select role.id, module.id
from sys_role role
cross join sys_module module
where role.id = 4
  and role.short_name = 'super_admin'
  and module.id between 1500 and 1504
on conflict (role_id, module_id) do nothing;
