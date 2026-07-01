-- Table: public.sale_doc_table

CREATE TABLE public.sale_doc_table (
    id bigint NOT NULL,
    product_table_id integer NOT NULL,
    amount numeric(24,8) NOT NULL,
    vat_rate_id smallint,
    vat_amount numeric(24,8) DEFAULT 0 NOT NULL,
    total_amount numeric(24,8) NOT NULL,
    cost_price numeric(24,8) DEFAULT 0 NOT NULL,
    owner_id bigint NOT NULL
);

CREATE SEQUENCE public.sale_doc_table_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.sale_doc_table_id_seq OWNED BY public.sale_doc_table.id;

ALTER TABLE ONLY public.sale_doc_table ALTER COLUMN id SET DEFAULT nextval('public.sale_doc_table_id_seq'::regclass);

insert into public.sale_doc_table (id, product_table_id, amount, vat_rate_id, vat_amount, total_amount, cost_price, owner_id) values
    ('98', '462', '18975.00000000', '2', '2277.00000000', '21252.00000000', '17250.00000000', '26');

SELECT pg_catalog.setval('public.sale_doc_table_id_seq', 98, true);

ALTER TABLE ONLY public.sale_doc_table
    ADD CONSTRAINT sale_doc_table_pkey PRIMARY KEY (id);

CREATE INDEX idx_sale_doc_table_product_id ON public.sale_doc_table USING btree (product_table_id);

CREATE INDEX idx_sale_doc_table_vat_rate_id ON public.sale_doc_table USING btree (vat_rate_id);

CREATE INDEX ix_sale_doc_table_owner_id ON public.sale_doc_table USING btree (owner_id);
CREATE UNIQUE INDEX ux_sale_doc_table_owner_product_table ON public.sale_doc_table USING btree (owner_id, product_table_id);

ALTER TABLE ONLY public.sale_doc_table
    ADD CONSTRAINT sale_doc_table_owner_id_fkey FOREIGN KEY (owner_id) REFERENCES public.sale_doc_product(id);

ALTER TABLE ONLY public.sale_doc_table
    ADD CONSTRAINT sale_doc_table_product_table_id_fkey FOREIGN KEY (product_table_id) REFERENCES public.inv_product_table(id);

ALTER TABLE ONLY public.sale_doc_table
    ADD CONSTRAINT sale_doc_table_vat_rate_id_fkey FOREIGN KEY (vat_rate_id) REFERENCES public.cmn_vat_rate(id);
