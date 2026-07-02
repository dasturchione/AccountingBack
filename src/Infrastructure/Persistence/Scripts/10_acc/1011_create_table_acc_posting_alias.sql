create table acc_posting_alias
(
	id						smallserial primary key,
	code					varchar(50) not null unique,			-- 'Inventory', 'Supplier', 'PaymentAccount', 'Employee' и т.д.
	name					varchar(250) not null					-- человекочитаемое: 'Товар на складе', 'Поставщик'
);

create table acc_posting_alias_translation
(
	posting_alias_id		smallint not null references acc_posting_alias(id),
	language_id				smallint not null references cmn_language(id),					
	name					varchar(250) not null,							-- человекочитаемое: 'товар на складе', 'поставщик'

	primary key (posting_alias_id, language_id)
);

insert into acc_posting_alias (code, name) values
	('Inventory',        'Tovar ombor qoldig''i'),
	('Expense',           'Xarajat (xizmat)'),
	('Supplier',          'Yetkazib beruvchi — qarzni yopish'),
	('SupplierAdvance',   'Yetkazib beruvchiga avans'),
	('Customer',          'Xaridor — qarzni yopish'),
	('CustomerAdvance',   'Xaridordan avans'),
	('SalesRevenue',      'Tovarlar realizatsiyasidan daromad'),
	('ServiceRevenue',    'Xizmatlardan daromad'),
	('VATIn',             'Hisobga olinadigan QQS'),
	('VATOut',            'To''lanadigan QQS'),
	('CostOfGoods',       'Sotilgan tovarlar tannarxi'),
	('CostOfService',     'Ko''rsatilgan xizmatlar tannarxi'),
	('AssetWriteOff',     'Aktivni hisobdan chiqarish (tovar/OS)'),
	('PaymentAccount',    'Pul hisobvarag''i (bank/kassa)'),
	('Employee',          'Xodim — mehnat haqi'),
	('EmployeeAdvance',   'Xodim — hisobdor summalar'),
	('Founder',           'Asoschi — dividendlar'),
	('LoanGiven',         'Berilgan qarz'),
	('LoanReceived',      'Olingan kredit/qarz'),
	('TaxVAT',            'QQS (byudjet)'),
	('TaxNDFL',           'JShDS'),
	('TaxProfit',         'Foyda solig''i'),
	('TaxExcise',         'Aktsiz solig''i'),
	('TaxProperty',       'Mol-mulk solig''i'),
	('TaxLand',           'Yer solig''i'),
	('TaxOther',          'Boshqa soliq va yig''imlar'),
	('SocialInsurance',   'Ijtimoiy soliq (YaIJ)'),
	('PensionFund',       'INPS'),
	('BankFee',           'Bank komissiyasi'),
	('TaxAuthority',      'Soliq organi / byudjet bilan hisob-kitob');

-- Узбекский (language_id = 1)
insert into acc_posting_alias_translation (posting_alias_id, language_id, name)
select a.id, 1, v.name
from acc_posting_alias a
join (values
	('Inventory',        'Tovar ombor qoldig''i'),
	('Expense',           'Xarajat (xizmat)'),
	('Supplier',          'Yetkazib beruvchi — qarzni yopish'),
	('SupplierAdvance',   'Yetkazib beruvchiga avans'),
	('Customer',          'Xaridor — qarzni yopish'),
	('CustomerAdvance',   'Xaridordan avans'),
	('SalesRevenue',      'Tovarlar realizatsiyasidan daromad'),
	('ServiceRevenue',    'Xizmatlardan daromad'),
	('VATIn',             'Hisobga olinadigan QQS'),
	('VATOut',            'To''lanadigan QQS'),
	('CostOfGoods',       'Sotilgan tovarlar tannarxi'),
	('CostOfService',     'Ko''rsatilgan xizmatlar tannarxi'),
	('AssetWriteOff',     'Aktivni hisobdan chiqarish (tovar/OS)'),
	('PaymentAccount',    'Pul hisobvarag''i (bank/kassa)'),
	('Employee',          'Xodim — mehnat haqi'),
	('EmployeeAdvance',   'Xodim — hisobdor summalar'),
	('Founder',           'Asoschi — dividendlar'),
	('LoanGiven',         'Berilgan qarz'),
	('LoanReceived',      'Olingan kredit/qarz'),
	('TaxVAT',            'QQS (byudjet)'),
	('TaxNDFL',           'JShDS'),
	('TaxProfit',         'Foyda solig''i'),
	('TaxExcise',         'Aktsiz solig''i'),
	('TaxProperty',       'Mol-mulk solig''i'),
	('TaxLand',           'Yer solig''i'),
	('TaxOther',          'Boshqa soliq va yig''imlar'),
	('SocialInsurance',   'Ijtimoiy soliq (YaIJ)'),
	('PensionFund',       'INPS'),
	('BankFee',           'Bank komissiyasi'),
	('TaxAuthority',      'Soliq organi / byudjet bilan hisob-kitob')
) as v(code, name) on v.code = a.code;

