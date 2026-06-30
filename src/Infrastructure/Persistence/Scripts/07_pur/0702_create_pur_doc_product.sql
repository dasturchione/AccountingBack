-- Table: public.pur_doc_product

CREATE TABLE public.pur_doc_product (
    id bigint NOT NULL,
    owner_id bigint NOT NULL,
    product_id integer NOT NULL,
    quantity numeric(19,6) NOT NULL,
    unit_id smallint NOT NULL,
    amount numeric(24,8) NOT NULL,
    vat_rate_id smallint,
    vat_amount numeric(24,8) NOT NULL,
    total_amount numeric(24,8) NOT NULL,
    unit_price numeric(24,8) NOT NULL
);

CREATE SEQUENCE public.pur_doc_product_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.pur_doc_product_id_seq OWNED BY public.pur_doc_product.id;

ALTER TABLE ONLY public.pur_doc_product ALTER COLUMN id SET DEFAULT nextval('public.pur_doc_product_id_seq'::regclass);

insert into public.pur_doc_product (id, owner_id, product_id, quantity, unit_id, amount, vat_rate_id, vat_amount, total_amount, unit_price) values
    ('14', '92', '23', '4.000000', '1', '40000.00000000', '2', '4800.00000000', '44800.00000000', '10000.00000000'),
    ('15', '93', '25', '2.000000', '5', '10000.00000000', '2', '1200.00000000', '11200.00000000', '5000.00000000'),
    ('16', '94', '25', '1.000000', '5', '10000.00000000', '2', '1200.00000000', '11200.00000000', '10000.00000000'),
    ('17', '95', '23', '3.000000', '1', '33000.00000000', '2', '3960.00000000', '36960.00000000', '11000.00000000'),
    ('18', '96', '24', '2.000000', '1', '30000.00000000', '3', '4500.00000000', '34500.00000000', '15000.00000000'),
    ('19', '97', '25', '2.000000', '5', '20000.00000000', '2', '2400.00000000', '22400.00000000', '10000.00000000');

SELECT pg_catalog.setval('public.pur_doc_product_id_seq', 19, true);

ALTER TABLE ONLY public.pur_doc_product
    ADD CONSTRAINT pur_doc_product_pkey PRIMARY KEY (id);

CREATE INDEX ix_pur_doc_product_owner_id ON public.pur_doc_product USING btree (owner_id);

CREATE INDEX ix_pur_doc_product_product_id ON public.pur_doc_product USING btree (product_id) WHERE (product_id IS NOT NULL);

ALTER TABLE ONLY public.pur_doc_product
    ADD CONSTRAINT pur_doc_product_owner_id_fkey FOREIGN KEY (owner_id) REFERENCES public.pur_doc(id);

ALTER TABLE ONLY public.pur_doc_product
    ADD CONSTRAINT pur_doc_product_product_id_fkey FOREIGN KEY (product_id) REFERENCES public.inv_product(id);

ALTER TABLE ONLY public.pur_doc_product
    ADD CONSTRAINT pur_doc_product_unit_id_fkey FOREIGN KEY (unit_id) REFERENCES public.cmn_unit(id);

ALTER TABLE ONLY public.pur_doc_product
    ADD CONSTRAINT pur_doc_product_vat_rate_id_fkey FOREIGN KEY (vat_rate_id) REFERENCES public.cmn_vat_rate(id);
