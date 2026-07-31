begin;

insert into sys_module_sub_group
    (id, code, short_name, full_name, created_date)
values
    (22, 'HR', 'Kadrlar', 'Xodimlar va kadrlar boshqaruvi', now())
on conflict (id) do update
set code = excluded.code,
    short_name = excluded.short_name,
    full_name = excluded.full_name;

insert into sys_module
    (id, code, short_name, full_name, sub_group_id, state_id, created_date, parent_id, route, icon, sort_order, is_visible)
values
    (1530, 'HR_VIEW', 'Kadrlar', 'Kadrlar moduli', 22, 1, now(), null, '/hr', 'groups', 90, true),
    (1531, 'HR_EMPLOYEE_VIEW', 'Xodimlar', 'Xodimlarni ko''rish', 22, 1, now(), 1530, '/hr/employees', null, 1, true),
    (1532, 'HR_EMPLOYEE_CREATE', 'Xodim yaratish', 'Xodimni ishga qabul qilish', 22, 1, now(), 1530, null, null, 2, false),
    (1533, 'HR_EMPLOYEE_UPDATE', 'Xodimni tahrirlash', 'Xodim va ishga qabul ma''lumotlarini tahrirlash', 22, 1, now(), 1530, null, null, 3, false),
    (1534, 'HR_EMPLOYEE_DELETE', 'Xodimni bo''shatish', 'Xodimni faolsizlantirish', 22, 1, now(), 1530, null, null, 4, false),
    (1535, 'HR_SCHEDULE_VIEW', 'Ish grafiklari', 'Xodimlarning shaxsiy ish grafiklarini ko''rish', 22, 1, now(), 1530, '/hr/work-schedules', null, 5, true),
    (1536, 'HR_SCHEDULE_MANAGE', 'Ish grafiklarini boshqarish', 'Xodimlarning shaxsiy ish grafiklarini boshqarish', 22, 1, now(), 1530, null, null, 6, false),
    (1537, 'HR_ABSENCE_VIEW', 'Ta''til va yo''qliklar', 'Ta''til, kasallik va boshqa yo''qliklarni ko''rish', 22, 1, now(), 1530, '/hr/absences', null, 7, true),
    (1538, 'HR_ABSENCE_CREATE', 'Yo''qlik yaratish', 'Ta''til, kasallik yoki boshqa yo''qlik yaratish', 22, 1, now(), 1530, null, null, 8, false),
    (1539, 'HR_ABSENCE_UPDATE', 'Yo''qlikni tahrirlash', 'Yo''qlik va uning fayllarini tahrirlash', 22, 1, now(), 1530, null, null, 9, false),
    (1540, 'HR_ABSENCE_DELETE', 'Yo''qlikni o''chirish', 'Yo''qlik hujjatini o''chirish', 22, 1, now(), 1530, null, null, 10, false),
    (1541, 'HR_CALENDAR_VIEW', 'Xodim kalendari', 'Ish, ta''til va kasallik kunlari kalendari', 22, 1, now(), 1530, '/hr/calendar', null, 11, true)
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

update sys_module
set is_visible = false
where id = 1501
  and code = 'PAYROLL_EMPLOYEE_VIEW';

insert into sys_role_module (role_id, module_id)
select role.id, module.id
from sys_role role
cross join sys_module module
where role.short_name = 'super_admin'
  and module.id between 1530 and 1541
on conflict (role_id, module_id) do nothing;

select setval(
    pg_get_serial_sequence('sys_module_sub_group', 'id'),
    (select max(id) from sys_module_sub_group));

select setval(
    pg_get_serial_sequence('sys_module', 'id'),
    (select max(id) from sys_module));

commit;
