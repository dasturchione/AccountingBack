drop table if exists acc_posting_rule_line;

drop table if exists acc_posting_rule;

drop table if exists acc_posting_alias_translation;

drop table if exists bank_operation_line;

drop table if exists counterparty_account_payment_purpose_hint;

drop table if exists acc_payment_purpose_translation; 

alter table bank_operation 
drop column if exists payment_purpose_id;

alter table cash_operation 
drop column if exists payment_purpose_id;

drop table if exists acc_payment_purpose; 

drop table if exists acc_posting_alias; 

drop table if exists acc_account_resolve_rule;
