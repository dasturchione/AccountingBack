
create table org_tax_settings 
(
    id integer   not null,
    organization_id integer not null,
    tax_type_id smallint not null,
    is_vat_payer boolean default false not null,
    vat_registration_number character varying(100),
    effective_from date default CURRENT_DATE not null,
    effective_to date,
    state_id smallint default 1 not null,
    created_date timestamp without time zone default now() not null,
    constraint ck_org_tax_settings_dates CHECK (((effective_to IS null) OR (effective_to >= effective_from))),
    constraint org_tax_settings_pkey primary key (id),
    constraint org_tax_settings_organization_id_fkey foreign key (organization_id) references org_organization(id),
    constraint org_tax_settings_state_id_fkey foreign key (state_id) references cmn_state(id),
    constraint org_tax_settings_tax_type_id_fkey foreign key (tax_type_id) references cmn_tax_type(id)
);

create index idx_org_tax_settings_effective_dates on org_tax_settings using btree (effective_from, effective_to);
create index idx_org_tax_settings_organization_id on org_tax_settings using btree (organization_id);
create index idx_org_tax_settings_tax_type_id on org_tax_settings using btree (tax_type_id);
