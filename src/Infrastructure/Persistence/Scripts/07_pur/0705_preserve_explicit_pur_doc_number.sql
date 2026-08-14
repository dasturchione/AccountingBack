-- Historical EDO imports supply an application-generated H-{year}-{sequence} number.
-- Preserve any explicit non-blank number; retain the legacy numeric fallback only when absent.
create or replace function set_pur_doc_number() returns trigger
    language plpgsql
as $$
begin
    if new.doc_number is not null and btrim(new.doc_number) <> '' then
        return new;
    end if;

    perform pg_advisory_xact_lock(hashtext('pur_doc_number'));
    new.doc_number := lpad((coalesce((
        select max(doc_number::bigint)
        from pur_doc
        where doc_number ~ '^[0-9]+$'
    ), 100000000) + 1)::text, 9, '0');
    return new;
end;
$$;
