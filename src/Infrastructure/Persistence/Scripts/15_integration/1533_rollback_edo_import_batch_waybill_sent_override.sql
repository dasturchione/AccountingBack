do $$
begin
    if to_regclass('public.edo_import_batch_document') is null then
        raise exception 'EDO unified batch document table is missing; refusing rollback.';
    end if;

    if exists
    (
        select 1
        from edo_import_batch_document
        where document_type = 'WAYBILL_LOCAL'
           or sent_override_applied = true
    ) then
        raise exception 'EDO unified batch extension rollback refused: waybill or sent-override data exists.';
    end if;

    alter table edo_import_batch_document
        drop constraint if exists ck_edo_import_batch_document_type;

    alter table edo_import_batch_document
        add constraint ck_edo_import_batch_document_type
        check (document_type = 'FACTURA');

    alter table edo_import_batch_document
        drop column if exists sent_override_applied;
end
$$;
