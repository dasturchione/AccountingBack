create table if not exists acc_document_account_type 
(
	id						smallserial not null primary key,
	code					varchar(50) not null unique,
	name					varchar(250) not null,
	description				varchar(500),
	state_id				smallint not null references cmn_state(id)
);
