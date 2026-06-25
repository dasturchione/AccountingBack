create table sys_module 
(
	id serial not null primary key,
	code varchar(100) not null,
	short_name varchar(250) not null,
	full_name varchar(300) not null,
	sub_group_id int not null references sys_module_sub_group(id),
	state_id smallint not null references cmn_state(id),
	created_date timestamp without time zone default now() not null         
);

create unique index sys_module_unique_index_code on sys_module (code);
create index sys_module_unique_index_sub_group_id on sys_module (sub_group_id);

INSERT INTO sys_module (id, code, short_name, full_name, sub_group_id, state_id, created_date) VALUES
(101, 'ROLE_VIEW',        'Rollar ro''yxati',     'Rollarni ko''rish',          1, 1, now()),
(102, 'ROLE_VIEW_DETAIL', 'Rol detail',           'Rolni batafsil ko''rish',    1, 1, now()),
(103, 'ROLE_CREATE',      'Rol yaratish',         'Yangi rol qo''shish',        1, 1, now()),
(104, 'ROLE_UPDATE',      'Rol tahrirlash',       'Rolni tahrirlash',           1, 1, now()),
(105, 'ROLE_DELETE',      'Rol o''chirish',       'Rolni o''chirish',           1, 1, now()),

-- ---- SYS: User (sub_group_id = 1) ----
(111, 'USER_VIEW',        'Foydalanuvchilar',     'Foydalanuvchilarni ko''rish', 1, 1, now()),
(112, 'USER_VIEW_DETAIL', 'Foydalanuvchi detail', 'Foydalanuvchini batafsil',   1, 1, now()),
(113, 'USER_CREATE',      'Foydalanuvchi yaratish','Yangi foydalanuvchi',       1, 1, now()),
(114, 'USER_UPDATE',      'Foydalanuvchi tahrir', 'Foydalanuvchini tahrirlash', 1, 1, now()),
(115, 'USER_DELETE',      'Foydalanuvchi o''chirish','Foydalanuvchini o''chirish',1,1,now()),

-- ---- ORG: Organization (sub_group_id = 2) ----
(201, 'ORGANIZATION_VIEW',        'Tashkilotlar',         'Tashkilotlar ro''yxati',       2, 1, now()),
(202, 'ORGANIZATION_VIEW_DETAIL', 'Tashkilot detail',     'Tashkilotni batafsil ko''rish',2, 1, now()),
(203, 'ORGANIZATION_CREATE',      'Tashkilot yaratish',   'Yangi tashkilot',              2, 1, now()),
(204, 'ORGANIZATION_UPDATE',      'Tashkilot tahrirlash', 'Tashkilotni tahrirlash',       2, 1, now()),
(205, 'ORGANIZATION_DELETE',      'Tashkilot o''chirish', 'Tashkilotni o''chirish',       2, 1, now()),

-- ---- ORG: Branch (sub_group_id = 2) ----
(211, 'BRANCH_VIEW',        'Filiallar',       'Filiallar ro''yxati',       2, 1, now()),
(212, 'BRANCH_VIEW_DETAIL', 'Filial detail',   'Filialni batafsil ko''rish',2, 1, now()),
(213, 'BRANCH_CREATE',      'Filial yaratish', 'Yangi filial',              2, 1, now()),
(214, 'BRANCH_UPDATE',      'Filial tahrirlash','Filialni tahrirlash',      2, 1, now()),
(215, 'BRANCH_DELETE',      'Filial o''chirish','Filialni o''chirish',      2, 1, now()),

-- ---- ORG: Department (sub_group_id = 2) ----
(221, 'DEPARTMENT_VIEW',        'Bo''limlar',       'Bo''limlar ro''yxati',       2, 1, now()),
(222, 'DEPARTMENT_VIEW_DETAIL', 'Bo''lim detail',   'Bo''limni batafsil ko''rish',2, 1, now()),
(223, 'DEPARTMENT_CREATE',      'Bo''lim yaratish', 'Yangi bo''lim',              2, 1, now()),
(224, 'DEPARTMENT_UPDATE',      'Bo''lim tahrirlash','Bo''limni tahrirlash',      2, 1, now()),
(225, 'DEPARTMENT_DELETE',      'Bo''lim o''chirish','Bo''limni o''chirish',      2, 1, now()),

