insert into inv_warehouse_product_table
    (warehouse_id, product_table_id, product_table_status_id, received_date, created_date)
select
    product_table.current_warehouse_id,
    product_table.id,
    product_table.status_id,
    product_table.created_date,
    product_table.created_date
from inv_product_table as product_table
where product_table.current_warehouse_id is not null
on conflict (product_table_id) do update
set
    warehouse_id = excluded.warehouse_id,
    product_table_status_id = excluded.product_table_status_id;

drop index if exists idx_inv_product_table_current_warehouse_id;
drop index if exists idx_inv_product_table_org_warehouse_status;
drop index if exists idx_inv_product_table_org_warehouse_status_product;
drop index if exists ux_inv_product_table_org_marking;
drop index if exists ux_inv_product_table_org_serial;

alter table inv_product_table
    drop column current_warehouse_id,
    drop column organization_id,
    drop column state_id,
    drop column status_id;

create unique index ux_inv_product_table_marking_number
    on inv_product_table (marking_number)
    where marking_number is not null;

create unique index ux_inv_product_table_serial_number
    on inv_product_table (serial_number)
    where serial_number is not null;

create index idx_inv_warehouse_product_table_warehouse_status
    on inv_warehouse_product_table (warehouse_id, product_table_status_id);

create index idx_inv_warehouse_product_table_warehouse_status_product_table
    on inv_warehouse_product_table (warehouse_id, product_table_status_id, product_table_id);
