
create table acc_account_resolve_rule (
    id serial not null,
    policy_id smallint not null,
    alias character varying(250) not null,
    dimension_key character varying(250) not null,
    dimension_value character varying(250) not null,
    account_id integer not null,
    priority integer not null,
    constraint acc_account_resolve_rule_pkey primary key (id),
    constraint acc_account_resolve_rule_account_id_fkey foreign key (account_id) references acc_chart_account(id),
    constraint acc_account_resolve_rule_policy_id_fkey foreign key (policy_id) references acc_accounting_policy(id)
);

-- ===================== Inventory: Товар на складе =====================
insert into acc_account_resolve_rule (policy_id, alias, dimension_key, dimension_value, account_id, priority)
select 1, 'Inventory', '_none', '_default', id, 100 from acc_chart_account where code = '2910';
 
-- ===================== Expense: Расход (услуга) =====================
-- производство → 2010, продажи → 9410, администрирование → 9420, прочее → 9430
insert into acc_account_resolve_rule (policy_id, alias, dimension_key, dimension_value, account_id, priority)
select 1, 'Expense', 'serviceType', 'production', id, 10  from acc_chart_account where code = '2010'
	union all
select 1, 'Expense', 'serviceType', 'sales',       id, 10  from acc_chart_account where code = '9410'
	union all
select 1, 'Expense', 'serviceType', 'admin',       id, 10  from acc_chart_account where code = '9420'
	union all
select 1, 'Expense', 'serviceType', '_default',    id, 100 from acc_chart_account where code = '9430';
 
-- ===================== Supplier / SupplierAdvance =====================
insert into acc_account_resolve_rule (policy_id, alias, dimension_key, dimension_value, account_id, priority)
select 1, 'Supplier',        '_none', '_default', id, 100 from acc_chart_account where code = '6010'
	union all
select 1, 'SupplierAdvance', '_none', '_default', id, 100 from acc_chart_account where code = '4310';
 
-- ===================== Customer / CustomerAdvance =====================
insert into acc_account_resolve_rule (policy_id, alias, dimension_key, dimension_value, account_id, priority)
select 1, 'Customer',        '_none', '_default', id, 100 from acc_chart_account where code = '4010'
	union all
select 1, 'CustomerAdvance', '_none', '_default', id, 100 from acc_chart_account where code = '6310';
 
-- ===================== PaymentAccount: банк/касса =====================
insert into acc_account_resolve_rule (policy_id, alias, dimension_key, dimension_value, account_id, priority)
select 1, 'PaymentAccount', 'paymentMethod', 'bank',     id, 10  from acc_chart_account where code = '5110'
	union all
select 1, 'PaymentAccount', 'paymentMethod', 'cash',     id, 10  from acc_chart_account where code = '5010'
	union all
select 1, 'PaymentAccount', 'paymentMethod', '_default', id, 100 from acc_chart_account where code = '5110';
 
-- ===================== VAT =====================
-- Входной НДС (4410 — групповой счёт): постим на постящиеся субсчета.
-- По МПЗ/товарам → 4410.3, по услугам → 4410.4, по умолчанию → 4410.3.
insert into acc_account_resolve_rule (policy_id, alias, dimension_key, dimension_value, account_id, priority)
select 1, 'VATIn', 'vatKind', 'goods',    id, 10  from acc_chart_account where code = '4410.3'
	union all
select 1, 'VATIn', 'vatKind', 'services', id, 10  from acc_chart_account where code = '4410.4'
	union all
select 1, 'VATIn', 'vatKind', '_default', id, 100 from acc_chart_account where code = '4410.3';

-- Начисленный НДС (6410 — групповой счёт): постим на субсчёт 6410.1 (НДС при реализации).
insert into acc_account_resolve_rule (policy_id, alias, dimension_key, dimension_value, account_id, priority)
select 1, 'VATOut', '_none', '_default', id, 100 from acc_chart_account where code = '6410.1';
 
-- ===================== Revenue =====================
-- 9020 и 9030 — групповые счета: постим на субсчёт основной системы налогообложения (.1).
insert into acc_account_resolve_rule (policy_id, alias, dimension_key, dimension_value, account_id, priority)
select 1, 'SalesRevenue',   '_none', '_default', id, 100 from acc_chart_account where code = '9020.1'
	union all
select 1, 'ServiceRevenue', '_none', '_default', id, 100 from acc_chart_account where code = '9030.1';

-- ===================== Cost =====================
-- 9120 и 9130 — групповые счета: постим на субсчёт основной системы налогообложения (.1).
insert into acc_account_resolve_rule (policy_id, alias, dimension_key, dimension_value, account_id, priority)
select 1, 'CostOfGoods',   '_none', '_default', id, 100 from acc_chart_account where code = '9120.1'
	union all
select 1, 'CostOfService', '_none', '_default', id, 100 from acc_chart_account where code = '9130.1';
 
