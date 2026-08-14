create table rtl_sale_doc_payment
(
    id                      bigserial primary key,

    owner_id                bigint not null
        references rtl_sale_doc(id) on delete cascade,

    payment_method_id       smallint not null
        references rtl_payment_method(id),

    -- Для CARD
    bank_terminal_id        int
        references bank_terminal(id),

    -- Дебетовый бухгалтерский счет:
    -- CASH -> 5010
    -- CARD -> 5710
    -- TRANSFER -> 5110 и т.д.
    debit_account_id        int not null
        references acc_chart_account(id),

    amount                  numeric(24, 8) not null,

    -- Номер транзакции банка / процессинга
    transaction_number      varchar(100),

    constraint ck_rtl_sale_doc_payment_amount
        check (amount > 0)
);

create index ix_rtl_sale_doc_payment_owner_id
    on rtl_sale_doc_payment(owner_id);

create index ix_rtl_sale_doc_payment_payment_method_id
    on rtl_sale_doc_payment(payment_method_id);

create index ix_rtl_sale_doc_payment_bank_terminal_id
    on rtl_sale_doc_payment(bank_terminal_id);

create index ix_rtl_sale_doc_payment_debit_account_id
    on rtl_sale_doc_payment(debit_account_id);