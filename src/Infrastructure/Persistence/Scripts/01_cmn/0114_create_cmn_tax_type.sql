-- Table: public.cmn_tax_type

CREATE TABLE public.cmn_tax_type (
    id smallint NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(150) NOT NULL,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL
);

CREATE SEQUENCE public.cmn_tax_type_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.cmn_tax_type_id_seq OWNED BY public.cmn_tax_type.id;

ALTER TABLE ONLY public.cmn_tax_type ALTER COLUMN id SET DEFAULT nextval('public.cmn_tax_type_id_seq'::regclass);

insert into public.cmn_tax_type (id, code, name, state_id, created_date) values
    ('1', 'none', 'Soliqsiz', '1', '2026-06-06 16:40:32.193179'),
    ('2', 'vat', 'QQS', '1', '2026-06-06 16:40:32.193179');

SELECT pg_catalog.setval('public.cmn_tax_type_id_seq', 2, true);

ALTER TABLE ONLY public.cmn_tax_type
    ADD CONSTRAINT cmn_tax_type_pkey PRIMARY KEY (id);

CREATE UNIQUE INDEX idx_cmn_tax_type_code ON public.cmn_tax_type USING btree (code);

CREATE INDEX idx_cmn_tax_type_state_id ON public.cmn_tax_type USING btree (state_id);

ALTER TABLE ONLY public.cmn_tax_type
    ADD CONSTRAINT cmn_tax_type_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);
