-- Table: public.cmn_contract_type

CREATE TABLE public.cmn_contract_type (
    id smallint NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(150) NOT NULL,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL
);

CREATE SEQUENCE public.cmn_contract_type_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.cmn_contract_type_id_seq OWNED BY public.cmn_contract_type.id;

ALTER TABLE ONLY public.cmn_contract_type ALTER COLUMN id SET DEFAULT nextval('public.cmn_contract_type_id_seq'::regclass);

insert into public.cmn_contract_type (id, code, name, state_id, created_date) values
    ('1', 'supplier', 'Yetkazib beruvchi bilan shartnoma', '1', '2026-06-17 17:35:47.346557'),
    ('2', 'customer', 'Xaridor bilan shartnoma', '1', '2026-06-17 17:35:47.346557');

SELECT pg_catalog.setval('public.cmn_contract_type_id_seq', 2, true);

ALTER TABLE ONLY public.cmn_contract_type
    ADD CONSTRAINT cmn_contract_type_pkey PRIMARY KEY (id);

CREATE UNIQUE INDEX idx_cmn_contract_type_code ON public.cmn_contract_type USING btree (code);

CREATE INDEX idx_cmn_contract_type_state_id ON public.cmn_contract_type USING btree (state_id);

ALTER TABLE ONLY public.cmn_contract_type
    ADD CONSTRAINT cmn_contract_type_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);
