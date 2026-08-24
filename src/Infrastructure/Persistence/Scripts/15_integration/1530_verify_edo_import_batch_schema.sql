do $$
declare
    missing text;
begin
    select string_agg(x, ', ' order by x)
    into missing
    from (values
        ('public.edo_import_batch'),
        ('public.edo_import_batch_document'),
        ('public.ux_edo_import_batch_organization_idempotency'),
        ('public.ux_edo_import_batch_document_organization_batch_provider_document'),
        ('public.ux_edo_document_organization_id'),
        ('public.ux_pur_doc_organization_id'),
        ('public.ux_sale_doc_organization_id')
    ) required(x)
    where to_regclass(x) is null;

    if missing is not null then
        raise exception 'EDO unified batch schema is incomplete. Missing objects: %', missing;
    end if;

    select string_agg(expected.table_name || '.' || expected.column_name, ', '
                      order by expected.table_name, expected.column_name)
    into missing
    from (values
        ('edo_import_batch', 'organization_id'),
        ('edo_import_batch', 'provider_code'),
        ('edo_import_batch', 'plan_hash'),
        ('edo_import_batch', 'status'),
        ('edo_import_batch_document', 'batch_id'),
        ('edo_import_batch_document', 'organization_id'),
        ('edo_import_batch_document', 'provider_code'),
        ('edo_import_batch_document', 'provider_document_id'),
        ('edo_import_batch_document', 'direction'),
        ('edo_import_batch_document', 'document_type'),
        ('edo_import_batch_document', 'status'),
        ('edo_import_batch_document', 'purchase_document_id'),
        ('edo_import_batch_document', 'sale_document_id'),
        ('edo_import_batch_document', 'has_marking'),
        ('edo_import_batch_document', 'marking_count'),
        ('edo_import_batch_document', 'sent_override_applied')
    ) expected(table_name, column_name)
    where not exists
    (
        select 1
        from information_schema.columns actual
        where actual.table_schema = 'public'
          and actual.table_name = expected.table_name
          and actual.column_name = expected.column_name
    );

    if missing is not null then
        raise exception 'EDO unified batch schema is incomplete. Missing columns: %', missing;
    end if;

    if exists
    (
        select 1
        from information_schema.columns
        where table_schema = 'public'
          and table_name in ('edo_import_batch', 'edo_import_batch_document')
          and (column_name ilike '%raw%' or column_name in ('marking_code', 'marking_number', 'provider_json', 'raw_json'))
    ) then
        raise exception 'EDO unified batch schema must not persist raw provider or marking values.';
    end if;

    if not exists
    (
        select 1
        from pg_constraint
        where conname = 'ck_edo_import_batch_provider'
          and contype = 'c'
    ) or not exists
    (
        select 1
        from pg_constraint
        where conname = 'ck_edo_import_batch_document_local_link'
          and contype = 'c'
    ) then
        raise exception 'EDO unified batch schema checks are incomplete.';
    end if;

    if not exists
    (
        select 1
        from pg_constraint
        where conname = 'edo_import_batch_document_edo_document_organization_fkey'
          and contype = 'f'
    ) or not exists
    (
        select 1
        from pg_constraint
        where conname = 'edo_import_batch_document_purchase_organization_fkey'
          and contype = 'f'
    ) or not exists
    (
        select 1
        from pg_constraint
        where conname = 'edo_import_batch_document_sale_organization_fkey'
          and contype = 'f'
    ) then
        raise exception 'EDO unified batch organization-scoped local link FKs are incomplete.';
    end if;

    if not exists
    (
        select 1
        from pg_indexes
        where schemaname = 'public'
          and indexname = 'ux_edo_import_batch_document_organization_batch_provider_document'
          and indexdef ilike '%(organization_id, batch_id, provider_document_id)%'
    ) then
        raise exception 'EDO unified batch document uniqueness index is invalid.';
    end if;

    if not exists
    (
        select 1
        from pg_constraint
        where conname = 'edo_import_batch_document_edo_document_organization_fkey'
          and pg_get_constraintdef(oid) ilike '%FOREIGN KEY (organization_id, edo_document_id)%'
    ) or not exists
    (
        select 1
        from pg_constraint
        where conname = 'edo_import_batch_document_purchase_organization_fkey'
          and pg_get_constraintdef(oid) ilike '%FOREIGN KEY (organization_id, purchase_document_id)%'
    ) or not exists
    (
        select 1
        from pg_constraint
        where conname = 'edo_import_batch_document_sale_organization_fkey'
          and pg_get_constraintdef(oid) ilike '%FOREIGN KEY (organization_id, sale_document_id)%'
    ) then
        raise exception 'EDO unified batch composite FK columns are invalid.';
    end if;

    if exists
    (
        select 1
        from pg_constraint
        where conname = 'edo_import_batch_document_batch_organization_provider_fkey'
          and confdeltype <> 'c'
    ) or exists
    (
        select 1
        from pg_constraint
        where conname in (
            'edo_import_batch_document_organization_id_fkey',
            'edo_import_batch_document_edo_document_organization_fkey',
            'edo_import_batch_document_purchase_organization_fkey',
            'edo_import_batch_document_sale_organization_fkey'
        )
          and confdeltype <> 'r'
    ) then
        raise exception 'EDO unified batch delete behavior is unsafe.';
    end if;
end
$$;
