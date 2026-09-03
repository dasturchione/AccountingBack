create table cmn_bank_statement_template_field
(
    id                       serial primary key,
    template_id              integer not null references cmn_bank_statement_template(id) on delete cascade,
    section_code             varchar(20) not null,
    target_code              varchar(50) not null,
    source_type              varchar(30) not null,
    anchor_code              varchar(20),
    absolute_row_index       integer,
    row_offset               smallint,
    column_index             smallint,
    search_direction         varchar(10),
    search_limit             smallint,
    locator_match_type       varchar(20),
    locator_value            varchar(500),
    extract_regex            varchar(1000),
    extract_group            varchar(100),
    constant_value           varchar(1000),
    value_type               varchar(20) not null,
    format                   varchar(100),
    transform_code           varchar(50) not null default 'NONE',
    is_required              boolean not null default false,

    constraint uq_cmn_bank_statement_template_field_target
        unique (template_id, section_code, target_code),
    constraint ck_cmn_bank_statement_template_field_section_code
        check (section_code in ('STATEMENT', 'TRANSACTION', 'TOTAL')),
    constraint ck_cmn_bank_statement_template_field_target_code
        check
        (
            (section_code = 'STATEMENT' and target_code in
                ('BANK_MFO', 'BANK_NAME', 'ACCOUNT_NUMBER', 'COMPANY_NAME', 'COMPANY_INN',
                 'PERIOD_FROM', 'PERIOD_TO', 'OPENING_BALANCE', 'CLOSING_BALANCE'))
            or
            (section_code = 'TRANSACTION' and target_code in
                ('DATE', 'DOC_NUMBER', 'OPERATION_CODE', 'COUNTERPARTY_MFO',
                 'COUNTERPARTY_ACCOUNT', 'COUNTERPARTY_INN', 'COUNTERPARTY_NAME',
                 'DEBIT', 'CREDIT', 'PURPOSE'))
            or
            (section_code = 'TOTAL' and target_code in ('TOTAL_DEBIT', 'TOTAL_CREDIT'))
        ),
    constraint ck_cmn_bank_statement_template_field_source_type
        check (source_type in
            ('ABSOLUTE_CELL', 'RELATIVE_CELL', 'SEARCH_CELL', 'CONSTANT', 'SUM_TRANSACTIONS')),
    constraint ck_cmn_bank_statement_template_field_anchor_code
        check (anchor_code is null or anchor_code in ('SHEET', 'HEADER', 'DATA_ROW', 'TOTAL_ROW')),
    constraint ck_cmn_bank_statement_template_field_coordinates
        check
        (
            (absolute_row_index is null or absolute_row_index > 0)
            and (column_index is null or column_index > 0)
            and (search_limit is null or search_limit > 0)
        ),
    constraint ck_cmn_bank_statement_template_field_search_direction
        check (search_direction is null or search_direction in ('UP', 'DOWN')),
    constraint ck_cmn_bank_statement_template_field_locator_match_type
        check (locator_match_type is null or locator_match_type in ('EXACT', 'CONTAINS', 'REGEX')),
    constraint ck_cmn_bank_statement_template_field_value_type
        check (value_type in ('STRING', 'DATE', 'DECIMAL')),
    constraint ck_cmn_bank_statement_template_field_transform_code
        check (transform_code in
            ('NONE', 'TRIM', 'NORMALIZE_WHITESPACE', 'NORMALIZE_KEY', 'NORMALIZE_MFO', 'AMOUNT_FROM_TEXT')),
    constraint ck_cmn_bank_statement_template_field_source_shape
        check
        (
            (source_type = 'ABSOLUTE_CELL' and
             anchor_code is not null and
             anchor_code = 'SHEET' and
             absolute_row_index is not null and
             row_offset is null and
             column_index is not null and
             search_direction is null and search_limit is null and
             locator_match_type is null and locator_value is null and
             constant_value is null)
            or
            (source_type = 'RELATIVE_CELL' and
             anchor_code is not null and
             anchor_code in ('HEADER', 'DATA_ROW', 'TOTAL_ROW') and
             absolute_row_index is null and
             row_offset is not null and
             column_index is not null and
             search_direction is null and search_limit is null and
             locator_match_type is null and locator_value is null and
             constant_value is null)
            or
            (source_type = 'SEARCH_CELL' and
             anchor_code is not null and
             anchor_code = 'HEADER' and
             absolute_row_index is null and
             row_offset is null and
             column_index is not null and
             search_direction is not null and
             search_direction in ('UP', 'DOWN') and
             search_limit is not null and
             locator_match_type is not null and
             locator_match_type in ('EXACT', 'CONTAINS', 'REGEX') and
             locator_value is not null and
             nullif(btrim(locator_value), '') is not null and
             constant_value is null)
            or
            (source_type = 'CONSTANT' and
             anchor_code is null and
             absolute_row_index is null and row_offset is null and column_index is null and
             search_direction is null and search_limit is null and
             locator_match_type is null and locator_value is null and
             constant_value is not null)
            or
            (source_type = 'SUM_TRANSACTIONS' and
             section_code = 'TOTAL' and
             target_code in ('TOTAL_DEBIT', 'TOTAL_CREDIT') and
             anchor_code is null and
             absolute_row_index is null and row_offset is null and column_index is null and
             search_direction is null and search_limit is null and
             locator_match_type is null and locator_value is null and
             constant_value is null)
        )
);

create index idx_cmn_bank_statement_template_field_template_id
    on cmn_bank_statement_template_field using btree (template_id);

comment on table cmn_bank_statement_template_field is
    'Declarative mapping from Excel cells to supported bank statement result fields.';

comment on column cmn_bank_statement_template_field.extract_regex is
    'Optional regular expression applied after locating the source cell; extract_group selects a named or numbered group.';
