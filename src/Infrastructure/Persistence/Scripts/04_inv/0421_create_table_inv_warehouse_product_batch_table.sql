create table inv_warehouse_product_batch_table
(
    batch_id           bigint not null
        references inv_warehouse_product_batch(id)
        on delete cascade,

    product_table_id   int not null
        references inv_product_table(id),

    created_date       timestamp without time zone
        default now() not null,

    constraint pk_inv_warehouse_product_batch_table
        primary key (batch_id, product_table_id)
);

create index idx_inv_warehouse_product_batch_table_product_table_id
    on inv_warehouse_product_batch_table (product_table_id);