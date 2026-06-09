create table acc_posting_rule_line
(
	id serial primary key,
	rule_id int not null references acc_posting_rule(id) on delete cascade,
	sort_order int default 0 not null,
	debit_account_id int null references acc_chart_account(id),
	credit_account_id int null references acc_chart_account(id),
	amount_source varchar(100) not null,
	quantity_source varchar(100) null,
	content_template varchar(1000) null,
	state_id smallint not null references cmn_state(id),
	created_date timestamp without time zone default now() not null
);

create index idx_acc_posting_rule_line_rule_id
on acc_posting_rule_line (rule_id);

create index idx_acc_posting_rule_line_debit_account_id
on acc_posting_rule_line (debit_account_id);

create index idx_acc_posting_rule_line_credit_account_id
on acc_posting_rule_line (credit_account_id);
