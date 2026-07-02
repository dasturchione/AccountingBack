create table inv_transfer_doc_table
(
    id                    bigint primary key,
    owner_id              bigint not null references inv_transfer_line(id) on delete cascade,
    product_table_id      integer not null references inv_product_table(id),
    source_warehouse_id   integer not null references inv_warehouse(id),
    destination_warehouse_id integer not null references inv_warehouse(id),
    cost_price            numeric(24,8) not null);

create index ix_inv_transfer_doc_table_owner_id on inv_transfer_doc_table (owner_id);
create index idx_inv_transfer_doc_table_product_table_id on inv_transfer_doc_table (product_table_id);
create index idx_inv_transfer_doc_table_source_warehouse_id on inv_transfer_doc_table (source_warehouse_id);
create index idx_inv_transfer_doc_table_destination_warehouse_id on inv_transfer_doc_table (destination_warehouse_id);
create unique index ux_inv_transfer_doc_table_owner_product_table on inv_transfer_doc_table (owner_id, product_table_id);
