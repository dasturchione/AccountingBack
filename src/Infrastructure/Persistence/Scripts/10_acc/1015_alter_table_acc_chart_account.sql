truncate table acc_reg_entry_subkonto restart identity;

truncate table acc_chart_account restart identity cascade;

truncate table acc_chart_account restart identity cascade;

alter table acc_chart_account 
add column number varchar(50) not null; 

alter table acc_chart_account 
add column organization_id int not null references org_organization (id);

create unique index ux_acc_chart_account_organization_number
on acc_chart_account (organization_id, number);

create index idx_acc_chart_account_number
on acc_chart_account (number);

alter table acc_chart_account
alter column code drop not null;

alter table acc_chart_account 
add column is_department boolean not null default false;

alter table acc_chart_account 
add column is_tax_accounting boolean not null default false;

alter table acc_chart_account 
add column is_off_balance boolean not null default false;
