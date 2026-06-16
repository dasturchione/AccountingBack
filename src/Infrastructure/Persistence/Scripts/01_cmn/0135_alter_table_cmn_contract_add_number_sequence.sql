create sequence if not exists contract_number_seq
    start with 100000001
    increment by 1
    no minvalue
    no maxvalue
    cache 1;

do $$
declare
    max_contract_number bigint;
begin
    select coalesce(max(contract_number::bigint), 100000000)
      into max_contract_number
      from cmn_contract
     where contract_number ~ '^[0-9]+$';

    perform setval(
        'contract_number_seq',
        greatest(max_contract_number, 100000000),
        true
    );
end;
$$;

create or replace function set_cmn_contract_number()
returns trigger as $$
begin
    new.contract_number := lpad(nextval('contract_number_seq')::text, 9, '0');
    return new;
end;
$$ language plpgsql;

drop trigger if exists set_cmn_contract_number_trigger on cmn_contract;

create trigger set_cmn_contract_number_trigger
before insert on cmn_contract
for each row
execute function set_cmn_contract_number();
