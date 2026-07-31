begin;

insert into sys_module_sub_group (id, code, short_name, full_name, created_date)
values (22, 'PAYROLL', 'Oylik maosh', 'Oylik maosh va xodimlar bilan hisob-kitob', now())
on conflict (id) do update
set code = excluded.code,
    short_name = excluded.short_name,
    full_name = excluded.full_name;

insert into sys_module
    (id, code, short_name, full_name, sub_group_id, state_id, created_date, parent_id, route, icon, sort_order, is_visible)
values
    (1500, 'PAYROLL_VIEW', 'Oylik maosh', 'Oylik maosh moduli', 22, 1, now(), null, '/payroll', 'payments', 100, true),
    (1501, 'PAYROLL_EMPLOYEE_VIEW', 'Xodimlar', 'Xodimlarni korish', 22, 1, now(), 1500, '/payroll/employees', null, 1, true),
    (1502, 'PAYROLL_EMPLOYEE_CREATE', 'Xodim yaratish', 'Xodim yaratish', 22, 1, now(), 1500, null, null, 2, false),
    (1503, 'PAYROLL_EMPLOYEE_UPDATE', 'Xodimni tahrirlash', 'Xodimni tahrirlash', 22, 1, now(), 1500, null, null, 3, false),
    (1504, 'PAYROLL_EMPLOYEE_DELETE', 'Xodimni ochirish', 'Xodimni ochirish', 22, 1, now(), 1500, null, null, 4, false),
    (1505, 'PAYROLL_COMPONENT_VIEW', 'Hisoblash turlari', 'Hisoblash turlarini korish', 22, 1, now(), 1500, '/payroll/components', null, 5, true),
    (1506, 'PAYROLL_COMPONENT_CREATE', 'Hisoblash turi yaratish', 'Hisoblash turi yaratish', 22, 1, now(), 1500, null, null, 6, false),
    (1507, 'PAYROLL_COMPONENT_UPDATE', 'Hisoblash turini tahrirlash', 'Hisoblash turini tahrirlash', 22, 1, now(), 1500, null, null, 7, false),
    (1508, 'PAYROLL_COMPONENT_DELETE', 'Hisoblash turini ochirish', 'Hisoblash turini ochirish', 22, 1, now(), 1500, null, null, 8, false),
    (1509, 'PAYROLL_PERIOD_VIEW', 'Hisob davrlari', 'Hisob davrlarini korish', 22, 1, now(), 1500, '/payroll/periods', null, 9, true),
    (1510, 'PAYROLL_PERIOD_MANAGE', 'Hisob davrini boshqarish', 'Hisob davrini ochish va yopish', 22, 1, now(), 1500, null, null, 10, false),
    (1511, 'PAYROLL_TIMESHEET_VIEW', 'Tabel', 'Tabellarni korish', 22, 1, now(), 1500, '/payroll/timesheets', null, 11, true),
    (1512, 'PAYROLL_TIMESHEET_CREATE', 'Tabel yaratish', 'Tabel yaratish', 22, 1, now(), 1500, null, null, 12, false),
    (1513, 'PAYROLL_TIMESHEET_UPDATE', 'Tabelni tahrirlash', 'Tabelni tahrirlash', 22, 1, now(), 1500, null, null, 13, false),
    (1514, 'PAYROLL_TIMESHEET_CONFIRM', 'Tabelni tasdiqlash', 'Tabelni tasdiqlash', 22, 1, now(), 1500, null, null, 14, false),
    (1515, 'PAYROLL_TIMESHEET_CANCEL', 'Tabelni bekor qilish', 'Tabelni bekor qilish', 22, 1, now(), 1500, null, null, 15, false),
    (1516, 'PAYROLL_DOCUMENT_VIEW', 'Oylik hisoblash', 'Oylik hisoblash hujjatlarini korish', 22, 1, now(), 1500, '/payroll/documents', null, 16, true),
    (1517, 'PAYROLL_DOCUMENT_CALCULATE', 'Oylikni hisoblash', 'Oylikni hisoblash', 22, 1, now(), 1500, null, null, 17, false),
    (1518, 'PAYROLL_DOCUMENT_CONFIRM', 'Oylikni tasdiqlash', 'Oylikni tasdiqlash va otkazish', 22, 1, now(), 1500, null, null, 18, false),
    (1519, 'PAYROLL_DOCUMENT_CANCEL', 'Oylikni bekor qilish', 'Oylikni storno qilish', 22, 1, now(), 1500, null, null, 19, false),
    (1520, 'PAYROLL_DOCUMENT_DELETE', 'Oylik hujjatini ochirish', 'Draft oylik hujjatini ochirish', 22, 1, now(), 1500, null, null, 20, false),
    (1521, 'PAYROLL_PAYMENT_VIEW', 'Oylik tolovlari', 'Oylik tolovlarini korish', 22, 1, now(), 1500, '/payroll/payments', null, 21, true),
    (1522, 'PAYROLL_PAYMENT_CREATE', 'Tolov yaratish', 'Avans yoki oylik tolovi yaratish', 22, 1, now(), 1500, null, null, 22, false),
    (1523, 'PAYROLL_PAYMENT_CONFIRM', 'Tolovni tasdiqlash', 'Bank yoki kassa tolovini tasdiqlash', 22, 1, now(), 1500, null, null, 23, false),
    (1524, 'PAYROLL_PAYMENT_CANCEL', 'Tolovni bekor qilish', 'Bank yoki kassa tolovini bekor qilish', 22, 1, now(), 1500, null, null, 24, false),
    (1525, 'PAYROLL_REPORT_VIEW', 'Oylik hisobotlari', 'Vedomost va xodim hisob varaqasi', 22, 1, now(), 1500, '/payroll/reports', null, 25, true)
on conflict (id) do update
set code = excluded.code,
    short_name = excluded.short_name,
    full_name = excluded.full_name,
    sub_group_id = excluded.sub_group_id,
    state_id = excluded.state_id,
    parent_id = excluded.parent_id,
    route = excluded.route,
    icon = excluded.icon,
    sort_order = excluded.sort_order,
    is_visible = excluded.is_visible;

insert into sys_role_module (role_id, module_id)
select role.id, module.id
from sys_role role
cross join sys_module module
where role.short_name = 'super_admin'
  and module.id between 1500 and 1525
on conflict (role_id, module_id) do nothing;

select setval(pg_get_serial_sequence('sys_module_sub_group', 'id'),
              (select max(id) from sys_module_sub_group));
select setval(pg_get_serial_sequence('sys_module', 'id'),
              (select max(id) from sys_module));

commit;
