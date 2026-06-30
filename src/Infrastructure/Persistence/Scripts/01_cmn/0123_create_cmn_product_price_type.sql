-- Table: public.cmn_product_price_type

CREATE TABLE public.cmn_product_price_type (
    id smallint NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(150) NOT NULL
);

CREATE SEQUENCE public.cmn_product_price_type_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.cmn_product_price_type_id_seq OWNED BY public.cmn_product_price_type.id;

ALTER TABLE ONLY public.cmn_product_price_type ALTER COLUMN id SET DEFAULT nextval('public.cmn_product_price_type_id_seq'::regclass);

insert into public.cmn_product_price_type (id, code, name) values
    ('1', 'AVERAGE_COST_PRICE', 'Hisoblangan o''rtacha tannarx'),
    ('2', 'FIXED_SALE_PRICE', 'Belgilangan sotuv narxi');

SELECT pg_catalog.setval('public.cmn_product_price_type_id_seq', 2, true);

ALTER TABLE ONLY public.cmn_product_price_type
    ADD CONSTRAINT cmn_product_price_type_code_key UNIQUE (code);

ALTER TABLE ONLY public.cmn_product_price_type
    ADD CONSTRAINT cmn_product_price_type_pkey PRIMARY KEY (id);
