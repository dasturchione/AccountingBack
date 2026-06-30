-- Table: public.acc_subkonto_type

CREATE TABLE public.acc_subkonto_type (
    id smallint NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(150) NOT NULL,
    source_table character varying(100) NOT NULL,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL
);

CREATE SEQUENCE public.acc_subkonto_type_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.acc_subkonto_type_id_seq OWNED BY public.acc_subkonto_type.id;

ALTER TABLE ONLY public.acc_subkonto_type ALTER COLUMN id SET DEFAULT nextval('public.acc_subkonto_type_id_seq'::regclass);

insert into public.acc_subkonto_type (id, code, name, source_table, state_id, created_date) values
    ('1', 'product', 'Tovar / xizmat', 'inv_product', '1', '2026-06-25 10:43:24.6827'),
    ('2', 'warehouse', 'Ombor', 'inv_warehouse', '1', '2026-06-25 10:43:24.6827'),
    ('3', 'counterparty', 'Kontragent', 'counterparty_card', '1', '2026-06-25 10:43:24.6827'),
    ('4', 'bank_account', 'Bank hisobi', 'org_bank_account', '1', '2026-06-25 10:43:24.6827'),
    ('5', 'cash_box', 'Kassa', 'cash_box', '1', '2026-06-25 10:43:24.6827'),
    ('6', 'employee', 'Xodim', 'sys_user', '1', '2026-06-25 10:43:24.6827'),
    ('7', 'tax', 'Soliq', 'cmn_tax_type', '1', '2026-06-25 10:43:24.6827'),
    ('8', 'bank_operation', 'Bank operatsiyasi', 'bank_operation', '1', '2026-06-25 10:43:24.6827'),
    ('9', 'contract', 'Shartnoma', 'cmn_contract', '1', '2026-06-25 10:43:24.6827'),
    ('10', 'puchase', 'Xarid', 'pur_doc', '1', '2026-06-25 10:43:24.6827'),
    ('11', 'sale', 'Sotuv', 'sale_doc', '1', '2026-06-25 10:43:24.6827');

SELECT pg_catalog.setval('public.acc_subkonto_type_id_seq', 11, true);

ALTER TABLE ONLY public.acc_subkonto_type
    ADD CONSTRAINT acc_subkonto_type_pkey PRIMARY KEY (id);

CREATE UNIQUE INDEX idx_acc_subkonto_type_code ON public.acc_subkonto_type USING btree (code);

CREATE INDEX idx_acc_subkonto_type_state_id ON public.acc_subkonto_type USING btree (state_id);

ALTER TABLE ONLY public.acc_subkonto_type
    ADD CONSTRAINT acc_subkonto_type_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);
