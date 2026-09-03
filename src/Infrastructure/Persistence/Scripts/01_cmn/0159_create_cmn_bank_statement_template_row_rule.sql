create table cmn_bank_statement_template_row_rule
(
    id                       serial primary key,
    template_id              integer not null references cmn_bank_statement_template(id) on delete cascade,
    priority                 smallint not null,
    row_kind                 varchar(20) not null,
    column_index             smallint not null,
    operator_code            varchar(20) not null,
    compare_value            varchar(500),
    normalization_code       varchar(30) not null default 'TRIM',

    constraint uq_cmn_bank_statement_template_row_rule_priority
        unique (template_id, priority),
    constraint ck_cmn_bank_statement_template_row_rule_priority
        check (priority > 0),
    constraint ck_cmn_bank_statement_template_row_rule_row_kind
        check (row_kind in ('DATA', 'TOTAL', 'STOP', 'SKIP')),
    constraint ck_cmn_bank_statement_template_row_rule_column_index
        check (column_index > 0),
    constraint ck_cmn_bank_statement_template_row_rule_operator_code
        check (operator_code in ('IS_DATE', 'EXACT', 'CONTAINS', 'REGEX', 'IS_EMPTY')),
    constraint ck_cmn_bank_statement_template_row_rule_compare_value
        check
        (
            (operator_code in ('EXACT', 'CONTAINS', 'REGEX') and
             compare_value is not null and
             nullif(btrim(compare_value), '') is not null)
            or
            (operator_code in ('IS_DATE', 'IS_EMPTY') and compare_value is null)
        ),
    constraint ck_cmn_bank_statement_template_row_rule_normalization_code
        check (normalization_code in ('NONE', 'TRIM', 'NORMALIZE_WHITESPACE', 'UPPER'))
);

create index idx_cmn_bank_statement_template_row_rule_template_id
    on cmn_bank_statement_template_row_rule using btree (template_id);

comment on table cmn_bank_statement_template_row_rule is
    'Priority-ordered rules that classify rows inside each detected account block.';
