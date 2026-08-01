create table organization_edo_provider
(
    organization_id integer not null,
    provider        character varying(20) not null,
    created_date    timestamp without time zone default now() not null,
    updated_date    timestamp without time zone,
    constraint organization_edo_provider_pkey primary key (organization_id),
    constraint organization_edo_provider_organization_id_fkey
        foreign key (organization_id) references org_organization(id),
    constraint ck_organization_edo_provider_provider
        check (provider in ('DIDOX', 'FAKTURA', 'EDOCS'))
);
