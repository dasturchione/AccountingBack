insert into cmn_product_table_status 
	(id, code, name, state_id) 
values
	(1, 'IN_STOCK',					'На складе',                  1),
	(2, 'RESERVED',					'Зарезервирован',             1),
	(3, 'SOLD',						'Продан',                     1),
	(4, 'RETURNED_TO_SUPPLIER',		'Возвращен поставщику',       1),
	(5, 'RETURNED_FROM_CUSTOMER',	'Возвращен покупателем',      1),
	(6, 'WRITTEN_OFF',				'Списан',                     1),
	(7, 'LOST',						'Утерян',                     1),
	(8, 'BLOCKED',					'Заблокирован',               1);