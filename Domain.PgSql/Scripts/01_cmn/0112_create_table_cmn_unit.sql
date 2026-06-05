create table cmn_unit
(
	id smallserial primary key,
	code varchar(20) not null,
	name varchar(100) not null,
	state_id smallint not null references cmn_state(id)
);

create unique index idx_cmn_unit_code on cmn_unit (code);
