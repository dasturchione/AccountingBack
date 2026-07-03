
create table sys_module_sub_group (
    id integer   not null,
    code character varying(100) not null,
    short_name character varying(250) not null,
    full_name character varying(300) not null,
    created_date timestamp without time zone default now() not null,
    constraint sys_module_sub_group_pkey primary key (id)
);

insert into sys_module_sub_group (id, code, short_name, full_name, created_date) values
    ('1', 'SYS', 'Tizim', 'Tizim sozlamalari', '2026-06-08 11:44:36.687616'),
    ('2', 'ORG', 'Tashkilot', 'Tashkilot boshqaruvi', '2026-06-08 11:44:36.687616'),
    ('3', 'COUNTERPARTY', 'Kontragent', 'Kontragentlar', '2026-06-08 11:44:36.687616'),
    ('4', 'INVENTORY', 'Inventar', 'Tovar va ombor', '2026-06-08 11:44:36.687616'),
    ('5', 'BANK', 'Bank', 'Bank operatsiyalari', '2026-06-08 11:44:36.687616'),
    ('6', 'CASH', 'Kassa', 'Kassa operatsiyalari', '2026-06-08 11:44:36.687616'),
    ('7', 'PURCHASE', 'Xarid', 'Xarid hujjatlari', '2026-06-08 11:44:36.687616'),
    ('8', 'SALE', 'Sotuv', 'Sotuv hujjatlari', '2026-06-08 11:44:36.687616'),
    ('9', 'ACCOUNTING', 'Buxgalteriya', 'Buxgalteriya registrlari', '2026-06-08 11:44:36.687616'),
    ('10', 'REGISTER', 'Registrlar', 'Qoldiq registrlari', '2026-06-08 11:44:36.687616'),
    ('11', 'MANUAL', 'Ma''lumotnoma', 'Ma''lumotnoma ma''lumotlari', '2026-06-08 11:44:36.687616'),
    ('12', 'PRICING_CONDITION', 'Narxlash qoidasi', 'Narxlash qoidasi', '2026-06-27 16:21:38.259971'),
    ('13', 'SALE_CONDITION', 'Sotuv qoidasi', 'Sotuv qoidasi', '2026-06-27 16:21:38.259971'),
    ('14', 'POSTING_RULE', 'Postings qoidasi', 'Postings qoidasi', '2026-06-27 16:21:38.259971');

select setval('sys_module_sub_group_id_seq', 15, true);

alter table ONLY sys_module_sub_group
    ADD constraint sys_module_sub_group_pkey primary key (id);

create unique index sys_module_sub_group_unique_index_code on sys_module_sub_group using btree (code);

