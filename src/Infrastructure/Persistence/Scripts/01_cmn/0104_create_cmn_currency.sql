-- Table: public.cmn_currency

CREATE TABLE public.cmn_currency (
    id smallint NOT NULL,
    code character varying(10) NOT NULL,
    name character varying(100) NOT NULL,
    symbol character varying(10),
    state_id smallint NOT NULL
);

CREATE SEQUENCE public.cmn_currency_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.cmn_currency_id_seq OWNED BY public.cmn_currency.id;

ALTER TABLE ONLY public.cmn_currency ALTER COLUMN id SET DEFAULT nextval('public.cmn_currency_id_seq'::regclass);

insert into public.cmn_currency (id, code, name, symbol, state_id) values
    ('1', 'UZS', 'Uzbek so''m', 'so''m', '1'),
    ('2', 'USD', 'US Dollar', '$', '1'),
    ('3', 'RUB', 'Russian Ruble', '₽', '1'),
    ('4', 'EUR', 'Euro', '€', '1');

SELECT pg_catalog.setval('public.cmn_currency_id_seq', 4, true);

ALTER TABLE ONLY public.cmn_currency
    ADD CONSTRAINT cmn_currency_pkey PRIMARY KEY (id);

CREATE UNIQUE INDEX idx_cmn_currency_code ON public.cmn_currency USING btree (code);

ALTER TABLE ONLY public.cmn_currency
    ADD CONSTRAINT cmn_currency_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);
