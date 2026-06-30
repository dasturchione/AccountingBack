-- Table: public.inv_product_table

CREATE TABLE public.inv_product_table (
    id integer NOT NULL,
    product_id integer NOT NULL,
    organization_id integer NOT NULL,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL,
    serial_number character varying(250),
    marking_number character varying(250),
    status_id smallint DEFAULT 1 NOT NULL
);

CREATE SEQUENCE public.inv_product_table_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.inv_product_table_id_seq OWNED BY public.inv_product_table.id;

ALTER TABLE ONLY public.inv_product_table ALTER COLUMN id SET DEFAULT nextval('public.inv_product_table_id_seq'::regclass);

insert into public.inv_product_table (id, product_id, organization_id, state_id, created_date, serial_number, marking_number, status_id) values
    ('455', '23', '8', '1', '2026-06-27 18:07:17.497344', NULL, '0104780074206893217UkCJ6Gu*gG_j.wf6WnX91XUWh92JkB/xWNpbjVhcmtkWVdsT3lhVTVhYjdUQm8yQgOW3lE=', '1'),
    ('456', '23', '8', '1', '2026-06-27 18:07:17.497737', NULL, '010478007420371721NZVF0x+-fnd<lcpI11Wc91UZF092SDB2UZccAGRfCmSPNYJYumzGxmcnTaumTSS1HOIBuOE=', '1'),
    ('457', '23', '8', '1', '2026-06-27 18:07:17.497799', NULL, '0104780074203717210t*wRjFf>m9eK-''dqeRV91UZF092b0xBdzcw+3GSPfZP95mjWrYDKhom4PWWecsUayvv2sQ=', '1'),
    ('458', '23', '8', '1', '2026-06-27 18:07:17.497799', NULL, '010478010915122821)nUfDN1p=8G.G2nk8_sJ91UZF092d3lJcOvPHgtC3DxncIcpDZVxRY9BDkfyC1+gYcgxFyQ=', '1'),
    ('459', '23', '8', '1', '2026-06-29 14:59:14.793003', NULL, 'qwdqdqwdqwdqwdqdqd', '1'),
    ('460', '23', '8', '1', '2026-06-29 14:59:14.793431', NULL, 'qdqdqdqdqdqdqdqdqdq2855', '1'),
    ('461', '23', '8', '1', '2026-06-29 14:59:14.793497', NULL, 'qdqdqdqdqdq98d4q8d789qwd7q', '1'),
    ('463', '24', '8', '1', '2026-06-29 15:01:54.008417', NULL, 'aohjfoiwhio', '1'),
    ('462', '24', '8', '1', '2026-06-29 15:01:54.008414', NULL, 'fwliejfoiwjfopwjp''ef856', '2');

SELECT pg_catalog.setval('public.inv_product_table_id_seq', 463, true);

ALTER TABLE ONLY public.inv_product_table
    ADD CONSTRAINT inv_product_table_pkey PRIMARY KEY (id);

CREATE INDEX ix_inv_product_table_status_id ON public.inv_product_table USING btree (status_id);

CREATE UNIQUE INDEX ux_inv_product_table_org_marking ON public.inv_product_table USING btree (organization_id, marking_number) WHERE (marking_number IS NOT NULL);

CREATE UNIQUE INDEX ux_inv_product_table_org_serial ON public.inv_product_table USING btree (organization_id, serial_number) WHERE (serial_number IS NOT NULL);

ALTER TABLE ONLY public.inv_product_table
    ADD CONSTRAINT inv_product_table_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);

ALTER TABLE ONLY public.inv_product_table
    ADD CONSTRAINT inv_product_table_product_id_fkey FOREIGN KEY (product_id) REFERENCES public.inv_product(id);

ALTER TABLE ONLY public.inv_product_table
    ADD CONSTRAINT inv_product_table_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);

ALTER TABLE ONLY public.inv_product_table
    ADD CONSTRAINT inv_product_table_status_id_fkey FOREIGN KEY (status_id) REFERENCES public.cmn_product_table_status(id);
