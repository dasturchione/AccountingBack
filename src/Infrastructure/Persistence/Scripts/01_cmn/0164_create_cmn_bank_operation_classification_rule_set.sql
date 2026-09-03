create table cmn_bank_operation_classification_rule_set
(
    id              serial primary key,
    bank_id         integer not null references cmn_bank(id),
    code            varchar(100) not null,
    name            varchar(250) not null,
    version         smallint not null,
    state_id        smallint not null default 1 references cmn_state(id),
    created_date    timestamp without time zone not null default now(),

    constraint uq_cmn_bank_operation_classification_rule_set_code unique (code),
    constraint uq_cmn_bank_operation_classification_rule_set_bank_version unique (bank_id, version),
    constraint ck_cmn_bank_operation_classification_rule_set_code check (code ~ '^[A-Z0-9_]+$'),
    constraint ck_cmn_bank_operation_classification_rule_set_name check (nullif(btrim(name), '') is not null),
    constraint ck_cmn_bank_operation_classification_rule_set_version check (version > 0)
);

create index ix_cmn_bank_operation_classification_rule_set_bank_id
    on cmn_bank_operation_classification_rule_set (bank_id);

create index ix_cmn_bank_operation_classification_rule_set_state_id
    on cmn_bank_operation_classification_rule_set (state_id);
