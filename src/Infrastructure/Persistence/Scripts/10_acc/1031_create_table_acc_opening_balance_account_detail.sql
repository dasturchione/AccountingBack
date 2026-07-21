create table acc_opening_balance_account_detail
(
    id bigserial primary key,

    opening_balance_account_id bigint not null
        references acc_opening_balance_account(id)
        on delete cascade,

    debit_amount numeric(24, 8) not null default 0,
    credit_amount numeric(24, 8) not null default 0,

    quantity numeric(19, 6),

    currency_id smallint not null references cmn_currency(id),

    currency_amount numeric(24, 8),
    exchange_rate numeric(24, 8),

    description character varying(500),

    sort_order integer not null default 1,

    created_date timestamp without time zone not null default now(),

    check
    (
        debit_amount >= 0
        and credit_amount >= 0
    ),

    check
    (
        (debit_amount > 0 and credit_amount = 0)
        or
        (credit_amount > 0 and debit_amount = 0)
    ),

    check
    (
        quantity is null
        or quantity <> 0
    ),

    check
    (
        currency_id is not null
        or
        (
            currency_amount is null
            and exchange_rate is null
        )
    )
);

create index ix_acc_opening_balance_account_detail_account
    on acc_opening_balance_account_detail(opening_balance_account_id);

