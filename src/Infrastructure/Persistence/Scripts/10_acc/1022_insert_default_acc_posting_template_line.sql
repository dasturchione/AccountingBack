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

-- Приход денег (OPERATION_DEBIT): DT PaymentAccount | CT <alias корреспондента>
insert into acc_posting_template_line
	(template_id, order_number, debit_alias, credit_alias, amount_source, is_optional)
select id, 1, 'PaymentAccount', 'Customer',         'Total', true from acc_posting_template where code = 'DEBIT_OPERATION'
	union all
select id, 1, 'PaymentAccount', 'CustomerAdvance',  'Total', true from acc_posting_template where code = 'DEBIT_OPERATION'
	union all
select id, 1, 'PaymentAccount', 'LoanReceived',     'Total', true from acc_posting_template where code = 'DEBIT_OPERATION'
	union all
select id, 1, 'PaymentAccount', 'EmployeeAdvance',  'Total', true from acc_posting_template where code = 'DEBIT_OPERATION'; -- возврат подотчётных остатков

-- Расход денег (CREDIT_OPERATION): DT <alias корреспондента> | CT PaymentAccount
insert into acc_posting_template_line
	(template_id, order_number, debit_alias, credit_alias, amount_source, is_optional)
select id, 1, 'Supplier',         'PaymentAccount', 'Total', true from acc_posting_template where code = 'CREDIT_OPERATION'
	union all
select id, 1, 'SupplierAdvance',  'PaymentAccount', 'Total', true from acc_posting_template where code = 'CREDIT_OPERATION'
	union all
select id, 1, 'Employee',         'PaymentAccount', 'Total', true from acc_posting_template where code = 'CREDIT_OPERATION'
	union all
select id, 1, 'EmployeeAdvance',  'PaymentAccount', 'Total', true from acc_posting_template where code = 'CREDIT_OPERATION'
	union all
select id, 1, 'Founder',          'PaymentAccount', 'Total', true from acc_posting_template where code = 'CREDIT_OPERATION'
	union all
select id, 1, 'TaxAuthority',     'PaymentAccount', 'Total', true from acc_posting_template where code = 'CREDIT_OPERATION'
	union all
select id, 1, 'LoanGiven',        'PaymentAccount', 'Total', true from acc_posting_template where code = 'CREDIT_OPERATION';