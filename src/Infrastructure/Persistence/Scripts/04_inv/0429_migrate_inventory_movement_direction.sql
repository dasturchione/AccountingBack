-- The direction is relative to warehouse_id in each register row.
-- A warehouse transfer therefore has one OUT row for the source warehouse
-- and one IN row for the destination warehouse.
alter table inv_warehouse_product_movement
    add column if not exists direction_id smallint;

do $$
begin
    if exists
    (
        select 1
        from information_schema.columns
        where table_schema = current_schema()
          and table_name = 'inv_warehouse_product_movement'
          and column_name = 'movement_sign'
    ) then
        update inv_warehouse_product_movement
        set direction_id = movement_sign
        where direction_id is null;
    end if;

    if exists
    (
        select 1
        from inv_warehouse_product_movement
        where direction_id is null
           or direction_id not in (-1, 1)
    ) then
        raise exception 'Cannot migrate inv_warehouse_product_movement: invalid movement direction exists';
    end if;
end
$$;

alter table inv_warehouse_product_movement
    alter column direction_id set not null;

alter table inv_warehouse_product_movement
    drop constraint if exists inv_warehouse_product_movement_direction_id_fkey;

alter table inv_warehouse_product_movement
    add constraint inv_warehouse_product_movement_direction_id_fkey
        foreign key (direction_id) references cmn_movement_direction(id);

create index if not exists idx_inv_warehouse_product_movement_direction_id
    on inv_warehouse_product_movement using btree (direction_id);

alter table inv_warehouse_product_movement
    drop column if exists movement_sign;

-- An adjustment document currently has one adjustment type for all its lines,
-- so its direction belongs to the document header. CORRECTION may later be
-- explicitly selected as either IN or OUT; existing rows preserve current IN behavior.
alter table inv_inventory_adjustment_doc
    add column if not exists direction_id smallint;

update inv_inventory_adjustment_doc
set direction_id = case upper(adjustment_type)
    when 'POSITIVE_ADJUSTMENT' then 1
    when 'FOUND_STOCK' then 1
    when 'CORRECTION' then 1
    when 'NEGATIVE_ADJUSTMENT' then -1
    when 'WRITE_OFF' then -1
    when 'DAMAGE' then -1
    when 'LOSS' then -1
    else null
end
where direction_id is null;

do $$
begin
    if exists
    (
        select 1
        from inv_inventory_adjustment_doc
        where direction_id is null
           or direction_id not in (-1, 1)
    ) then
        raise exception 'Cannot migrate inv_inventory_adjustment_doc: unknown adjustment type or invalid direction exists';
    end if;
end
$$;

alter table inv_inventory_adjustment_doc
    alter column direction_id set not null;

alter table inv_inventory_adjustment_doc
    drop constraint if exists inv_inventory_adjustment_doc_direction_id_fkey;

alter table inv_inventory_adjustment_doc
    add constraint inv_inventory_adjustment_doc_direction_id_fkey
        foreign key (direction_id) references cmn_movement_direction(id);

alter table inv_inventory_adjustment_doc
    drop constraint if exists ck_inv_inventory_adjustment_doc_type_direction;

alter table inv_inventory_adjustment_doc
    add constraint ck_inv_inventory_adjustment_doc_type_direction check
    (
        (upper(adjustment_type) in ('POSITIVE_ADJUSTMENT', 'FOUND_STOCK') and direction_id = 1)
        or
        (upper(adjustment_type) in ('NEGATIVE_ADJUSTMENT', 'WRITE_OFF', 'DAMAGE', 'LOSS') and direction_id = -1)
        or
        (upper(adjustment_type) = 'CORRECTION' and direction_id in (-1, 1))
    );

create index if not exists idx_inv_inventory_adjustment_doc_direction_id
    on inv_inventory_adjustment_doc using btree (direction_id);
