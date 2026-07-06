create table sys_module
(
    id integer not null,
    code character varying(100) not null,
    short_name character varying(250) not null,
    full_name character varying(300) not null,
    sub_group_id integer not null,
    state_id smallint not null,
    created_date timestamp without time zone default now() not null,
    parent_id integer,
    route character varying(250),
    icon character varying(100),
    sort_order integer default 0 not null,
    is_visible boolean default true not null,
    constraint sys_module_pkey primary key (id),
    constraint sys_module_state_id_fkey foreign key (state_id) references cmn_state (id),
    constraint sys_module_sub_group_id_fkey foreign key (sub_group_id) references sys_module_sub_group (id),
    constraint sys_module_parent_id_fkey foreign key (parent_id) references sys_module (id) DEFERRABLE INITIALLY DEFERRED
);

create unique index sys_module_unique_index_code on sys_module using btree (code);
create index sys_module_unique_index_sub_group_id on sys_module using btree (sub_group_id);
create index idx_sys_module_parent_id on sys_module using btree (parent_id);
create index idx_sys_module_sort_order on sys_module using btree (sort_order);
create index idx_sys_module_is_visible on sys_module using btree (is_visible);

insert into sys_module (id, code, short_name, full_name, sub_group_id, state_id, created_date)
values
    ('101', 'ROLE_VIEW', 'Rollar ro''yxati', 'Rollarni ko''rish', '1', '1', '2026-06-08 11:46:35.397683'),
    ('102', 'ROLE_VIEW_DETAIL', 'Rol detail', 'Rolni batafsil ko''rish', '1', '1', '2026-06-08 11:46:35.397683'),
    ('103', 'ROLE_CREATE', 'Rol yaratish', 'Yangi rol qo''shish', '1', '1', '2026-06-08 11:46:35.397683'),
    ('104', 'ROLE_UPDATE', 'Rol tahrirlash', 'Rolni tahrirlash', '1', '1', '2026-06-08 11:46:35.397683'),
    ('105', 'ROLE_DELETE', 'Rol o''chirish', 'Rolni o''chirish', '1', '1', '2026-06-08 11:46:35.397683'),

    ('111', 'USER_VIEW', 'Foydalanuvchilar', 'Foydalanuvchilarni ko''rish', '1', '1', '2026-06-08 11:46:35.397683'),
    ('112', 'USER_VIEW_DETAIL', 'Foydalanuvchi detail', 'Foydalanuvchini batafsil', '1', '1', '2026-06-08 11:46:35.397683'),
    ('113', 'USER_CREATE', 'Foydalanuvchi yaratish', 'Yangi foydalanuvchi', '1', '1', '2026-06-08 11:46:35.397683'),
    ('114', 'USER_UPDATE', 'Foydalanuvchi tahrir', 'Foydalanuvchini tahrirlash', '1', '1', '2026-06-08 11:46:35.397683'),
    ('115', 'USER_DELETE', 'Foydalanuvchi o''chirish', 'Foydalanuvchini o''chirish', '1', '1', '2026-06-08 11:46:35.397683'),
    ('1143', 'NOTIFICATION_MANAGE', 'Bildirishnomalar', 'Bildirishnomalarni boshqarish', '1', '1', '2026-07-06 00:00:00'),
    ('1144', 'PLATFORM_TENANT_MANAGE', 'Tenantlar', 'Tenantlarni boshqarish', '1', '1', '2026-07-06 00:00:00'),
    ('1145', 'PLATFORM_USER_MANAGE', 'Platform foydalanuvchilari', 'Platform foydalanuvchilarini boshqarish', '1', '1', '2026-07-06 00:00:00'),
    ('1146', 'PLATFORM_ORGANIZATION_MANAGE', 'Platform tashkilotlari', 'Platform tashkilotlarini boshqarish', '1', '1', '2026-07-06 00:00:00'),
    ('1147', 'PLATFORM_ACCOUNTANT_WORKSPACE_MANAGE', 'Buxgalter workspace', 'Buxgalter workspace larini boshqarish', '1', '1', '2026-07-06 00:00:00'),
    ('1148', 'PLATFORM_USER_ORGANIZATION_MANAGE', 'User tashkilot bog''lash', 'Foydalanuvchi va tashkilot bog''lanishini boshqarish', '1', '1', '2026-07-06 00:00:00'),

    ('201', 'ORGANIZATION_VIEW', 'Tashkilotlar', 'Tashkilotlar ro''yxati', '2', '1', '2026-06-08 11:46:35.397683'),
    ('202', 'ORGANIZATION_VIEW_DETAIL', 'Tashkilot detail', 'Tashkilotni batafsil ko''rish', '2', '1', '2026-06-08 11:46:35.397683'),
    ('203', 'ORGANIZATION_CREATE', 'Tashkilot yaratish', 'Yangi tashkilot', '2', '1', '2026-06-08 11:46:35.397683'),
    ('204', 'ORGANIZATION_UPDATE', 'Tashkilot tahrirlash', 'Tashkilotni tahrirlash', '2', '1', '2026-06-08 11:46:35.397683'),
    ('205', 'ORGANIZATION_DELETE', 'Tashkilot o''chirish', 'Tashkilotni o''chirish', '2', '1', '2026-06-08 11:46:35.397683'),
    ('1149', 'ORGANIZATION_SETUP_MANAGE', 'Tashkilot sozlamasi', 'Tashkilot boshlang''ich sozlamalarini boshqarish', '2', '1', '2026-07-06 00:00:00'),

    ('211', 'BRANCH_VIEW', 'Filiallar', 'Filiallar ro''yxati', '2', '1', '2026-06-08 11:46:35.397683'),
    ('212', 'BRANCH_VIEW_DETAIL', 'Filial detail', 'Filialni batafsil ko''rish', '2', '1', '2026-06-08 11:46:35.397683'),
    ('213', 'BRANCH_CREATE', 'Filial yaratish', 'Yangi filial', '2', '1', '2026-06-08 11:46:35.397683'),
    ('214', 'BRANCH_UPDATE', 'Filial tahrirlash', 'Filialni tahrirlash', '2', '1', '2026-06-08 11:46:35.397683'),
    ('215', 'BRANCH_DELETE', 'Filial o''chirish', 'Filialni o''chirish', '2', '1', '2026-06-08 11:46:35.397683'),

    ('221', 'DEPARTMENT_VIEW', 'Bo''limlar', 'Bo''limlar ro''yxati', '2', '1', '2026-06-08 11:46:35.397683'),
    ('222', 'DEPARTMENT_VIEW_DETAIL', 'Bo''lim detail', 'Bo''limni batafsil ko''rish', '2', '1', '2026-06-08 11:46:35.397683'),
    ('223', 'DEPARTMENT_CREATE', 'Bo''lim yaratish', 'Yangi bo''lim', '2', '1', '2026-06-08 11:46:35.397683'),
    ('224', 'DEPARTMENT_UPDATE', 'Bo''lim tahrirlash', 'Bo''limni tahrirlash', '2', '1', '2026-06-08 11:46:35.397683'),
    ('225', 'DEPARTMENT_DELETE', 'Bo''lim o''chirish', 'Bo''limni o''chirish', '2', '1', '2026-06-08 11:46:35.397683'),

    ('231', 'POSITION_VIEW', 'Lavozimlar', 'Lavozimlar ro''yxati', '2', '1', '2026-06-08 11:46:35.397683'),
    ('232', 'POSITION_VIEW_DETAIL', 'Lavozim detail', 'Lavozimni batafsil ko''rish', '2', '1', '2026-06-08 11:46:35.397683'),
    ('233', 'POSITION_CREATE', 'Lavozim yaratish', 'Yangi lavozim', '2', '1', '2026-06-08 11:46:35.397683'),
    ('234', 'POSITION_UPDATE', 'Lavozim tahrirlash', 'Lavozimni tahrirlash', '2', '1', '2026-06-08 11:46:35.397683'),
    ('235', 'POSITION_DELETE', 'Lavozim o''chirish', 'Lavozimni o''chirish', '2', '1', '2026-06-08 11:46:35.397683'),

    ('301', 'COUNTERPARTY_CARD_VIEW', 'Kontragentlar', 'Kontragentlar ro''yxati', '3', '1', '2026-06-08 11:46:35.397683'),
    ('302', 'COUNTERPARTY_CARD_VIEW_DETAIL', 'Kontragent detail', 'Kontragentni batafsil ko''rish', '3', '1', '2026-06-08 11:46:35.397683'),
    ('303', 'COUNTERPARTY_CARD_CREATE', 'Kontragent yaratish', 'Yangi kontragent', '3', '1', '2026-06-08 11:46:35.397683'),
    ('304', 'COUNTERPARTY_CARD_UPDATE', 'Kontragent tahrirlash', 'Kontragentni tahrirlash', '3', '1', '2026-06-08 11:46:35.397683'),
    ('305', 'COUNTERPARTY_CARD_DELETE', 'Kontragent o''chirish', 'Kontragentni o''chirish', '3', '1', '2026-06-08 11:46:35.397683'),

    ('311', 'COUNTERPARTY_BANK_ACCOUNT_VIEW', 'Kontragent bank hisoblari', 'Ro''yxat', '3', '1', '2026-06-08 11:46:35.397683'),
    ('312', 'COUNTERPARTY_BANK_ACCOUNT_VIEW_DETAIL', 'Kontragent bank hisobi detail', 'Batafsil', '3', '1', '2026-06-08 11:46:35.397683'),
    ('313', 'COUNTERPARTY_BANK_ACCOUNT_CREATE', 'Kontragent bank hisobi yaratish', 'Yangi', '3', '1', '2026-06-08 11:46:35.397683'),
    ('314', 'COUNTERPARTY_BANK_ACCOUNT_UPDATE', 'Kontragent bank hisobi tahrirlash', 'Tahrirlash', '3', '1', '2026-06-08 11:46:35.397683'),
    ('315', 'COUNTERPARTY_BANK_ACCOUNT_DELETE', 'Kontragent bank hisobi o''chirish', 'O''chirish', '3', '1', '2026-06-08 11:46:35.397683'),

    ('321', 'COUNTERPARTY_CONTACT_VIEW', 'Kontragent kontaktlari', 'Ro''yxat', '3', '1', '2026-06-08 11:46:35.397683'),
    ('322', 'COUNTERPARTY_CONTACT_VIEW_DETAIL', 'Kontragent kontakt detail', 'Batafsil', '3', '1', '2026-06-08 11:46:35.397683'),
    ('323', 'COUNTERPARTY_CONTACT_CREATE', 'Kontragent kontakt yaratish', 'Yangi', '3', '1', '2026-06-08 11:46:35.397683'),
    ('324', 'COUNTERPARTY_CONTACT_UPDATE', 'Kontragent kontakt tahrirlash', 'Tahrirlash', '3', '1', '2026-06-08 11:46:35.397683'),
    ('325', 'COUNTERPARTY_CONTACT_DELETE', 'Kontragent kontakt o''chirish', 'O''chirish', '3', '1', '2026-06-08 11:46:35.397683'),

    ('401', 'PRODUCT_GROUP_VIEW', 'Tovar guruhlari', 'Ro''yxat', '4', '1', '2026-06-08 11:46:35.397683'),
    ('402', 'PRODUCT_GROUP_VIEW_DETAIL', 'Tovar guruhi detail', 'Batafsil', '4', '1', '2026-06-08 11:46:35.397683'),
    ('403', 'PRODUCT_GROUP_CREATE', 'Tovar guruhi yaratish', 'Yangi', '4', '1', '2026-06-08 11:46:35.397683'),
    ('404', 'PRODUCT_GROUP_UPDATE', 'Tovar guruhi tahrirlash', 'Tahrirlash', '4', '1', '2026-06-08 11:46:35.397683'),
    ('405', 'PRODUCT_GROUP_DELETE', 'Tovar guruhi o''chirish', 'O''chirish', '4', '1', '2026-06-08 11:46:35.397683'),

    ('411', 'PRODUCT_VIEW', 'Tovarlar', 'Ro''yxat', '4', '1', '2026-06-08 11:46:35.397683'),
    ('412', 'PRODUCT_VIEW_DETAIL', 'Tovar detail', 'Batafsil', '4', '1', '2026-06-08 11:46:35.397683'),
    ('413', 'PRODUCT_CREATE', 'Tovar yaratish', 'Yangi', '4', '1', '2026-06-08 11:46:35.397683'),
    ('414', 'PRODUCT_UPDATE', 'Tovar tahrirlash', 'Tahrirlash', '4', '1', '2026-06-08 11:46:35.397683'),
    ('415', 'PRODUCT_DELETE', 'Tovar o''chirish', 'O''chirish', '4', '1', '2026-06-08 11:46:35.397683'),

    ('421', 'PRODUCT_PRICE_VIEW', 'Tovar narxlari', 'Ro''yxat', '4', '1', '2026-06-08 11:46:35.397683'),
    ('422', 'PRODUCT_PRICE_VIEW_DETAIL', 'Tovar narxi detail', 'Batafsil', '4', '1', '2026-06-08 11:46:35.397683'),
    ('423', 'PRODUCT_PRICE_CREATE', 'Tovar narxi yaratish', 'Yangi', '4', '1', '2026-06-08 11:46:35.397683'),
    ('424', 'PRODUCT_PRICE_UPDATE', 'Tovar narxi tahrirlash', 'Tahrirlash', '4', '1', '2026-06-08 11:46:35.397683'),
    ('425', 'PRODUCT_PRICE_DELETE', 'Tovar narxi o''chirish', 'O''chirish', '4', '1', '2026-06-08 11:46:35.397683'),

    ('431', 'WAREHOUSE_VIEW', 'Omborlar', 'Ro''yxat', '4', '1', '2026-06-08 11:46:35.397683'),
    ('432', 'WAREHOUSE_VIEW_DETAIL', 'Ombor detail', 'Batafsil', '4', '1', '2026-06-08 11:46:35.397683'),
    ('433', 'WAREHOUSE_CREATE', 'Ombor yaratish', 'Yangi', '4', '1', '2026-06-08 11:46:35.397683'),
    ('434', 'WAREHOUSE_UPDATE', 'Ombor tahrirlash', 'Tahrirlash', '4', '1', '2026-06-08 11:46:35.397683'),
    ('435', 'WAREHOUSE_DELETE', 'Ombor o''chirish', 'O''chirish', '4', '1', '2026-06-08 11:46:35.397683'),

    ('441', 'PRODUCT_TABLE_VIEW', 'Tovar kartochkalari', 'Tovar kartochkalarini ko''rish', '4', '1', '2026-06-20 10:15:52.743562'),

    ('501', 'ORG_BANK_ACCOUNT_VIEW', 'Tashkilot bank hisoblari', 'Ro''yxat', '5', '1', '2026-06-08 11:46:35.397683'),
    ('502', 'ORG_BANK_ACCOUNT_VIEW_DETAIL', 'Tashkilot bank hisobi detail', 'Batafsil', '5', '1', '2026-06-08 11:46:35.397683'),
    ('503', 'ORG_BANK_ACCOUNT_CREATE', 'Tashkilot bank hisobi yaratish', 'Yangi', '5', '1', '2026-06-08 11:46:35.397683'),
    ('504', 'ORG_BANK_ACCOUNT_UPDATE', 'Tashkilot bank hisobi tahrirlash', 'Tahrirlash', '5', '1', '2026-06-08 11:46:35.397683'),
    ('505', 'ORG_BANK_ACCOUNT_DELETE', 'Tashkilot bank hisobi o''chirish', 'O''chirish', '5', '1', '2026-06-08 11:46:35.397683'),

    ('511', 'BANK_OPERATION_VIEW', 'Bank operatsiyalari', 'Ro''yxat', '5', '1', '2026-06-08 11:46:35.397683'),
    ('512', 'BANK_OPERATION_VIEW_DETAIL', 'Bank operatsiyasi detail', 'Batafsil', '5', '1', '2026-06-08 11:46:35.397683'),
    ('513', 'BANK_OPERATION_CREATE', 'Bank operatsiyasi yaratish', 'Yangi', '5', '1', '2026-06-08 11:46:35.397683'),
    ('514', 'BANK_OPERATION_UPDATE', 'Bank operatsiyasi tahrirlash', 'Tahrirlash', '5', '1', '2026-06-08 11:46:35.397683'),
    ('515', 'BANK_OPERATION_DELETE', 'Bank operatsiyasi o''chirish', 'O''chirish', '5', '1', '2026-06-08 11:46:35.397683'),
    ('516', 'CONFIRM_BANK_OPERATION', 'Bank operatsiyasini tasdiqlash', 'Tasdiqlash', '5', '1', '2026-06-08 11:46:35.397683'),
    ('517', 'CANCEL_BANK_OPERATION', 'Bank operatsiyasini bekor qilish', 'Bekor qilish', '5', '1', '2026-06-08 11:46:35.397683'),

    ('521', 'BANK_STATEMENT_PARSE', 'Bank statement import', 'Bank Excel statement faylini JSON qilib parse qilish', '5', '1', '2026-06-24 13:12:30.3099'),

    ('531', 'BANK_VIEW', 'Banklar', 'Banklar ro''yxati', '5', '1', '2026-06-24 19:08:36.998172'),
    ('532', 'BANK_VIEW_DETAIL', 'Bank detail', 'Bankni batafsil ko''rish', '5', '1', '2026-06-24 19:08:36.998172'),
    ('533', 'BANK_CREATE', 'Bank yaratish', 'Yangi bank qo''shish', '5', '1', '2026-06-24 19:08:36.998172'),
    ('534', 'BANK_UPDATE', 'Bank tahrirlash', 'Bankni tahrirlash', '5', '1', '2026-06-24 19:08:36.998172'),
    ('535', 'BANK_DELETE', 'Bank o''chirish', 'Bankni o''chirish', '5', '1', '2026-06-24 19:08:36.998172'),

    ('601', 'CASH_BOX_VIEW', 'Kassalar', 'Ro''yxat', '6', '1', '2026-06-08 11:46:35.397683'),
    ('602', 'CASH_BOX_VIEW_DETAIL', 'Kassa detail', 'Batafsil', '6', '1', '2026-06-08 11:46:35.397683'),
    ('603', 'CASH_BOX_CREATE', 'Kassa yaratish', 'Yangi', '6', '1', '2026-06-08 11:46:35.397683'),
    ('604', 'CASH_BOX_UPDATE', 'Kassa tahrirlash', 'Tahrirlash', '6', '1', '2026-06-08 11:46:35.397683'),
    ('605', 'CASH_BOX_DELETE', 'Kassa o''chirish', 'O''chirish', '6', '1', '2026-06-08 11:46:35.397683'),

    ('611', 'CASH_OPERATION_VIEW', 'Kassa operatsiyalari', 'Ro''yxat', '6', '1', '2026-06-08 11:46:35.397683'),
    ('612', 'CASH_OPERATION_VIEW_DETAIL', 'Kassa operatsiyasi detail', 'Batafsil', '6', '1', '2026-06-08 11:46:35.397683'),
    ('613', 'CASH_OPERATION_CREATE', 'Kassa operatsiyasi yaratish', 'Yangi', '6', '1', '2026-06-08 11:46:35.397683'),
    ('614', 'CASH_OPERATION_UPDATE', 'Kassa operatsiyasi tahrirlash', 'Tahrirlash', '6', '1', '2026-06-08 11:46:35.397683'),
    ('615', 'CASH_OPERATION_DELETE', 'Kassa operatsiyasi o''chirish', 'O''chirish', '6', '1', '2026-06-08 11:46:35.397683'),
    ('616', 'CONFIRM_CASH_OPERATION', 'Kassa operatsiyasini tasdiqlash', 'Tasdiqlash', '6', '1', '2026-06-08 11:46:35.397683'),
    ('617', 'CANCEL_CASH_OPERATION', 'Kassa operatsiyasini bekor qilish', 'Bekor qilish', '6', '1', '2026-06-08 11:46:35.397683'),

    ('701', 'PURCHASE_DOC_VIEW', 'Xarid hujjatlari', 'Ro''yxat', '7', '1', '2026-06-08 11:46:35.397683'),
    ('702', 'PURCHASE_DOC_VIEW_DETAIL', 'Xarid hujjati detail', 'Batafsil', '7', '1', '2026-06-08 11:46:35.397683'),
    ('703', 'PURCHASE_DOC_CREATE', 'Xarid hujjati yaratish', 'Yangi', '7', '1', '2026-06-08 11:46:35.397683'),
    ('704', 'PURCHASE_DOC_UPDATE', 'Xarid hujjati tahrirlash', 'Tahrirlash', '7', '1', '2026-06-08 11:46:35.397683'),
    ('705', 'PURCHASE_DOC_DELETE', 'Xarid hujjati o''chirish', 'O''chirish', '7', '1', '2026-06-08 11:46:35.397683'),
    ('706', 'CONFIRM_PURCHASE', 'Xaridni tasdiqlash', 'Tasdiqlash', '7', '1', '2026-06-08 11:46:35.397683'),
    ('707', 'CANCEL_PURCHASE', 'Xaridni bekor qilish', 'Bekor qilish', '7', '1', '2026-06-08 11:46:35.397683'),

    ('711', 'PURCHASE_DOC_TABLE_VIEW', 'Xarid satrlari', 'Ro''yxat', '7', '1', '2026-06-08 11:46:35.397683'),
    ('712', 'PURCHASE_DOC_TABLE_VIEW_DETAIL', 'Xarid satri detail', 'Batafsil', '7', '1', '2026-06-08 11:46:35.397683'),
    ('713', 'PURCHASE_DOC_TABLE_CREATE', 'Xarid satri yaratish', 'Yangi', '7', '1', '2026-06-08 11:46:35.397683'),
    ('714', 'PURCHASE_DOC_TABLE_UPDATE', 'Xarid satri tahrirlash', 'Tahrirlash', '7', '1', '2026-06-08 11:46:35.397683'),
    ('715', 'PURCHASE_DOC_TABLE_DELETE', 'Xarid satri o''chirish', 'O''chirish', '7', '1', '2026-06-08 11:46:35.397683'),

    ('721', 'CONTRACT_VIEW', 'Shartnomalar', 'Shartnomalar ro''yxati', '7', '1', '2026-06-16 17:32:01.442269'),
    ('722', 'CONTRACT_VIEW_DETAIL', 'Shartnoma detail', 'Shartnomani batafsil ko''rish', '7', '1', '2026-06-16 17:32:01.442269'),
    ('723', 'CONTRACT_CREATE', 'Shartnoma yaratish', 'Yangi shartnoma qo''shish', '7', '1', '2026-06-16 17:32:01.442269'),
    ('724', 'CONTRACT_UPDATE', 'Shartnoma tahrirlash', 'Shartnomani tahrirlash', '7', '1', '2026-06-16 17:32:01.442269'),
    ('725', 'CONTRACT_DELETE', 'Shartnoma o''chirish', 'Shartnomani o''chirish', '7', '1', '2026-06-16 17:32:01.442269'),

    ('801', 'SALE_DOC_VIEW', 'Sotuv hujjatlari', 'Ro''yxat', '8', '1', '2026-06-08 11:46:35.397683'),
    ('802', 'SALE_DOC_VIEW_DETAIL', 'Sotuv hujjati detail', 'Batafsil', '8', '1', '2026-06-08 11:46:35.397683'),
    ('803', 'SALE_DOC_CREATE', 'Sotuv hujjati yaratish', 'Yangi', '8', '1', '2026-06-08 11:46:35.397683'),
    ('804', 'SALE_DOC_UPDATE', 'Sotuv hujjati tahrirlash', 'Tahrirlash', '8', '1', '2026-06-08 11:46:35.397683'),
    ('805', 'SALE_DOC_DELETE', 'Sotuv hujjati o''chirish', 'O''chirish', '8', '1', '2026-06-08 11:46:35.397683'),
    ('806', 'CONFIRM_SALE', 'Sotuvni tasdiqlash', 'Tasdiqlash', '8', '1', '2026-06-08 11:46:35.397683'),
    ('807', 'CANCEL_SALE', 'Sotuvni bekor qilish', 'Bekor qilish', '8', '1', '2026-06-08 11:46:35.397683'),

    ('811', 'SALE_DOC_TABLE_VIEW', 'Sotuv satrlari', 'Ro''yxat', '8', '1', '2026-06-08 11:46:35.397683'),
    ('812', 'SALE_DOC_TABLE_VIEW_DETAIL', 'Sotuv satri detail', 'Batafsil', '8', '1', '2026-06-08 11:46:35.397683'),
    ('813', 'SALE_DOC_TABLE_CREATE', 'Sotuv satri yaratish', 'Yangi', '8', '1', '2026-06-08 11:46:35.397683'),
    ('814', 'SALE_DOC_TABLE_UPDATE', 'Sotuv satri tahrirlash', 'Tahrirlash', '8', '1', '2026-06-08 11:46:35.397683'),
    ('815', 'SALE_DOC_TABLE_DELETE', 'Sotuv satri o''chirish', 'O''chirish', '8', '1', '2026-06-08 11:46:35.397683'),

    ('901', 'CHART_ACCOUNT_VIEW', 'Hisoblar rejasi', 'Ro''yxat', '9', '1', '2026-06-08 11:46:35.397683'),
    ('902', 'CHART_ACCOUNT_VIEW_DETAIL', 'Hisoblar rejasi detail', 'Batafsil', '9', '1', '2026-06-08 11:46:35.397683'),
    ('903', 'CHART_ACCOUNT_CREATE', 'Hisob yaratish', 'Yangi', '9', '1', '2026-06-08 11:46:35.397683'),
    ('904', 'CHART_ACCOUNT_UPDATE', 'Hisob tahrirlash', 'Tahrirlash', '9', '1', '2026-06-08 11:46:35.397683'),
    ('905', 'CHART_ACCOUNT_DELETE', 'Hisob o''chirish', 'O''chirish', '9', '1', '2026-06-08 11:46:35.397683'),

    ('911', 'ACC_REG_ENTRY_VIEW', 'Buxg. yozuvlar', 'Ro''yxat', '9', '1', '2026-06-08 11:46:35.397683'),
    ('914', 'ACC_REG_ENTRY_UPDATE', 'Buxg. yozuv tahrirlash', 'Tahrirlash', '9', '1', '2026-06-08 11:46:35.397683'),

    ('1001', 'COUNTERPARTY_REG_BALANCE_VIEW', 'Kontragent qoldiqlari', 'Ro''yxat', '10', '1', '2026-06-08 11:46:35.397683'),
    ('1002', 'COUNTERPARTY_REG_BALANCE_VIEW_DETAIL', 'Kontragent qoldig''i detail', 'Batafsil', '10', '1', '2026-06-08 11:46:35.397683'),
    ('1003', 'COUNTERPARTY_REG_BALANCE_CREATE', 'Kontragent qoldig''i yaratish', 'Yangi', '10', '1', '2026-06-08 11:46:35.397683'),
    ('1004', 'COUNTERPARTY_REG_BALANCE_UPDATE', 'Kontragent qoldig''i tahrirlash', 'Tahrirlash', '10', '1', '2026-06-08 11:46:35.397683'),
    ('1005', 'COUNTERPARTY_REG_BALANCE_DELETE', 'Kontragent qoldig''i o''chirish', 'O''chirish', '10', '1', '2026-06-08 11:46:35.397683'),

    ('1011', 'INVENTORY_REG_BALANCE_VIEW', 'Inventar qoldiqlari', 'Ro''yxat', '10', '1', '2026-06-08 11:46:35.397683'),
    ('1012', 'INVENTORY_REG_BALANCE_VIEW_DETAIL', 'Inventar qoldig''i detail', 'Batafsil', '10', '1', '2026-06-08 11:46:35.397683'),

    ('1021', 'MONEY_REG_BALANCE_VIEW', 'Pul qoldiqlari', 'Ro''yxat', '10', '1', '2026-06-08 11:46:35.397683'),
    ('1022', 'MONEY_REG_BALANCE_VIEW_DETAIL', 'Pul qoldig''i detail', 'Batafsil', '10', '1', '2026-06-08 11:46:35.397683'),
    ('1023', 'MONEY_REG_BALANCE_CREATE', 'Pul qoldig''i yaratish', 'Yangi', '10', '1', '2026-06-08 11:46:35.397683'),
    ('1024', 'MONEY_REG_BALANCE_UPDATE', 'Pul qoldig''i tahrirlash', 'Tahrirlash', '10', '1', '2026-06-08 11:46:35.397683'),
    ('1025', 'MONEY_REG_BALANCE_DELETE', 'Pul qoldig''i o''chirish', 'O''chirish', '10', '1', '2026-06-08 11:46:35.397683'),

    ('1026', 'PRICING_CONDITION_VIEW', 'Narxlash qoidasi', 'Ro''yxat', '12', '1', '2026-06-27 16:26:12.016132'),
    ('1027', 'PRICING_CONDITION_VIEW_DETAIL', 'Narxlash qoidasi detail', 'Batafsil', '12', '1', '2026-06-27 16:26:12.016132'),
    ('1028', 'PRICING_CONDITION_CREATE', 'Narxlash qoidasi yaratish', 'Yangi', '12', '1', '2026-06-27 16:26:12.016132'),
    ('1030', 'PRICING_CONDITION_DELETE', 'Narxlash qoidasi o''chirish', 'O''chirish', '12', '1', '2026-06-27 16:26:12.016132'),

    ('1031', 'SALE_CONDITION_VIEW', 'Sotuv qoidasi', 'Ro''yxat', '13', '1', '2026-06-27 16:26:12.016132'),
    ('1032', 'SALE_CONDITION_VIEW_DETAIL', 'Sotuv qoidasi detail', 'Batafsil', '13', '1', '2026-06-27 16:26:12.016132'),
    ('1033', 'SALE_CONDITION_CREATE', 'Sotuv qoidasi yaratish', 'Yangi', '13', '1', '2026-06-27 16:26:12.016132'),
    ('1035', 'SALE_CONDITION_DELETE', 'Sotuv qoidasi o''chirish', 'O''chirish', '13', '1', '2026-06-27 16:26:12.016132'),

    ('1036', 'POSTING_RULE_VIEW', 'Postings qoidasi', 'Ro''yxat', '14', '1', '2026-06-27 16:26:12.016132'),
    ('1037', 'POSTING_RULE_VIEW_DETAIL', 'Postings qoidasi detail', 'Batafsil', '14', '1', '2026-06-27 16:26:12.016132'),

    ('1101', 'MANUAL_VIEW', 'Ma''lumotnoma', 'Ma''lumotnoma ma''lumotlarini ko''rish', '11', '1', '2026-06-08 11:46:35.397683'),
    ('1150', 'BARCODE_GENERATE', 'Barcode generatsiya', 'Barcode va QR kod generatsiya qilish', '11', '1', '2026-07-06 00:00:00'),
    ('1102', 'CURRENCY_VIEW', 'Valyutalar', 'Valyutalar ro''yxati', '11', '1', '2026-07-04 00:00:00'),
    ('1103', 'CURRENCY_VIEW_DETAIL', 'Valyuta detail', 'Valyutani batafsil ko''rish', '11', '1', '2026-07-04 00:00:00'),
    ('1104', 'CURRENCY_CREATE', 'Valyuta yaratish', 'Yangi valyuta qo''shish', '11', '1', '2026-07-04 00:00:00'),
    ('1105', 'CURRENCY_UPDATE', 'Valyuta tahrirlash', 'Valyutani tahrirlash', '11', '1', '2026-07-04 00:00:00'),
    ('1106', 'CURRENCY_DELETE', 'Valyuta o''chirish', 'Valyutani o''chirish', '11', '1', '2026-07-04 00:00:00'),

    ('1134', 'TAX_VIEW', 'Soliqlar', 'Soliqlar ro''yxati', '11', '1', '2026-07-04 00:00:00'),
    ('1135', 'TAX_VIEW_DETAIL', 'Soliq detail', 'Soliqni batafsil ko''rish', '11', '1', '2026-07-04 00:00:00'),
    ('1136', 'TAX_CREATE', 'Soliq yaratish', 'Yangi soliq qo''shish', '11', '1', '2026-07-04 00:00:00'),
    ('1137', 'TAX_UPDATE', 'Soliq tahrirlash', 'Soliqni tahrirlash', '11', '1', '2026-07-04 00:00:00'),
    ('1138', 'TAX_DELETE', 'Soliq o''chirish', 'Soliqni o''chirish', '11', '1', '2026-07-04 00:00:00'),

    ('1128', 'CURRENCY_RATE_IMPORT', 'Valyuta kursini import qilish', 'Markaziy bankdan valyuta kursini import qilish', '11', '1', '2026-07-04 00:00:00'),
    ('1129', 'CURRENCY_RATE_SYNC', 'Valyuta kursini sinxronlash', 'Markaziy bankdan valyuta kursini sanaga ko''ra sinxronlash', '11', '1', '2026-07-04 00:00:00'),
    ('1130', 'CURRENCY_REVALUATION_VIEW', 'Valyuta qayta baholash', 'Valyuta qayta baholash ro''yxati', '11', '1', '2026-07-04 00:00:00'),
    ('1131', 'CURRENCY_REVALUATION_CREATE', 'Valyuta qayta baholash yaratish', 'Valyuta qayta baholash preview va yaratish', '11', '1', '2026-07-04 00:00:00'),
    ('1132', 'CURRENCY_REVALUATION_CONFIRM', 'Valyuta qayta baholash tasdiqlash', 'Valyuta qayta baholashni tasdiqlash', '11', '1', '2026-07-04 00:00:00'),
    ('1133', 'CURRENCY_REVALUATION_CANCEL', 'Valyuta qayta baholash bekor qilish', 'Valyuta qayta baholashni bekor qilish', '11', '1', '2026-07-04 00:00:00'),

    ('1151', 'WAREHOUSE_TRANSFER_VIEW', 'Ombor ko''chirishlar', 'Ro''yxat', '15', '1', '2026-07-03 00:00:00'),
    ('1152', 'WAREHOUSE_TRANSFER_VIEW_DETAIL', 'Ombor ko''chirish detail', 'Batafsil', '15', '1', '2026-07-03 00:00:00'),
    ('1153', 'WAREHOUSE_TRANSFER_CREATE', 'Ombor ko''chirish yaratish', 'Yangi', '15', '1', '2026-07-03 00:00:00'),
    ('1154', 'WAREHOUSE_TRANSFER_UPDATE', 'Ombor ko''chirish tahrirlash', 'Tahrirlash', '15', '1', '2026-07-03 00:00:00'),
    ('1155', 'WAREHOUSE_TRANSFER_DELETE', 'Ombor ko''chirish o''chirish', 'O''chirish', '15', '1', '2026-07-03 00:00:00'),
    ('1156', 'CONFIRM_WAREHOUSE_TRANSFER', 'Ombor ko''chirishni tasdiqlash', 'Tasdiqlash', '15', '1', '2026-07-03 00:00:00'),
    ('1157', 'CANCEL_WAREHOUSE_TRANSFER', 'Ombor ko''chirishni bekor qilish', 'Bekor qilish', '15', '1', '2026-07-03 00:00:00'),

    ('1109', 'INVENTORY_ADJUSTMENT_VIEW', 'Inventar tuzatishlar', 'Ro''yxat', '16', '1', '2026-07-03 00:00:00'),
    ('1110', 'INVENTORY_ADJUSTMENT_VIEW_DETAIL', 'Inventar tuzatish detail', 'Batafsil', '16', '1', '2026-07-03 00:00:00'),
    ('1111', 'INVENTORY_ADJUSTMENT_CREATE', 'Inventar tuzatish yaratish', 'Yangi', '16', '1', '2026-07-03 00:00:00'),
    ('1112', 'INVENTORY_ADJUSTMENT_UPDATE', 'Inventar tuzatish tahrirlash', 'Tahrirlash', '16', '1', '2026-07-03 00:00:00'),
    ('1113', 'INVENTORY_ADJUSTMENT_DELETE', 'Inventar tuzatish o''chirish', 'O''chirish', '16', '1', '2026-07-03 00:00:00'),
    ('1114', 'CONFIRM_INVENTORY_ADJUSTMENT', 'Inventar tuzatishni tasdiqlash', 'Tasdiqlash', '16', '1', '2026-07-03 00:00:00'),
    ('1115', 'CANCEL_INVENTORY_ADJUSTMENT', 'Inventar tuzatishni bekor qilish', 'Bekor qilish', '16', '1', '2026-07-03 00:00:00'),

    ('1116', 'INVENTORY_COUNT_VIEW', 'Inventar sanog''i', 'Ro''yxat', '17', '1', '2026-07-03 00:00:00'),
    ('1117', 'INVENTORY_COUNT_VIEW_DETAIL', 'Inventar sanog''i detail', 'Batafsil', '17', '1', '2026-07-03 00:00:00'),
    ('1118', 'INVENTORY_COUNT_CREATE', 'Inventar sanog''i yaratish', 'Yangi', '17', '1', '2026-07-03 00:00:00'),
    ('1119', 'INVENTORY_COUNT_UPDATE', 'Inventar sanog''i tahrirlash', 'Tahrirlash', '17', '1', '2026-07-03 00:00:00'),
    ('1120', 'INVENTORY_COUNT_DELETE', 'Inventar sanog''i o''chirish', 'O''chirish', '17', '1', '2026-07-03 00:00:00'),
    ('1121', 'CONFIRM_INVENTORY_COUNT', 'Inventar sanog''ini tasdiqlash', 'Tasdiqlash', '17', '1', '2026-07-03 00:00:00'),
    ('1122', 'CANCEL_INVENTORY_COUNT', 'Inventar sanog''ini bekor qilish', 'Bekor qilish', '17', '1', '2026-07-03 00:00:00'),

    ('1158', 'FA_ASSET_VIEW', 'Asosiy vositalar', 'Asosiy vositalar ro''yxatini ko''rish', '11', '1', '2026-07-06 00:00:00'),
    ('1159', 'FA_ASSET_VIEW_DETAIL', 'Asosiy vosita detail', 'Asosiy vositani batafsil ko''rish', '11', '1', '2026-07-06 00:00:00'),
    ('1160', 'FA_ASSET_CREATE', 'Asosiy vosita yaratish', 'Yangi asosiy vosita qo''shish', '11', '1', '2026-07-06 00:00:00'),
    ('1161', 'FA_ASSET_UPDATE', 'Asosiy vosita tahrirlash', 'Asosiy vositani tahrirlash', '11', '1', '2026-07-06 00:00:00'),
    ('1162', 'FA_ASSET_DELETE', 'Asosiy vosita o''chirish', 'Asosiy vositani o''chirish', '11', '1', '2026-07-06 00:00:00'),
    ('1163', 'FA_RECEIPT_VIEW', 'OS qabuli', 'Asosiy vosita qabul hujjatlarini ko''rish', '11', '1', '2026-07-06 00:00:00'),
    ('1164', 'FA_RECEIPT_VIEW_DETAIL', 'OS qabuli detail', 'Asosiy vosita qabul hujjatini batafsil ko''rish', '11', '1', '2026-07-06 00:00:00'),
    ('1165', 'FA_RECEIPT_CREATE', 'OS qabuli yaratish', 'Yangi asosiy vosita qabul hujjati yaratish', '11', '1', '2026-07-06 00:00:00'),
    ('1166', 'FA_RECEIPT_UPDATE', 'OS qabuli tahrirlash', 'Asosiy vosita qabul hujjatini tahrirlash', '11', '1', '2026-07-06 00:00:00'),
    ('1167', 'FA_RECEIPT_DELETE', 'OS qabuli o''chirish', 'Asosiy vosita qabul hujjatini o''chirish', '11', '1', '2026-07-06 00:00:00'),
    ('1168', 'FA_RECEIPT_CONFIRM', 'OS qabuli tasdiqlash', 'Asosiy vosita qabul hujjatini tasdiqlash', '11', '1', '2026-07-06 00:00:00'),
    ('1169', 'FA_RECEIPT_CANCEL', 'OS qabuli bekor qilish', 'Asosiy vosita qabul hujjatini bekor qilish', '11', '1', '2026-07-06 00:00:00');


