alter table bank_operation 
add column counterparty_bank_account_id int null references counterparty_bank_account(id);

create index idx_bank_operation_counterparty_bank_account_id
    on bank_operation (counterparty_bank_account_id);