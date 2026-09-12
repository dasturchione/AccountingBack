-- Kadr buyrug'i (prikaz) ruxsatlari.
insert into sys_module (
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
    seed.code,
    seed.short_name,
    seed.full_name,
    (select id from sys_module_sub_group where code = 'HR'),
    1,
    now(),
    seed.sort_order,
    seed.is_visible
from (
    values
        ('HR_ORDER_VIEW', 'Kadr buyruqlari', 'Kadr buyruqlarini (prikaz) ko''rish', 12, true),
        ('HR_ORDER_CREATE', 'Buyruq yaratish', 'Kadr buyrug''ini yaratish', 13, false),
        ('HR_ORDER_UPDATE', 'Buyruqni tahrirlash', 'Qoralama kadr buyrug''ini tahrirlash', 14, false),
        ('HR_ORDER_CONFIRM', 'Buyruqni tasdiqlash', 'Kadr buyrug''ini tasdiqlash', 15, false),
        ('HR_ORDER_CANCEL', 'Buyruqni bekor qilish', 'Tasdiqlangan kadr buyrug''ini bekor qilish', 16, false),
        ('HR_ORDER_DELETE', 'Buyruqni o''chirish', 'Qoralama kadr buyrug''ini o''chirish', 17, false)
) as seed (code, short_name, full_name, sort_order, is_visible)
on conflict (code) do update
set short_name = excluded.short_name,
    full_name = excluded.full_name,
    sub_group_id = excluded.sub_group_id,
    state_id = excluded.state_id,
    sort_order = excluded.sort_order,
    is_visible = excluded.is_visible;
