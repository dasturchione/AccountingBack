insert into sys_module
(
    code,
    short_name,
    full_name,
    sub_group_id,
    state_id,
    created_date,
    sort_order,
    is_visible
)
select
    'MANUAL_GET_MOVEMENT_DIRECTIONS',
    'Manual Get Movement Directions',
    'Manual Get Movement Directions',
    subgroup.id,
    1,
    now(),
    0,
    false
from sys_module_sub_group as subgroup
where subgroup.code = 'MANUAL'
on conflict (code) do update set
    short_name = excluded.short_name,
    full_name = excluded.full_name,
    sub_group_id = excluded.sub_group_id,
    state_id = excluded.state_id,
    sort_order = excluded.sort_order,
    is_visible = excluded.is_visible;

-- Preserve access for roles that already had the former shared permission.
insert into sys_role_module (role_id, module_id, created_date)
select
    role_module.role_id,
    movement_direction_module.id,
    now()
from sys_role_module as role_module
join sys_module as operation_type_module
    on operation_type_module.id = role_module.module_id
   and operation_type_module.code = 'MANUAL_GET_OPERATION_TYPES'
join sys_module as movement_direction_module
    on movement_direction_module.code = 'MANUAL_GET_MOVEMENT_DIRECTIONS'
on conflict (role_id, module_id) do nothing;