-- ---- ORG: Position (sub_group_id = 2) ----
(231, 'POSITION_VIEW',        'Lavozimlar',       'Lavozimlar ro''yxati',       2, 1, now()),
(232, 'POSITION_VIEW_DETAIL', 'Lavozim detail',   'Lavozimni batafsil ko''rish',2, 1, now()),
(233, 'POSITION_CREATE',      'Lavozim yaratish', 'Yangi lavozim',              2, 1, now()),
(234, 'POSITION_UPDATE',      'Lavozim tahrirlash','Lavozimni tahrirlash',      2, 1, now()),
(235, 'POSITION_DELETE',      'Lavozim o''chirish','Lavozimni o''chirish',      2, 1, now()),

-- ---- COUNTERPARTY: Card (sub_group_id = 3) ----
(301, 'COUNTERPARTY_CARD_VIEW',        'Kontragentlar',        'Kontragentlar ro''yxati',       3, 1, now()),
(302, 'COUNTERPARTY_CARD_VIEW_DETAIL', 'Kontragent detail',    'Kontragentni batafsil ko''rish',3, 1, now()),
(303, 'COUNTERPARTY_CARD_CREATE',      'Kontragent yaratish',  'Yangi kontragent',              3, 1, now()),
(304, 'COUNTERPARTY_CARD_UPDATE',      'Kontragent tahrirlash','Kontragentni tahrirlash',       3, 1, now()),
(305, 'COUNTERPARTY_CARD_DELETE',      'Kontragent o''chirish','Kontragentni o''chirish',       3, 1, now()),

-- ---- COUNTERPARTY: BankAccount (sub_group_id = 3) ----
(311, 'COUNTERPARTY_BANK_ACCOUNT_VIEW',        'Kontragent bank hisoblari',       'Ro''yxat',       3, 1, now()),
(312, 'COUNTERPARTY_BANK_ACCOUNT_VIEW_DETAIL', 'Kontragent bank hisobi detail',   'Batafsil',       3, 1, now()),
(313, 'COUNTERPARTY_BANK_ACCOUNT_CREATE',      'Kontragent bank hisobi yaratish', 'Yangi',          3, 1, now()),
(314, 'COUNTERPARTY_BANK_ACCOUNT_UPDATE',      'Kontragent bank hisobi tahrirlash','Tahrirlash',    3, 1, now()),
(315, 'COUNTERPARTY_BANK_ACCOUNT_DELETE',      'Kontragent bank hisobi o''chirish','O''chirish',    3, 1, now()),

-- ---- COUNTERPARTY: Contact (sub_group_id = 3) ----
(321, 'COUNTERPARTY_CONTACT_VIEW',        'Kontragent kontaktlari',       'Ro''yxat',   3, 1, now()),
(322, 'COUNTERPARTY_CONTACT_VIEW_DETAIL', 'Kontragent kontakt detail',    'Batafsil',   3, 1, now()),
(323, 'COUNTERPARTY_CONTACT_CREATE',      'Kontragent kontakt yaratish',  'Yangi',      3, 1, now()),
(324, 'COUNTERPARTY_CONTACT_UPDATE',      'Kontragent kontakt tahrirlash','Tahrirlash', 3, 1, now()),
(325, 'COUNTERPARTY_CONTACT_DELETE',      'Kontragent kontakt o''chirish','O''chirish', 3, 1, now()),

