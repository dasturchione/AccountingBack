-- Table: public.inv_product

CREATE TABLE public.inv_product (
    id integer NOT NULL,
    organization_id integer NOT NULL,
    product_group_id integer,
    unit_id smallint NOT NULL,
    barcode character varying(100),
    name character varying(250) NOT NULL,
    description character varying(1000),
    is_service boolean DEFAULT false NOT NULL,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL,
    mxik character varying(17),
    is_piece_tracked boolean DEFAULT false NOT NULL,
    code character varying(100),
    sku character varying(100),
    article character varying(100),
    default_vat_rate_id smallint,
    inventory_account_id integer,
    income_account_id integer,
    expense_account_id integer,
    cogs_account_id integer,
    min_stock numeric(18,3),
    CONSTRAINT ck_inv_product_mxik CHECK (((mxik IS NULL) OR ((mxik)::text ~ '^[A-Za-z0-9]{17}$'::text)))
);

CREATE SEQUENCE public.inv_product_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.inv_product_id_seq OWNED BY public.inv_product.id;

ALTER TABLE ONLY public.inv_product ALTER COLUMN id SET DEFAULT nextval('public.inv_product_id_seq'::regclass);

insert into public.inv_product (id, organization_id, product_group_id, unit_id, barcode, name, description, is_service, state_id, created_date, mxik, is_piece_tracked) values
    ('23', '8', '13', '1', '08418001001005219', 'ARTEL, икки камерали HD 316 FND ECO FROST қора-жилосиз ранг', '', 'f', '1', '2026-06-27 15:18:21.488187', '08418001001005219', 't'),
    ('24', '8', '13', '1', '08418001001005223', 'ARTEL, икки камерали HD 341 FND ECO FROST ёмғирли-асфалт ранг', '', 'f', '1', '2026-06-27 15:18:21.488261', '08418001001005223', 't'),
    ('25', '8', '14', '5', '09903001001000000', 'Газ таъминоти хизматлари', '', 't', '1', '2026-06-27 15:21:30.132046', '09903001001000000', 'f'),
    ('26', '8', '14', '5', '09905001001000000', 'Электр энергия хизматлари', '', 't', '1', '2026-06-27 15:21:30.132198', '09905001001000000', 'f');

SELECT pg_catalog.setval('public.inv_product_id_seq', 26, true);

ALTER TABLE ONLY public.inv_product
    ADD CONSTRAINT inv_product_pkey PRIMARY KEY (id);

CREATE INDEX idx_inv_product_barcode ON public.inv_product USING btree (barcode);

CREATE INDEX idx_inv_product_name ON public.inv_product USING btree (name);

CREATE INDEX idx_inv_product_organization_id ON public.inv_product USING btree (organization_id);

CREATE INDEX idx_inv_product_product_group_id ON public.inv_product USING btree (product_group_id);

CREATE INDEX idx_inv_product_state_id ON public.inv_product USING btree (state_id);

CREATE INDEX idx_inv_product_unit_id ON public.inv_product USING btree (unit_id);

CREATE INDEX ix_inv_product_mxik ON public.inv_product USING btree (mxik) WHERE (mxik IS NOT NULL);

ALTER TABLE ONLY public.inv_product
    ADD CONSTRAINT inv_product_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);

ALTER TABLE ONLY public.inv_product
    ADD CONSTRAINT inv_product_product_group_id_fkey FOREIGN KEY (product_group_id) REFERENCES public.inv_product_group(id);

ALTER TABLE ONLY public.inv_product
    ADD CONSTRAINT inv_product_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);

ALTER TABLE ONLY public.inv_product
    ADD CONSTRAINT inv_product_unit_id_fkey FOREIGN KEY (unit_id) REFERENCES public.cmn_unit(id);

CREATE INDEX idx_inv_product_code ON public.inv_product USING btree (code);

CREATE INDEX idx_inv_product_sku ON public.inv_product USING btree (sku);

CREATE INDEX idx_inv_product_article ON public.inv_product USING btree (article);

CREATE INDEX idx_inv_product_default_vat_rate_id ON public.inv_product USING btree (default_vat_rate_id);

CREATE INDEX idx_inv_product_inventory_account_id ON public.inv_product USING btree (inventory_account_id);

CREATE INDEX idx_inv_product_income_account_id ON public.inv_product USING btree (income_account_id);

CREATE INDEX idx_inv_product_expense_account_id ON public.inv_product USING btree (expense_account_id);

CREATE INDEX idx_inv_product_cogs_account_id ON public.inv_product USING btree (cogs_account_id);

CREATE UNIQUE INDEX uidx_inv_product_org_code ON public.inv_product USING btree (organization_id, code) WHERE (code IS NOT NULL);

ALTER TABLE ONLY public.inv_product
    ADD CONSTRAINT inv_product_default_vat_rate_id_fkey FOREIGN KEY (default_vat_rate_id) REFERENCES public.cmn_vat_rate(id);

ALTER TABLE ONLY public.inv_product
    ADD CONSTRAINT inv_product_inventory_account_id_fkey FOREIGN KEY (inventory_account_id) REFERENCES public.acc_chart_account(id);

ALTER TABLE ONLY public.inv_product
    ADD CONSTRAINT inv_product_income_account_id_fkey FOREIGN KEY (income_account_id) REFERENCES public.acc_chart_account(id);

ALTER TABLE ONLY public.inv_product
    ADD CONSTRAINT inv_product_expense_account_id_fkey FOREIGN KEY (expense_account_id) REFERENCES public.acc_chart_account(id);

ALTER TABLE ONLY public.inv_product
    ADD CONSTRAINT inv_product_cogs_account_id_fkey FOREIGN KEY (cogs_account_id) REFERENCES public.acc_chart_account(id);
