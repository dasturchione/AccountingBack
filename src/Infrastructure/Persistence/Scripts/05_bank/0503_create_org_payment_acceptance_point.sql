create table org_payment_acceptance_point
(
    id                  serial primary key,
    organization_id     int not null references org_organization(id),
    type_id             smallint not null references cmn_payment_acceptance_point_type(id),
    bank_account_id     int references org_bank_account(id),
    code                varchar(50) not null,
    name                varchar(250) not null,
    merchant_id         varchar(150),
    external_id         varchar(150),
    serial_number       varchar(150),
    state_id            smallint not null references cmn_state(id),
    created_date        timestamp without time zone not null default now()
);

create unique index ux_org_payment_acceptance_point_organization_code
    on org_payment_acceptance_point(organization_id, code);

create unique index ux_org_payment_acceptance_point_organization_external_id
    on org_payment_acceptance_point(organization_id, external_id)
    where external_id is not null;

create unique index ux_org_payment_acceptance_point_organization_serial_number
    on org_payment_acceptance_point(organization_id, serial_number)
    where serial_number is not null;

create index ix_org_payment_acceptance_point_organization_state
    on org_payment_acceptance_point(organization_id, state_id);

create index ix_org_payment_acceptance_point_type_id
    on org_payment_acceptance_point(type_id);

create index ix_org_payment_acceptance_point_bank_account_id
    on org_payment_acceptance_point(bank_account_id);

create index ix_org_payment_acceptance_point_merchant_id
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

create trigger trg_org_payment_acceptance_point_check_organization
before insert or update of organization_id, bank_account_id
on org_payment_acceptance_point
for each row execute function check_payment_acceptance_point_organization();
