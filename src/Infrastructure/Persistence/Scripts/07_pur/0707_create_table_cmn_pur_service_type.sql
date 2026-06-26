create table if not exists cmn_pur_service_type
(
    id             serial primary key,
    name           varchar(250) not null,
    account_id     integer not null references acc_chart_account(id),
    state_id       smallint not null references cmn_state(id),
    created_date   timestamp not null default now()
);

create index if not exists idx_cmn_pur_service_type_account_id
    on cmn_pur_service_type(account_id);
