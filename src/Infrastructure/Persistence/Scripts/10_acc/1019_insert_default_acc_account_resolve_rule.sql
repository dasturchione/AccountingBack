-- Inventory: товар (фиксированный счёт 2910, без категорий — пока единственный вариант)
insert into acc_account_resolve_rule 
	(policy_id, alias, dimension_key, dimension_value, account_id, priority)
select 1, 'Inventory', 'category', '_default', id, 100 from acc_chart_account where code = '2910';

-- Supplier / Customer / их авансы
insert into acc_account_resolve_rule 
	(policy_id, alias, dimension_key, dimension_value, account_id, priority)
select 1, 'Supplier',         '_none', '_default', id, 100 from acc_chart_account where code = '6010'
	union all
select 1, 'Customer',         '_none', '_default', id, 100 from acc_chart_account where code = '4010'
	union all
select 1, 'CustomerAdvance',  '_none', '_default', id, 100 from acc_chart_account where code = '6310'
	union all
select 1, 'SupplierAdvance',  '_none', '_default', id, 100 from acc_chart_account where code = '4310';

insert into acc_account_resolve_rule 
	(policy_id, alias, dimension_key, dimension_value, account_id, priority)
select 1, 'PaymentAccount', 'paymentMethod', 'bank',    id, 10  from acc_chart_account where code = '5110'
	union all
select 1, 'PaymentAccount', 'paymentMethod', 'cash',    id, 10  from acc_chart_account where code = '5010'
	union all
select 1, 'PaymentAccount', 'paymentMethod', '_default', id, 100 from acc_chart_account where code = '5110';;

-- VAT
insert into acc_account_resolve_rule 
	(policy_id, alias, dimension_key, dimension_value, account_id, priority)
select 1, 'VATIn',  '_none', '_default', id, 100 from acc_chart_account where code = '4410'
	union all
select 1, 'VATOut', '_none', '_default', id, 100 from acc_chart_account where code = '6410';

-- Expense: вид услуги -> 2010 / 9420 / 9430
insert into acc_account_resolve_rule 
	(policy_id, alias, dimension_key, dimension_value, account_id, priority)
select 1, 'Expense', 'serviceType', 'production', id, 10  from acc_chart_account where code = '2010'
	union all
select 1, 'Expense', 'serviceType', 'admin',      id, 10  from acc_chart_account where code = '9420'
	union all
select 1, 'Expense', 'serviceType', '_default',   id, 100 from acc_chart_account where code = '9430';

-- Revenue / CostOfGoods / CostOfService
insert into acc_account_resolve_rule 
	(policy_id, alias, dimension_key, dimension_value, account_id, priority)
select 1, 'SalesRevenue',   '_none', '_default', id, 100 from acc_chart_account where code = '9020'
	union all
select 1, 'CostOfGoods',    '_none', '_default', id, 100 from acc_chart_account where code = '9120'
	union all
select 1, 'ServiceRevenue', '_none', '_default', id, 100 from acc_chart_account where code = '9030'
	union all
select 1, 'CostOfService',  '_none', '_default', id, 100 from acc_chart_account where code = '9130';


-- Работает только для случая "списали именно товар" (1010-1090 обрабатывается в коде отдельно)
insert into acc_account_resolve_rule 
	(policy_id, alias, dimension_key, dimension_value, account_id, priority)
select 1, 'AssetWriteOff', 'assetType', 'inventory', id, 10  from acc_chart_account where code = '2910'
	union all
select 1, 'AssetWriteOff', 'assetType', '_default',  id, 100 from acc_chart_account where code = '2910';