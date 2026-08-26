create table cmn_bank_operation_category
(
    id              smallserial primary key,
    code            varchar(50) not null,
    name            varchar(250) not null,
    state_id        smallint not null default 1 references cmn_state(id),
    created_date    timestamp without time zone not null default now(),

    constraint uq_cmn_bank_operation_category_code unique (code),
    constraint ck_cmn_bank_operation_category_code check (code ~ '^[A-Z0-9_]+$'),
    constraint ck_cmn_bank_operation_category_name check (nullif(btrim(name), '') is not null)
);

create index ix_cmn_bank_operation_category_state_id
    on cmn_bank_operation_category (state_id);
