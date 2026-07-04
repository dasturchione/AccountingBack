insert into sys_module (id, code, short_name, full_name, sub_group_id, state_id, created_date, parent_id, route, icon, sort_order, is_visible) values
    ('1123', 'CURRENCY_RATE_VIEW', 'Valyuta kurslari', 'Valyuta kurslari ro''yxati', '11', '1', '2026-07-04 00:00:00', null, '/currency-rates', 'currency-exchange', '50', '1'),
    ('1124', 'CURRENCY_RATE_VIEW_DETAIL', 'Valyuta kursi detail', 'Valyuta kursini batafsil ko''rish', '11', '1', '2026-07-04 00:00:00', null, '/currency-rates', 'currency-exchange', '51', '1'),
    ('1125', 'CURRENCY_RATE_CREATE', 'Valyuta kursi yaratish', 'Yangi valyuta kursi qo''shish', '11', '1', '2026-07-04 00:00:00', null, '/currency-rates', 'currency-exchange', '52', '1'),
    ('1126', 'CURRENCY_RATE_UPDATE', 'Valyuta kursi tahrirlash', 'Valyuta kursini tahrirlash', '11', '1', '2026-07-04 00:00:00', null, '/currency-rates', 'currency-exchange', '53', '1'),
    ('1127', 'CURRENCY_RATE_DELETE', 'Valyuta kursi o''chirish', 'Valyuta kursini o''chirish', '11', '1', '2026-07-04 00:00:00', null, '/currency-rates', 'currency-exchange', '54', '1');

select setval('sys_module_id_seq', 1127, true);
