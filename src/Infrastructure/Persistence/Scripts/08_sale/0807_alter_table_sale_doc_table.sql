alter table sale_doc_table 
drop column owner_id; 

alter table sale_doc_table 
drop column quantity;

alter table sale_doc_table 
drop column price; 

alter table sale_doc_table
add column owner_id bigint
    references sale_doc_product(id);

create index ix_sale_doc_table_owner_id
    on sale_doc_table(owner_id);