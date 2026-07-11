alter table cash_operation
add column cash_chart_account_id int null references acc_chart_account(id);

alter table cash_operation
add column offset_account_id int null references acc_chart_account(id);

create index idx_cash_operation_cash_chart_account_id
on cash_operation (cash_chart_account_id);

create index idx_cash_operation_offset_account_id
on cash_operation (offset_account_id);