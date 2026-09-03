create table cmn_regulated_obligation_periodicity
(
    id              smallserial primary key,
    code            varchar(50) not null,
    name            varchar(150) not null,
    state_id        smallint not null default 1 references cmn_state(id),
    created_date    timestamp without time zone not null default now(),

    constraint uq_cmn_regulated_obligation_periodicity_code
        unique (code),
    constraint ck_cmn_regulated_obligation_periodicity_code
        check (code ~ '^[A-Z0-9_]+$'),
    constraint ck_cmn_regulated_obligation_periodicity_name
        check (nullif(btrim(name), '') is not null)
);

create index ix_cmn_regulated_obligation_periodicity_state_id
    on cmn_regulated_obligation_periodicity(state_id);
