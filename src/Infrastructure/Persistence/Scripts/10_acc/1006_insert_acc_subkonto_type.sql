insert into acc_subkonto_type 
	(id, code, name, source_table, state_id)
values
	(1,  'product',			'Tovar / xizmat',		'inv_product',			1),
	(2,  'warehouse',		'Ombor',				'inv_warehouse',		1),
	(3,  'counterparty',	'Kontragent',			'counterparty_card',	1),
	(4,  'bank_account',	'Bank hisobi',			'org_bank_account',		1),
	(5,  'cash_box',		'Kassa',				'cash_box',				1),
	(6,  'employee',		'Xodim',				'sys_user',				1),
	(7,  'tax',				'Soliq',				'cmn_tax_type',			1), 
	(8,  'bank_operation',	'Bank operatsiyasi',	'bank_operation',		1),
	(9,  'contract',		'Shartnoma',			'cmn_contract',			1),
	(10, 'puchase',			'Xarid',				'pur_doc',				1),
	(11, 'sale',			'Sotuv',				'sale_doc',				1)
on conflict do nothing;
