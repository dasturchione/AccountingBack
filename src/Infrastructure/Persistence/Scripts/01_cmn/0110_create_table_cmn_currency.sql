create table cmn_currency
(
	id smallserial primary key,
	code varchar(10) not null,
	name varchar(100) not null,
	symbol varchar(10) null,
	state_id smallint not null references cmn_state(id)
);

create unique index idx_cmn_currency_code on cmn_currency (code);
