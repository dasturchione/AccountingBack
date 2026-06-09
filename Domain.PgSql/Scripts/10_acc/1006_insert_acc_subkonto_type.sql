insert into acc_subkonto_type (id, code, name, source_table, state_id)
values
(1, 'product', 'Tovar / xizmat', 'inv_product', 1),
(2, 'warehouse', 'Ombor', 'inv_warehouse', 1),
(3, 'counterparty', 'Kontragent', 'counterparty_card', 1),
(4, 'bank_account', 'Bank hisobi', 'org_bank_account', 1),
(5, 'cash_box', 'Kassa', 'cash_box', 1),
(6, 'employee', 'Xodim', 'sys_user', 1),
(7, 'document', 'Hujjat', 'document', 1),
(8, 'tax', 'Soliq', 'cmn_tax_type', 1)
on conflict do nothing;
