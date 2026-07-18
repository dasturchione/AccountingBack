create table sale_shipment_table
(
    id                      bigserial primary key,

    shipment_product_id     bigint not null
        references sale_shipment_product(id)
        on delete cascade,

    product_table_id        int not null
        references inv_product_table(id),

    created_date            timestamp without time zone not null default now(),

    unique (product_table_id)
);

-- Получение всех серийных единиц строки комплектации
create index idx_sale_shipment_table_shipment_product_id
    on sale_shipment_table (shipment_product_id);