alter table pur_doc  
add column external_doc_number varchar(100) null; 

create index idx_pur_doc_external_doc_number
on pur_doc (external_doc_number);