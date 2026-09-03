alter table payment_acceptance_point_operation
    add column if not exists related_document_id bigint references cmn_document_registry(id);

create index if not exists ix_payment_acceptance_point_operation_related_document_id
    on payment_acceptance_point_operation(related_document_id)
    where related_document_id is not null;

create index if not exists ix_payment_acceptance_point_operation_pending
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
            'Payment acceptance point % is inactive or does not belong to organization %',
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

drop trigger if exists trg_payment_acceptance_point_operation_check_organization
    on payment_acceptance_point_operation;

create trigger trg_payment_acceptance_point_operation_check_organization
before insert or update of organization_id, payment_acceptance_point_id, related_document_id
on payment_acceptance_point_operation
for each row execute function check_payment_acceptance_point_operation_organization();

create or replace function check_retail_payment_acceptance_point()
returns trigger
language plpgsql
as
$$
declare
    payment_method_code varchar(30);
    sale_organization_id int;
begin
    select code
    into payment_method_code
    from rtl_payment_method
    where id = new.payment_method_id;

    if payment_method_code = 'CASH' and new.payment_acceptance_point_id is not null then
        raise exception 'Cash payment must not have a payment acceptance point';
    end if;

    if payment_method_code <> 'CASH' and new.payment_acceptance_point_id is null then
        raise exception 'Payment acceptance point is required for non-cash payment method %', payment_method_code;
    end if;

    if new.payment_acceptance_point_id is not null then
        select organization_id
        into sale_organization_id
        from rtl_sale_doc
        where id = new.owner_id;

        if not exists
        (
            select 1
            from org_payment_acceptance_point
            where id = new.payment_acceptance_point_id
              and organization_id = sale_organization_id
              and state_id = 1
        )
        then
            raise exception
                'Payment acceptance point % is inactive or does not belong to retail sale organization',
                new.payment_acceptance_point_id;
        end if;
    end if;

    return new;
end;
$$;

drop trigger if exists trg_rtl_sale_doc_payment_check_acceptance_point
    on rtl_sale_doc_payment;

create trigger trg_rtl_sale_doc_payment_check_acceptance_point
before insert or update of owner_id, payment_method_id, payment_acceptance_point_id
on rtl_sale_doc_payment
for each row execute function check_retail_payment_acceptance_point();
