create table edo_auth_signing_session
(
    session_id              character varying(64) not null,
    organization_id         integer not null,
    provider                character varying(20) not null,
    challenge_id            character varying(256) not null,
    provider_challenge_id   character varying(256),
    certificate_serial_number character varying(256),
    signing_mode            character varying(40) not null,
    expires_at              timestamp without time zone not null,
    consumed_at             timestamp without time zone,
    created_date            timestamp without time zone default now() not null,
    constraint edo_auth_signing_session_pkey primary key (session_id),
    constraint edo_auth_signing_session_organization_id_fkey
        foreign key (organization_id) references org_organization(id),
    constraint ck_edo_auth_signing_session_provider
        check (provider in ('DIDOX', 'FAKTURA', 'EDOCS'))
);

create index idx_edo_auth_signing_session_organization_provider
    on edo_auth_signing_session using btree (organization_id, provider);

create index idx_edo_auth_signing_session_expires_at
    on edo_auth_signing_session using btree (expires_at);
