create table cmn_bank_operation_classification_condition
(
    id                    serial primary key,
    rule_id               integer not null references cmn_bank_operation_classification_rule(id) on delete cascade,
    condition_order       smallint not null,
    field_code            varchar(50) not null,
    operator_code         varchar(30) not null,
    value_source_code     varchar(30) not null default 'LITERAL',
    compare_value         varchar(1000),
    normalization_code    varchar(30) not null default 'NORMALIZE_WHITESPACE',

    constraint uq_cmn_bank_operation_classification_condition_order unique (rule_id, condition_order),
    constraint ck_cmn_bank_operation_classification_condition_order check (condition_order > 0),
    constraint ck_cmn_bank_operation_classification_condition_field check
        (field_code in ('COUNTERPARTY_INN', 'COUNTERPARTY_NAME', 'COUNTERPARTY_ACCOUNT',
                        'PURPOSE', 'BANK_DOCUMENT_NUMBER', 'OPERATION_CODE')),
    constraint ck_cmn_bank_operation_classification_condition_operator check
        (operator_code in ('EQUALS', 'NOT_EQUALS', 'STARTS_WITH', 'NOT_STARTS_WITH',
                           'CONTAINS', 'NOT_CONTAINS', 'CONTAINS_ANY', 'NOT_CONTAINS_ANY')),
    constraint ck_cmn_bank_operation_classification_condition_value_source check
        (value_source_code in ('LITERAL', 'ORGANIZATION_INN')),
    constraint ck_cmn_bank_operation_classification_condition_normalization check
        (normalization_code in ('NONE', 'TRIM', 'NORMALIZE_WHITESPACE', 'NORMALIZE_KEY', 'LOWER')),
    constraint ck_cmn_bank_operation_classification_condition_shape check
    (
        (value_source_code = 'ORGANIZATION_INN'
            and field_code = 'COUNTERPARTY_INN'
            and operator_code in ('EQUALS', 'NOT_EQUALS')
            and compare_value is null)
        or
        (value_source_code = 'LITERAL'
            and operator_code in ('EQUALS', 'NOT_EQUALS', 'STARTS_WITH', 'NOT_STARTS_WITH',
                                  'CONTAINS', 'NOT_CONTAINS')
            and compare_value is not null
            and nullif(btrim(compare_value), '') is not null)
        or
        (value_source_code = 'LITERAL'
            and operator_code in ('CONTAINS_ANY', 'NOT_CONTAINS_ANY')
            and compare_value is null)
    )
);

create index ix_cmn_bank_operation_classification_condition_rule_id
    on cmn_bank_operation_classification_condition (rule_id);
