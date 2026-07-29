create table inv_opening_inventory_product
(
    id                      bigint generated always as identity
        primary key,

    owner_id                bigint not null
        references inv_opening_inventory(id)
        on delete cascade,

    product_id              integer not null
        references inv_product(id),

    quantity                numeric(19,6) not null
        check (quantity > 0),

    unit_id                 smallint not null
        references cmn_unit(id),

    unit_price              numeric(24,8) not null
        check (unit_price >= 0),

    amount                  numeric(24,8) not null
        check (amount >= 0),

    debit_account_id        integer not null
        references acc_chart_account(id)
);

create index ix_inv_opening_inventory_product_owner
    on inv_opening_inventory_product
    (
        owner_id
    );

create index ix_inv_opening_inventory_product_product
    on inv_opening_inventory_product
    (
        product_id
    );

create index ix_inv_opening_inventory_product_account
    on inv_opening_inventory_product
    (
        debit_account_id
    );

create unique index ux_inv_opening_inventory_product_owner_product
    on inv_opening_inventory_product
    (
        owner_id,
        product_id
    );