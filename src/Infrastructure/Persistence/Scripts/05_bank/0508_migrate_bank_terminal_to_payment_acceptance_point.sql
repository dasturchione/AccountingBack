begin;

do
$$
begin
    if to_regclass('public.org_payment_acceptance_point') is null then
        raise exception
            'Table org_payment_acceptance_point does not exist. Run 0503_create_org_payment_acceptance_point.sql first.';
    end if;
end;
$$;

alter table org_payment_acceptance_point
    add column if not exists type_id smallint references cmn_payment_acceptance_point_type(id),
    add column if not exists code varchar(50);

do
$$
begin
    if exists
    (
        select 1
        from information_schema.columns
        where table_schema = 'public'
          and table_name = 'org_payment_acceptance_point'
          and column_name = 'external_terminal_id'
    )
       and not exists
       (
           select 1
           from information_schema.columns
           where table_schema = 'public'
             and table_name = 'org_payment_acceptance_point'
             and column_name = 'external_id'
       )
    then
        alter table org_payment_acceptance_point
            rename column external_terminal_id to external_id;
    end if;
end;
$$;

alter table org_payment_acceptance_point
    alter column name type varchar(250),
    alter column merchant_id type varchar(150),
    alter column external_id type varchar(150),
    alter column serial_number type varchar(150);

update org_payment_acceptance_point
set type_id = (select id from cmn_payment_acceptance_point_type where code = 'POS')
where type_id is null;

update org_payment_acceptance_point
set code = 'PAP-' || lpad(id::varchar, 12, '0')
where code is null or btrim(code) = '';

alter table org_payment_acceptance_point
    alter column type_id set not null,
    alter column code set not null;

create temporary table tmp_bank_terminal_map
(
    old_id int primary key,
    new_id int not null
);

do
$$
declare
    terminal_row record;
    target_id int;
    match_count int;
    pos_type_id smallint;
    point_sequence regclass;
    max_point_id int;
    generated_code varchar(50);
begin
    if to_regclass('public.bank_terminal') is null then
        return;
    end if;

    select id
    into pos_type_id
    from cmn_payment_acceptance_point_type
    where code = 'POS';

    if pos_type_id is null then
        raise exception 'Payment acceptance point type POS was not found';
    end if;

    point_sequence := pg_get_serial_sequence('org_payment_acceptance_point', 'id')::regclass;
    select coalesce(max(id), 0)
    into max_point_id
    from org_payment_acceptance_point;

    if max_point_id = 0 then
        perform setval(point_sequence, 1, false);
    else
        perform setval(point_sequence, max_point_id, true);
    end if;

    for terminal_row in
        select
            id,
            organization_id,
            bank_account_id,
            name,
            merchant_id,
            external_terminal_id,
            serial_number,
            state_id,
            created_date
        from bank_terminal
        order by id
    loop
        select count(distinct point.id), min(point.id)
        into match_count, target_id
        from org_payment_acceptance_point point
        where point.organization_id = terminal_row.organization_id
          and
          (
              (terminal_row.external_terminal_id is not null and point.external_id = terminal_row.external_terminal_id)
              or (terminal_row.serial_number is not null and point.serial_number = terminal_row.serial_number)
              or
              (
                  terminal_row.merchant_id is not null
                  and point.merchant_id = terminal_row.merchant_id
                  and point.bank_account_id is not distinct from terminal_row.bank_account_id
                  and point.name = terminal_row.name
              )
              or point.id = terminal_row.id
          );

        if match_count > 1 then
            raise exception
                'Bank terminal % matches more than one payment acceptance point',
                terminal_row.id;
        end if;

        if target_id is null then
            if not exists
            (
                select 1
                from org_payment_acceptance_point
                where id = terminal_row.id
            )
            then
                target_id := terminal_row.id;
            else
                target_id := nextval(point_sequence);
            end if;

            generated_code := 'PAP-' || lpad(target_id::varchar, 12, '0');
            while exists
            (
                select 1
                from org_payment_acceptance_point
                where id = target_id
                   or (organization_id = terminal_row.organization_id and code = generated_code)
            )
            loop
                target_id := nextval(point_sequence);
                generated_code := 'PAP-' || lpad(target_id::varchar, 12, '0');
            end loop;

            insert into org_payment_acceptance_point
            (
                id,
                organization_id,
                type_id,
                bank_account_id,
                code,
                name,
                merchant_id,
                external_id,
                serial_number,
                state_id,
                created_date
            )
            values
            (
                target_id,
                terminal_row.organization_id,
                pos_type_id,
                terminal_row.bank_account_id,
                generated_code,
                terminal_row.name,
                terminal_row.merchant_id,
                terminal_row.external_terminal_id,
                terminal_row.serial_number,
                terminal_row.state_id,
                terminal_row.created_date
            );
        end if;

        insert into tmp_bank_terminal_map (old_id, new_id)
        values (terminal_row.id, target_id);
    end loop;

    select coalesce(max(id), 0)
    into max_point_id
    from org_payment_acceptance_point;

    if max_point_id = 0 then
        perform setval(point_sequence, 1, false);
    else
        perform setval(point_sequence, max_point_id, true);
    end if;
