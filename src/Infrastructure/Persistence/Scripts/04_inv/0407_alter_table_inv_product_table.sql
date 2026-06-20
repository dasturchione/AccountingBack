alter table inv_product_table
add column status_id smallint not null
    default 1
    references cmn_product_table_status(id);

create index ix_inv_product_table_status_id
    on inv_product_table(status_id);

alter table inv_product_table 
add column cost_price numeric(18,2) not null 
	default 0;
