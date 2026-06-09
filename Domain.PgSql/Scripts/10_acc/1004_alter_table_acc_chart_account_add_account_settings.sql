alter table acc_chart_account
add column if not exists account_type_id smallint null references acc_account_type(id),
add column if not exists is_quantity boolean default false not null,
add column if not exists is_currency boolean default false not null;

create index if not exists idx_acc_chart_account_account_type_id
on acc_chart_account (account_type_id);
