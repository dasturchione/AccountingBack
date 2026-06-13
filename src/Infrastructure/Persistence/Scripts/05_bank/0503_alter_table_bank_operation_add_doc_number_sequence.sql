create sequence if not exists doc_number_bank_operation_seq
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
      from bank_operation
     where doc_number ~ '^[0-9]+$';

    perform setval(
        'doc_number_bank_operation_seq',
        greatest(max_doc_number, 100000000),
        true
    );
end;
$$;

create or replace function set_bank_operation_doc_number()
returns trigger as $$
begin
    new.doc_number := lpad(nextval('doc_number_bank_operation_seq')::text, 9, '0');
    return new;
end;
$$ language plpgsql;

drop trigger if exists set_bank_operation_doc_number_trigger on bank_operation;

create trigger set_bank_operation_doc_number_trigger
before insert on bank_operation
for each row
execute function set_bank_operation_doc_number();
