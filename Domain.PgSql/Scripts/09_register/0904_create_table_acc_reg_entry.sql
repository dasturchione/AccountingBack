create table acc_reg_entry
(
	id bigserial primary key,
	organization_id int not null references org_organization(id),
	document_type_id smallint not null references cmn_document_type(id),
	document_id bigint not null,
	debit_account_id int null references acc_chart_account(id),
	credit_account_id int null references acc_chart_account(id),
	currency_id smallint not null references cmn_currency(id),
	amount numeric(18,2) not null,
	doc_date timestamp without time zone not null,
	created_date timestamp without time zone default now() not null
);

create index idx_acc_reg_entry_organization_id on acc_reg_entry (organization_id);
create index idx_acc_reg_entry_document on acc_reg_entry (document_type_id, document_id);
create index idx_acc_reg_entry_debit_account_id on acc_reg_entry (debit_account_id);
create index idx_acc_reg_entry_credit_account_id on acc_reg_entry (credit_account_id);
create index idx_acc_reg_entry_currency_id on acc_reg_entry (currency_id);
create index idx_acc_reg_entry_doc_date on acc_reg_entry (doc_date);