-- ---- INVENTORY: ProductGroup (sub_group_id = 4) ----
(401, 'PRODUCT_GROUP_VIEW',        'Tovar guruhlari',       'Ro''yxat',   4, 1, now()),
(402, 'PRODUCT_GROUP_VIEW_DETAIL', 'Tovar guruhi detail',   'Batafsil',   4, 1, now()),
(403, 'PRODUCT_GROUP_CREATE',      'Tovar guruhi yaratish', 'Yangi',      4, 1, now()),
(404, 'PRODUCT_GROUP_UPDATE',      'Tovar guruhi tahrirlash','Tahrirlash',4, 1, now()),
(405, 'PRODUCT_GROUP_DELETE',      'Tovar guruhi o''chirish','O''chirish',4, 1, now()),

-- ---- INVENTORY: Product (sub_group_id = 4) ----
(411, 'PRODUCT_VIEW',        'Tovarlar',        'Ro''yxat',   4, 1, now()),
(412, 'PRODUCT_VIEW_DETAIL', 'Tovar detail',    'Batafsil',   4, 1, now()),
(413, 'PRODUCT_CREATE',      'Tovar yaratish',  'Yangi',      4, 1, now()),
(414, 'PRODUCT_UPDATE',      'Tovar tahrirlash','Tahrirlash', 4, 1, now()),
(415, 'PRODUCT_DELETE',      'Tovar o''chirish','O''chirish', 4, 1, now()),

-- ---- INVENTORY: ProductPrice (sub_group_id = 4) ----
(421, 'PRODUCT_PRICE_VIEW',        'Tovar narxlari',       'Ro''yxat',   4, 1, now()),
(422, 'PRODUCT_PRICE_VIEW_DETAIL', 'Tovar narxi detail',   'Batafsil',   4, 1, now()),
(423, 'PRODUCT_PRICE_CREATE',      'Tovar narxi yaratish', 'Yangi',      4, 1, now()),
(424, 'PRODUCT_PRICE_UPDATE',      'Tovar narxi tahrirlash','Tahrirlash',4, 1, now()),
(425, 'PRODUCT_PRICE_DELETE',      'Tovar narxi o''chirish','O''chirish',4, 1, now()),

-- ---- INVENTORY: Warehouse (sub_group_id = 4) ----
(431, 'WAREHOUSE_VIEW',        'Omborlar',       'Ro''yxat',   4, 1, now()),
(432, 'WAREHOUSE_VIEW_DETAIL', 'Ombor detail',   'Batafsil',   4, 1, now()),
(433, 'WAREHOUSE_CREATE',      'Ombor yaratish', 'Yangi',      4, 1, now()),
(434, 'WAREHOUSE_UPDATE',      'Ombor tahrirlash','Tahrirlash',4, 1, now()),
(435, 'WAREHOUSE_DELETE',      'Ombor o''chirish','O''chirish',4, 1, now()),

-- ---- INVENTORY: ProductTable (sub_group_id = 4) ----
(441, 'PRODUCT_TABLE_VIEW',        'Tovar kartochkalari',       'Tovar kartochkalarini ko''rish',   4, 1, now()),
(442, 'PRODUCT_TABLE_VIEW_DETAIL', 'Tovar kartochkasi detail',  'Tovar kartochkasini batafsil',     4, 1, now()),

-- ---- BANK: OrgBankAccount (sub_group_id = 5) ----
(501, 'ORG_BANK_ACCOUNT_VIEW',        'Tashkilot bank hisoblari',       'Ro''yxat',   5, 1, now()),
(502, 'ORG_BANK_ACCOUNT_VIEW_DETAIL', 'Tashkilot bank hisobi detail',   'Batafsil',   5, 1, now()),
(503, 'ORG_BANK_ACCOUNT_CREATE',      'Tashkilot bank hisobi yaratish', 'Yangi',      5, 1, now()),
(504, 'ORG_BANK_ACCOUNT_UPDATE',      'Tashkilot bank hisobi tahrirlash','Tahrirlash',5, 1, now()),
(505, 'ORG_BANK_ACCOUNT_DELETE',      'Tashkilot bank hisobi o''chirish','O''chirish',5, 1, now()),

