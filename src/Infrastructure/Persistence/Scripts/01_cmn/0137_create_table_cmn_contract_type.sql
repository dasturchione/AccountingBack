create table if not exists cmn_contract_type
(
	id smallserial primary key,
	code varchar(50) not null,
	name varchar(150) not null,
	state_id smallint not null references cmn_state(id),
	created_date timestamp without time zone default now() not null
);

create unique index if not exists idx_cmn_contract_type_code on cmn_contract_type (code);
create index if not exists idx_cmn_contract_type_state_id on cmn_contract_type (state_id);
