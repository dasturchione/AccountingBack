-- Table: public.acc_accounting_policy

CREATE TABLE public.acc_accounting_policy (
    id smallint NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(250) NOT NULL,
    state_id smallint NOT NULL
);

CREATE SEQUENCE public.acc_accounting_policy_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.acc_accounting_policy_id_seq OWNED BY public.acc_accounting_policy.id;

ALTER TABLE ONLY public.acc_accounting_policy ALTER COLUMN id SET DEFAULT nextval('public.acc_accounting_policy_id_seq'::regclass);

insert into public.acc_accounting_policy (id, code, name, state_id) values
    ('1', 'STANDARD', 'Стандартная политика РУз', '1');

SELECT pg_catalog.setval('public.acc_accounting_policy_id_seq', 1, true);

ALTER TABLE ONLY public.acc_accounting_policy
    ADD CONSTRAINT acc_accounting_policy_code_key UNIQUE (code);

ALTER TABLE ONLY public.acc_accounting_policy
    ADD CONSTRAINT acc_accounting_policy_pkey PRIMARY KEY (id);

ALTER TABLE ONLY public.acc_accounting_policy
    ADD CONSTRAINT acc_accounting_policy_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);
