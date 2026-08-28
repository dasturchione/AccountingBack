insert into sys_module_sub_group(code, short_name, full_name, created_date)
select
    'PAYMENT_ACCEPTANCE_POINT',
    'To''lov qabul qilish nuqtasi',
    'To''lov qabul qilish nuqtalarini boshqarish',
    now()
where not exists
(
    select 1
    from sys_module_sub_group
    where code = 'PAYMENT_ACCEPTANCE_POINT'
);

update sys_module
set sub_group_id = (select id from sys_module_sub_group where code = 'PAYMENT_ACCEPTANCE_POINT')
where sub_group_id = (select id from sys_module_sub_group where code = 'BANK_TERMINAL');

delete from sys_module_sub_group
where code = 'BANK_TERMINAL';

update sys_module set code = 'PAYMENT_ACCEPTANCE_POINT_VIEW'
where code = 'BANK_TERMINAL_VIEW';
update sys_module set code = 'PAYMENT_ACCEPTANCE_POINT_VIEW_DETAIL'
where code = 'BANK_TERMINAL_VIEW_DETAIL';
update sys_module set code = 'PAYMENT_ACCEPTANCE_POINT_CREATE'
where code = 'BANK_TERMINAL_CREATE';
update sys_module set code = 'PAYMENT_ACCEPTANCE_POINT_UPDATE'
where code = 'BANK_TERMINAL_UPDATE';
update sys_module set code = 'PAYMENT_ACCEPTANCE_POINT_DELETE'
where code = 'BANK_TERMINAL_DELETE';
update sys_module set code = 'MANUAL_GET_PAYMENT_ACCEPTANCE_POINTS'
where code = 'MANUAL_GET_BANK_TERMINALS';

with permissions(code, short_name, full_name, sort_order, template_code) as
(
    values
        ('PAYMENT_ACCEPTANCE_POINT_OPERATION_VIEW', 'To''lov nuqtasi operatsiyalari', 'To''lov qabul qilish nuqtasi operatsiyalari ro''yxati', 1, 'PAYMENT_ACCEPTANCE_POINT_VIEW'),
        ('PAYMENT_ACCEPTANCE_POINT_OPERATION_VIEW_DETAIL', 'To''lov nuqtasi operatsiyasi tafsiloti', 'To''lov qabul qilish nuqtasi operatsiyasini batafsil ko''rish', 2, 'PAYMENT_ACCEPTANCE_POINT_VIEW_DETAIL'),
        ('PAYMENT_ACCEPTANCE_POINT_OPERATION_CREATE', 'To''lov nuqtasi operatsiyasini yaratish', 'Yangi to''lov qabul qilish nuqtasi operatsiyasini yaratish', 3, 'PAYMENT_ACCEPTANCE_POINT_CREATE'),
        ('PAYMENT_ACCEPTANCE_POINT_OPERATION_UPDATE', 'To''lov nuqtasi operatsiyasini tahrirlash', 'To''lov qabul qilish nuqtasi operatsiyasini tahrirlash', 4, 'PAYMENT_ACCEPTANCE_POINT_UPDATE'),
        ('PAYMENT_ACCEPTANCE_POINT_OPERATION_DELETE', 'To''lov nuqtasi operatsiyasini o''chirish', 'To''lov qabul qilish nuqtasi operatsiyasini o''chirish', 5, 'PAYMENT_ACCEPTANCE_POINT_DELETE'),
        ('PAYMENT_ACCEPTANCE_POINT_OPERATION_CONFIRM', 'To''lov nuqtasi operatsiyasini tasdiqlash', 'To''lov qabul qilish nuqtasi operatsiyasini tasdiqlash', 6, 'PAYMENT_ACCEPTANCE_POINT_UPDATE'),
        ('PAYMENT_ACCEPTANCE_POINT_OPERATION_CANCEL', 'To''lov nuqtasi operatsiyasini bekor qilish', 'To''lov qabul qilish nuqtasi operatsiyasini bekor qilish', 7, 'PAYMENT_ACCEPTANCE_POINT_UPDATE'),
        ('PAYMENT_ACCEPTANCE_POINT_OPERATION_BALANCE', 'To''lov nuqtasi qoldig''i', 'To''lov qabul qilish nuqtasi qoldig''ini ko''rish', 8, 'PAYMENT_ACCEPTANCE_POINT_VIEW'),
        ('MANUAL_GET_PAYMENT_ACCEPTANCE_POINT_TYPES', 'Manual Get Payment Acceptance Point Types', 'Manual Get Payment Acceptance Point Types', 0, 'MANUAL_GET_PAYMENT_ACCEPTANCE_POINTS')
)
insert into sys_module(code, short_name, full_name, sub_group_id, state_id, created_date, sort_order, is_visible)
select
    permission.code,
    permission.short_name,
    permission.full_name,
    case
        when permission.code like 'MANUAL_%'
            then (select id from sys_module_sub_group where code = 'MANUAL')
        else (select id from sys_module_sub_group where code = 'PAYMENT_ACCEPTANCE_POINT')
    end,
    1,
    now(),
    permission.sort_order,
    false
from permissions permission
on conflict (code) do update set
    short_name = excluded.short_name,
    full_name = excluded.full_name,
    sub_group_id = excluded.sub_group_id,
    state_id = excluded.state_id,
    sort_order = excluded.sort_order,
    is_visible = excluded.is_visible;

with permission_map(new_code, template_code) as
(
    values
        ('PAYMENT_ACCEPTANCE_POINT_OPERATION_VIEW', 'PAYMENT_ACCEPTANCE_POINT_VIEW'),
        ('PAYMENT_ACCEPTANCE_POINT_OPERATION_VIEW_DETAIL', 'PAYMENT_ACCEPTANCE_POINT_VIEW_DETAIL'),
        ('PAYMENT_ACCEPTANCE_POINT_OPERATION_CREATE', 'PAYMENT_ACCEPTANCE_POINT_CREATE'),
        ('PAYMENT_ACCEPTANCE_POINT_OPERATION_UPDATE', 'PAYMENT_ACCEPTANCE_POINT_UPDATE'),
        ('PAYMENT_ACCEPTANCE_POINT_OPERATION_DELETE', 'PAYMENT_ACCEPTANCE_POINT_DELETE'),
        ('PAYMENT_ACCEPTANCE_POINT_OPERATION_CONFIRM', 'PAYMENT_ACCEPTANCE_POINT_UPDATE'),
        ('PAYMENT_ACCEPTANCE_POINT_OPERATION_CANCEL', 'PAYMENT_ACCEPTANCE_POINT_UPDATE'),
        ('PAYMENT_ACCEPTANCE_POINT_OPERATION_BALANCE', 'PAYMENT_ACCEPTANCE_POINT_VIEW'),
        ('MANUAL_GET_PAYMENT_ACCEPTANCE_POINT_TYPES', 'MANUAL_GET_PAYMENT_ACCEPTANCE_POINTS')
)
insert into sys_role_module(role_id, module_id, created_date)
select role_module.role_id, new_module.id, now()
from permission_map map
join sys_module template_module on template_module.code = map.template_code
join sys_role_module role_module on role_module.module_id = template_module.id
join sys_module new_module on new_module.code = map.new_code
on conflict (role_id, module_id) do nothing;
