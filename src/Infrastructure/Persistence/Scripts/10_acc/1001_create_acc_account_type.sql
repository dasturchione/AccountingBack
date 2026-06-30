-- Table: public.acc_account_type

CREATE TABLE public.acc_account_type (
    id smallint NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(150) NOT NULL,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL
);

CREATE SEQUENCE public.acc_account_type_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.acc_account_type_id_seq OWNED BY public.acc_account_type.id;

ALTER TABLE ONLY public.acc_account_type ALTER COLUMN id SET DEFAULT nextval('public.acc_account_type_id_seq'::regclass);

insert into public.acc_account_type (id, code, name, state_id, created_date) values
    ('1', 'active', 'Aktiv', '1', '2026-06-09 15:28:20.198084'),
    ('2', 'passive', 'Passiv', '1', '2026-06-09 15:28:20.198084'),
    ('3', 'active_passive', 'Aktiv-passiv', '1', '2026-06-09 15:28:20.198084'),
    ('4', 'off_balance', 'Balansdan tashqari', '1', '2026-06-09 15:28:20.198084');

SELECT pg_catalog.setval('public.acc_account_type_id_seq', 4, true);

ALTER TABLE ONLY public.acc_account_type
    ADD CONSTRAINT acc_account_type_pkey PRIMARY KEY (id);

CREATE UNIQUE INDEX idx_acc_account_type_code ON public.acc_account_type USING btree (code);

CREATE INDEX idx_acc_account_type_state_id ON public.acc_account_type USING btree (state_id);

ALTER TABLE ONLY public.acc_account_type
    ADD CONSTRAINT acc_account_type_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);
