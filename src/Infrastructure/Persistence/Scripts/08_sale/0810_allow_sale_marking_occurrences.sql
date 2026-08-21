-- Allows the same physical ProductTable occurrence to be recorded more than once
-- in one sale line. Physical stock processing still uses DISTINCT ProductTable IDs
-- for the whole document.
drop index if exists ux_sale_doc_table_owner_product_table;

create index if not exists ix_sale_doc_table_owner_product_table
    on sale_doc_table using btree (owner_id, product_table_id);
