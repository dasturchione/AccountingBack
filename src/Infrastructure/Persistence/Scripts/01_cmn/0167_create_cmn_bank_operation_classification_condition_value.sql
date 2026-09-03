create table cmn_bank_operation_classification_condition_value
(
    id                 serial primary key,
    condition_id       integer not null references cmn_bank_operation_classification_condition(id) on delete cascade,
    value_order        smallint not null,
    compare_value      varchar(500) not null,

    constraint uq_cmn_bank_operation_classification_condition_value_order unique (condition_id, value_order),
    constraint uq_cmn_bank_operation_classification_condition_value unique (condition_id, compare_value),
    constraint ck_cmn_bank_operation_classification_condition_value_order check (value_order > 0),
    constraint ck_cmn_bank_operation_classification_condition_value_text check (nullif(btrim(compare_value), '') is not null)
);

create index ix_cmn_bank_operation_classification_condition_value_condition_id
    on cmn_bank_operation_classification_condition_value (condition_id);
