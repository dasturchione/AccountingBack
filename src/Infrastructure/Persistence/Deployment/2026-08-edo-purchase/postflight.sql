-- Read-only release gate. Any exception means binaries must not be started.
\ir ../../Scripts/15_integration/1526_verify_edo_historical_import_schema.sql

do $$
declare
    table_count integer;
begin
    select count(*)
    into table_count
    from information_schema.tables
    where table_schema = 'public'
      and table_type = 'BASE TABLE';

    if table_count <> 207 then
        raise exception 'Release schema parity failed: expected 207 public base tables, found %.', table_count;
    end if;
end
$$;
