alter table acc_chart_account_subkonto 
drop column if exists organization_id; 

alter table acc_chart_account_subkonto 
drop column if exists is_required; 
