create table cmn_bank_operation_classification_rule
(
    id              serial primary key,
    rule_set_id     integer not null references cmn_bank_operation_classification_rule_set(id) on delete cascade,
    category_id     smallint not null references cmn_bank_operation_category(id),
    code            varchar(100) not null,
    name            varchar(250) not null,
    priority        smallint not null,
    direction_id    smallint references cmn_movement_direction(id),
    is_fallback     boolean not null default false,
    state_id        smallint not null default 1 references cmn_state(id),

    constraint uq_cmn_bank_operation_classification_rule_code unique (rule_set_id, code),
    constraint uq_cmn_bank_operation_classification_rule_priority unique (rule_set_id, priority),
    constraint ck_cmn_bank_operation_classification_rule_code check (code ~ '^[A-Z0-9_]+$'),
    constraint ck_cmn_bank_operation_classification_rule_name check (nullif(btrim(name), '') is not null),
    constraint ck_cmn_bank_operation_classification_rule_priority check (priority > 0),
    constraint ck_cmn_bank_operation_classification_rule_fallback_direction
        check (not is_fallback or direction_id is null)
);

create index ix_cmn_bank_operation_classification_rule_rule_set_id
    on cmn_bank_operation_classification_rule (rule_set_id);

create index ix_cmn_bank_operation_classification_rule_category_id
    on cmn_bank_operation_classification_rule (category_id);

create index ix_cmn_bank_operation_classification_rule_state_id
    on cmn_bank_operation_classification_rule (state_id);
