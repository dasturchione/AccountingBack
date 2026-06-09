create table acc_account_type
(
	id smallserial primary key,
	code varchar(50) not null,
	name varchar(150) not null,
	state_id smallint not null references cmn_state(id),
	created_date timestamp without time zone default now() not null
);

create unique index idx_acc_account_type_code on acc_account_type (code);
create index idx_acc_account_type_state_id on acc_account_type (state_id);
