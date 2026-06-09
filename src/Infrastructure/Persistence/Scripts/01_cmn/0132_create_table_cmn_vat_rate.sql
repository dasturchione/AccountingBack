create table cmn_vat_rate
(
	id smallserial primary key,
	code varchar(50) not null,
	name varchar(150) not null,
	rate numeric(5,2) not null,
	state_id smallint not null references cmn_state(id),
	created_date timestamp without time zone default now() not null
);

create unique index idx_cmn_vat_rate_code on cmn_vat_rate (code);
create index idx_cmn_vat_rate_state_id on cmn_vat_rate (state_id);
