create table cmn_document_sequence 
(
    id integer   not null,
    organization_id integer not null,
    document_type_id smallint not null,
    prefix character varying(50),
    suffix character varying(50),
    current_number bigint default 0 not null,
    padding smallint default 5 not null,
    year smallint,
    month smallint,
    reset_period character varying(20) default 'yearly'::character varying not null,
    state_id smallint default 1 not null,
    created_date timestamp without time zone default now() not null,
    constraint cmn_document_sequence_pkey primary key (id),
    constraint cmn_document_sequence_document_type_id_fkey foreign key (document_type_id) references cmn_document_type(id),
    constraint cmn_document_sequence_organization_id_fkey foreign key (organization_id) references org_organization(id),
    constraint cmn_document_sequence_state_id_fkey foreign key (state_id) references cmn_state(id)
);

create index idx_cmn_document_sequence_document_type_id on cmn_document_sequence using btree (document_type_id);
create index idx_cmn_document_sequence_state_id on cmn_document_sequence using btree (state_id);
create unique index uidx_cmn_document_sequence_scope on cmn_document_sequence using btree (organization_id, document_type_id, COALESCE((year)::integer, 0), COALESCE((month)::integer, 0));
