create table acc_chart_account_preset
(
    id              smallserial primary key,
    code            varchar(50) not null unique,
    name            varchar(255) not null,
    description     varchar(500) null,
    state_id        smallint not null           references cmn_state(id),

    created_date    timestamp without time zone not null default now()
);

insert into acc_chart_account_preset
    (id, code, name, description, state_id, created_date)
values
    (
        1,
        'default_chart_account',
        'Standart hisobvaraqlar rejasi',
        'Tashkilot uchun standart hisobvaraqlar rejasi andozasi',
        1,
        now()
    );
