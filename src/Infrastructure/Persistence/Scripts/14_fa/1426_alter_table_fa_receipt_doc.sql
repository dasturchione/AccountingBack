alter table fa_receipt_doc 
	add column if not exists receipt_type_id smallint not null
		constraint fa_receipt_doc_receipt_type_id_fkey references fa_receipt_type (id);

create index if not exists idx_fa_receipt_doc_receipt_type_id 
	on fa_receipt_doc using btree (receipt_type_id);

alter table fa_receipt_doc 
	drop column if exists receipt_type;