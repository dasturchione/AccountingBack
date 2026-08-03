create table if not exists cmn_document_number_sequence
(
	id						bigserial primary key,
	organization_id			int not null references org_organization(id),
	document_type_id		smallint not null references cmn_document_type(id),
	document_year           smallint not null,
    prefix                  varchar(20) null,
    last_number             bigint not null default 0,
    number_length           smallint null,
    last_document_date      timestamp without time zone NOT NULL DEFAULT now(),

    created_at              timestamp without time zone NOT NULL DEFAULT now(),
    updated_at              timestamp without time zone NOT NULL DEFAULT now(),

    constraint uq_cmn_document_number_sequence
        unique (
            organization_id,
            document_type_id,
            document_year
        ),

    constraint ck_cmn_document_number_sequence_last_number
        check (last_number >= 0),

    constraint ck_cmn_document_number_sequence_number_length
    check (
        number_length is null
        or number_length between 1 and 20
    ),

    constraint ck_cmn_document_number_sequence_date_year
        check (
            last_document_date is null
            or extract(year from last_document_date)::smallint = document_year
        )
);

create index if not exists ix_cmn_document_number_sequence_organization_id
    on cmn_document_number_sequence (organization_id);

create index if not exists ix_cmn_document_number_sequence_document_type_id
    on cmn_document_number_sequence (document_type_id);