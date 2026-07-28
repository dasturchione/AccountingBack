create table inv_warehouse_product_batch
(
    id                      bigserial primary key,
    organization_id         int not null references org_organization(id),
    warehouse_id            int not null references inv_warehouse(id),
    product_id              int not null references inv_product(id),

    receipt_movement_id     bigint not null
        references inv_warehouse_product_movement(id),

    batch_number            varchar(100),

    initial_quantity        numeric(19,6) not null
        check (initial_quantity > 0),

    remaining_quantity      numeric(19,6) not null
        check (
            remaining_quantity >= 0
            and remaining_quantity <= initial_quantity
        ),

    unit_cost               numeric(24,8) check (unit_cost is null or unit_cost >= 0),

    received_date           timestamp without time zone not null,
    expiry_date             date,

    created_date            timestamp without time zone
        default now() not null,

    constraint uq_inv_warehouse_product_batch_receipt_movement
        unique (receipt_movement_id)
);

create index idx_inv_warehouse_product_batch_warehouse_product
    on inv_warehouse_product_batch
    (organization_id, warehouse_id, product_id);

create index idx_inv_warehouse_product_batch_available
    on inv_warehouse_product_batch
    (
        organization_id,
        warehouse_id,
        product_id,
        received_date,
        id
    )
    include (remaining_quantity, unit_cost)
    where remaining_quantity > 0;

