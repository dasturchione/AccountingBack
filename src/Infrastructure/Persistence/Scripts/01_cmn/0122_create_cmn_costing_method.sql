-- Table: public.cmn_costing_method

CREATE TABLE public.cmn_costing_method (
    id smallint NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(100) NOT NULL
);

CREATE SEQUENCE public.cmn_costing_method_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.cmn_costing_method_id_seq OWNED BY public.cmn_costing_method.id;

ALTER TABLE ONLY public.cmn_costing_method ALTER COLUMN id SET DEFAULT nextval('public.cmn_costing_method_id_seq'::regclass);

insert into public.cmn_costing_method (id, code, name) values
    ('1', 'FIFO', 'FIFO - birinchi kirgan birinchi chiqadi'),
    ('2', 'LIFO', 'LIFO - oxirgi kirgan birinchi chiqadi'),
    ('3', 'AVERAGE', 'O''rtacha tannarx');

SELECT pg_catalog.setval('public.cmn_costing_method_id_seq', 3, true);

ALTER TABLE ONLY public.cmn_costing_method
    ADD CONSTRAINT cmn_costing_method_code_key UNIQUE (code);

ALTER TABLE ONLY public.cmn_costing_method
    ADD CONSTRAINT cmn_costing_method_pkey PRIMARY KEY (id);
