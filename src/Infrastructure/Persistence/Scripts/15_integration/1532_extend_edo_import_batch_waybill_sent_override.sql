do $$
begin
    if to_regclass('public.edo_import_batch_document') is null then
        raise exception 'EDO unified batch document table is required before 1532.';
    end if;

    if exists
    (
        select 1
        from edo_import_batch_document
        where document_type not in ('FACTURA', 'WAYBILL_LOCAL')
    ) then
        raise exception 'EDO unified batch contains an unsupported document type; refusing upgrade.';
    end if;

    if not exists
    (
        select 1
        from information_schema.columns
        where table_schema = 'public'
          and table_name = 'edo_import_batch_document'
          and column_name = 'sent_override_applied'
    ) then
        alter table edo_import_batch_document
            add column sent_override_applied boolean default false not null;
    end if;

    alter table edo_import_batch_document
        drop constraint if exists ck_edo_import_batch_document_type;

    alter table edo_import_batch_document
        add constraint ck_edo_import_batch_document_type
        check (document_type in ('FACTURA', 'WAYBILL_LOCAL'));
end
$$;
