create table cmn_operation_type
(
	id smallserial primary key,
	code varchar(50) not null,
	name varchar(150) not null,
	state_id smallint not null references cmn_state(id),
	created_date timestamp without time zone default now() not null
);

create unique index idx_cmn_operation_type_code on cmn_operation_type (code);
create index idx_cmn_operation_type_state_id on cmn_operation_type (state_id);
