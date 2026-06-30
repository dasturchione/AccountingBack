-- Table: public.inv_product_price

CREATE TABLE public.inv_product_price (
    id bigint NOT NULL,
    organization_id integer NOT NULL,
    product_id integer NOT NULL,
    currency_id smallint NOT NULL,
    price_type_id smallint NOT NULL,
    unit_id smallint NOT NULL,
    price numeric(24,8) NOT NULL,
    start_date timestamp without time zone DEFAULT now() NOT NULL,
    end_date timestamp without time zone,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL,
    CONSTRAINT ck_inv_product_price_dates CHECK (((end_date IS NULL) OR (end_date >= start_date))),
    CONSTRAINT ck_inv_product_price_price CHECK ((price >= (0)::numeric))
);

CREATE SEQUENCE public.inv_product_price_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.inv_product_price_id_seq OWNED BY public.inv_product_price.id;

ALTER TABLE ONLY public.inv_product_price ALTER COLUMN id SET DEFAULT nextval('public.inv_product_price_id_seq'::regclass);

insert into public.inv_product_price (id, organization_id, product_id, currency_id, price_type_id, unit_id, price, start_date, end_date, state_id, created_date) values
    ('1', '8', '23', '1', '1', '1', '11680.00000000', '2026-06-01 00:00:00', NULL, '1', '2026-06-29 14:56:50.128533'),
    ('2', '8', '24', '1', '1', '1', '17250.00000000', '2026-06-29 15:01:54.112203', NULL, '1', '2026-06-29 15:01:54.112203');

SELECT pg_catalog.setval('public.inv_product_price_id_seq', 2, true);

ALTER TABLE ONLY public.inv_product_price
    ADD CONSTRAINT inv_product_price_pkey PRIMARY KEY (id);

CREATE INDEX idx_inv_product_price_currency_id ON public.inv_product_price USING btree (currency_id);

CREATE INDEX idx_inv_product_price_dates ON public.inv_product_price USING btree (start_date, end_date);

CREATE INDEX idx_inv_product_price_organization_id ON public.inv_product_price USING btree (organization_id);

CREATE INDEX idx_inv_product_price_price_type_id ON public.inv_product_price USING btree (price_type_id);

CREATE INDEX idx_inv_product_price_product_id ON public.inv_product_price USING btree (product_id);

CREATE INDEX idx_inv_product_price_product_type_dates ON public.inv_product_price USING btree (organization_id, product_id, price_type_id, state_id, start_date, end_date);

CREATE INDEX idx_inv_product_price_state_id ON public.inv_product_price USING btree (state_id);

ALTER TABLE ONLY public.inv_product_price
    ADD CONSTRAINT inv_product_price_currency_id_fkey FOREIGN KEY (currency_id) REFERENCES public.cmn_currency(id);

ALTER TABLE ONLY public.inv_product_price
    ADD CONSTRAINT inv_product_price_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);

ALTER TABLE ONLY public.inv_product_price
    ADD CONSTRAINT inv_product_price_price_type_id_fkey FOREIGN KEY (price_type_id) REFERENCES public.cmn_product_price_type(id);

ALTER TABLE ONLY public.inv_product_price
    ADD CONSTRAINT inv_product_price_product_id_fkey FOREIGN KEY (product_id) REFERENCES public.inv_product(id);

ALTER TABLE ONLY public.inv_product_price
    ADD CONSTRAINT inv_product_price_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);

ALTER TABLE ONLY public.inv_product_price
    ADD CONSTRAINT inv_product_price_unit_id_fkey FOREIGN KEY (unit_id) REFERENCES public.cmn_unit(id);
