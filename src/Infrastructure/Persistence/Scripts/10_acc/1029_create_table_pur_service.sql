create table if not exists pur_service
(
    id              bigserial primary key,
    name            varchar(255) not null,
    description     text,
    service_type_id int not null references cmn_pur_service_type(id),
    state_id        smallint not null references cmn_state(id),
    created_date    timestamp not null default now()
);

create index if not exists idx_pur_service_service_type_id
    on pur_service(service_type_id);
