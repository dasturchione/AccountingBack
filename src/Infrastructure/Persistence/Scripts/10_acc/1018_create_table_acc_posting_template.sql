create table acc_posting_template 
(
	id						smallserial primary key,
	code					varchar(50) not null unique,
	name					varchar(250) not null,
	document_type_id		smallint not null references cmn_posting_operation_type (id)
);