insert into sys_module (id, code, short_name, full_name, sub_group_id, state_id, created_date, parent_id, route, icon, sort_order, is_visible)
values
    ('1123', 'CURRENCY_RATE_VIEW', 'Valyuta kurslari', 'Valyuta kurslari ro''yxati', '11', '1', '2026-07-04 00:00:00', null, '/currency-rates', 'currency-exchange', '50', '1'),
    ('1124', 'CURRENCY_RATE_VIEW_DETAIL', 'Valyuta kursi detail', 'Valyuta kursini batafsil ko''rish', '11', '1', '2026-07-04 00:00:00', null, '/currency-rates', 'currency-exchange', '51', '1'),
    ('1125', 'CURRENCY_RATE_CREATE', 'Valyuta kursi yaratish', 'Yangi valyuta kursi qo''shish', '11', '1', '2026-07-04 00:00:00', null, '/currency-rates', 'currency-exchange', '52', '1'),
    ('1126', 'CURRENCY_RATE_UPDATE', 'Valyuta kursi tahrirlash', 'Valyuta kursini tahrirlash', '11', '1', '2026-07-04 00:00:00', null, '/currency-rates', 'currency-exchange', '53', '1'),
    ('1127', 'CURRENCY_RATE_DELETE', 'Valyuta kursi o''chirish', 'Valyuta kursini o''chirish', '11', '1', '2026-07-04 00:00:00', null, '/currency-rates', 'currency-exchange', '54', '1'),

    ('1139', 'AUDIT_LOG_VIEW', 'Audit log', 'Audit loglarni ko''rish', '1', '1', '2026-07-05 00:00:00', null, '/audit-logs', 'history', '90', '1'),

    ('1140', 'SETTINGS_MANAGE', 'Tizim sozlamalari', 'Tizim sozlamalarini boshqarish', '1', '1', '2026-07-05 00:00:00', null, '/settings', 'settings', '92', '1'),

    ('1141', 'DASHBOARD_VIEW', 'Dashboard', 'Super Admin dashboard statistikasi', '1', '1', '2026-07-05 00:00:00', null, '/dashboard', 'dashboard', '91', '1');

select setval('sys_module_id_seq', greatest((select coalesce(max(id), 0) from sys_module), 1169), true);
