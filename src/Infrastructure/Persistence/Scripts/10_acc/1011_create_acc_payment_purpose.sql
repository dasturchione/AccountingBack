-- Table: public.acc_payment_purpose

CREATE TABLE public.acc_payment_purpose (
    id smallint NOT NULL,
    code character varying(50) NOT NULL,
    alias_id smallint NOT NULL,
    name character varying(250) NOT NULL,
    requires_counterparty boolean DEFAULT true NOT NULL
);

CREATE SEQUENCE public.acc_payment_purpose_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.acc_payment_purpose_id_seq OWNED BY public.acc_payment_purpose.id;

ALTER TABLE ONLY public.acc_payment_purpose ALTER COLUMN id SET DEFAULT nextval('public.acc_payment_purpose_id_seq'::regclass);

insert into public.acc_payment_purpose (id, code, alias_id, name, requires_counterparty) values (1, 'SUPPLIER_PAYMENT', 3, 'Supplier payment', true);
insert into public.acc_payment_purpose (id, code, alias_id, name, requires_counterparty) values (2, 'SUPPLIER_ADVANCE', 4, 'Advance to supplier', true);
insert into public.acc_payment_purpose (id, code, alias_id, name, requires_counterparty) values (3, 'CUSTOMER_RECEIPT', 5, 'Payment from customer', true);
insert into public.acc_payment_purpose (id, code, alias_id, name, requires_counterparty) values (4, 'CUSTOMER_ADVANCE', 6, 'Advance from customer', true);
insert into public.acc_payment_purpose (id, code, alias_id, name, requires_counterparty) values (5, 'SALARY', 15, 'Salary', true);
insert into public.acc_payment_purpose (id, code, alias_id, name, requires_counterparty) values (6, 'ACCOUNTABLE_ADVANCE', 16, 'Accountable amounts', true);
insert into public.acc_payment_purpose (id, code, alias_id, name, requires_counterparty) values (7, 'DIVIDENDS', 17, 'Dividend payment', true);
insert into public.acc_payment_purpose (id, code, alias_id, name, requires_counterparty) values (8, 'LOAN_GIVEN', 18, 'Loan given', true);
insert into public.acc_payment_purpose (id, code, alias_id, name, requires_counterparty) values (9, 'LOAN_REPAYMENT', 19, 'Loan repayment', false);
insert into public.acc_payment_purpose (id, code, alias_id, name, requires_counterparty) values (10, 'LOAN_RECEIVED', 19, 'Loan received', false);
insert into public.acc_payment_purpose (id, code, alias_id, name, requires_counterparty) values (11, 'TAX_VAT', 20, 'VAT', false);
insert into public.acc_payment_purpose (id, code, alias_id, name, requires_counterparty) values (12, 'TAX_NDFL', 21, 'Personal income tax', false);
insert into public.acc_payment_purpose (id, code, alias_id, name, requires_counterparty) values (13, 'TAX_PROFIT', 22, 'Profit tax', false);
insert into public.acc_payment_purpose (id, code, alias_id, name, requires_counterparty) values (14, 'TAX_PROPERTY', 24, 'Property tax', false);
insert into public.acc_payment_purpose (id, code, alias_id, name, requires_counterparty) values (15, 'TAX_LAND', 25, 'Land tax', false);
insert into public.acc_payment_purpose (id, code, alias_id, name, requires_counterparty) values (16, 'SOCIAL_INSURANCE', 27, 'Social insurance tax', false);
insert into public.acc_payment_purpose (id, code, alias_id, name, requires_counterparty) values (17, 'PENSION_FUND', 28, 'Pension fund', false);
insert into public.acc_payment_purpose (id, code, alias_id, name, requires_counterparty) values (18, 'BANK_FEE', 29, 'Bank fee', false);

SELECT pg_catalog.setval('public.acc_payment_purpose_id_seq', 18, true);

ALTER TABLE ONLY public.acc_payment_purpose
    ADD CONSTRAINT acc_payment_purpose_code_key UNIQUE (code);

ALTER TABLE ONLY public.acc_payment_purpose
    ADD CONSTRAINT acc_payment_purpose_pkey PRIMARY KEY (id);

ALTER TABLE ONLY public.acc_payment_purpose
    ADD CONSTRAINT acc_payment_purpose_alias_id_fkey FOREIGN KEY (alias_id) REFERENCES public.acc_posting_alias(id);
