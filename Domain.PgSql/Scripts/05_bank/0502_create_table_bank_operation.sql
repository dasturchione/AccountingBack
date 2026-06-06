create table bank_operation
(
	id bigserial primary key,
	organization_id int not null references org_organization(id),
	bank_account_id int not null references org_bank_account(id),
	operation_type_id smallint not null references cmn_operation_type(id),
	payment_type_id smallint null references cmn_payment_type(id),
	counterparty_id int null references counterparty_card(id),
	doc_number varchar(100) not null,
	doc_date timestamp without time zone not null,
	currency_id smallint not null references cmn_currency(id),
	amount numeric(18,2) not null,
	comment varchar(1000) null,
	status_id smallint not null references cmn_document_status(id),
	state_id smallint not null references cmn_state(id),
	created_date timestamp without time zone default now() not null
);

create index idx_bank_operation_organization_id on bank_operation (organization_id);
create index idx_bank_operation_bank_account_id on bank_operation (bank_account_id);
create index idx_bank_operation_operation_type_id on bank_operation (operation_type_id);
create index idx_bank_operation_counterparty_id on bank_operation (counterparty_id);
create index idx_bank_operation_doc_date on bank_operation (doc_date);
create index idx_bank_operation_status_id on bank_operation (status_id);
create index idx_bank_operation_state_id on bank_operation (state_id);
