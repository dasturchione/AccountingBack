-- pur_doc_table.product_id -> product_table_id (inv_product_table ga FK)
alter table pur_doc_table
    drop constraint if exists pur_doc_product_id_fkey;

alter table pur_doc_table
    drop constraint if exists pur_doc_table_product_id_fkey;

alter table pur_doc_table
    rename column product_id to product_table_id;

drop index if exists idx_pur_doc_table_product_id;

create index idx_pur_doc_table_product_table_id on pur_doc_table (product_table_id);

alter table pur_doc_table
    add constraint pur_doc_table_product_table_id_fkey
        foreign key (product_table_id) references inv_product_table(id);
