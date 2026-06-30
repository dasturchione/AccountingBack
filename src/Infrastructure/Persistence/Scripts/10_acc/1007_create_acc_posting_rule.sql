-- Table: public.acc_posting_rule

CREATE TABLE public.acc_posting_rule (
    id smallint NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(250) NOT NULL
);

CREATE SEQUENCE public.acc_posting_rule_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.acc_posting_rule_id_seq OWNED BY public.acc_posting_rule.id;

ALTER TABLE ONLY public.acc_posting_rule ALTER COLUMN id SET DEFAULT nextval('public.acc_posting_rule_id_seq'::regclass);

insert into public.acc_posting_rule (id, code, name) values
    ('1', 'PURCHASE_GOODS', 'Поступление товара'),
    ('2', 'PURCHASE_SERVICE', 'Получение услуги'),
    ('3', 'SALE_GOODS', 'Реализация товара'),
    ('4', 'SALE_SERVICE', 'Оказанная услуга'),
    ('5', 'DEBIT_OPERATION', 'Банковская/кассовая операция — приход'),
    ('6', 'CREDIT_OPERATION', 'Банковская/кассовая операция — расход');

SELECT pg_catalog.setval('public.acc_posting_rule_id_seq', 6, true);

ALTER TABLE ONLY public.acc_posting_rule
    ADD CONSTRAINT acc_posting_rule_code_key UNIQUE (code);

ALTER TABLE ONLY public.acc_posting_rule
    ADD CONSTRAINT acc_posting_rule_pkey PRIMARY KEY (id);
