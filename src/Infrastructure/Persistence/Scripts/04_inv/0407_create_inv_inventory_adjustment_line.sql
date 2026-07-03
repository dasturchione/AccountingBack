create table inv_inventory_adjustment_line
(
    id              bigint primary key,
    owner_id        bigint not null references inv_inventory_adjustment_doc(id) on delete cascade,
    product_id      integer not null references inv_product(id),
    unit_id         smallint not null references cmn_unit(id),
    quantity        numeric(24,8) not null,
    comment         varchar(1000));

create index ix_inv_inventory_adjustment_line_owner_id on inv_inventory_adjustment_line (owner_id);
create index idx_inv_inventory_adjustment_line_product_id on inv_inventory_adjustment_line (product_id);
create index idx_inv_inventory_adjustment_line_unit_id on inv_inventory_adjustment_line (unit_id);

