-- Table: public.cmn_counterparty_type

CREATE TABLE public.cmn_counterparty_type (
    id smallint NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(100) NOT NULL,
    state_id smallint NOT NULL
);

CREATE SEQUENCE public.cmn_counterparty_type_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.cmn_counterparty_type_id_seq OWNED BY public.cmn_counterparty_type.id;

ALTER TABLE ONLY public.cmn_counterparty_type ALTER COLUMN id SET DEFAULT nextval('public.cmn_counterparty_type_id_seq'::regclass);

insert into public.cmn_counterparty_type (id, code, name, state_id) values
    ('1', 'client', 'Mijoz', '1'),
    ('2', 'supplier', 'Yetkazib beruvchi', '1'),
    ('3', 'client_supplier', 'Mijoz va yetkazib beruvchi', '1');

SELECT pg_catalog.setval('public.cmn_counterparty_type_id_seq', 3, true);

ALTER TABLE ONLY public.cmn_counterparty_type
    ADD CONSTRAINT cmn_counterparty_type_pkey PRIMARY KEY (id);

CREATE UNIQUE INDEX idx_cmn_counterparty_type_code ON public.cmn_counterparty_type USING btree (code);

ALTER TABLE ONLY public.cmn_counterparty_type
    ADD CONSTRAINT cmn_counterparty_type_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);
