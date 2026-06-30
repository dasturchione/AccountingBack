-- Table: public.cmn_price_rounding_method

CREATE TABLE public.cmn_price_rounding_method (
    id smallint NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(100) NOT NULL
);

CREATE SEQUENCE public.cmn_price_rounding_method_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.cmn_price_rounding_method_id_seq OWNED BY public.cmn_price_rounding_method.id;

ALTER TABLE ONLY public.cmn_price_rounding_method ALTER COLUMN id SET DEFAULT nextval('public.cmn_price_rounding_method_id_seq'::regclass);

insert into public.cmn_price_rounding_method (id, code, name) values
    ('1', 'NONE', 'O''zgartirmasdan'),
    ('2', 'UP', 'Yuqoriga yaxlitlash'),
    ('3', 'DOWN', 'Pastga yaxlitlash'),
    ('4', 'NEAREST', 'Eng yaqin qiymatga yaxlitlash');

SELECT pg_catalog.setval('public.cmn_price_rounding_method_id_seq', 4, true);

ALTER TABLE ONLY public.cmn_price_rounding_method
    ADD CONSTRAINT cmn_price_rounding_method_code_key UNIQUE (code);

ALTER TABLE ONLY public.cmn_price_rounding_method
    ADD CONSTRAINT cmn_price_rounding_method_pkey PRIMARY KEY (id);