end;
$$;

do
$$
declare
    has_old_column boolean;
    has_new_column boolean;
    new_column_references_old_table boolean;
    fk_row record;
    unresolved_count bigint;
begin
    if to_regclass('public.rtl_sale_doc_payment') is null then
        raise exception 'Table rtl_sale_doc_payment does not exist';
    end if;

    select exists
    (
        select 1
        from information_schema.columns
        where table_schema = 'public'
          and table_name = 'rtl_sale_doc_payment'
          and column_name = 'bank_terminal_id'
    )
    into has_old_column;

    select exists
    (
        select 1
        from information_schema.columns
        where table_schema = 'public'
          and table_name = 'rtl_sale_doc_payment'
          and column_name = 'payment_acceptance_point_id'
    )
    into has_new_column;

    select exists
    (
        select 1
        from pg_constraint
        where conrelid = 'rtl_sale_doc_payment'::regclass
          and confrelid = to_regclass('public.bank_terminal')
          and contype = 'f'
    )
    into new_column_references_old_table;

    if has_old_column then
        select count(*)
        into unresolved_count
        from rtl_sale_doc_payment payment
        left join tmp_bank_terminal_map terminal_map
            on terminal_map.old_id = payment.bank_terminal_id
        where payment.bank_terminal_id is not null
          and terminal_map.old_id is null;

        if unresolved_count > 0 then
            raise exception
                '% retail sale payments reference bank terminals that were not migrated',
                unresolved_count;
        end if;

        if has_new_column then
            select count(*)
            into unresolved_count
            from rtl_sale_doc_payment payment
            join tmp_bank_terminal_map terminal_map
                on terminal_map.old_id = payment.bank_terminal_id
            where payment.payment_acceptance_point_id is not null
              and payment.payment_acceptance_point_id <> terminal_map.new_id;

            if unresolved_count > 0 then
                raise exception
                    '% retail sale payments have conflicting old and new payment point references',
                    unresolved_count;
            end if;

            update rtl_sale_doc_payment payment
            set payment_acceptance_point_id = terminal_map.new_id
            from tmp_bank_terminal_map terminal_map
            where payment.bank_terminal_id = terminal_map.old_id
              and payment.payment_acceptance_point_id is null;
        end if;
    end if;

    for fk_row in
        select constraint_row.conname
        from pg_constraint constraint_row
        where constraint_row.conrelid = 'rtl_sale_doc_payment'::regclass
          and constraint_row.confrelid = to_regclass('public.bank_terminal')
          and constraint_row.contype = 'f'
    loop
        execute format(
            'alter table rtl_sale_doc_payment drop constraint %I',
            fk_row.conname);
    end loop;

    if has_old_column and not has_new_column then
        update rtl_sale_doc_payment payment
        set bank_terminal_id = terminal_map.new_id
        from tmp_bank_terminal_map terminal_map
        where payment.bank_terminal_id = terminal_map.old_id;

        alter table rtl_sale_doc_payment
            rename column bank_terminal_id to payment_acceptance_point_id;
        has_new_column := true;
    elsif has_old_column and has_new_column then
        alter table rtl_sale_doc_payment
            drop column bank_terminal_id;
    elsif has_new_column and new_column_references_old_table then
        update rtl_sale_doc_payment payment
        set payment_acceptance_point_id = terminal_map.new_id
        from tmp_bank_terminal_map terminal_map
        where payment.payment_acceptance_point_id = terminal_map.old_id;
    end if;

    if not has_new_column then
        raise exception 'Column rtl_sale_doc_payment.payment_acceptance_point_id was not created';
    end if;

    select count(*)
    into unresolved_count
    from rtl_sale_doc_payment payment
    left join org_payment_acceptance_point point
        on point.id = payment.payment_acceptance_point_id
    where payment.payment_acceptance_point_id is not null
      and point.id is null;

    if unresolved_count > 0 then
        raise exception
            '% retail sale payments reference missing payment acceptance points',
            unresolved_count;
    end if;

    if not exists
    (
        select 1
        from pg_constraint constraint_row
        where constraint_row.conrelid = 'rtl_sale_doc_payment'::regclass
          and constraint_row.confrelid = 'org_payment_acceptance_point'::regclass
          and constraint_row.contype = 'f'
    )
    then
        alter table rtl_sale_doc_payment
            add constraint rtl_sale_doc_payment_payment_acceptance_point_id_fkey
            foreign key (payment_acceptance_point_id)
            references org_payment_acceptance_point(id);
    end if;
