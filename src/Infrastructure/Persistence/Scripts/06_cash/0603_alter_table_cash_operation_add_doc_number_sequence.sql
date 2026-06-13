create sequence if not exists doc_number_cash_operation_seq
    start with 100000001
    increment by 1
    no minvalue
    no maxvalue
    cache 1;

do $$
declare
    max_doc_number bigint;
begin
    select coalesce(max(doc_number::bigint), 100000000)
      into max_doc_number
      from cash_operation
     where doc_number ~ '^[0-9]+$';

    perform setval(
        'doc_number_cash_operation_seq',
        greatest(max_doc_number, 100000000),
        true
    );
end;
$$;

create or replace function set_cash_operation_doc_number()
returns trigger as $$
begin
    new.doc_number := lpad(nextval('doc_number_cash_operation_seq')::text, 9, '0');
    return new;
end;
$$ language plpgsql;

drop trigger if exists set_cash_operation_doc_number_trigger on cash_operation;

create trigger set_cash_operation_doc_number_trigger
before insert on cash_operation
for each row
execute function set_cash_operation_doc_number();
