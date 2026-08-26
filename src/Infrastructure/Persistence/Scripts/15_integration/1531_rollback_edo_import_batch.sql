do $$
begin
    if to_regclass('public.edo_import_batch_document') is not null
       and exists (select 1 from edo_import_batch_document limit 1) then
        raise exception 'EDO unified batch rollback refused: batch documents exist.';
    end if;

    if to_regclass('public.edo_import_batch') is not null
       and exists (select 1 from edo_import_batch limit 1) then
        raise exception 'EDO unified batch rollback refused: batches exist.';
    end if;
end
$$;

drop table if exists edo_import_batch_document;
drop table if exists edo_import_batch;

drop index if exists ux_edo_document_organization_id;
drop index if exists ux_pur_doc_organization_id;
drop index if exists ux_sale_doc_organization_id;
