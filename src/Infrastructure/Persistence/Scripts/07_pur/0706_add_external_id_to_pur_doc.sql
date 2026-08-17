alter table pur_doc
    add column if not exists external_id varchar(150) null;

create index if not exists ix_pur_doc_external_id
    on pur_doc (external_id);