-- ---- BANK: BankOperation (sub_group_id = 5) ----
(511, 'BANK_OPERATION_VIEW',        'Bank operatsiyalari',       'Ro''yxat',   5, 1, now()),
(512, 'BANK_OPERATION_VIEW_DETAIL', 'Bank operatsiyasi detail',  'Batafsil',   5, 1, now()),
(513, 'BANK_OPERATION_CREATE',      'Bank operatsiyasi yaratish','Yangi',      5, 1, now()),
(514, 'BANK_OPERATION_UPDATE',      'Bank operatsiyasi tahrirlash','Tahrirlash',5,1, now()),
(515, 'BANK_OPERATION_DELETE',      'Bank operatsiyasi o''chirish','O''chirish',5,1, now()),

-- ---- BANK: BankStatementParser (sub_group_id = 5) ----
(521, 'BANK_STATEMENT_PARSE', 'Bank statement import', 'Bank Excel statement faylini JSON qilib parse qilish', 5, 1, now()),

-- ---- BANK: Bank dictionary (sub_group_id = 5) ----
(531, 'BANK_VIEW',        'Banklar',          'Banklar ro''yxati',        5, 1, now()),
(532, 'BANK_VIEW_DETAIL', 'Bank detail',      'Bankni batafsil ko''rish', 5, 1, now()),
(533, 'BANK_CREATE',      'Bank yaratish',    'Yangi bank qo''shish',     5, 1, now()),
(534, 'BANK_UPDATE',      'Bank tahrirlash',  'Bankni tahrirlash',        5, 1, now()),
(535, 'BANK_DELETE',      'Bank o''chirish',  'Bankni o''chirish',        5, 1, now()),

-- ---- CASH: CashBox (sub_group_id = 6) ----
(601, 'CASH_BOX_VIEW',        'Kassalar',       'Ro''yxat',   6, 1, now()),
(602, 'CASH_BOX_VIEW_DETAIL', 'Kassa detail',   'Batafsil',   6, 1, now()),
(603, 'CASH_BOX_CREATE',      'Kassa yaratish', 'Yangi',      6, 1, now()),
(604, 'CASH_BOX_UPDATE',      'Kassa tahrirlash','Tahrirlash',6, 1, now()),
(605, 'CASH_BOX_DELETE',      'Kassa o''chirish','O''chirish',6, 1, now()),

-- ---- CASH: CashOperation (sub_group_id = 6) ----
(611, 'CASH_OPERATION_VIEW',        'Kassa operatsiyalari',       'Ro''yxat',   6, 1, now()),
(612, 'CASH_OPERATION_VIEW_DETAIL', 'Kassa operatsiyasi detail',  'Batafsil',   6, 1, now()),
(613, 'CASH_OPERATION_CREATE',      'Kassa operatsiyasi yaratish','Yangi',      6, 1, now()),
(614, 'CASH_OPERATION_UPDATE',      'Kassa operatsiyasi tahrirlash','Tahrirlash',6,1,now()),
(615, 'CASH_OPERATION_DELETE',      'Kassa operatsiyasi o''chirish','O''chirish',6,1,now()),

-- ---- PURCHASE: PurchaseDoc (sub_group_id = 7) ----
(701, 'PURCHASE_DOC_VIEW',        'Xarid hujjatlari',       'Ro''yxat',   7, 1, now()),
(702, 'PURCHASE_DOC_VIEW_DETAIL', 'Xarid hujjati detail',   'Batafsil',   7, 1, now()),
(703, 'PURCHASE_DOC_CREATE',      'Xarid hujjati yaratish', 'Yangi',      7, 1, now()),
(704, 'PURCHASE_DOC_UPDATE',      'Xarid hujjati tahrirlash','Tahrirlash',7, 1, now()),
(705, 'PURCHASE_DOC_DELETE',      'Xarid hujjati o''chirish','O''chirish',7, 1, now()),

