create table acc_document_account_role_translation
(
	document_account_role_id	smallint not null references acc_document_account_role(id),
	language_id					smallint not null references cmn_language(id),
	name						varchar(250) not null,
	description					varchar(500),

	primary key (document_account_role_id, language_id)
);

BEGIN;

INSERT INTO acc_document_account_role_translation
    (language_id, document_account_role_id, name, description)
VALUES
    -- uz
    (1, 1, 'Yetkazib beruvchi hisobi', 'Xarid hujjatida yetkazib beruvchi qarzi uchun ishlatiladigan hisob roli.'),
    (1, 2, 'Xarid debet hisobi', 'Xarid qilingan tovar, material, xizmat yoki xarajat uchun debet hisob roli.'),
    (1, 3, 'Xarid QQS hisobi', 'Xarid bo‘yicha QQS summasi uchun hisob roli.'),

    (1, 4, 'Xaridor hisobi', 'Sotuv hujjatida xaridor qarzi uchun ishlatiladigan hisob roli.'),
    (1, 5, 'Sotuv QQS hisobi', 'Sotuv bo‘yicha QQS majburiyati uchun hisob roli.'),
    (1, 6, 'Sotuv tovar/material hisobi', 'Sotuvda tovar yoki materialni ombordan chiqarish uchun hisob roli.'),
    (1, 7, 'Sotuv daromad hisobi', 'Tovar, ish yoki xizmat sotishdan daromad uchun hisob roli.'),
    (1, 8, 'Sotuv tannarx hisobi', 'Sotilgan tovar, ish yoki xizmat tannarxi uchun hisob roli.'),

    (1, 9, 'Bank hisobi', 'Bank operatsiyasida asosiy bank buxgalteriya hisobi roli.'),
    (1, 10, 'Kassa hisobi', 'Kassa operatsiyasida asosiy kassa buxgalteriya hisobi roli.'),
    (1, 11, 'Qarshi hisob', 'Bank yoki kassa operatsiyasida ikkinchi tomon hisob roli.'),

    -- ru
    (2, 1, 'Счет поставщика', 'Роль счета для задолженности перед поставщиком в документах закупки.'),
    (2, 2, 'Дебетовый счет закупки', 'Роль дебетового счета для товара, материала, услуги или расхода при закупке.'),
    (2, 3, 'НДС по закупке', 'Роль счета для НДС по закупке.'),

    (2, 4, 'Счет покупателя', 'Роль счета для задолженности покупателя в документах продажи.'),
    (2, 5, 'НДС по продаже', 'Роль счета для обязательства по НДС при продаже.'),
    (2, 6, 'Счет товара/материала при продаже', 'Роль счета для списания товара или материала со склада при продаже.'),
    (2, 7, 'Счет дохода от продажи', 'Роль счета для дохода от продажи товара, работы или услуги.'),
    (2, 8, 'Счет себестоимости продажи', 'Роль счета для себестоимости проданного товара, работы или услуги.'),

    (2, 9, 'Банковский счет', 'Роль основного банковского бухгалтерского счета в банковской операции.'),
    (2, 10, 'Кассовый счет', 'Роль основного кассового бухгалтерского счета в кассовой операции.'),
    (2, 11, 'Корреспондирующий счет', 'Роль второго счета в банковской или кассовой операции.'),

    -- en
    (3, 1, 'Supplier account', 'Account role for supplier liabilities in purchase documents.'),
    (3, 2, 'Purchase debit account', 'Debit account role for goods, materials, services or expenses in purchases.'),
    (3, 3, 'Purchase VAT account', 'Account role for VAT on purchases.'),

    (3, 4, 'Customer account', 'Account role for customer receivables in sales documents.'),
    (3, 5, 'Sales VAT account', 'Account role for VAT liability on sales.'),
    (3, 6, 'Sales inventory/material account', 'Account role for issuing goods or materials from inventory during sales.'),
    (3, 7, 'Sales income account', 'Account role for revenue from goods, works or services.'),
    (3, 8, 'Sales cost account', 'Account role for cost of sold goods, works or services.'),

    (3, 9, 'Bank account', 'Main bank accounting account role in bank operations.'),
    (3, 10, 'Cash account', 'Main cash accounting account role in cash operations.'),
    (3, 11, 'Offset account', 'Opposite account role in bank or cash operations.');

COMMIT;
