-- money_reg_balance stores actual movements relative to SourceType/SourceId.
-- For a cash transfer the source and destination are separate OUT and IN rows.
alter table money_reg_balance
    add column if not exists direction_id smallint;

do $$
begin
    if exists
    (
        select 1
        from information_schema.columns
        where table_schema = current_schema()
          and table_name = 'money_reg_balance'
          and column_name = 'operation_type_id'
    ) then
        update money_reg_balance
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
        from money_reg_balance
        where direction_id is null
           or direction_id not in (-1, 1)
    ) then
        raise exception 'Cannot migrate money_reg_balance: unsupported operation type or invalid direction exists';
    end if;
end
$$;

alter table money_reg_balance
    alter column direction_id set not null;

alter table money_reg_balance
    drop constraint if exists money_reg_balance_direction_id_fkey;

alter table money_reg_balance
    add constraint money_reg_balance_direction_id_fkey
        foreign key (direction_id) references cmn_movement_direction(id);

alter table money_reg_balance
    drop constraint if exists money_reg_balance_operation_type_id_fkey;

alter table money_reg_balance
    drop column if exists operation_type_id;

create index if not exists idx_money_reg_balance_direction_id
    on money_reg_balance using btree (direction_id);
