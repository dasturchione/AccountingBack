create table inv_warehouse_product_movement
(
    id                  bigserial primary key,
    organization_id     int not null references org_organization(id),
    warehouse_id        int not null references inv_warehouse(id),
    product_id          int not null references inv_product(id),

    document_type_id    smallint not null references cmn_document_type(id),
    document_id         bigint not null,
    document_line_id    bigint,

    quantity            numeric(19,6) not null
                            check (quantity > 0),

    movement_sign       smallint not null
                            check (movement_sign in (-1, 1)),

    movement_date       timestamp without time zone not null,
    created_date        timestamp without time zone default now() not null
);

create index idx_inv_warehouse_product_movement_warehouse_product
    on inv_warehouse_product_movement
    (organization_id, warehouse_id, product_id);

create index idx_inv_warehouse_product_movement_document
    on inv_warehouse_product_movement
    (organization_id, document_type_id, document_id);

create index idx_inv_warehouse_product_movement_document_line
    on inv_warehouse_product_movement
    (organization_id, document_type_id, document_id, document_line_id);

create index idx_inv_warehouse_product_movement_date
    on inv_warehouse_product_movement
    (organization_id, movement_date);
