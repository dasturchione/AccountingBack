-- Table: public.acc_account_resolve_rule

CREATE TABLE public.acc_account_resolve_rule (
    id integer NOT NULL,
    policy_id smallint NOT NULL,
    alias character varying(250) NOT NULL,
    dimension_key character varying(250) NOT NULL,
    dimension_value character varying(250) NOT NULL,
    account_id integer NOT NULL,
    priority integer NOT NULL
);

CREATE SEQUENCE public.acc_account_resolve_rule_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.acc_account_resolve_rule_id_seq OWNED BY public.acc_account_resolve_rule.id;

ALTER TABLE ONLY public.acc_account_resolve_rule ALTER COLUMN id SET DEFAULT nextval('public.acc_account_resolve_rule_id_seq'::regclass);

insert into public.acc_account_resolve_rule (id, policy_id, alias, dimension_key, dimension_value, account_id, priority) values
    ('16', '1', 'Inventory', 'category', '_default', '1014', '100'),
    ('17', '1', 'Supplier', '_none', '_default', '1036', '100'),
    ('18', '1', 'Customer', '_none', '_default', '1015', '100'),
    ('19', '1', 'CustomerAdvance', '_none', '_default', '1037', '100'),
    ('20', '1', 'SupplierAdvance', '_none', '_default', '1016', '100'),
    ('21', '1', 'PaymentAccount', 'paymentMethod', 'bank', '1027', '10'),
    ('22', '1', 'PaymentAccount', 'paymentMethod', '_default', '1027', '100'),
    ('23', '1', 'VATIn', '_none', '_default', '1017', '100'),
    ('24', '1', 'VATOut', '_none', '_default', '1038', '100'),
    ('25', '1', 'Expense', 'serviceType', 'production', '1013', '10'),
    ('26', '1', 'Expense', 'serviceType', 'admin', '1034', '10'),
    ('27', '1', 'Expense', 'serviceType', '_default', '1035', '100'),
    ('28', '1', 'SalesRevenue', '_none', '_default', '1041', '100'),
    ('29', '1', 'CostOfGoods', '_none', '_default', '1028', '100'),
    ('30', '1', 'ServiceRevenue', '_none', '_default', '1044', '100'),
    ('31', '1', 'CostOfService', '_none', '_default', '1031', '100'),
    ('32', '1', 'AssetWriteOff', 'assetType', 'inventory', '1014', '10'),
    ('33', '1', 'AssetWriteOff', 'assetType', '_default', '1014', '100'),
    ('34', '1', 'CashBoxSource', '_none', '_default', '1027', '100'),
    ('35', '1', 'CashBoxDestination', '_none', '_default', '1027', '100');

SELECT pg_catalog.setval('public.acc_account_resolve_rule_id_seq', 35, true);

ALTER TABLE ONLY public.acc_account_resolve_rule
    ADD CONSTRAINT acc_account_resolve_rule_pkey PRIMARY KEY (id);

ALTER TABLE ONLY public.acc_account_resolve_rule
    ADD CONSTRAINT acc_account_resolve_rule_account_id_fkey FOREIGN KEY (account_id) REFERENCES public.acc_chart_account(id);

ALTER TABLE ONLY public.acc_account_resolve_rule
    ADD CONSTRAINT acc_account_resolve_rule_policy_id_fkey FOREIGN KEY (policy_id) REFERENCES public.acc_accounting_policy(id);
