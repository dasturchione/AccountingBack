create table sale_shipment_product
(
    id                      bigserial primary key,
    owner_id                bigint not null
        references sale_shipment_doc(id)
        on delete cascade,

    sale_doc_product_id     bigint
        references sale_doc_product(id),

    product_id              int not null
        references inv_product(id),

    unit_id                 smallint not null
        references cmn_unit(id),

    quantity                numeric(19,6) not null
        check (quantity > 0),

    created_date            timestamp without time zone not null default now()
);

-- Получение всех строк складского документа
create index idx_sale_shipment_product_owner_id
    on sale_shipment_product (owner_id);

-- Связь со строкой бухгалтерского документа продажи
create index idx_sale_shipment_product_sale_doc_product_id
    on sale_shipment_product (sale_doc_product_id)
    where sale_doc_product_id is not null;

-- Поиск товара внутри складских документов
create index idx_sale_shipment_product_product_id
    on sale_shipment_product (product_id);

-- Запрет повторного добавления одной строки продажи
create unique index uq_sale_shipment_product_sale_doc_product_id
    on sale_shipment_product (sale_doc_product_id)
    where sale_doc_product_id is not null;