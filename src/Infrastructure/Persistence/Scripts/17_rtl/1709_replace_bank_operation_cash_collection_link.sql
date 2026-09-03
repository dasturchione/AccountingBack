alter table bank_operation
    add column related_document_id bigint references cmn_document_registry(id);

update bank_operation as operation
set related_document_id = document.id
from cmn_document_registry as document
where operation.cash_collection_doc_id is not null
  and document.document_type_id = 24
  and document.document_id = operation.cash_collection_doc_id
  and document.organization_id = operation.organization_id;

drop index if exists ux_bank_operation_active_cash_collection_doc_id;
drop index if exists idx_bank_operation_cash_collection_doc_id;

alter table bank_operation
    drop column cash_collection_doc_id;

create index idx_bank_operation_related_document_id
    on bank_operation (related_document_id);

create or replace function bank_operation_validate_related_document()
returns trigger
language plpgsql
as
$$
declare
    related_document_type_id smallint;
    related_document_organization_id int;
begin
    if new.related_document_id is null then
        return new;
    end if;

    select document_type_id, organization_id
    into related_document_type_id, related_document_organization_id
    from cmn_document_registry
    where id = new.related_document_id;

    if not found then
        return new;
    end if;

    if related_document_organization_id <> new.organization_id then
        raise exception 'Related document belongs to another organization.'
            using errcode = '23514';
    end if;

    if new.state_id <> 1 or
       new.status_id = 3 or
       related_document_type_id <> 24 then
        return new;
    end if;

    perform pg_advisory_xact_lock(new.related_document_id);

    if exists
    (
        select 1
        from bank_operation
        where related_document_id = new.related_document_id
          and state_id = 1
          and status_id <> 3
          and id <> new.id
    ) then
        raise exception 'Cash collection document is already linked to an active bank operation.'
            using errcode = '23505';
    end if;

    return new;
end;
$$;

create trigger trg_bank_operation_related_document
    before insert or update of related_document_id, organization_id, state_id, status_id
    on bank_operation
    for each row
    execute function bank_operation_validate_related_document();
