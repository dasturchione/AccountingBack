alter table pur_doc_table
	add column if not exists item_type_id smallint not null default 1 references cmn_purchase_item_type(id),
	add column if not exists service_id int null references inv_product(id),
	add column if not exists service_name varchar(250) null,
	add column if not exists expense_account_id int null references acc_chart_account(id);

alter table pur_doc_table
	alter column product_table_id drop not null;

alter table pur_doc_table
	drop constraint if exists chk_pur_doc_table_item_type_fields;

alter table pur_doc_table
	add constraint chk_pur_doc_table_item_type_fields
		check (
			(
				item_type_id = 1
				and product_table_id is not null
				and service_id is null
				and service_name is null
				and expense_account_id is null
			)
			or
			(
				item_type_id = 2
				and product_table_id is null
				and (service_id is not null or service_name is not null)
				and expense_account_id is not null
			)
		);

create index if not exists idx_pur_doc_table_item_type_id on pur_doc_table (item_type_id);
create index if not exists idx_pur_doc_table_service_id on pur_doc_table (service_id);
create index if not exists idx_pur_doc_table_expense_account_id on pur_doc_table (expense_account_id);

