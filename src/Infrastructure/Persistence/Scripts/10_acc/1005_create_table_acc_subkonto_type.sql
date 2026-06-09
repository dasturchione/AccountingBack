create table acc_subkonto_type
(
	id smallserial primary key,
	code varchar(50) not null,
	name varchar(150) not null,
	source_table varchar(100) not null,
	state_id smallint not null references cmn_state(id),
	created_date timestamp without time zone default now() not null
);

create unique index idx_acc_subkonto_type_code on acc_subkonto_type (code);
create index idx_acc_subkonto_type_state_id on acc_subkonto_type (state_id);
