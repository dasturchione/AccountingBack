create table acc_accounting_policy 
(
	id						smallserial primary key,
	code					varchar(50) not null unique,
	name					varchar(250) not null,
	state_id				smallint not null references cmn_state(id)
);