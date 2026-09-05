with permissions(code, short_name, full_name, subgroup_code) as
(
    values
        ('MANUAL_GET_REGULATED_OBLIGATIONS', 'Majburiyatlar ma''lumotnomasi', 'Tartibga solinadigan majburiyatlar ma''lumotnomasi', 'MANUAL'),
        ('MANUAL_GET_REGULATED_OBLIGATION_PERIODICITIES', 'Majburiyat davriyliklari', 'Tartibga solinadigan majburiyatlar davriyliklari', 'MANUAL'),
        ('REGULATED_OBLIGATION_SETTING_VIEW', 'Majburiyat sozlamalari', 'Tashkilot majburiyatlari sozlamalarini ko''rish', 'ORG'),
        ('REGULATED_OBLIGATION_SETTING_VIEW_DETAIL', 'Majburiyat sozlamasi', 'Tashkilot majburiyati sozlamasini batafsil ko''rish', 'ORG'),
        ('REGULATED_OBLIGATION_SETTING_CREATE', 'Majburiyat sozlamasini yaratish', 'Tashkilot majburiyati sozlamasini yaratish', 'ORG'),
        ('REGULATED_OBLIGATION_SETTING_UPDATE', 'Majburiyat sozlamasini tahrirlash', 'Tashkilot majburiyati sozlamasini tahrirlash', 'ORG')
)
insert into sys_module(code, short_name, full_name, sub_group_id, state_id, created_date, sort_order, is_visible)
select permission.code,
       permission.short_name,
       permission.full_name,
       subgroup.id,
       1,
       now(),
       0,
       false
from permissions permission
join sys_module_sub_group subgroup on subgroup.code = permission.subgroup_code
on conflict (code) do update
set short_name = excluded.short_name,
    full_name = excluded.full_name,
    sub_group_id = excluded.sub_group_id,
    state_id = excluded.state_id,
    sort_order = excluded.sort_order,
    is_visible = excluded.is_visible;

with permission_map(new_code, template_code) as
(
    values
        ('MANUAL_GET_REGULATED_OBLIGATIONS', 'MANUAL_GET_TAX_TYPES'),
        ('MANUAL_GET_REGULATED_OBLIGATION_PERIODICITIES', 'MANUAL_GET_TAX_TYPES'),
        ('REGULATED_OBLIGATION_SETTING_VIEW', 'SETUP_UPDATE_TAX_SETTINGS'),
        ('REGULATED_OBLIGATION_SETTING_VIEW_DETAIL', 'SETUP_UPDATE_TAX_SETTINGS'),
        ('REGULATED_OBLIGATION_SETTING_CREATE', 'SETUP_UPDATE_TAX_SETTINGS'),
        ('REGULATED_OBLIGATION_SETTING_UPDATE', 'SETUP_UPDATE_TAX_SETTINGS')
)
insert into sys_role_module(role_id, module_id, created_date)
select role_module.role_id,
       new_module.id,
       now()
from permission_map map
join sys_module template_module on template_module.code = map.template_code
join sys_role_module role_module on role_module.module_id = template_module.id
join sys_module new_module on new_module.code = map.new_code
on conflict (role_id, module_id) do nothing;

delete from sys_role_module
where module_id in
(
    select id
    from sys_module
    where code in ('MANUAL_GET_TAX_TYPES', 'SETUP_UPDATE_TAX_SETTINGS')
);

delete from sys_module
where code in ('MANUAL_GET_TAX_TYPES', 'SETUP_UPDATE_TAX_SETTINGS');
