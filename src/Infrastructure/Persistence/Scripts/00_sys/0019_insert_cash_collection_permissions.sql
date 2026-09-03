with permissions(code, short_name, full_name, sort_order, template_code) as
(
    values
        ('CASH_COLLECTION_VIEW', 'Inkassatsiyalarni ko''rish', 'Kassadan bankka inkassatsiya hujjatlarini ko''rish', 77, 'CASH_OPERATION_VIEW'),
        ('CASH_COLLECTION_VIEW_DETAIL', 'Inkassatsiyani ko''rish', 'Inkassatsiya hujjatini batafsil ko''rish', 78, 'CASH_OPERATION_VIEW_DETAIL'),
        ('CASH_COLLECTION_CREATE', 'Inkassatsiya yaratish', 'Kassadan bankka inkassatsiya hujjatini yaratish', 79, 'CASH_OPERATION_CREATE'),
        ('CASH_COLLECTION_UPDATE', 'Inkassatsiyani tahrirlash', 'Inkassatsiya hujjatini tahrirlash', 80, 'CASH_OPERATION_UPDATE'),
        ('CASH_COLLECTION_DELETE', 'Inkassatsiyani o''chirish', 'Inkassatsiya hujjatini o''chirish', 81, 'CASH_OPERATION_DELETE'),
        ('CASH_COLLECTION_SEND_TO_BANK', 'Inkassatsiyani yo''lga chiqarish', 'Naqd pulni kassadan chiqarib yo''ldagi pul sifatida hisobga olish', 82, 'CONFIRM_CASH_OPERATION'),
        ('CASH_COLLECTION_CANCEL', 'Inkassatsiyani bekor qilish', 'Inkassatsiya hujjatini bekor qilish', 83, 'CANCEL_CASH_OPERATION')
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
        ('CASH_COLLECTION_VIEW', 'CASH_OPERATION_VIEW'),
        ('CASH_COLLECTION_VIEW_DETAIL', 'CASH_OPERATION_VIEW_DETAIL'),
        ('CASH_COLLECTION_CREATE', 'CASH_OPERATION_CREATE'),
        ('CASH_COLLECTION_UPDATE', 'CASH_OPERATION_UPDATE'),
        ('CASH_COLLECTION_DELETE', 'CASH_OPERATION_DELETE'),
        ('CASH_COLLECTION_SEND_TO_BANK', 'CONFIRM_CASH_OPERATION'),
        ('CASH_COLLECTION_CANCEL', 'CANCEL_CASH_OPERATION')
)
insert into sys_role_module(role_id, module_id, created_date)
select role_module.role_id, new_module.id, now()
from permission_map map
join sys_module template_module on template_module.code = map.template_code
join sys_role_module role_module on role_module.module_id = template_module.id
join sys_module new_module on new_module.code = map.new_code
on conflict (role_id, module_id) do nothing;
