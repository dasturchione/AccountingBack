create table acc_document_account_role
(
	id					smallserial not null primary key,
	code				varchar(50) not null unique,
	name				varchar(250) not null,
	description			varchar(500),
	state_id			smallint not null references cmn_state(id)
);

BEGIN;

INSERT INTO acc_document_account_role
    (id, code, name, description, state_id)
VALUES
    (1, 'supplier_settlement', 'Yetkazib beruvchi hisobi', 'Xarid hujjatida yetkazib beruvchi qarzi uchun ishlatiladigan hisob roli.', 1),
    (2, 'purchase_debit', 'Xarid debet hisobi', 'Xarid qilingan tovar, material, xizmat yoki xarajat uchun debet hisob roli.', 1),
    (3, 'purchase_vat', 'Xarid QQS hisobi', 'Xarid bo‘yicha QQS summasi uchun hisob roli.', 1),

    (4, 'customer_settlement', 'Xaridor hisobi', 'Sotuv hujjatida xaridor qarzi uchun ishlatiladigan hisob roli.', 1),
    (5, 'sale_vat', 'Sotuv QQS hisobi', 'Sotuv bo‘yicha QQS majburiyati uchun hisob roli.', 1),
    (6, 'sale_inventory', 'Sotuv tovar/material hisobi', 'Sotuvda tovar yoki materialni ombordan chiqarish uchun hisob roli.', 1),
    (7, 'sale_income', 'Sotuv daromad hisobi', 'Tovar, ish yoki xizmat sotishdan daromad uchun hisob roli.', 1),
    (8, 'sale_cost', 'Sotuv tannarx hisobi', 'Sotilgan tovar, ish yoki xizmat tannarxi uchun hisob roli.', 1),

    (9, 'bank_account', 'Bank hisobi', 'Bank operatsiyasida asosiy bank buxgalteriya hisobi roli.', 1),
    (10, 'cash_account', 'Kassa hisobi', 'Kassa operatsiyasida asosiy kassa buxgalteriya hisobi roli.', 1),
    (11, 'offset_account', 'Qarshi hisob', 'Bank yoki kassa operatsiyasida ikkinchi tomon hisob roli.', 1);

SELECT setval(
    pg_get_serial_sequence('acc_document_account_role', 'id'),
    (SELECT MAX(id) FROM acc_document_account_role)
);

COMMIT;