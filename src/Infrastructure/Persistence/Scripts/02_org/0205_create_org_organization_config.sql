-- Table: public.org_organization_config

CREATE TABLE public.org_organization_config (
    organization_id integer NOT NULL,
    inventory_valuation_method character varying(20) DEFAULT 'fifo'::character varying NOT NULL,
    CONSTRAINT org_organization_config_inventory_valuation_method_check CHECK (((inventory_valuation_method)::text = ANY ((ARRAY['fifo'::character varying, 'lifo'::character varying, 'average'::character varying])::text[])))
);

ALTER TABLE ONLY public.org_organization_config
    ADD CONSTRAINT org_organization_config_pkey PRIMARY KEY (organization_id);

ALTER TABLE ONLY public.org_organization_config
    ADD CONSTRAINT org_organization_config_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);
