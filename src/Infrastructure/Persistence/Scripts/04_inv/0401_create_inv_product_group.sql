-- Table: public.inv_product_group

CREATE TABLE public.inv_product_group (
    id integer NOT NULL,
    organization_id integer NOT NULL,
    name character varying(250) NOT NULL,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL
);

CREATE SEQUENCE public.inv_product_group_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.inv_product_group_id_seq OWNED BY public.inv_product_group.id;

ALTER TABLE ONLY public.inv_product_group ALTER COLUMN id SET DEFAULT nextval('public.inv_product_group_id_seq'::regclass);

insert into public.inv_product_group (id, organization_id, name, state_id, created_date) values
    ('13', '8', 'Muzlatgichlar', '1', '2026-06-27 15:18:21.48753'),
    ('14', '8', 'Komunnal xizmatlar', '1', '2026-06-27 15:21:30.13086');

SELECT pg_catalog.setval('public.inv_product_group_id_seq', 14, true);

ALTER TABLE ONLY public.inv_product_group
    ADD CONSTRAINT inv_product_group_pkey PRIMARY KEY (id);

CREATE INDEX idx_inv_product_group_organization_id ON public.inv_product_group USING btree (organization_id);

CREATE INDEX idx_inv_product_group_state_id ON public.inv_product_group USING btree (state_id);

ALTER TABLE ONLY public.inv_product_group
    ADD CONSTRAINT inv_product_group_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);

ALTER TABLE ONLY public.inv_product_group
    ADD CONSTRAINT inv_product_group_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);
