-- sale_doc_table.product_id -> product_table_id (inv_product_table ga FK)
alter table sale_doc_table
    drop constraint if exists sale_doc_product_id_fkey;

alter table sale_doc_table
    drop constraint if exists sale_doc_table_product_id_fkey;

alter table sale_doc_table
    rename column product_id to product_table_id;

drop index if exists idx_sale_doc_table_product_id;

create index idx_sale_doc_table_product_table_id on sale_doc_table (product_table_id);

alter table sale_doc_table
    add constraint sale_doc_table_product_table_id_fkey
        foreign key (product_table_id) references inv_product_table(id);
