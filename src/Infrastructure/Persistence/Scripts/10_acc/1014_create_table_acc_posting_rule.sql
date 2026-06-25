create table acc_posting_rule 
(
	id						smallserial primary key,
	code					varchar(50) not null unique,
	name					varchar(250) not null
);