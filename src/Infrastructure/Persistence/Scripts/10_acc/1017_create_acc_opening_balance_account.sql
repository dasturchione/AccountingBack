create table acc_opening_balance_account
(
    id bigserial primary key,

    opening_balance_id bigint not null references acc_opening_balance(id) on delete cascade,

    chart_account_id integer not null references acc_chart_account(id),

    debit_amount numeric(24, 8) not null default 0,
    credit_amount numeric(24, 8) not null default 0,

    created_date timestamp without time zone not null default now(),

    unique
    (
        opening_balance_id,
        chart_account_id
    ),

    check
    (
        debit_amount >= 0 and credit_amount >= 0
    ),

    check
    (
        (debit_amount > 0 and credit_amount = 0)
        or
        (credit_amount > 0 and debit_amount = 0)
    )
);

create index ix_acc_opening_balance_account_document
    on acc_opening_balance_account(opening_balance_id);

create index ix_acc_opening_balance_account_chart_account
    on acc_opening_balance_account(chart_account_id);
