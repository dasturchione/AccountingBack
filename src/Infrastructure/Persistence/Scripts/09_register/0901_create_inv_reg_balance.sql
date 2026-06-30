-- Table: public.inv_reg_balance

CREATE TABLE public.inv_reg_balance (
    id bigint NOT NULL,
    organization_id integer NOT NULL,
    document_type_id smallint NOT NULL,
    document_id bigint NOT NULL,
    warehouse_id integer NOT NULL,
    product_id integer NOT NULL,
    operation_type_id smallint NOT NULL,
    quantity numeric(18,3) NOT NULL,
    amount numeric(18,2) NOT NULL,
    doc_date timestamp without time zone NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL,
    posting_batch_id bigint,
    source_line_id bigint,
    reversal_entry_id bigint
);

CREATE SEQUENCE public.inv_reg_balance_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.inv_reg_balance_id_seq OWNED BY public.inv_reg_balance.id;

ALTER TABLE ONLY public.inv_reg_balance ALTER COLUMN id SET DEFAULT nextval('public.inv_reg_balance_id_seq'::regclass);

insert into public.inv_reg_balance (id, organization_id, document_type_id, document_id, warehouse_id, product_id, operation_type_id, quantity, amount, doc_date, created_date) values
    ('203', '8', '1', '92', '7', '23', '1', '1.000', '11200.00', '2026-06-27 18:05:43', '2026-06-27 18:07:18.670817'),
    ('204', '8', '1', '92', '7', '23', '1', '1.000', '11200.00', '2026-06-27 18:05:43', '2026-06-27 18:07:18.670939'),
    ('205', '8', '1', '92', '7', '23', '1', '1.000', '11200.00', '2026-06-27 18:05:43', '2026-06-27 18:07:18.67094'),
    ('206', '8', '1', '92', '7', '23', '1', '1.000', '11200.00', '2026-06-27 18:05:43', '2026-06-27 18:07:18.67094'),
    ('207', '8', '1', '95', '7', '23', '1', '1.000', '12320.00', '2026-06-15 14:58:00', '2026-06-29 14:59:16.180774'),
    ('208', '8', '1', '95', '7', '23', '1', '1.000', '12320.00', '2026-06-15 14:58:00', '2026-06-29 14:59:16.180929'),
    ('209', '8', '1', '95', '7', '23', '1', '1.000', '12320.00', '2026-06-15 14:58:00', '2026-06-29 14:59:16.180931'),
    ('210', '8', '1', '96', '7', '24', '1', '1.000', '17250.00', '2026-06-29 14:59:15', '2026-06-29 15:01:54.095025'),
    ('211', '8', '1', '96', '7', '24', '1', '1.000', '17250.00', '2026-06-29 14:59:15', '2026-06-29 15:01:54.095028');

SELECT pg_catalog.setval('public.inv_reg_balance_id_seq', 211, true);

ALTER TABLE ONLY public.inv_reg_balance
    ADD CONSTRAINT inv_reg_balance_pkey PRIMARY KEY (id);

CREATE INDEX idx_inv_reg_balance_doc_date ON public.inv_reg_balance USING btree (doc_date);

CREATE INDEX idx_inv_reg_balance_document ON public.inv_reg_balance USING btree (document_type_id, document_id);

CREATE INDEX idx_inv_reg_balance_organization_id ON public.inv_reg_balance USING btree (organization_id);

CREATE INDEX idx_inv_reg_balance_product_id ON public.inv_reg_balance USING btree (product_id);

CREATE INDEX idx_inv_reg_balance_warehouse_id ON public.inv_reg_balance USING btree (warehouse_id);

ALTER TABLE ONLY public.inv_reg_balance
    ADD CONSTRAINT inv_reg_balance_document_type_id_fkey FOREIGN KEY (document_type_id) REFERENCES public.cmn_document_type(id);

ALTER TABLE ONLY public.inv_reg_balance
    ADD CONSTRAINT inv_reg_balance_operation_type_id_fkey FOREIGN KEY (operation_type_id) REFERENCES public.cmn_operation_type(id);

ALTER TABLE ONLY public.inv_reg_balance
    ADD CONSTRAINT inv_reg_balance_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);

ALTER TABLE ONLY public.inv_reg_balance
    ADD CONSTRAINT inv_reg_balance_product_id_fkey FOREIGN KEY (product_id) REFERENCES public.inv_product(id);

ALTER TABLE ONLY public.inv_reg_balance
    ADD CONSTRAINT inv_reg_balance_warehouse_id_fkey FOREIGN KEY (warehouse_id) REFERENCES public.inv_warehouse(id);

CREATE INDEX idx_inv_reg_balance_posting_batch_id ON public.inv_reg_balance USING btree (posting_batch_id);

CREATE INDEX idx_inv_reg_balance_reversal_entry_id ON public.inv_reg_balance USING btree (reversal_entry_id);

ALTER TABLE ONLY public.inv_reg_balance
    ADD CONSTRAINT inv_reg_balance_posting_batch_id_fkey FOREIGN KEY (posting_batch_id) REFERENCES public.acc_posting_batch(id);