end;
$$;

drop index if exists ix_rtl_sale_doc_payment_bank_terminal_id;

create index if not exists ix_rtl_sale_doc_payment_payment_acceptance_point_id
    on rtl_sale_doc_payment(payment_acceptance_point_id);

do
$$
declare
    remaining_reference_count int;
begin
    if to_regclass('public.bank_terminal') is null then
        return;
    end if;

    select count(*)
    into remaining_reference_count
    from pg_constraint
    where confrelid = 'bank_terminal'::regclass
      and contype = 'f';

    if remaining_reference_count > 0 then
        raise exception
            'bank_terminal still has % foreign key references and cannot be removed safely',
            remaining_reference_count;
    end if;

    drop table bank_terminal;
end;
$$;

drop sequence if exists bank_terminal_id_seq;
drop function if exists check_bank_terminal_organization();

drop index if exists ix_bank_terminal_organization_id;
drop index if exists ix_bank_terminal_bank_account_id;
drop index if exists ix_bank_terminal_state_id;
drop index if exists ix_bank_terminal_organization_id_state_id;
drop index if exists ix_bank_terminal_merchant_id;
drop index if exists ux_bank_terminal_organization_external_terminal_id;
drop index if exists ux_bank_terminal_organization_serial_number;

create unique index if not exists ux_org_payment_acceptance_point_organization_code
    on org_payment_acceptance_point(organization_id, code);

create unique index if not exists ux_org_payment_acceptance_point_organization_external_id
    on org_payment_acceptance_point(organization_id, external_id)
    where external_id is not null;

create unique index if not exists ux_org_payment_acceptance_point_organization_serial_number
    on org_payment_acceptance_point(organization_id, serial_number)
    where serial_number is not null;

create index if not exists ix_org_payment_acceptance_point_organization_state
    on org_payment_acceptance_point(organization_id, state_id);

create index if not exists ix_org_payment_acceptance_point_type_id
    on org_payment_acceptance_point(type_id);

create index if not exists ix_org_payment_acceptance_point_bank_account_id
    on org_payment_acceptance_point(bank_account_id);

create index if not exists ix_org_payment_acceptance_point_merchant_id
    on org_payment_acceptance_point(merchant_id)
    where merchant_id is not null;

create or replace function check_payment_acceptance_point_organization()
returns trigger
language plpgsql
as
$$
begin
    if new.bank_account_id is not null
       and not exists
       (
           select 1
           from org_bank_account
           where id = new.bank_account_id
             and organization_id = new.organization_id
       )
    then
        raise exception
            'Bank account % does not belong to organization %',
            new.bank_account_id,
            new.organization_id;
    end if;

    return new;
end;
$$;

drop trigger if exists trg_org_payment_acceptance_point_check_organization
    on org_payment_acceptance_point;

create trigger trg_org_payment_acceptance_point_check_organization
before insert or update of organization_id, bank_account_id
on org_payment_acceptance_point
for each row execute function check_payment_acceptance_point_organization();

drop table if exists tmp_bank_terminal_map;

commit;
