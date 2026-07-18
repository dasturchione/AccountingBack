create table sale_shipment_product_batch
(
    shipment_product_id bigint not null
        references sale_shipment_product(id)
        on delete cascade,

    batch_id            bigint not null
        references inv_warehouse_product_batch(id),

    quantity            numeric(19,6) not null
        check (quantity > 0),

    primary key (shipment_product_id, batch_id)
);

-- Поиск всех документов комплектации, использующих конкретную партию
create index idx_sale_shipment_product_batch_batch_id
    on sale_shipment_product_batch (batch_id);