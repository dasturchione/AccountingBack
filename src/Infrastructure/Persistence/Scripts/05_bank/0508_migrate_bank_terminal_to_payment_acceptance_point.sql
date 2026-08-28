do
$$
begin
    if to_regclass('public.bank_terminal') is not null
       and to_regclass('public.org_payment_acceptance_point') is null
    then
        alter table bank_terminal rename to org_payment_acceptance_point;
    end if;
end;
$$;

do
$$
begin
    if to_regclass('public.bank_terminal_id_seq') is not null
       and to_regclass('public.org_payment_acceptance_point_id_seq') is null
    then
        alter sequence bank_terminal_id_seq rename to org_payment_acceptance_point_id_seq;
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

do
$$
begin
    if exists
    (
        select 1
        from information_schema.columns
        where table_schema = 'public'
          and table_name = 'rtl_sale_doc_payment'
          and column_name = 'bank_terminal_id'
    )
    then
        alter table rtl_sale_doc_payment
            rename column bank_terminal_id to payment_acceptance_point_id;
    end if;
end;
$$;

drop index if exists ix_rtl_sale_doc_payment_bank_terminal_id;
create index if not exists ix_rtl_sale_doc_payment_payment_acceptance_point_id
    on rtl_sale_doc_payment(payment_acceptance_point_id);

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

drop function if exists check_bank_terminal_organization();
