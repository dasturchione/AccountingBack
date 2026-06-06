create table money_reg_balance
(
	id bigserial primary key,
	organization_id int not null references org_organization(id),
	document_type_id smallint not null references cmn_document_type(id),
	document_id bigint not null,
	source_type varchar(20) not null,
	source_id int not null,
	operation_type_id smallint not null references cmn_operation_type(id),
	currency_id smallint not null references cmn_currency(id),
	amount numeric(18,2) not null,
	doc_date timestamp without time zone not null,
	created_date timestamp without time zone default now() not null
);

create index idx_money_reg_balance_organization_id on money_reg_balance (organization_id);
create index idx_money_reg_balance_document on money_reg_balance (document_type_id, document_id);
create index idx_money_reg_balance_source on money_reg_balance (source_type, source_id);
create index idx_money_reg_balance_currency_id on money_reg_balance (currency_id);
create index idx_money_reg_balance_doc_date on money_reg_balance (doc_date);
