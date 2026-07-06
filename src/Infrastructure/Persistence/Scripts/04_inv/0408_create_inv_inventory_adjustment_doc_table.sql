create table inv_inventory_adjustment_doc_table
(
    id bigint primary key,
    owner_id bigint not null references inv_inventory_adjustment_line(id) on delete cascade,
    product_table_id integer references inv_product_table(id),
    cost_price numeric(24,8) not null,
    original_status_id smallint,
    original_state_id smallint,
    original_warehouse_id integer,
    was_created boolean not null default false
);

create index ix_inv_inventory_adjustment_doc_table_owner_id on inv_inventory_adjustment_doc_table (owner_id);
create index idx_inv_inventory_adjustment_doc_table_product_table_id on inv_inventory_adjustment_doc_table (product_table_id);
create unique index ux_inv_inventory_adjustment_doc_table_owner_product_table on inv_inventory_adjustment_doc_table (owner_id, product_table_id);
