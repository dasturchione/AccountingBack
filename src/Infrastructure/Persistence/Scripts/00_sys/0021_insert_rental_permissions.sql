insert into sys_module_sub_group(code, short_name, full_name, created_date)
values ('RENTAL', 'Ijara', 'Ijara shartnomalari va hisob-kitoblari', now())
on conflict (code) do update set
    short_name = excluded.short_name,
    full_name = excluded.full_name;

with permissions(code, short_name, full_name, sort_order, is_visible, template_code) as
(
    values
        ('RENTAL_CONTRACT_VIEW', 'Ijara shartnomalari', 'Ijara shartnomalarini ko''rish', 1, true, 'CONTRACT_VIEW'),
        ('RENTAL_CONTRACT_VIEW_DETAIL', 'Ijara shartnomasi', 'Ijara shartnomasini batafsil ko''rish', 2, false, 'CONTRACT_VIEW_DETAIL'),
        ('RENTAL_CONTRACT_CREATE', 'Ijara shartnomasi yaratish', 'Ijara shartnomasini yaratish', 3, false, 'CONTRACT_CREATE'),
        ('RENTAL_CONTRACT_UPDATE', 'Ijara shartnomasini tahrirlash', 'Ijara shartnomasini tahrirlash', 4, false, 'CONTRACT_UPDATE'),
        ('RENTAL_CONTRACT_DELETE', 'Ijara shartnomasini o''chirish', 'Draft ijara shartnomasini o''chirish', 5, false, 'CONTRACT_DELETE'),
        ('RENTAL_CONTRACT_ACTIVATE', 'Ijara shartnomasini faollashtirish', 'Ijara shartnomasini hisoblash uchun faollashtirish', 6, false, 'CONTRACT_UPDATE'),
        ('RENTAL_CONTRACT_CANCEL', 'Ijara shartnomasini bekor qilish', 'Ijara shartnomasini bekor qilish', 7, false, 'CONTRACT_DELETE'),
        ('RENTAL_ACCRUAL_VIEW', 'Ijara hisob-kitoblari', 'Ijara hisob-kitoblarini ko''rish', 8, true, 'PAYROLL_DOCUMENT_VIEW'),
        ('RENTAL_ACCRUAL_VIEW_DETAIL', 'Ijara hisob-kitobi', 'Ijara hisob-kitobini batafsil ko''rish', 9, false, 'PAYROLL_DOCUMENT_VIEW'),
        ('RENTAL_ACCRUAL_UPDATE', 'Ijara hisob-kitobini tahrirlash', 'Draft ijara hisob-kitobidagi hisoblarni tahrirlash', 10, false, 'PAYROLL_DOCUMENT_CALCULATE'),
        ('RENTAL_ACCRUAL_DELETE', 'Ijara hisob-kitobini o''chirish', 'Draft ijara hisob-kitobini o''chirish', 11, false, 'PAYROLL_DOCUMENT_DELETE'),
        ('RENTAL_ACCRUAL_GENERATE', 'Ijara hisob-kitobini yaratish', 'Muddati kelgan ijara qarzlarini yaratish', 12, false, 'PAYROLL_DOCUMENT_CALCULATE'),
        ('RENTAL_ACCRUAL_POST', 'Ijara hisob-kitobini o''tkazish', 'Ijara hisob-kitobi provodkalarini yaratish', 13, false, 'PAYROLL_DOCUMENT_CONFIRM'),
        ('RENTAL_ACCRUAL_CANCEL', 'Ijara hisob-kitobini bekor qilish', 'Ijara hisob-kitobini storno qilish', 14, false, 'PAYROLL_DOCUMENT_CANCEL')
)
insert into sys_module(code, short_name, full_name, sub_group_id, state_id, created_date, sort_order, is_visible)
select permission.code, permission.short_name, permission.full_name, subgroup.id, 1, now(), permission.sort_order, permission.is_visible
from permissions permission
join sys_module_sub_group subgroup on subgroup.code = 'RENTAL'
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
        ('RENTAL_CONTRACT_VIEW', 'CONTRACT_VIEW'),
        ('RENTAL_CONTRACT_VIEW_DETAIL', 'CONTRACT_VIEW_DETAIL'),
        ('RENTAL_CONTRACT_CREATE', 'CONTRACT_CREATE'),
        ('RENTAL_CONTRACT_UPDATE', 'CONTRACT_UPDATE'),
        ('RENTAL_CONTRACT_DELETE', 'CONTRACT_DELETE'),
        ('RENTAL_CONTRACT_ACTIVATE', 'CONTRACT_UPDATE'),
        ('RENTAL_CONTRACT_CANCEL', 'CONTRACT_DELETE'),
        ('RENTAL_ACCRUAL_VIEW', 'PAYROLL_DOCUMENT_VIEW'),
        ('RENTAL_ACCRUAL_VIEW_DETAIL', 'PAYROLL_DOCUMENT_VIEW'),
        ('RENTAL_ACCRUAL_UPDATE', 'PAYROLL_DOCUMENT_CALCULATE'),
        ('RENTAL_ACCRUAL_DELETE', 'PAYROLL_DOCUMENT_DELETE'),
        ('RENTAL_ACCRUAL_GENERATE', 'PAYROLL_DOCUMENT_CALCULATE'),
        ('RENTAL_ACCRUAL_POST', 'PAYROLL_DOCUMENT_CONFIRM'),
        ('RENTAL_ACCRUAL_CANCEL', 'PAYROLL_DOCUMENT_CANCEL')
)
insert into sys_role_module(role_id, module_id, created_date)
select role_module.role_id, new_module.id, now()
from permission_map map
join sys_module template_module on template_module.code = map.template_code
join sys_role_module role_module on role_module.module_id = template_module.id
join sys_module new_module on new_module.code = map.new_code
on conflict (role_id, module_id) do nothing;
