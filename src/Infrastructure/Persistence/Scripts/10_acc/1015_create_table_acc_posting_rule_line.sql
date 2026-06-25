create table acc_posting_rule_line 
(
	id						serial primary key,
	template_id				smallint not null references acc_posting_rule(id),
	order_number			smallint not null,
	debit_alias				varchar(250) not null,
	credit_alias			varchar(250) not null,
	amount_source			varchar(20) null, 
	is_optional				boolean not null default true	
);