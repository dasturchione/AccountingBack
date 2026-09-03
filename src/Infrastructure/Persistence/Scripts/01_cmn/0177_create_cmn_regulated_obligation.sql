create table cmn_regulated_obligation
(
    id              smallserial primary key,
    category_id     smallint not null references cmn_regulated_obligation_category(id),
    code            varchar(50) not null,
    name            varchar(250) not null,
    state_id        smallint not null default 1 references cmn_state(id),
    created_date    timestamp without time zone not null default now(),

    constraint uq_cmn_regulated_obligation_code
        unique (code),
    constraint ck_cmn_regulated_obligation_code
        check (code ~ '^[A-Z0-9_]+$'),
    constraint ck_cmn_regulated_obligation_name
        check (nullif(btrim(name), '') is not null)
);

create index ix_cmn_regulated_obligation_category_id
    on cmn_regulated_obligation(category_id);

create index ix_cmn_regulated_obligation_state_id
    on cmn_regulated_obligation(state_id);