-- ---- PURCHASE: PurchaseDocTable (sub_group_id = 7) ----
(711, 'PURCHASE_DOC_TABLE_VIEW',        'Xarid satrlari',       'Ro''yxat',   7, 1, now()),
(712, 'PURCHASE_DOC_TABLE_VIEW_DETAIL', 'Xarid satri detail',   'Batafsil',   7, 1, now()),
(713, 'PURCHASE_DOC_TABLE_CREATE',      'Xarid satri yaratish', 'Yangi',      7, 1, now()),
(714, 'PURCHASE_DOC_TABLE_UPDATE',      'Xarid satri tahrirlash','Tahrirlash',7, 1, now()),
(715, 'PURCHASE_DOC_TABLE_DELETE',      'Xarid satri o''chirish','O''chirish',7, 1, now()),

-- ---- PURCHASE: Contract (sub_group_id = 7) ----
(721, 'CONTRACT_VIEW',        'Shartnomalar',          'Shartnomalar ro''yxati',        7, 1, now()),
(722, 'CONTRACT_VIEW_DETAIL', 'Shartnoma detail',      'Shartnomani batafsil ko''rish', 7, 1, now()),
(723, 'CONTRACT_CREATE',      'Shartnoma yaratish',    'Yangi shartnoma qo''shish',     7, 1, now()),
(724, 'CONTRACT_UPDATE',      'Shartnoma tahrirlash',  'Shartnomani tahrirlash',        7, 1, now()),
(725, 'CONTRACT_DELETE',      'Shartnoma o''chirish',  'Shartnomani o''chirish',        7, 1, now()),

-- ---- PURCHASE: PurchaseService (sub_group_id = 7) ----
(731, 'PURCHASE_SERVICE_VIEW',        'Xarid xizmatlari',       'Ro''yxat',   7, 1, now()),
(732, 'PURCHASE_SERVICE_VIEW_DETAIL', 'Xarid xizmati detail',   'Batafsil',   7, 1, now()),
(733, 'PURCHASE_SERVICE_CREATE',      'Xarid xizmati yaratish', 'Yangi',      7, 1, now()),
(734, 'PURCHASE_SERVICE_UPDATE',      'Xarid xizmati tahrirlash','Tahrirlash',7, 1, now()),
(735, 'PURCHASE_SERVICE_DELETE',      'Xarid xizmati o''chirish','O''chirish',7, 1, now()),

-- ---- SALE: SaleDoc (sub_group_id = 8) ----
(801, 'SALE_DOC_VIEW',        'Sotuv hujjatlari',       'Ro''yxat',   8, 1, now()),
(802, 'SALE_DOC_VIEW_DETAIL', 'Sotuv hujjati detail',   'Batafsil',   8, 1, now()),
(803, 'SALE_DOC_CREATE',      'Sotuv hujjati yaratish', 'Yangi',      8, 1, now()),
(804, 'SALE_DOC_UPDATE',      'Sotuv hujjati tahrirlash','Tahrirlash',8, 1, now()),
(805, 'SALE_DOC_DELETE',      'Sotuv hujjati o''chirish','O''chirish',8, 1, now()),

-- ---- SALE: SaleDocTable (sub_group_id = 8) ----
(811, 'SALE_DOC_TABLE_VIEW',        'Sotuv satrlari',       'Ro''yxat',   8, 1, now()),
(812, 'SALE_DOC_TABLE_VIEW_DETAIL', 'Sotuv satri detail',   'Batafsil',   8, 1, now()),
(813, 'SALE_DOC_TABLE_CREATE',      'Sotuv satri yaratish', 'Yangi',      8, 1, now()),
(814, 'SALE_DOC_TABLE_UPDATE',      'Sotuv satri tahrirlash','Tahrirlash',8, 1, now()),
(815, 'SALE_DOC_TABLE_DELETE',      'Sotuv satri o''chirish','O''chirish',8, 1, now()),

-- ---- ACCOUNTING: ChartAccount (sub_group_id = 9) ----
(901, 'CHART_ACCOUNT_VIEW',        'Hisoblar rejasi',       'Ro''yxat',   9, 1, now()),
(902, 'CHART_ACCOUNT_VIEW_DETAIL', 'Hisoblar rejasi detail','Batafsil',   9, 1, now()),
(903, 'CHART_ACCOUNT_CREATE',      'Hisob yaratish',        'Yangi',      9, 1, now()),
(904, 'CHART_ACCOUNT_UPDATE',      'Hisob tahrirlash',      'Tahrirlash', 9, 1, now()),
(905, 'CHART_ACCOUNT_DELETE',      'Hisob o''chirish',      'O''chirish', 9, 1, now()),

