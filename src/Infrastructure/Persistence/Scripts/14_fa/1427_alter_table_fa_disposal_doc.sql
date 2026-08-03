alter table fa_disposal_doc 
	add column if not exists disposal_type_id smallint not null 
		constraint fa_disposal_doc_disposal_type_id_fkey references fa_disposal_type (id);

create index if not exists idx_fa_disposal_doc_disposal_type_id
	on fa_disposal_doc using btree (disposal_type_id);

alter table fa_disposal_doc 
	drop column if exists disposal_type;