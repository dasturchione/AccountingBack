create table acc_document_account_type_role
(
	id							serial primary key,
	document_account_type_id	smallint not null references acc_document_account_type(id),
	document_account_role_id	smallint not null references acc_document_account_role(id),

	account_side				varchar(10) not null,

	is_required					boolean not null default true,
	sort_order					int not null default 1,

	constraint chk_acc_document_type_account_role_side check (account_side in ('debit', 'credit')),

	unique (document_account_type_id, document_account_role_id)
);

BEGIN;

INSERT INTO acc_document_account_type_role
    (
        id,
        document_account_type_id,
        document_account_role_id,
        account_side,
        is_required,
        sort_order
    )
VALUES
    -- 1. purchase_goods / Tovar xaridi
    -- DT 2910/1010... | CT 6010
    -- DT 4410         | CT 6010
    (1, 1, 1, 'credit', true,  1), -- supplier_settlement
    (2, 1, 2, 'debit',  true,  2), -- purchase_debit
    (3, 1, 3, 'debit',  false, 3), -- purchase_vat

    -- 2. purchase_service / Xizmat xaridi
    -- DT 2010/9420/9430 | CT 6010
    -- DT 4410           | CT 6010
    (4, 2, 1, 'credit', true,  1), -- supplier_settlement
    (5, 2, 2, 'debit',  true,  2), -- purchase_debit
    (6, 2, 3, 'debit',  false, 3), -- purchase_vat

    -- 3. sale_goods / Tovar realizatsiyasi
    -- DT 4010 | CT 9020
    -- DT 4010 | CT 6410
    -- DT 9120 | CT 2910
    (7,  3, 4, 'debit',  true,  1), -- customer_settlement
    (8,  3, 7, 'credit', true,  2), -- sale_income
    (9,  3, 5, 'credit', false, 3), -- sale_vat
    (10, 3, 8, 'debit',  true,  4), -- sale_cost
    (11, 3, 6, 'credit', true,  5), -- sale_inventory

    -- 4. sale_service / Xizmat ko‘rsatish
    -- DT 4010 | CT 9030
    -- DT 4010 | CT 6410
    -- optional: DT 9130 | CT 2910/1010-1090
    (12, 4, 4, 'debit',  true,  1), -- customer_settlement
    (13, 4, 7, 'credit', true,  2), -- sale_income
    (14, 4, 5, 'credit', false, 3), -- sale_vat
    (15, 4, 8, 'debit',  false, 4), -- sale_cost
    (16, 4, 6, 'credit', false, 5), -- sale_inventory

    -- 5. bank_income / Bank kirim
    -- DT 5110 | CT 4010/6310/...
    (17, 5, 9,  'debit',  true, 1), -- bank_account
    (18, 5, 11, 'credit', true, 2), -- offset_account

    -- 6. bank_expense / Bank chiqim
    -- DT 6010/4310/6410/9420/... | CT 5110
    (19, 6, 9,  'credit', true, 1), -- bank_account
    (20, 6, 11, 'debit',  true, 2), -- offset_account

    -- 7. cash_income / Kassa kirim
    -- DT 5010 | CT 4010/6310/...
    (21, 7, 10, 'debit',  true, 1), -- cash_account
    (22, 7, 11, 'credit', true, 2), -- offset_account

    -- 8. cash_expense / Kassa chiqim
    -- DT 6010/4310/6410/9420/... | CT 5010
    (23, 8, 10, 'credit', true, 1), -- cash_account
    (24, 8, 11, 'debit',  true, 2); -- offset_account

SELECT setval(
    pg_get_serial_sequence('acc_document_account_type_role', 'id'),
    (SELECT MAX(id) FROM acc_document_account_type_role)
);

COMMIT;