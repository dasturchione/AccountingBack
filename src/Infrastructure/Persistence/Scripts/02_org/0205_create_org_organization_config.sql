
create table org_organization_config 
(
    organization_id integer not null,
    inventory_valuation_method character varying(20) default 'fifo'::character varying not null,
    accounting_policy_id smallint,
    base_currency_id smallint,
    accounting_start_date date,
    fiscal_year_start_month smallint default 1 not null,
    constraint org_organization_config_inventory_valuation_method_check CHECK (((inventory_valuation_method)::text = ANY ((ARRAY['fifo'::character varying, 'lifo'::character varying, 'average'::character varying])::text[]))),
    constraint org_organization_config_pkey primary key (organization_id),
    constraint org_organization_config_organization_id_fkey foreign key (organization_id) references org_organization(id),
    constraint org_organization_config_fiscal_year_start_month_check CHECK (((fiscal_year_start_month >= 1) AND (fiscal_year_start_month <= 12)))
);

create index idx_org_organization_config_accounting_policy_id on org_organization_config using btree (accounting_policy_id);
create index idx_org_organization_config_base_currency_id on org_organization_config using btree (base_currency_id);
