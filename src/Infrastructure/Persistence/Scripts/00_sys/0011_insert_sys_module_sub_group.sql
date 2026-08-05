begin;

delete from sys_module;
select setval('sys_module_sub_group_id_seq', 1, false);

insert into sys_module_sub_group (code, short_name, full_name, created_date)
values
    ('SYS', 'Tizim', 'Tizim sozlamalari', now()),
    ('ORG', 'Tashkilot', 'Tashkilot boshqaruvi', now()),
    ('COUNTERPARTY', 'Kontragent', 'Kontragentlar', now()),
    ('INVENTORY', 'Inventar', 'Tovar va ombor', now()),
    ('BANK', 'Bank', 'Bank operatsiyalari', now()),
    ('CASH', 'Kassa', 'Kassa operatsiyalari', now()),
    ('PURCHASE', 'Xarid', 'Xarid hujjatlari', now()),
    ('SALE', 'Sotuv', 'Sotuv hujjatlari', now()),
    ('ACCOUNTING', 'Buxgalteriya', 'Buxgalteriya registrlari', now()),
    ('REGISTER', 'Registrlar', 'Qoldiq registrlari', now()),
    ('MANUAL', 'Ma''lumotnoma', 'Ma''lumotnoma ma''lumotlari', now()),
    ('POSTING_RULE', 'Postings qoidasi', 'Postings qoidasi', now()),
    ('WAREHOUSE_TRANSFER', 'Ombor ko''chirish', 'Omborlar o''rtasida ko''chirish', now()),
    ('INVENTORY_ADJUSTMENT', 'Inventar tuzatish', 'Inventar tuzatish hujjatlari', now()),
    ('INVENTORY_COUNT', 'Inventar sanog''i', 'Inventar sanog''i hujjatlari', now()),
    ('DOCUMENT_ACCOUNT_SETTING', 'Hujjat hisob sozlamalari', 'Hujjat hisob sozlamalari', now()),
    ('OPENING_BALANCE', 'Boshlang''ich qoldiq', 'Boshlang''ich qoldiqlar', now()),
    ('OPENING_INVENTORY', 'Boshlang''ich tovar qoldig''i', 'Boshlang''ich tovar qoldiqlari', now()),
    ('HR', 'Kadrlar', 'Xodimlar va kadrlar boshqaruvi', now()),
    ('PAYROLL', 'Oylik maosh', 'Oylik maosh va xodimlar bilan hisob-kitob', now()),
    ('FA', 'Asosiy vosita', 'Asosiy vositalar boshqaruvi', now()),
    ('PRICING_CONDITION', 'Narxlash qoidasi', 'Narxlash qoidasi', now()),
    ('SALE_CONDITION', 'Sotuv qoidasi', 'Sotuv qoidasi', now())
on conflict (code) do update
set short_name = excluded.short_name,
    full_name = excluded.full_name;

commit;