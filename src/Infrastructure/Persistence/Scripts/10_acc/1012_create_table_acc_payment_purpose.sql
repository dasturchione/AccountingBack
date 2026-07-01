-- =========================================================
-- acc_payment_purpose / acc_payment_purpose_translation
-- =========================================================

create table acc_payment_purpose
(
	id							smallserial primary key,
	code						varchar(50) not null unique,
	alias_id					smallint not null references acc_posting_alias(id),
	name						varchar(250) not null,
	requires_counterparty		boolean not null default true,
	operation_type_id			smallint null references cmn_operation_type(id)
);

create table acc_payment_purpose_translation
(
	payment_purpose_id			smallint not null references acc_payment_purpose(id),
	language_id					smallint not null references cmn_language(id),
	name						varchar(250) not null,

	primary key (payment_purpose_id, language_id)
);

-- =========================================================
-- INSERT acc_payment_purpose (name заполняется английским значением как fallback)
-- =========================================================

insert into acc_payment_purpose (code, alias_id, operation_type_id, name, requires_counterparty)
select v.code, a.id, ot.id, v.name, v.requires_counterparty
from (values
	('SUPPLIER_PAYMENT',    'Supplier',         'out', 'Supplier payment',        true),
	('SUPPLIER_ADVANCE',    'SupplierAdvance',  'out', 'Advance to supplier',     true),
	('CUSTOMER_RECEIPT',    'Customer',         'in',  'Payment from customer',   true),
	('CUSTOMER_ADVANCE',    'CustomerAdvance',  'in',  'Advance from customer',   true),
	('SALARY',              'Employee',        'out', 'Salary',                  true),
	('ACCOUNTABLE_ADVANCE', 'EmployeeAdvance',  'out', 'Accountable amounts',     true),
	('DIVIDENDS',           'Founder',         'out', 'Dividend payment',        true),
	('LOAN_GIVEN',          'LoanGiven',       'out', 'Loan given',              true),
	('LOAN_RECEIVED',       'LoanReceived',    'in',  'Loan received',           false),
	('LOAN_REPAYMENT',      'LoanReceived',    'out', 'Loan repayment',          false),
	('TAX_VAT',             'TaxVAT',          'out', 'VAT',                     false),
	('TAX_NDFL',            'TaxNDFL',         'out', 'Personal income tax',     false),
	('TAX_PROFIT',          'TaxProfit',       'out', 'Profit tax',              false),
	('TAX_PROPERTY',        'TaxProperty',     'out', 'Property tax',            false),
	('TAX_LAND',            'TaxLand',         'out', 'Land tax',                false),
	('SOCIAL_INSURANCE',    'SocialInsurance', 'out', 'Social insurance tax',    false),
	('PENSION_FUND',        'PensionFund',     'out', 'Pension fund',            false),
	('BANK_FEE',            'BankFee',         'out', 'Bank fee',                false)
) as v(code, alias_code, operation_type_code, name, requires_counterparty)
join acc_posting_alias a on a.code = v.alias_code
join cmn_operation_type ot on ot.code = v.operation_type_code;

-- =========================================================
-- INSERT acc_payment_purpose_translation
-- =========================================================

-- Узбекский (language_id = 1)
insert into acc_payment_purpose_translation (payment_purpose_id, language_id, name)
select p.id, 1, v.name
from acc_payment_purpose p
join (values
	('SUPPLIER_PAYMENT',    'Yetkazib beruvchiga to''lov'),
	('SUPPLIER_ADVANCE',    'Yetkazib beruvchiga avans'),
	('CUSTOMER_RECEIPT',    'Xaridordan to''lov'),
	('CUSTOMER_ADVANCE',    'Xaridordan avans'),
	('SALARY',              'Mehnat haqi'),
	('ACCOUNTABLE_ADVANCE', 'Hisobdor summalar'),
	('DIVIDENDS',           'Dividendlar to''lovi'),
	('LOAN_GIVEN',          'Qarz berish'),
	('LOAN_RECEIVED',       'Kredit olish'),
	('LOAN_REPAYMENT',      'Kreditni qaytarish'),
	('TAX_VAT',             'QQS'),
	('TAX_NDFL',            'JShDS'),
	('TAX_PROFIT',          'Foyda solig''i'),
	('TAX_PROPERTY',        'Mol-mulk solig''i'),
	('TAX_LAND',            'Yer solig''i'),
	('SOCIAL_INSURANCE',    'Ijtimoiy soliq (YaIJ)'),
	('PENSION_FUND',        'INPS'),
	('BANK_FEE',            'Bank komissiyasi')
) as v(code, name) on v.code = p.code;

-- Русский (language_id = 2)
insert into acc_payment_purpose_translation (payment_purpose_id, language_id, name)
select p.id, 2, v.name
from acc_payment_purpose p
join (values
	('SUPPLIER_PAYMENT',    'Оплата поставщику'),
	('SUPPLIER_ADVANCE',    'Аванс поставщику'),
	('CUSTOMER_RECEIPT',    'Оплата от клиента'),
	('CUSTOMER_ADVANCE',    'Аванс от клиента'),
	('SALARY',              'Заработная плата'),
	('ACCOUNTABLE_ADVANCE', 'Подотчётные суммы'),
	('DIVIDENDS',           'Выплата дивидендов'),
	('LOAN_GIVEN',          'Выдача займа'),
	('LOAN_RECEIVED',       'Получение кредита'),
	('LOAN_REPAYMENT',      'Погашение кредита'),
	('TAX_VAT',             'НДС'),
	('TAX_NDFL',            'НДФЛ'),
	('TAX_PROFIT',          'Налог на прибыль'),
	('TAX_PROPERTY',        'Налог на имущество'),
	('TAX_LAND',             'Земельный налог'),
	('SOCIAL_INSURANCE',    'Социальный налог (ЕСП)'),
	('PENSION_FUND',        'ИНПС'),
	('BANK_FEE',             'Банковская комиссия')
) as v(code, name) on v.code = p.code;

-- English (language_id = 3)
insert into acc_payment_purpose_translation (payment_purpose_id, language_id, name)
select p.id, 3, v.name
from acc_payment_purpose p
join (values
	('SUPPLIER_PAYMENT',    'Supplier payment'),
	('SUPPLIER_ADVANCE',    'Advance to supplier'),
	('CUSTOMER_RECEIPT',    'Payment from customer'),
	('CUSTOMER_ADVANCE',    'Advance from customer'),
	('SALARY',              'Salary'),
	('ACCOUNTABLE_ADVANCE', 'Accountable amounts'),
	('DIVIDENDS',           'Dividend payment'),
	('LOAN_GIVEN',          'Loan given'),
	('LOAN_RECEIVED',       'Loan received'),
	('LOAN_REPAYMENT',      'Loan repayment'),
	('TAX_VAT',             'VAT'),
	('TAX_NDFL',             'Personal income tax'),
	('TAX_PROFIT',           'Profit tax'),
	('TAX_PROPERTY',        'Property tax'),
	('TAX_LAND',             'Land tax'),
	('SOCIAL_INSURANCE',    'Social insurance tax'),
	('PENSION_FUND',        'Pension fund'),
	('BANK_FEE',             'Bank fee')
) as v(code, name) on v.code = p.code;
