alter table acc_posting_rule_line
	add column if not exists debit_account_source varchar(100) null,
	add column if not exists credit_account_source varchar(100) null;

