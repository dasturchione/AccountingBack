create table cmn_document_registry
(
    id                  bigserial primary key,
    organization_id     int not null references org_organization(id),
    document_type_id    smallint not null references cmn_document_type(id),
    document_id         bigint not null,
    doc_number          varchar(100) not null,
    doc_date            timestamp without time zone not null,
    amount              numeric(24, 8) not null default 0,
    currency_id         smallint references cmn_currency(id),
    status_id           smallint references cmn_document_status(id),
    state_id            smallint not null default 1 references cmn_state(id),
    created_date        timestamp without time zone not null default now(),
    updated_date        timestamp without time zone,

    constraint uq_cmn_document_registry_document
        unique (document_type_id, document_id),

    constraint ck_cmn_document_registry_doc_number_not_empty
        check (btrim(doc_number) <> '')
);

create index ix_cmn_document_registry_organization_date
    on cmn_document_registry (organization_id, doc_date desc);

create index ix_cmn_document_registry_doc_number
    on cmn_document_registry (organization_id, doc_number);

create index ix_cmn_document_registry_status_id
    on cmn_document_registry (status_id);
