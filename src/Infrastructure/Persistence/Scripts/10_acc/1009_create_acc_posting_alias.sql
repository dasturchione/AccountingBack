create table acc_posting_alias 
(
    id smallint not null,
    code character varying(50) not null,
    name character varying(250) not null,
    constraint acc_posting_alias_code_key UNIQUE (code),
    constraint acc_posting_alias_pkey primary key (id)
);

insert into acc_posting_alias (id, code, name) values (1, 'Inventory', 'Tovar ombor qoldig''i');
insert into acc_posting_alias (id, code, name) values (2, 'Expense', 'Xarajat (xizmat)');
insert into acc_posting_alias (id, code, name) values (3, 'Supplier', 'Yetkazib beruvchi — qarzni yopish');
insert into acc_posting_alias (id, code, name) values (4, 'SupplierAdvance', 'Yetkazib beruvchiga avans');
insert into acc_posting_alias (id, code, name) values (5, 'Customer', 'Xaridor — qarzni yopish');
insert into acc_posting_alias (id, code, name) values (6, 'CustomerAdvance', 'Xaridordan avans');
insert into acc_posting_alias (id, code, name) values (7, 'SalesRevenue', 'Tovarlar realizatsiyasidan daromad');
insert into acc_posting_alias (id, code, name) values (8, 'ServiceRevenue', 'Xizmatlardan daromad');
insert into acc_posting_alias (id, code, name) values (9, 'VATIn', 'Hisobga olinadigan QQS');
insert into acc_posting_alias (id, code, name) values (10, 'VATOut', 'To''lanadigan QQS');
insert into acc_posting_alias (id, code, name) values (11, 'CostOfGoods', 'Sotilgan tovarlar tannarxi');
insert into acc_posting_alias (id, code, name) values (12, 'CostOfService', 'Ko''rsatilgan xizmatlar tannarxi');
insert into acc_posting_alias (id, code, name) values (13, 'AssetWriteOff', 'Aktivni hisobdan chiqarish (tovar/OS)');
insert into acc_posting_alias (id, code, name) values (14, 'PaymentAccount', 'Pul hisobvarag''i (bank/kassa)');
insert into acc_posting_alias (id, code, name) values (15, 'Employee', 'Xodim — mehnat haqi');
insert into acc_posting_alias (id, code, name) values (16, 'EmployeeAdvance', 'Xodim — hisobdor summalar');
insert into acc_posting_alias (id, code, name) values (17, 'Founder', 'Asoschi — dividendlar');
insert into acc_posting_alias (id, code, name) values (18, 'LoanGiven', 'Berilgan qarz');
insert into acc_posting_alias (id, code, name) values (19, 'LoanReceived', 'Olingan kredit/qarz');
insert into acc_posting_alias (id, code, name) values (20, 'TaxVAT', 'QQS (byudjet)');
insert into acc_posting_alias (id, code, name) values (21, 'TaxNDFL', 'JShDS');
insert into acc_posting_alias (id, code, name) values (22, 'TaxProfit', 'Foyda solig''i');
insert into acc_posting_alias (id, code, name) values (23, 'TaxExcise', 'Aktsiz solig''i');
insert into acc_posting_alias (id, code, name) values (24, 'TaxProperty', 'Mol-mulk solig''i');
insert into acc_posting_alias (id, code, name) values (25, 'TaxLand', 'Yer solig''i');
insert into acc_posting_alias (id, code, name) values (26, 'TaxOther', 'Boshqa soliq va yig''imlar');
insert into acc_posting_alias (id, code, name) values (27, 'SocialInsurance', 'Ijtimoiy soliq (YaIJ)');
insert into acc_posting_alias (id, code, name) values (28, 'PensionFund', 'INPS');
insert into acc_posting_alias (id, code, name) values (29, 'BankFee', 'Bank komissiyasi');
insert into acc_posting_alias (id, code, name) values (30, 'CashBoxSource', 'Naqd pulni hisobvaraqlari (keluvchi)');
insert into acc_posting_alias (id, code, name) values (31, 'CashBoxDestination', 'Naqd pulni hisobvaraqlari (chiquvchi)');
insert into acc_posting_alias (id, code, name) values (32, 'CurrencyAsset', 'Currency monetary asset');
insert into acc_posting_alias (id, code, name) values (33, 'CurrencyRevaluationGain', 'Currency revaluation gain');
insert into acc_posting_alias (id, code, name) values (34, 'CurrencyRevaluationLoss', 'Currency revaluation loss');

-- FA-P4: Asosiy vositalar (OS) uchun aliaslar — buxgalteriya provodka poydevori.
insert into acc_posting_alias (id, code, name) values (35, 'FixedAsset', 'Asosiy vosita (0100)');
insert into acc_posting_alias (id, code, name) values (36, 'FixedAssetInProgress', 'OS ni sotib olish / kapital qo''yilma (0800)');
insert into acc_posting_alias (id, code, name) values (37, 'FixedAssetDepreciation', 'OS amortizatsiyasi (0200)');
insert into acc_posting_alias (id, code, name) values (38, 'FixedAssetExpense', 'OS bo''yicha xarajat');
insert into acc_posting_alias (id, code, name) values (39, 'FixedAssetDisposalLoss', 'OS chiqib ketishidan zarar');
insert into acc_posting_alias (id, code, name) values (40, 'FixedAssetDisposalGain', 'OS chiqib ketishidan tushum/foyda');
insert into acc_posting_alias (id, code, name) values (41, 'FixedAssetRevaluationSurplus', 'OS qayta baholash zaxirasi');
insert into acc_posting_alias (id, code, name) values (42, 'FixedAssetRevaluationLoss', 'OS qayta baholash zarari');
