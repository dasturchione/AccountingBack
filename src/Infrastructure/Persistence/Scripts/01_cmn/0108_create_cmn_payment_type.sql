-- Table: public.cmn_payment_type

CREATE TABLE public.cmn_payment_type (
    id smallint NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(100) NOT NULL,
    state_id smallint NOT NULL
);

CREATE SEQUENCE public.cmn_payment_type_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.cmn_payment_type_id_seq OWNED BY public.cmn_payment_type.id;

ALTER TABLE ONLY public.cmn_payment_type ALTER COLUMN id SET DEFAULT nextval('public.cmn_payment_type_id_seq'::regclass);

insert into public.cmn_payment_type (id, code, name, state_id) values
    ('1', 'cash', 'Naqd', '1'),
    ('2', 'bank', 'Bank', '1'),
    ('3', 'card', 'Karta', '1'),
    ('4', 'transfer', 'O''tkazma', '1');

SELECT pg_catalog.setval('public.cmn_payment_type_id_seq', 4, true);

ALTER TABLE ONLY public.cmn_payment_type
    ADD CONSTRAINT cmn_payment_type_pkey PRIMARY KEY (id);

CREATE UNIQUE INDEX idx_cmn_payment_type_code ON public.cmn_payment_type USING btree (code);

ALTER TABLE ONLY public.cmn_payment_type
    ADD CONSTRAINT cmn_payment_type_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);
