create table inv_warehouse_product 
(
    warehouse_id                int not null references inv_warehouse(id),
    product_id                  int not null references inv_product(id),
    unit_id                     smallint not null references cmn_unit(id),

    quantity                    numeric(19, 6) not null default 0,
    reserved_quantity           numeric(19, 6) not null default 0,
    blocked_quantity            numeric(19, 6) not null default 0,

    available_quantity numeric(19, 6) 
        generated always as (
            quantity - reserved_quantity - blocked_quantity
        ) stored,

    min_quantity numeric(19, 6) not null default 0,

    created_at timestamp without time zone not null default current_timestamp,

    primary key (warehouse_id, product_id),

    check (quantity >= 0),
    check (reserved_quantity >= 0),
    check (blocked_quantity >= 0),
    check (min_quantity >= 0),
    check (quantity >= reserved_quantity + blocked_quantity)
);

create index idx_inv_warehouse_product_product_id
    on inv_warehouse_product (product_id);

create index idx_inv_warehouse_product_unit_id
    on inv_warehouse_product (unit_id);
