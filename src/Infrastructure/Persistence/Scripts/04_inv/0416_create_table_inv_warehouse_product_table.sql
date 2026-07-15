create table inv_warehouse_product_table
(
	warehouse_id                int not null references inv_warehouse(id),
	product_table_id            int not null references inv_product_table(id) primary key,
	product_table_status_id     smallint not null references cmn_product_table_status(id),
	received_date               timestamp without time zone default now() not null,
	created_date                timestamp without time zone default now() not null
);

create index idx_inv_warehouse_product_table_warehouse_id
    on inv_warehouse_product_table (warehouse_id);

create index idx_inv_warehouse_product_table_status_id
    on inv_warehouse_product_table (product_table_status_id);