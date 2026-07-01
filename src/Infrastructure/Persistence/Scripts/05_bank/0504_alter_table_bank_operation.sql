alter table bank_operation 
add column counterparty_bank_account_id int null references counterparty_bank_account(id);

create index idx_bank_operation_counterparty_bank_account_id
    on bank_operation (counterparty_bank_account_id);

alter table bank_operation 
add column contract_id bigint null references cmn_contract(id);

create index idx_bank_operation_contract_id
    on bank_operation (contract_id);
