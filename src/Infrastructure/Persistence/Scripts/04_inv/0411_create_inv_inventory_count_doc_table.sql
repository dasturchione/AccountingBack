create table inv_inventory_count_doc_table
(
    id                bigint primary key,
    owner_id          bigint not null references inv_inventory_count_line(id) on delete cascade,
    product_table_id  integer references inv_product_table(id),
    barcode           varchar(100),
    serial_number     varchar(250),
    marking_number    varchar(250),
    cost_price        numeric(24,8) not null);

create index ix_inv_inventory_count_doc_table_owner_id on inv_inventory_count_doc_table (owner_id);
create index idx_inv_inventory_count_doc_table_product_table_id on inv_inventory_count_doc_table (product_table_id);
create unique index ux_inv_inventory_count_doc_table_owner_product_table on inv_inventory_count_doc_table (owner_id, product_table_id);

