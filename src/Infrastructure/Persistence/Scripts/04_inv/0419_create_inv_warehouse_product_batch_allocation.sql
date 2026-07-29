create table inv_warehouse_product_batch_allocation
(
    id                      bigserial primary key,

    issue_movement_id       bigint not null references inv_warehouse_product_movement(id) on delete cascade,

    batch_id                bigint not null references inv_warehouse_product_batch(id),

    quantity                numeric(19,6) not null check (quantity > 0),

    unit_cost               numeric(24,8) check (unit_cost is null or unit_cost >= 0),

    created_date            timestamp without time zone default now() not null,

    constraint uq_inv_warehouse_product_batch_allocation unique (issue_movement_id, batch_id)
);

create index idx_inv_warehouse_product_batch_allocation_batch
    on inv_warehouse_product_batch_allocation (batch_id);

create index idx_inv_warehouse_product_batch_allocation_movement
    on inv_warehouse_product_batch_allocation (issue_movement_id);
