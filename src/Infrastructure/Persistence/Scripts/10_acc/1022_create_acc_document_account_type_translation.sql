create table acc_document_account_type_translation
(
	language_id					smallint not null references cmn_language(id),
	document_account_type_id	smallint not null references acc_document_account_type(id),
	name						varchar(250) not null,
	description					varchar(500),

	primary key (language_id, document_account_type_id)
);

BEGIN;

INSERT INTO acc_document_account_type_translation
    (language_id, document_account_type_id, name, description)
VALUES
    -- uz
    (1, 1, 'Tovar xaridi', 'Tovarlarni xarid qilish hujjati.'),
    (1, 2, 'Xizmat xaridi', 'Xizmatlarni xarid qilish hujjati.'),
    (1, 3, 'Tovar realizatsiyasi', 'Tovarlarni sotish hujjati.'),
    (1, 4, 'Xizmat ko‘rsatish', 'Xizmatlarni sotish yoki ko‘rsatish hujjati.'),
    (1, 5, 'Bank kirim', 'Bank hisobvarag‘iga pul kelib tushishi.'),
    (1, 6, 'Bank chiqim', 'Bank hisobvarag‘idan pul chiqishi.'),
    (1, 7, 'Kassa kirim', 'Kassaga naqd pul kirimi.'),
    (1, 8, 'Kassa chiqim', 'Kassadan naqd pul chiqimi.'),

    -- ru
    (2, 1, 'Закупка товара', 'Документ закупки товаров.'),
    (2, 2, 'Закупка услуги', 'Документ закупки услуг.'),
    (2, 3, 'Реализация товара', 'Документ продажи товаров.'),
    (2, 4, 'Оказание услуг', 'Документ продажи или оказания услуг.'),
    (2, 5, 'Банковский приход', 'Поступление денежных средств на банковский счет.'),
    (2, 6, 'Банковский расход', 'Списание денежных средств с банковского счета.'),
    (2, 7, 'Кассовый приход', 'Поступление наличных денежных средств в кассу.'),
    (2, 8, 'Кассовый расход', 'Выдача наличных денежных средств из кассы.'),

    -- en
    (3, 1, 'Purchase of goods', 'Document for purchasing goods.'),
    (3, 2, 'Purchase of services', 'Document for purchasing services.'),
    (3, 3, 'Sale of goods', 'Document for selling goods.'),
    (3, 4, 'Sale of services', 'Document for selling or providing services.'),
    (3, 5, 'Bank income', 'Receipt of funds to a bank account.'),
    (3, 6, 'Bank expense', 'Withdrawal of funds from a bank account.'),
    (3, 7, 'Cash income', 'Receipt of cash into cashbox.'),
    (3, 8, 'Cash expense', 'Cash withdrawal from cashbox.');

COMMIT;
