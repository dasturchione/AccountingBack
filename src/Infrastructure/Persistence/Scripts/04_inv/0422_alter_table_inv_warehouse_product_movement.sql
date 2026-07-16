alter table inv_warehouse_product_movement
    add column if not exists product_table_id int references inv_product_table(id),
    add column if not exists unit_cost numeric(24,8)
        check (unit_cost is null or unit_cost >= 0);

create index if not exists idx_inv_warehouse_product_movement_product_table
    on inv_warehouse_product_movement (organization_id, product_table_id)
    where product_table_id is not null;