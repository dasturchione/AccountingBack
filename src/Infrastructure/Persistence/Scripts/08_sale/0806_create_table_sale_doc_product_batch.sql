create table sale_doc_product_batch
(
    id                          bigserial primary key,
    sale_doc_product_id         bigint not null
                                      references sale_doc_product(id)
                                      on delete cascade,

    warehouse_product_batch_id  bigint not null
                                      references inv_warehouse_product_batch(id),

	quantity                    numeric(19,6) not null
                                      check (quantity > 0),

	constraint uq_sale_doc_product_batch
        unique (sale_doc_product_id, warehouse_product_batch_id)
);

create index idx_sale_doc_product_batch_product
    on sale_doc_product_batch (sale_doc_product_id);

create index idx_sale_doc_product_batch_batch
    on sale_doc_product_batch (warehouse_product_batch_id);
	