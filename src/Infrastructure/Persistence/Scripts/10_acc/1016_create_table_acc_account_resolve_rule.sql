create table acc_account_resolve_rule 
(
	id						serial primary key,
	policy_id				smallint not null references acc_accounting_policy(id),
	alias					varchar(250) not null,
	dimension_key			varchar(250) not null,
	dimension_value			varchar(250) not null,
	account_id				int not null references acc_chart_account(id),
	priority				int not null
);