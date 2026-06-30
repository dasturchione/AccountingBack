-- Table: public.org_organization_config

CREATE TABLE public.org_organization_config (
    organization_id integer NOT NULL,
    inventory_valuation_method character varying(20) DEFAULT 'fifo'::character varying NOT NULL,
    accounting_policy_id smallint,
    base_currency_id smallint,
    accounting_start_date date,
    fiscal_year_start_month smallint DEFAULT 1 NOT NULL,
    CONSTRAINT org_organization_config_inventory_valuation_method_check CHECK (((inventory_valuation_method)::text = ANY ((ARRAY['fifo'::character varying, 'lifo'::character varying, 'average'::character varying])::text[])))
);

ALTER TABLE ONLY public.org_organization_config
    ADD CONSTRAINT org_organization_config_pkey PRIMARY KEY (organization_id);

ALTER TABLE ONLY public.org_organization_config
    ADD CONSTRAINT org_organization_config_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);

CREATE INDEX idx_org_organization_config_accounting_policy_id ON public.org_organization_config USING btree (accounting_policy_id);

CREATE INDEX idx_org_organization_config_base_currency_id ON public.org_organization_config USING btree (base_currency_id);

ALTER TABLE ONLY public.org_organization_config
    ADD CONSTRAINT org_organization_config_accounting_policy_id_fkey FOREIGN KEY (accounting_policy_id) REFERENCES public.acc_accounting_policy(id);

ALTER TABLE ONLY public.org_organization_config
    ADD CONSTRAINT org_organization_config_base_currency_id_fkey FOREIGN KEY (base_currency_id) REFERENCES public.cmn_currency(id);

ALTER TABLE ONLY public.org_organization_config
    ADD CONSTRAINT org_organization_config_fiscal_year_start_month_check CHECK (((fiscal_year_start_month >= 1) AND (fiscal_year_start_month <= 12)));
