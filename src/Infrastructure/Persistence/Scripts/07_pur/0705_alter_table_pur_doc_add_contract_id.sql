alter table pur_doc
	add column if not exists contract_id bigint null;

alter table pur_doc
	drop constraint if exists pur_doc_contract_id_fkey;

alter table pur_doc
	add constraint pur_doc_contract_id_fkey
		foreign key (contract_id) references cmn_contract(id);

create index if not exists idx_pur_doc_contract_id on pur_doc (contract_id);
