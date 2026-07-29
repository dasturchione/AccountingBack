create table inv_opening_inventory_table
(
    id                      bigint generated always as identity
        primary key,

    owner_id                bigint not null
        references inv_opening_inventory_product(id)
        on delete cascade,

    product_table_id        integer not null
        references inv_product_table(id),

    amount                  numeric(24,8) not null
        check (amount >= 0)
);

create index ix_inv_opening_inventory_table_owner_id
    on inv_opening_inventory_table
    (
        owner_id
    );

create unique index ux_inv_opening_inventory_table_owner_id_product_table_id
    on inv_opening_inventory_table
    (
        owner_id,
        product_table_id
    );