create table sys_module_sub_group
(
	id serial not null primary key,
	code varchar(100) not null,
	short_name varchar(250) not null,
	full_name varchar(300) not null,
	created_date timestamp without time zone default now() not null
);

create unique index sys_module_sub_group_unique_index_code on sys_module_sub_group (code);

INSERT INTO sys_module_sub_group (id, code, short_name, full_name, created_date) VALUES
(1,  'SYS',          'Tizim',        'Tizim sozlamalari',          now()),
(2,  'ORG',          'Tashkilot',    'Tashkilot boshqaruvi',        now()),
(3,  'COUNTERPARTY', 'Kontragent',   'Kontragentlar',               now()),
(4,  'INVENTORY',    'Inventar',     'Tovar va ombor',              now()),
(5,  'BANK',         'Bank',         'Bank operatsiyalari',         now()),
(6,  'CASH',         'Kassa',        'Kassa operatsiyalari',        now()),
(7,  'PURCHASE',     'Xarid',        'Xarid hujjatlari',            now()),
(8,  'SALE',         'Sotuv',        'Sotuv hujjatlari',            now()),
(9,  'ACCOUNTING',   'Buxgalteriya', 'Buxgalteriya registrlari',   now()),
(10, 'REGISTER',     'Registrlar',   'Qoldiq registrlari',          now()),
(11, 'MANUAL',       'Ma''lumotnoma','Ma''lumotnoma ma''lumotlari', now())
ON CONFLICT (id) DO NOTHING;