create table cmn_payment_acceptance_point_type
(
    id              smallserial primary key,
    code            varchar(50) not null unique,
    name            varchar(150) not null,
    state_id        smallint not null references cmn_state(id),
    created_date    timestamp without time zone not null default now()
);

insert into cmn_payment_acceptance_point_type(code, name, state_id)
values
    ('POS', 'POS terminal', 1),
    ('QR', 'QR payment', 1),
    ('PAYMENT_LINK', 'Payment link', 1),
    ('MARKETPLACE', 'Marketplace', 1),
    ('MOBILE_APP', 'Mobile application', 1),
    ('OTHER', 'Other', 1)
on conflict (code) do update set
    name = excluded.name,
    state_id = excluded.state_id;
