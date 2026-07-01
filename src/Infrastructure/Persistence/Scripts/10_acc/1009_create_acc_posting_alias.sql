-- Table: public.acc_posting_alias

CREATE TABLE public.acc_posting_alias (
    id smallint NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(250) NOT NULL
);

CREATE SEQUENCE public.acc_posting_alias_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.acc_posting_alias_id_seq OWNED BY public.acc_posting_alias.id;

ALTER TABLE ONLY public.acc_posting_alias ALTER COLUMN id SET DEFAULT nextval('public.acc_posting_alias_id_seq'::regclass);

insert into public.acc_posting_alias (id, code, name) values (1, 'Inventory', 'Tovar ombor qoldig''i');
insert into public.acc_posting_alias (id, code, name) values (2, 'Expense', 'Xarajat (xizmat)');
insert into public.acc_posting_alias (id, code, name) values (3, 'Supplier', 'Yetkazib beruvchi — qarzni yopish');
insert into public.acc_posting_alias (id, code, name) values (4, 'SupplierAdvance', 'Yetkazib beruvchiga avans');
insert into public.acc_posting_alias (id, code, name) values (5, 'Customer', 'Xaridor — qarzni yopish');
insert into public.acc_posting_alias (id, code, name) values (6, 'CustomerAdvance', 'Xaridordan avans');
insert into public.acc_posting_alias (id, code, name) values (7, 'SalesRevenue', 'Tovarlar realizatsiyasidan daromad');
insert into public.acc_posting_alias (id, code, name) values (8, 'ServiceRevenue', 'Xizmatlardan daromad');
insert into public.acc_posting_alias (id, code, name) values (9, 'VATIn', 'Hisobga olinadigan QQS');
insert into public.acc_posting_alias (id, code, name) values (10, 'VATOut', 'To''lanadigan QQS');
insert into public.acc_posting_alias (id, code, name) values (11, 'CostOfGoods', 'Sotilgan tovarlar tannarxi');
insert into public.acc_posting_alias (id, code, name) values (12, 'CostOfService', 'Ko''rsatilgan xizmatlar tannarxi');
insert into public.acc_posting_alias (id, code, name) values (13, 'AssetWriteOff', 'Aktivni hisobdan chiqarish (tovar/OS)');
insert into public.acc_posting_alias (id, code, name) values (14, 'PaymentAccount', 'Pul hisobvarag''i (bank/kassa)');
insert into public.acc_posting_alias (id, code, name) values (15, 'Employee', 'Xodim — mehnat haqi');
insert into public.acc_posting_alias (id, code, name) values (16, 'EmployeeAdvance', 'Xodim — hisobdor summalar');
insert into public.acc_posting_alias (id, code, name) values (17, 'Founder', 'Asoschi — dividendlar');
insert into public.acc_posting_alias (id, code, name) values (18, 'LoanGiven', 'Berilgan qarz');
insert into public.acc_posting_alias (id, code, name) values (19, 'LoanReceived', 'Olingan kredit/qarz');
insert into public.acc_posting_alias (id, code, name) values (20, 'TaxVAT', 'QQS (byudjet)');
insert into public.acc_posting_alias (id, code, name) values (21, 'TaxNDFL', 'JShDS');
insert into public.acc_posting_alias (id, code, name) values (22, 'TaxProfit', 'Foyda solig''i');
insert into public.acc_posting_alias (id, code, name) values (23, 'TaxExcise', 'Aktsiz solig''i');
insert into public.acc_posting_alias (id, code, name) values (24, 'TaxProperty', 'Mol-mulk solig''i');
insert into public.acc_posting_alias (id, code, name) values (25, 'TaxLand', 'Yer solig''i');
insert into public.acc_posting_alias (id, code, name) values (26, 'TaxOther', 'Boshqa soliq va yig''imlar');
insert into public.acc_posting_alias (id, code, name) values (27, 'SocialInsurance', 'Ijtimoiy soliq (YaIJ)');
insert into public.acc_posting_alias (id, code, name) values (28, 'PensionFund', 'INPS');
insert into public.acc_posting_alias (id, code, name) values (29, 'BankFee', 'Bank komissiyasi');
insert into public.acc_posting_alias (id, code, name) values (30, 'CashBoxSource', 'Naqd pulni hisobvaraqlari (keluvchi)');
insert into public.acc_posting_alias (id, code, name) values (31, 'CashBoxDestination', 'Naqd pulni hisobvaraqlari (chiquvchi)');

SELECT pg_catalog.setval('public.acc_posting_alias_id_seq', 31, true);

ALTER TABLE ONLY public.acc_posting_alias
    ADD CONSTRAINT acc_posting_alias_code_key UNIQUE (code);

ALTER TABLE ONLY public.acc_posting_alias
    ADD CONSTRAINT acc_posting_alias_pkey PRIMARY KEY (id);
