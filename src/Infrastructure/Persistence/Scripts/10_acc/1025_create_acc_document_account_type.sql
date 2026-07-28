create table acc_document_account_type 
(
	id						smallserial not null primary key,
	code					varchar(50) not null unique,
	name					varchar(250) not null,
	description				varchar(500),
	state_id				smallint not null references cmn_state(id)
);

BEGIN;

INSERT INTO acc_document_account_type
    (id, code, name, description, state_id)
VALUES
    (1, 'purchase_goods', 'Tovar xaridi', 'Tovarlarni xarid qilish hujjati.', 1),
    (2, 'purchase_service', 'Xizmat xaridi', 'Xizmatlarni xarid qilish hujjati.', 1),

    (3, 'sale_goods', 'Tovar realizatsiyasi', 'Tovarlarni sotish hujjati.', 1),
    (4, 'sale_service', 'Xizmat ko‘rsatish', 'Xizmatlarni sotish yoki ko‘rsatish hujjati.', 1),

    (5, 'bank_income', 'Bank kirim', 'Bank hisobvarag‘iga pul kelib tushishi.', 1),
    (6, 'bank_expense', 'Bank chiqim', 'Bank hisobvarag‘idan pul chiqishi.', 1),

    (7, 'cash_income', 'Kassa kirim', 'Kassaga naqd pul kirimi.', 1),
    (8, 'cash_expense', 'Kassa chiqim', 'Kassadan naqd pul chiqimi.', 1);

SELECT setval(
    pg_get_serial_sequence('acc_document_account_type', 'id'),
    (SELECT MAX(id) FROM acc_document_account_type)
);

COMMIT;