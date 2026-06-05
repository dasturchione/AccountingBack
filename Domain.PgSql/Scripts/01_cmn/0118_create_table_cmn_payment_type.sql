create table cmn_payment_type
(
	id smallserial primary key,
	code varchar(50) not null,
	name varchar(100) not null,
	state_id smallint not null references cmn_state(id)
);

create unique index idx_cmn_payment_type_code on cmn_payment_type (code);
