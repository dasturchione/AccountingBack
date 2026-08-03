create table edo_document_signing_session
(
    session_id       character varying(64) not null,
    organization_id  integer not null,
    provider          character varying(20) not null,
    document_id      bigint not null,
    signing_mode     character varying(40) not null,
    expires_at       timestamp without time zone not null,
    consumed_at      timestamp without time zone,
    created_date      timestamp without time zone default now() not null,
    constraint edo_document_signing_session_pkey primary key (session_id),
    constraint edo_document_signing_session_organization_id_fkey
        foreign key (organization_id) references org_organization(id),
    constraint edo_document_signing_session_document_id_fkey
        foreign key (document_id) references edo_document(id) on delete cascade,
    constraint ck_edo_document_signing_session_provider
        check (provider in ('DIDOX', 'FAKTURA', 'EDOCS'))
);

create index idx_edo_document_signing_session_scope
    on edo_document_signing_session using btree (organization_id, provider, document_id);