-- ===================== AssetWriteOff: списание актива =====================
-- счёт приходит явно из кода (2910 или 1010-1090), _default не нужен
insert into acc_account_resolve_rule (policy_id, alias, dimension_key, dimension_value, account_id, priority)
select 1, 'AssetWriteOff', 'assetType', 'inventory', id, 10 from acc_chart_account where code = '2910';
 
-- ===================== Employee / EmployeeAdvance =====================
insert into acc_account_resolve_rule (policy_id, alias, dimension_key, dimension_value, account_id, priority)
select 1, 'Employee',        '_none', '_default', id, 100 from acc_chart_account where code = '6710'
	union all
select 1, 'EmployeeAdvance', '_none', '_default', id, 100 from acc_chart_account where code = '6970';
 
-- ===================== Founder =====================
insert into acc_account_resolve_rule (policy_id, alias, dimension_key, dimension_value, account_id, priority)
select 1, 'Founder', '_none', '_default', id, 100 from acc_chart_account where code = '6610';
 
-- ===================== Loan =====================
insert into acc_account_resolve_rule (policy_id, alias, dimension_key, dimension_value, account_id, priority)
select 1, 'LoanGiven',    '_none', '_default', id, 100 from acc_chart_account where code = '4720'
	union all
select 1, 'LoanReceived', '_none', '_default', id, 100 from acc_chart_account where code = '6810';
 
-- ===================== Налоги =====================
-- 6410/6420/6510/6530 — групповые счета: постим на постящиеся субсчета (.1).
insert into acc_account_resolve_rule (policy_id, alias, dimension_key, dimension_value, account_id, priority)
select 1, 'TaxVAT',          '_none', '_default', id, 100 from acc_chart_account where code = '6410.1'
	union all
select 1, 'TaxNDFL',         '_none', '_default', id, 100 from acc_chart_account where code = '6420.1'
	union all
select 1, 'TaxProfit',       '_none', '_default', id, 100 from acc_chart_account where code = '6430'
	union all
select 1, 'TaxExcise',       '_none', '_default', id, 100 from acc_chart_account where code = '6440'
	union all
select 1, 'TaxProperty',     '_none', '_default', id, 100 from acc_chart_account where code = '6450'
	union all
select 1, 'TaxLand',         '_none', '_default', id, 100 from acc_chart_account where code = '6460'
	union all
select 1, 'TaxOther',        '_none', '_default', id, 100 from acc_chart_account where code = '6490'
	union all
select 1, 'SocialInsurance', '_none', '_default', id, 100 from acc_chart_account where code = '6510.1'
	union all
select 1, 'PensionFund',     '_none', '_default', id, 100 from acc_chart_account where code = '6530.1';
 
-- ===================== BankFee =====================
insert into acc_account_resolve_rule (policy_id, alias, dimension_key, dimension_value, account_id, priority)
select 1, 'BankFee', '_none', '_default', id, 100 from acc_chart_account where code = '9430';

insert into acc_account_resolve_rule (policy_id, alias, dimension_key, dimension_value, account_id, priority)
select 1, 'CurrencyAsset', '_none', '_default', id, 100 from acc_chart_account where code = '5020';

insert into acc_account_resolve_rule (policy_id, alias, dimension_key, dimension_value, account_id, priority)
select 1, 'CurrencyRevaluationGain', '_none', '_default', id, 100 from acc_chart_account where code = '9030.1';

insert into acc_account_resolve_rule (policy_id, alias, dimension_key, dimension_value, account_id, priority)
select 1, 'CurrencyRevaluationLoss', '_none', '_default', id, 100 from acc_chart_account where code = '9430';

-- ===================== Fixed Assets (Основные средства) =====================
-- Проводки идут на постящиеся субсчета-листья (0190/0290/0820), а не на групповые 0100/0200/0800.
insert into acc_account_resolve_rule (policy_id, alias, dimension_key, dimension_value, account_id, priority)
select 1, 'FixedAsset',            '_none', '_default', id, 100 from acc_chart_account where code = '0190'
	union all
select 1, 'FixedAssetInProgress',  '_none', '_default', id, 100 from acc_chart_account where code = '0820'
	union all
select 1, 'FixedAssetDepreciation','_none', '_default', id, 100 from acc_chart_account where code = '0290'
	union all
select 1, 'FixedAssetExpense',     '_none', '_default', id, 100 from acc_chart_account where code = '9430'
	union all
select 1, 'FixedAssetDisposalLoss','_none', '_default', id, 100 from acc_chart_account where code = '9430';

-- Входной НДС при приобретении ОС (счёт 4410.1). Измерение vatKind='fixedAsset'.
insert into acc_account_resolve_rule (policy_id, alias, dimension_key, dimension_value, account_id, priority)
select 1, 'VATIn', 'vatKind', 'fixedAsset', id, 10 from acc_chart_account where code = '4410.1';

