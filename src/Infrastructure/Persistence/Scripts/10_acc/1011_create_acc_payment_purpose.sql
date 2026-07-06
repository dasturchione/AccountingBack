create table acc_payment_purpose
(
	id smallserial primary key,
	code varchar(50) not null unique,
	alias_id smallint not null references acc_posting_alias(id),
	name varchar(250) not null,
	operation_type_id smallint not null references cmn_operation_type(id),
	requires_counterparty boolean not null default true
);

create index idx_acc_payment_purpose_alias_id on acc_payment_purpose (alias_id);
create index idx_acc_payment_purpose_operation_type_id on acc_payment_purpose (operation_type_id);

insert into acc_payment_purpose (id, code, alias_id, name, operation_type_id, requires_counterparty) values
    (1, 'DEFAULT_IN', 6, 'Default incoming payment purpose', 1, false),
    (2, 'DEFAULT_OUT', 4, 'Default outgoing payment purpose', 2, false);

select setval('acc_payment_purpose_id_seq', greatest((select coalesce(max(id), 0) from acc_payment_purpose), 2), true);
