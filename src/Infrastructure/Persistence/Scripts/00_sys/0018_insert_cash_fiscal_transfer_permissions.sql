with permissions(code, short_name, full_name, sort_order, template_code) as
(
    values
        ('CASH_FISCAL_TRANSFER_VIEW', 'Kassa ko''chirishlarini ko''rish', 'Fiskal va asosiy kassa o''rtasidagi ko''chirishlarni ko''rish', 70, 'CASH_OPERATION_VIEW'),
        ('CASH_FISCAL_TRANSFER_VIEW_DETAIL', 'Kassa ko''chirishini ko''rish', 'Kassa ko''chirishini batafsil ko''rish', 71, 'CASH_OPERATION_VIEW_DETAIL'),
        ('CASH_FISCAL_TRANSFER_CREATE', 'Kassa ko''chirishini yaratish', 'Kassa ko''chirish hujjatini yaratish', 72, 'CASH_OPERATION_CREATE'),
        ('CASH_FISCAL_TRANSFER_UPDATE', 'Kassa ko''chirishini tahrirlash', 'Kassa ko''chirish hujjatini tahrirlash', 73, 'CASH_OPERATION_UPDATE'),
        ('CASH_FISCAL_TRANSFER_DELETE', 'Kassa ko''chirishini o''chirish', 'Kassa ko''chirish hujjatini o''chirish', 74, 'CASH_OPERATION_DELETE'),
        ('CONFIRM_CASH_FISCAL_TRANSFER', 'Kassa ko''chirishini o''tkazish', 'Kassa ko''chirish hujjatini o''tkazish', 75, 'CONFIRM_CASH_OPERATION'),
        ('CANCEL_CASH_FISCAL_TRANSFER', 'Kassa ko''chirishini bekor qilish', 'Kassa ko''chirish hujjatini bekor qilish', 76, 'CANCEL_CASH_OPERATION')
)
insert into sys_module(code, short_name, full_name, sub_group_id, state_id, created_date, sort_order, is_visible)
select p.code, p.short_name, p.full_name, subgroup.id, 1, now(), p.sort_order, false
from permissions p
join sys_module_sub_group subgroup on subgroup.code = 'CASH'
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
        ('CASH_FISCAL_TRANSFER_VIEW', 'CASH_OPERATION_VIEW'),
        ('CASH_FISCAL_TRANSFER_VIEW_DETAIL', 'CASH_OPERATION_VIEW_DETAIL'),
        ('CASH_FISCAL_TRANSFER_CREATE', 'CASH_OPERATION_CREATE'),
        ('CASH_FISCAL_TRANSFER_UPDATE', 'CASH_OPERATION_UPDATE'),
        ('CASH_FISCAL_TRANSFER_DELETE', 'CASH_OPERATION_DELETE'),
        ('CONFIRM_CASH_FISCAL_TRANSFER', 'CONFIRM_CASH_OPERATION'),
        ('CANCEL_CASH_FISCAL_TRANSFER', 'CANCEL_CASH_OPERATION')
)
insert into sys_role_module(role_id, module_id, created_date)
select role_module.role_id, new_module.id, now()
from permission_map map
join sys_module template_module on template_module.code = map.template_code
join sys_role_module role_module on role_module.module_id = template_module.id
join sys_module new_module on new_module.code = map.new_code
on conflict (role_id, module_id) do nothing;
