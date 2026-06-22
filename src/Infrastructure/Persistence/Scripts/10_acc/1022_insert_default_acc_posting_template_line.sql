-- Tovar (поступление)
insert into acc_posting_template_line 
	(template_id, order_number, debit_alias, credit_alias, amount_source, is_optional)
select id, 1, 'Inventory', 'Supplier', 'Base', false from acc_posting_template where code = 'PURCHASE_GOODS'
	union all
select id, 2, 'VATIn',     'Supplier', 'VAT',   false from acc_posting_template where code = 'PURCHASE_GOODS';

-- Xizmat (получение)
insert into acc_posting_template_line 
	(template_id, order_number, debit_alias, credit_alias, amount_source, is_optional)
select id, 1, 'Expense', 'Supplier', 'Base', false from acc_posting_template where code = 'PURCHASE_SERVICE'
	union all
select id, 2, 'VATIn',   'Supplier', 'VAT',   false from acc_posting_template where code = 'PURCHASE_SERVICE';

-- Realizatsiya tovar
insert into acc_posting_template_line 
	(template_id, order_number, debit_alias, credit_alias, amount_source, is_optional)
select id, 1, 'Customer',    'SalesRevenue', 'Base', false from acc_posting_template where code = 'SALE_GOODS'
	union all
select id, 2, 'Customer',    'VATOut',       'VAT',   false from acc_posting_template where code = 'SALE_GOODS'
	union all
select id, 3, 'CostOfGoods', 'Inventory',    'Cost',  false from acc_posting_template where code = 'SALE_GOODS';

-- Ko'rsatilgan xizmat (+ опциональное списание своего товара/ОС)
insert into acc_posting_template_line 
	(template_id, order_number, debit_alias, credit_alias, amount_source, is_optional)
select id, 1, 'Customer',      'ServiceRevenue', 'Base', false from acc_posting_template where code = 'SALE_SERVICE'
	union all
select id, 2, 'Customer',      'VATOut',         'VAT',   false from acc_posting_template where code = 'SALE_SERVICE'
	union all
select id, 3, 'CostOfService', 'AssetWriteOff',  'Cost',  true  from acc_posting_template where code = 'SALE_SERVICE';

-- Mijozdan pul keldi
insert into acc_posting_template_line 
	(template_id, order_number, debit_alias, credit_alias, amount_source, is_optional)
select id, 1, 'PaymentAccount', 'CustomerAdvance', 'Total', false from acc_posting_template where code = 'CUSTOMER_PAYMENT_ADVANCE'
	union all
select id, 1, 'PaymentAccount', 'Customer',        'Total', false from acc_posting_template where code = 'CUSTOMER_PAYMENT';

-- Ta'minotchiga pul berildi
insert into acc_posting_template_line 
	(template_id, order_number, debit_alias, credit_alias, amount_source, is_optional)
select id, 1, 'SupplierAdvance', 'PaymentAccount', 'Total', false from acc_posting_template where code = 'SUPPLIER_PAYMENT_ADVANCE'
	union all
select id, 1, 'Supplier',        'PaymentAccount', 'Total', false from acc_posting_template where code = 'SUPPLIER_PAYMENT';