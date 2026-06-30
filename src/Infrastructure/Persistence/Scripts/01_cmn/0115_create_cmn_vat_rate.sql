-- Table: public.cmn_vat_rate

CREATE TABLE public.cmn_vat_rate (
    id smallint NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(150) NOT NULL,
    rate numeric(5,2) NOT NULL,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL
);

CREATE SEQUENCE public.cmn_vat_rate_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.cmn_vat_rate_id_seq OWNED BY public.cmn_vat_rate.id;

ALTER TABLE ONLY public.cmn_vat_rate ALTER COLUMN id SET DEFAULT nextval('public.cmn_vat_rate_id_seq'::regclass);

insert into public.cmn_vat_rate (id, code, name, rate, state_id, created_date) values
    ('1', 'vat_0', 'QQS 0%', '0.00', '1', '2026-06-06 16:40:45.727928'),
    ('2', 'vat_12', 'QQS 12%', '12.00', '1', '2026-06-06 16:40:45.727928'),
    ('3', 'vat_15', 'QQS 15%', '15.00', '1', '2026-06-06 16:40:45.727928'),
    ('4', 'vat_6', 'QQS 6%', '6.00', '1', '2026-06-27 13:53:02.619573');

SELECT pg_catalog.setval('public.cmn_vat_rate_id_seq', 4, true);

ALTER TABLE ONLY public.cmn_vat_rate
    ADD CONSTRAINT cmn_vat_rate_pkey PRIMARY KEY (id);

CREATE UNIQUE INDEX idx_cmn_vat_rate_code ON public.cmn_vat_rate USING btree (code);

CREATE INDEX idx_cmn_vat_rate_state_id ON public.cmn_vat_rate USING btree (state_id);

ALTER TABLE ONLY public.cmn_vat_rate
    ADD CONSTRAINT cmn_vat_rate_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);