-- Русский (language_id = 2)
insert into acc_posting_alias_translation (posting_alias_id, language_id, name)
select a.id, 2, v.name
from acc_posting_alias a
join (values
	('Inventory',        'Товар на складе'),
	('Expense',           'Расход (услуга)'),
	('Supplier',          'Поставщик — погашение долга'),
	('SupplierAdvance',   'Поставщик — аванс'),
	('Customer',          'Покупатель — погашение долга'),
	('CustomerAdvance',   'Покупатель — аванс'),
	('SalesRevenue',      'Доход от реализации товаров'),
	('ServiceRevenue',    'Доход от услуг'),
	('VATIn',             'НДС к зачёту'),
	('VATOut',            'НДС к уплате'),
	('CostOfGoods',       'Себестоимость реализованных товаров'),
	('CostOfService',     'Себестоимость услуг'),
	('AssetWriteOff',     'Списание актива (товар/ОС)'),
	('PaymentAccount',    'Денежный счёт (банк/касса)'),
	('Employee',          'Сотрудник — оплата труда'),
	('EmployeeAdvance',   'Сотрудник — подотчётные суммы'),
	('Founder',           'Учредитель — дивиденды'),
	('LoanGiven',         'Заём выданный'),
	('LoanReceived',      'Кредит/заём полученный'),
	('TaxVAT',            'НДС (бюджет)'),
	('TaxNDFL',           'НДФЛ'),
	('TaxProfit',         'Налог на прибыль'),
	('TaxExcise',         'Акцизы'),
	('TaxProperty',       'Налог на имущество'),
	('TaxLand',           'Земельный налог'),
	('TaxOther',          'Прочие налоги и сборы'),
	('SocialInsurance',   'Социальный налог (ЕСП)'),
	('PensionFund',       'ИНПС'),
	('BankFee',           'Банковская комиссия'),
	('TaxAuthority',      'Налоговый орган / расчёты с бюджетом')
) as v(code, name) on v.code = a.code;

-- English (language_id = 3)
insert into acc_posting_alias_translation (posting_alias_id, language_id, name)
select a.id, 3, v.name
from acc_posting_alias a
join (values
	('Inventory',        'Inventory on stock'),
	('Expense',           'Expense (service)'),
	('Supplier',          'Supplier — debt settlement'),
	('SupplierAdvance',   'Advance to supplier'),
	('Customer',          'Customer — debt settlement'),
	('CustomerAdvance',   'Advance from customer'),
	('SalesRevenue',      'Revenue from goods sales'),
	('ServiceRevenue',    'Revenue from services'),
	('VATIn',             'Input VAT'),
	('VATOut',            'Output VAT'),
	('CostOfGoods',       'Cost of goods sold'),
	('CostOfService',     'Cost of services rendered'),
	('AssetWriteOff',     'Asset write-off (inventory/fixed asset)'),
	('PaymentAccount',    'Cash account (bank/cash)'),
	('Employee',          'Employee — payroll'),
	('EmployeeAdvance',   'Employee — accountable amounts'),
	('Founder',           'Founder — dividends'),
	('LoanGiven',         'Loan given'),
	('LoanReceived',      'Loan/credit received'),
	('TaxVAT',            'VAT (budget)'),
	('TaxNDFL',           'Personal income tax'),
	('TaxProfit',         'Profit tax'),
	('TaxExcise',         'Excise tax'),
	('TaxProperty',       'Property tax'),
	('TaxLand',           'Land tax'),
	('TaxOther',          'Other taxes and fees'),
	('SocialInsurance',   'Social insurance tax'),
	('PensionFund',       'Pension fund'),
	('BankFee',           'Bank fee'),
	('TaxAuthority',      'Tax authority / budget settlements'
) as v(code, name) on v.code = a.code;