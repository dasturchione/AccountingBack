-- Table: public.cmn_bank

CREATE TABLE public.cmn_bank (
    id integer NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(250) NOT NULL,
    mfo character varying(20),
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL
);

CREATE SEQUENCE public.cmn_bank_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.cmn_bank_id_seq OWNED BY public.cmn_bank.id;

ALTER TABLE ONLY public.cmn_bank ALTER COLUMN id SET DEFAULT nextval('public.cmn_bank_id_seq'::regclass);

insert into public.cmn_bank (id, code, name, mfo, state_id, created_date) values
    ('3', 'KAPITAL', 'Kapitalbank', NULL, '1', '2026-06-06 16:39:53.224198'),
    ('4', 'HAMKOR', 'Hamkorbank', NULL, '1', '2026-06-06 16:39:53.224198'),
    ('5', 'CODEX191315', 'Codex Test Bank Updated', '99998', '1', '2026-06-24 19:13:15.478874'),
    ('2', 'IPOTEKA', 'Ipoteka bank', NULL, '1', '2026-06-06 16:39:53.224198'),
    ('1', 'NBU', 'Milliy bank', NULL, '1', '2026-06-06 16:39:53.224198'),
    ('6', 'Agro', 'AgroBank', '1234', '1', '2026-06-25 13:53:41.945698');

SELECT pg_catalog.setval('public.cmn_bank_id_seq', 6, true);

ALTER TABLE ONLY public.cmn_bank
    ADD CONSTRAINT cmn_bank_pkey PRIMARY KEY (id);

CREATE UNIQUE INDEX idx_cmn_bank_code ON public.cmn_bank USING btree (code);

CREATE INDEX idx_cmn_bank_state_id ON public.cmn_bank USING btree (state_id);

ALTER TABLE ONLY public.cmn_bank
    ADD CONSTRAINT cmn_bank_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);
