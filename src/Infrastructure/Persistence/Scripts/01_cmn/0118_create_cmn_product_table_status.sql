-- Table: public.cmn_product_table_status

CREATE TABLE public.cmn_product_table_status (
    id smallint NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(100) NOT NULL,
    state_id smallint NOT NULL
);

CREATE SEQUENCE public.cmn_product_table_status_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.cmn_product_table_status_id_seq OWNED BY public.cmn_product_table_status.id;

ALTER TABLE ONLY public.cmn_product_table_status ALTER COLUMN id SET DEFAULT nextval('public.cmn_product_table_status_id_seq'::regclass);

insert into public.cmn_product_table_status (id, code, name, state_id) values
    ('1', 'IN_STOCK', 'На складе', '1'),
    ('2', 'RESERVED', 'Зарезервирован', '1'),
    ('3', 'SOLD', 'Продан', '1'),
    ('4', 'RETURNED_TO_SUPPLIER', 'Возвращен поставщику', '1'),
    ('5', 'RETURNED_FROM_CUSTOMER', 'Возвращен покупателем', '1'),
    ('6', 'WRITTEN_OFF', 'Списан', '1'),
    ('7', 'LOST', 'Утерян', '1'),
    ('8', 'BLOCKED', 'Заблокирован', '1');

SELECT pg_catalog.setval('public.cmn_product_table_status_id_seq', 8, true);

ALTER TABLE ONLY public.cmn_product_table_status
    ADD CONSTRAINT cmn_product_table_status_code_key UNIQUE (code);

ALTER TABLE ONLY public.cmn_product_table_status
    ADD CONSTRAINT cmn_product_table_status_pkey PRIMARY KEY (id);

ALTER TABLE ONLY public.cmn_product_table_status
    ADD CONSTRAINT cmn_product_table_status_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);
