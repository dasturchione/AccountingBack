create table counterparty_reg_balance
(
	id bigserial primary key,
	organization_id int not null references org_organization(id),
	document_type_id smallint not null references cmn_document_type(id),
	document_id bigint not null,
	counterparty_id int not null references counterparty_card(id),
	operation_type_id smallint not null references cmn_operation_type(id),
	currency_id smallint not null references cmn_currency(id),
	amount numeric(18,2) not null,
	doc_date timestamp without time zone not null,
	created_date timestamp without time zone default now() not null
);

create index idx_counterparty_reg_balance_organization_id on counterparty_reg_balance (organization_id);
create index idx_counterparty_reg_balance_document on counterparty_reg_balance (document_type_id, document_id);
create index idx_counterparty_reg_balance_counterparty_id on counterparty_reg_balance (counterparty_id);
create index idx_counterparty_reg_balance_currency_id on counterparty_reg_balance (currency_id);
create index idx_counterparty_reg_balance_doc_date on counterparty_reg_balance (doc_date);
