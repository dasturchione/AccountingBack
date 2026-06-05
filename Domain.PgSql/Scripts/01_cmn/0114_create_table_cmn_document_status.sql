create table cmn_document_status
(
	id smallserial primary key,
	code varchar(50) not null,
	name varchar(100) not null,
	state_id smallint not null references cmn_state(id)
);

create unique index idx_cmn_document_status_code on cmn_document_status (code);
