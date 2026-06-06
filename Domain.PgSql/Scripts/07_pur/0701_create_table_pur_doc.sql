create table pur_doc
(
	id bigserial primary key,
	organization_id int not null references org_organization(id),
	doc_number varchar(100) not null,
	doc_date timestamp without time zone not null,
	counterparty_id int not null references counterparty_card(id),
	warehouse_id int not null references inv_warehouse(id),
	currency_id smallint not null references cmn_currency(id),
	total_amount numeric(18,2) not null default 0,
	vat_amount numeric(18,2) not null default 0,
	final_amount numeric(18,2) not null default 0,
	status_id smallint not null references cmn_document_status(id),
	comment varchar(1000) null,
	state_id smallint not null references cmn_state(id),
	created_date timestamp without time zone default now() not null
);

create index idx_pur_doc_organization_id on pur_doc (organization_id);
create index idx_pur_doc_counterparty_id on pur_doc (counterparty_id);
create index idx_pur_doc_warehouse_id on pur_doc (warehouse_id);
create index idx_pur_doc_doc_date on pur_doc (doc_date);
create index idx_pur_doc_status_id on pur_doc (status_id);
create index idx_pur_doc_state_id on pur_doc (state_id);
