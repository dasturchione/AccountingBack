create table counterparty_account_payment_purpose_hint
(
    counterparty_bank_account_id   integer not null references counterparty_bank_account(id),
    payment_purpose_id             smallint not null references acc_payment_purpose(id),
    usage_count                    integer not null default 1,
    last_used_date                 timestamp without time zone not null default now(),
    primary key (counterparty_bank_account_id, payment_purpose_id));

