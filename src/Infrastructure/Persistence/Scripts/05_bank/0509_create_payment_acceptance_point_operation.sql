alter table money_reg_balance
    alter column source_type type varchar(50),
    alter column amount type numeric(24, 8);

create table payment_acceptance_point_operation
(
    id                              bigserial primary key,
    organization_id                 int not null references org_organization(id),
    payment_acceptance_point_id     int not null references org_payment_acceptance_point(id),
    direction_id                    smallint not null references cmn_movement_direction(id),
    doc_number                      varchar(100) not null,
    doc_date                        timestamp without time zone not null,
    currency_id                     smallint not null references cmn_currency(id),
    amount                          numeric(24, 8) not null,
    exchange_rate                   numeric(18, 6) not null default 1,
    external_transaction_number     varchar(150),
    related_document_id             bigint references cmn_document_registry(id),
    comment                         varchar(1000),
    status_id                       smallint not null references cmn_document_status(id),
    state_id                        smallint not null default 1 references cmn_state(id),
    created_date                    timestamp without time zone not null default now(),
    posted_at                       timestamp without time zone,
    posted_by_user_id               int,
    cancelled_at                    timestamp without time zone,
    cancelled_by_user_id            int,

    constraint ck_payment_acceptance_point_operation_direction
        check (direction_id in (-1, 1)),
    constraint ck_payment_acceptance_point_operation_amount
        check (amount > 0),
    constraint ck_payment_acceptance_point_operation_exchange_rate
        check (exchange_rate > 0)
);

create unique index ux_payment_acceptance_point_operation_org_year_doc_number
    on payment_acceptance_point_operation
        (organization_id, (extract(year from doc_date)), doc_number);

create index ix_payment_acceptance_point_operation_point_date
    on payment_acceptance_point_operation
        (payment_acceptance_point_id, doc_date desc, id desc);

create index ix_payment_acceptance_point_operation_status_id
    on payment_acceptance_point_operation(status_id);

create index ix_payment_acceptance_point_operation_external_transaction_number
    on payment_acceptance_point_operation(external_transaction_number)
    where external_transaction_number is not null;

create index ix_payment_acceptance_point_operation_related_document_id
    on payment_acceptance_point_operation(related_document_id)
    where related_document_id is not null;

create index ix_payment_acceptance_point_operation_draft
    on payment_acceptance_point_operation(organization_id, doc_date desc, id desc)
    where status_id = 1 and state_id = 1;

create index ix_payment_acceptance_point_operation_pending
    on payment_acceptance_point_operation(organization_id, doc_date desc, id desc)
    where status_id = 4 and state_id = 1;

create or replace function check_payment_acceptance_point_operation_organization()
returns trigger
language plpgsql
as
$$
begin
    if not exists
    (
        select 1
        from org_payment_acceptance_point
        where id = new.payment_acceptance_point_id
          and organization_id = new.organization_id
          and state_id = 1
    )
    then
        raise exception
            'Payment acceptance point % does not belong to organization %',
            new.payment_acceptance_point_id,
            new.organization_id;
    end if;

    if new.related_document_id is not null
       and not exists
       (
           select 1
           from cmn_document_registry
           where id = new.related_document_id
             and organization_id = new.organization_id
             and state_id = 1
       )
    then
        raise exception
            'Related document % is inactive or does not belong to organization %',
            new.related_document_id,
            new.organization_id;
    end if;

    return new;
end;
$$;

create trigger trg_payment_acceptance_point_operation_check_organization
before insert or update of organization_id, payment_acceptance_point_id, related_document_id
on payment_acceptance_point_operation
for each row execute function check_payment_acceptance_point_operation_organization();

insert into cmn_document_registry
    (organization_id, document_type_id, document_id, doc_number, doc_date, amount, currency_id, status_id, state_id, created_date)
select organization_id, 25, id, doc_number, doc_date, amount, currency_id, status_id, state_id, created_date
from payment_acceptance_point_operation
on conflict (document_type_id, document_id) do update set
    organization_id = excluded.organization_id,
    doc_number = excluded.doc_number,
    doc_date = excluded.doc_date,
    amount = excluded.amount,
    currency_id = excluded.currency_id,
    status_id = excluded.status_id,
    state_id = excluded.state_id,
    updated_date = now();

create trigger trg_payment_acceptance_point_operation_document_registry
after insert or update or delete on payment_acceptance_point_operation
for each row execute function cmn_sync_document_registry('25', 'doc_number', 'doc_date', 'amount', 'currency_id', 'status_id', 'state_id');
