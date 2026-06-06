create table cash_operation
(
	id bigserial primary key,
	organization_id int not null references org_organization(id),
	cash_box_id int not null references cash_box(id),
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

create index idx_cash_operation_organization_id on cash_operation (organization_id);
create index idx_cash_operation_cash_box_id on cash_operation (cash_box_id);
create index idx_cash_operation_operation_type_id on cash_operation (operation_type_id);
create index idx_cash_operation_counterparty_id on cash_operation (counterparty_id);
create index idx_cash_operation_doc_date on cash_operation (doc_date);
create index idx_cash_operation_status_id on cash_operation (status_id);
create index idx_cash_operation_state_id on cash_operation (state_id);
