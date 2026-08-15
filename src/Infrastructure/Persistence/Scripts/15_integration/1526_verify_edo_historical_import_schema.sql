do $$
declare
    missing_objects text;
begin
    select string_agg(required_object, ', ' order by required_object)
    into missing_objects
    from (values
        ('public.edo_import_job'),
        ('public.edo_import_job_provider'),
        ('public.edo_import_candidate'),
        ('public.edo_import_candidate_line'),
        ('public.edo_import_candidate_marking'),
        ('public.edo_provider_product_mapping'),
        ('public.ux_edo_import_job_active_organization'),
        ('public.ux_edo_import_candidate_imported_purchase'),
        ('public.ux_edo_provider_product_mapping_identity'),
        ('public.ux_cmn_contract_provider_identity'),
        ('public.idx_edo_import_job_bulk_status')
    ) expected(required_object)
    where to_regclass(required_object) is null;

    if missing_objects is not null then
        raise exception 'EDO historical import schema is incomplete. Missing objects: %', missing_objects;
    end if;

    select string_agg(table_name || '.' || column_name, ', ' order by table_name, column_name)
    into missing_objects
    from (values
        ('edo_import_job', 'bulk_import_status'),
        ('edo_import_job', 'bulk_processed_count'),
        ('edo_import_job', 'bulk_last_safe_error_code'),
        ('edo_import_candidate', 'organization_id'),
        ('edo_import_candidate', 'imported_purchase_id'),
        ('edo_import_candidate', 'selected_contract_id'),
        ('edo_import_candidate_line', 'provider_product_name'),
        ('edo_import_candidate_marking', 'marking_number'),
        ('edo_provider_product_mapping', 'identity_hash'),
        ('cmn_contract', 'provider_code'),
        ('cmn_contract', 'provider_contract_number'),
        ('cmn_contract', 'provider_contract_date')
    ) expected(table_name, column_name)
    where not exists (
        select 1
        from information_schema.columns actual
        where actual.table_schema = 'public'
          and actual.table_name = expected.table_name
          and actual.column_name = expected.column_name
    );

    if missing_objects is not null then
        raise exception 'EDO historical import schema is incomplete. Missing columns: %', missing_objects;
    end if;

    if to_regprocedure('public.set_pur_doc_number()') is null then
        raise exception 'Historical Purchase document-number function is missing.';
    end if;

    if position('btrim(new.doc_number)' in pg_get_functiondef(to_regprocedure('public.set_pur_doc_number()'))) = 0 then
        raise exception 'set_pur_doc_number() does not preserve explicit historical document numbers.';
    end if;
end
$$;
