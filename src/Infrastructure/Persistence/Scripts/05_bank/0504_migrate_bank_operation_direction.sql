-- Bank operations currently support only IN and OUT. In this table the old
-- operation_type_id is therefore a movement direction rather than an operation kind.
alter table bank_operation
    add column if not exists direction_id smallint;

do $$
begin
    if exists
    (
        select 1
        from information_schema.columns
        where table_schema = current_schema()
          and table_name = 'bank_operation'
          and column_name = 'operation_type_id'
    ) then
        update bank_operation
        set direction_id = case operation_type_id
            when 1 then 1
            when 2 then -1
            else null
        end
        where direction_id is null;
    end if;

    if exists
    (
        select 1
        from bank_operation
        where direction_id is null
           or direction_id not in (-1, 1)
    ) then
        raise exception 'Cannot migrate bank_operation: unsupported operation type or invalid direction exists';
    end if;
end
$$;

alter table bank_operation
    alter column direction_id set not null;

alter table bank_operation
    drop constraint if exists bank_operation_direction_id_fkey;

alter table bank_operation
    add constraint bank_operation_direction_id_fkey
        foreign key (direction_id) references cmn_movement_direction(id);

drop index if exists idx_bank_operation_operation_type_id;

alter table bank_operation
    drop constraint if exists bank_operation_operation_type_id_fkey;

alter table bank_operation
    drop column if exists operation_type_id;

create index if not exists idx_bank_operation_direction_id
    on bank_operation using btree (direction_id);
