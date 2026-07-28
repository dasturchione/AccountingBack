create table acc_opening_balance_account_detail_subkonto
(
    opening_balance_account_detail_id bigint not null
        references acc_opening_balance_account_detail(id)
        on delete cascade,

    subkonto_type_id smallint not null
        references acc_subkonto_type(id),

    subkonto_id bigint not null,

    sort_order smallint not null,

    created_date timestamp without time zone not null default now(),

    primary key
    (
        opening_balance_account_detail_id,
        subkonto_type_id
    ),

    unique
    (
        opening_balance_account_detail_id,
        sort_order
    )
);


create index ix_acc_opening_balance_detail_subkonto
    on acc_opening_balance_account_detail_subkonto
    (
        subkonto_type_id,
        subkonto_id
    );
