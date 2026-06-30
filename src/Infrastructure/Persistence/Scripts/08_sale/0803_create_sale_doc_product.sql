-- Table: public.sale_doc_product

CREATE TABLE public.sale_doc_product (
    id bigint NOT NULL,
    owner_id bigint NOT NULL,
    product_id integer NOT NULL,
    quantity numeric(19,6) NOT NULL,
    unit_price numeric(24,8) NOT NULL,
    cost_price numeric(24,8) DEFAULT 0 NOT NULL,
    amount numeric(24,8) NOT NULL,
    vat_rate_id smallint,
    vat_amount numeric(24,8) DEFAULT 0 NOT NULL,
    total_amount numeric(24,8) NOT NULL,
    unit_id smallint DEFAULT 1 NOT NULL
);

CREATE SEQUENCE public.sale_doc_product_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.sale_doc_product_id_seq OWNED BY public.sale_doc_product.id;

ALTER TABLE ONLY public.sale_doc_product ALTER COLUMN id SET DEFAULT nextval('public.sale_doc_product_id_seq'::regclass);

insert into public.sale_doc_product (id, owner_id, product_id, quantity, unit_price, cost_price, amount, vat_rate_id, vat_amount, total_amount, unit_id) values
    ('26', '78', '24', '1.000000', '18975.00000000', '17250.00000000', '18975.00000000', '2', '2277.00000000', '21252.00000000', '1'),
    ('28', '80', '23', '5.000000', '13059.20000000', '11872.00000000', '65296.00000000', '2', '7835.52000000', '73131.52000000', '1');

SELECT pg_catalog.setval('public.sale_doc_product_id_seq', 28, true);

ALTER TABLE ONLY public.sale_doc_product
    ADD CONSTRAINT sale_doc_product_pkey PRIMARY KEY (id);

CREATE INDEX ix_sale_doc_product_owner_id ON public.sale_doc_product USING btree (owner_id);

CREATE INDEX ix_sale_doc_product_product_id ON public.sale_doc_product USING btree (product_id);

ALTER TABLE ONLY public.sale_doc_product
    ADD CONSTRAINT sale_doc_product_owner_id_fkey FOREIGN KEY (owner_id) REFERENCES public.sale_doc(id) ON DELETE CASCADE;

ALTER TABLE ONLY public.sale_doc_product
    ADD CONSTRAINT sale_doc_product_product_id_fkey FOREIGN KEY (product_id) REFERENCES public.inv_product(id);

ALTER TABLE ONLY public.sale_doc_product
    ADD CONSTRAINT sale_doc_product_unit_id_fkey FOREIGN KEY (unit_id) REFERENCES public.cmn_unit(id);

ALTER TABLE ONLY public.sale_doc_product
    ADD CONSTRAINT sale_doc_product_vat_rate_id_fkey FOREIGN KEY (vat_rate_id) REFERENCES public.cmn_vat_rate(id);
