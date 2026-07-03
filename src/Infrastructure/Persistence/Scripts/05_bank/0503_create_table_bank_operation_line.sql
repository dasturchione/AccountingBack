create table bank_operation_line
(
    id                  bigserial primary key,
    bank_operation_id   bigint not null references bank_operation(id) on delete cascade,
    order_number        smallint not null,
    payment_purpose_id  smallint not null references acc_payment_purpose(id),
    counterparty_id     integer references counterparty_card(id),
    amount              numeric(18,2) not null,
    comment             varchar(500),
    constraint uq_bank_operation_line_order unique (bank_operation_id, order_number));

create index idx_bank_operation_line_payment_purpose_id on bank_operation_line (payment_purpose_id);
create index idx_bank_operation_line_counterparty_id on bank_operation_line (counterparty_id);
