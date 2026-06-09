create table cmn_bank
(
	id serial primary key,
	code varchar(50) not null,
	name varchar(250) not null,
	mfo varchar(20) null,
	state_id smallint not null references cmn_state(id),
	created_date timestamp without time zone default now() not null
);

create unique index idx_cmn_bank_code on cmn_bank (code);
create index idx_cmn_bank_state_id on cmn_bank (state_id);
