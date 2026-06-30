-- Table: public.pur_doc_table

CREATE TABLE public.pur_doc_table (
    id bigint NOT NULL,
    owner_id bigint NOT NULL,
    product_table_id integer NOT NULL,
    amount numeric(24,8) NOT NULL,
    vat_rate_id smallint,
    vat_amount numeric(24,8) NOT NULL,
    total_amount numeric(24,8) NOT NULL
);

CREATE SEQUENCE public.pur_doc_table_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.pur_doc_table_id_seq OWNED BY public.pur_doc_table.id;

ALTER TABLE ONLY public.pur_doc_table ALTER COLUMN id SET DEFAULT nextval('public.pur_doc_table_id_seq'::regclass);

insert into public.pur_doc_table (id, owner_id, product_table_id, amount, vat_rate_id, vat_amount, total_amount) values
    ('15', '14', '455', '10000.00000000', '2', '1200.00000000', '11200.00000000'),
    ('16', '14', '456', '10000.00000000', '2', '1200.00000000', '11200.00000000'),
    ('17', '14', '457', '10000.00000000', '2', '1200.00000000', '11200.00000000'),
    ('18', '14', '458', '10000.00000000', '2', '1200.00000000', '11200.00000000'),
    ('19', '17', '459', '11000.00000000', '2', '1320.00000000', '12320.00000000'),
    ('20', '17', '460', '11000.00000000', '2', '1320.00000000', '12320.00000000'),
    ('21', '17', '461', '11000.00000000', '2', '1320.00000000', '12320.00000000'),
    ('22', '18', '462', '15000.00000000', '3', '2250.00000000', '17250.00000000'),
    ('23', '18', '463', '15000.00000000', '3', '2250.00000000', '17250.00000000');

SELECT pg_catalog.setval('public.pur_doc_table_id_seq', 23, true);

ALTER TABLE ONLY public.pur_doc_table
    ADD CONSTRAINT pur_doc_table_pkey PRIMARY KEY (id);

CREATE INDEX ix_pur_doc_table_owner_id_id ON public.pur_doc_table USING btree (owner_id, id);

CREATE UNIQUE INDEX ux_pur_doc_table_owner_id_product_table_id ON public.pur_doc_table USING btree (owner_id, product_table_id);

ALTER TABLE ONLY public.pur_doc_table
    ADD CONSTRAINT pur_doc_table_owner_id_fkey FOREIGN KEY (owner_id) REFERENCES public.pur_doc_product(id);

ALTER TABLE ONLY public.pur_doc_table
    ADD CONSTRAINT pur_doc_table_product_table_id_fkey FOREIGN KEY (product_table_id) REFERENCES public.inv_product_table(id);

ALTER TABLE ONLY public.pur_doc_table
    ADD CONSTRAINT pur_doc_table_vat_rate_id_fkey FOREIGN KEY (vat_rate_id) REFERENCES public.cmn_vat_rate(id);