-- ---- ACCOUNTING: AccRegEntry (sub_group_id = 9) ----
(911, 'ACC_REG_ENTRY_VIEW',        'Buxg. yozuvlar',       'Ro''yxat',   9, 1, now()),
(912, 'ACC_REG_ENTRY_VIEW_DETAIL', 'Buxg. yozuv detail',  'Batafsil',   9, 1, now()),
(913, 'ACC_REG_ENTRY_CREATE',      'Buxg. yozuv yaratish','Yangi',      9, 1, now()),
(914, 'ACC_REG_ENTRY_UPDATE',      'Buxg. yozuv tahrirlash','Tahrirlash',9,1, now()),
(915, 'ACC_REG_ENTRY_DELETE',      'Buxg. yozuv o''chirish','O''chirish',9,1, now()),

-- ---- REGISTER: CounterpartyRegBalance (sub_group_id = 10) ----
(1001, 'COUNTERPARTY_REG_BALANCE_VIEW',        'Kontragent qoldiqlari',       'Ro''yxat',   10, 1, now()),
(1002, 'COUNTERPARTY_REG_BALANCE_VIEW_DETAIL', 'Kontragent qoldig''i detail', 'Batafsil',   10, 1, now()),
(1003, 'COUNTERPARTY_REG_BALANCE_CREATE',      'Kontragent qoldig''i yaratish','Yangi',     10, 1, now()),
(1004, 'COUNTERPARTY_REG_BALANCE_UPDATE',      'Kontragent qoldig''i tahrirlash','Tahrirlash',10,1,now()),
(1005, 'COUNTERPARTY_REG_BALANCE_DELETE',      'Kontragent qoldig''i o''chirish','O''chirish',10,1,now()),

-- ---- REGISTER: InventoryRegBalance (sub_group_id = 10) ----
(1011, 'INVENTORY_REG_BALANCE_VIEW',        'Inventar qoldiqlari',       'Ro''yxat',   10, 1, now()),
(1012, 'INVENTORY_REG_BALANCE_VIEW_DETAIL', 'Inventar qoldig''i detail', 'Batafsil',   10, 1, now()),
(1013, 'INVENTORY_REG_BALANCE_CREATE',      'Inventar qoldig''i yaratish','Yangi',     10, 1, now()),
(1014, 'INVENTORY_REG_BALANCE_UPDATE',      'Inventar qoldig''i tahrirlash','Tahrirlash',10,1,now()),
(1015, 'INVENTORY_REG_BALANCE_DELETE',      'Inventar qoldig''i o''chirish','O''chirish',10,1,now()),

-- ---- REGISTER: MoneyRegBalance (sub_group_id = 10) ----
(1021, 'MONEY_REG_BALANCE_VIEW',        'Pul qoldiqlari',       'Ro''yxat',   10, 1, now()),
(1022, 'MONEY_REG_BALANCE_VIEW_DETAIL', 'Pul qoldig''i detail', 'Batafsil',   10, 1, now()),
(1023, 'MONEY_REG_BALANCE_CREATE',      'Pul qoldig''i yaratish','Yangi',     10, 1, now()),
(1024, 'MONEY_REG_BALANCE_UPDATE',      'Pul qoldig''i tahrirlash','Tahrirlash',10,1,now()),
(1025, 'MONEY_REG_BALANCE_DELETE',      'Pul qoldig''i o''chirish','O''chirish',10,1,now()),

-- ---- MANUAL: Ma'lumotnoma (sub_group_id = 11) ----
(1101, 'MANUAL_VIEW', 'Ma''lumotnoma', 'Ma''lumotnoma ma''lumotlarini ko''rish', 11, 1, now())

ON CONFLICT (id) DO NOTHING;
