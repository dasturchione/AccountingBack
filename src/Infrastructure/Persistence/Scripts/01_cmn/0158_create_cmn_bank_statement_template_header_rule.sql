create table cmn_bank_statement_template_header_rule
(
    id                       serial primary key,
    template_id              integer not null references cmn_bank_statement_template(id) on delete cascade,
    row_offset               smallint not null default 0,
    column_index             smallint not null,
    match_type               varchar(20) not null,
    expected_value           varchar(500) not null,
    normalization_code       varchar(30) not null default 'TRIM',
    is_required              boolean not null default true,

    constraint uq_cmn_bank_statement_template_header_rule_cell
        unique (template_id, row_offset, column_index),
    constraint ck_cmn_bank_statement_template_header_rule_column_index
        check (column_index > 0),
    constraint ck_cmn_bank_statement_template_header_rule_match_type
        check (match_type in ('EXACT', 'CONTAINS', 'REGEX')),
    constraint ck_cmn_bank_statement_template_header_rule_expected_value
        check (nullif(btrim(expected_value), '') is not null),
    constraint ck_cmn_bank_statement_template_header_rule_normalization_code
        check (normalization_code in ('NONE', 'TRIM', 'NORMALIZE_WHITESPACE', 'UPPER'))
);

create index idx_cmn_bank_statement_template_header_rule_template_id
    on cmn_bank_statement_template_header_rule using btree (template_id);

comment on table cmn_bank_statement_template_header_rule is
    'Cell rules used to recognize every account-block header in an Excel worksheet.';

comment on column cmn_bank_statement_template_header_rule.row_offset is
    'Relative row offset from the candidate header row; negative offsets inspect metadata above it.';
