create table acc_posting_rule
(
	id serial primary key,
	organization_id int null references org_organization(id),
	document_type_id smallint not null references cmn_document_type(id),
	operation_type_id smallint null references cmn_operation_type(id),
	code varchar(100) not null,
	name varchar(250) not null,
	state_id smallint not null references cmn_state(id),
	created_date timestamp without time zone default now() not null
);

create unique index idx_acc_posting_rule_unique
on acc_posting_rule (organization_id, document_type_id, operation_type_id, code);

create index idx_acc_posting_rule_document_type_id
on acc_posting_rule (document_type_id);

create index idx_acc_posting_rule_operation_type_id
on acc_posting_rule (operation_type_id);
