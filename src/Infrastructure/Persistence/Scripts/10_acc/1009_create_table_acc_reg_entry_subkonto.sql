create table acc_reg_entry_subkonto
(
	id bigserial primary key,
	entry_id bigint not null references acc_reg_entry(id) on delete cascade,
	side varchar(2) not null,
	subkonto_type_id smallint not null references acc_subkonto_type(id),
	sort_order int default 0 not null,
	entity_id bigint null,
	display_value varchar(500) null,
	created_date timestamp without time zone default now() not null
);

create index idx_acc_reg_entry_subkonto_entry_id
on acc_reg_entry_subkonto (entry_id);

create index idx_acc_reg_entry_subkonto_side
on acc_reg_entry_subkonto (side);

create index idx_acc_reg_entry_subkonto_type_id
on acc_reg_entry_subkonto (subkonto_type_id);

create index idx_acc_reg_entry_subkonto_entity
on acc_reg_entry_subkonto (subkonto_type_id, entity_id);
