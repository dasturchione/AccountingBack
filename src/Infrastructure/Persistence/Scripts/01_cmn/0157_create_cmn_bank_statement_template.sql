create table cmn_bank_statement_template
(
    id                       serial primary key,
    bank_id                  integer not null references cmn_bank(id),
    code                     varchar(100) not null,
    name                     varchar(250) not null,
    version                  smallint not null,
    sheet_name_match_type    varchar(20) not null default 'ANY',
    sheet_name_pattern       varchar(250),
    data_start_row_offset    smallint not null default 1,
    state_id                 smallint not null references cmn_state(id),
    created_date             timestamp without time zone not null default now(),

    constraint uq_cmn_bank_statement_template_code unique (code),
    constraint ck_cmn_bank_statement_template_code
        check (code ~ '^[A-Z0-9_]+$'),
    constraint ck_cmn_bank_statement_template_name
        check (nullif(btrim(name), '') is not null),
    constraint ck_cmn_bank_statement_template_version
        check (version > 0),
    constraint ck_cmn_bank_statement_template_sheet_name_match_type
        check (sheet_name_match_type in ('ANY', 'EXACT', 'CONTAINS', 'REGEX')),
    constraint ck_cmn_bank_statement_template_sheet_name_pattern
        check
        (
            (sheet_name_match_type = 'ANY' and sheet_name_pattern is null)
            or
            (sheet_name_match_type <> 'ANY' and
             sheet_name_pattern is not null and
             nullif(btrim(sheet_name_pattern), '') is not null)
        ),
    constraint ck_cmn_bank_statement_template_data_start_row_offset
        check (data_start_row_offset >= 0)
);

create index idx_cmn_bank_statement_template_bank_id
    on cmn_bank_statement_template using btree (bank_id);

create index idx_cmn_bank_statement_template_state_id
    on cmn_bank_statement_template using btree (state_id);

comment on table cmn_bank_statement_template is
    'Global SQL-seeded definitions of supported bank statement Excel templates.';

comment on column cmn_bank_statement_template.data_start_row_offset is
    'Number of rows between a matched account-block header and its first transaction row.';
