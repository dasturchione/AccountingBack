alter table bank_operation
add column bank_chart_account_id int null references acc_chart_account(id);

alter table bank_operation
add column offset_account_id int null references acc_chart_account(id);

create index idx_bank_operation_bank_chart_account_id
on bank_operation (bank_chart_account_id);

create index idx_bank_operation_offset_account_id
on bank_operation (offset_account_id);