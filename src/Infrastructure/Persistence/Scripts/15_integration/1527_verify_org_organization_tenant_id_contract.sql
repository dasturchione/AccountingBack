do $$
declare
    table_exists boolean;
    column_exists boolean;
    column_nullable text;
    tenant_foreign_key_exists boolean;
begin
    select to_regclass('public.org_organization') is not null
    into table_exists;

    if not table_exists then
        raise exception 'ORG_ORGANIZATION_TENANT_CONTRACT_TABLE_MISSING';
    end if;

    select exists (
        select 1
        from information_schema.columns
        where table_schema = 'public'
          and table_name = 'org_organization'
          and column_name = 'tenant_id'
    )
    into column_exists;

    if not column_exists then
        raise exception 'ORG_ORGANIZATION_TENANT_CONTRACT_COLUMN_MISSING';
    end if;

    select is_nullable
    from information_schema.columns
    where table_schema = 'public'
      and table_name = 'org_organization'
      and column_name = 'tenant_id'
    into column_nullable;

    if column_nullable <> 'NO' then
        raise exception 'ORG_ORGANIZATION_TENANT_CONTRACT_NULLABLE';
    end if;

    select exists (
        select 1
        from pg_constraint constraint_row
        join pg_class child_table
            on child_table.oid = constraint_row.conrelid
        join pg_namespace child_schema
            on child_schema.oid = child_table.relnamespace
        join pg_class parent_table
            on parent_table.oid = constraint_row.confrelid
        join pg_namespace parent_schema
            on parent_schema.oid = parent_table.relnamespace
        join pg_attribute child_column
            on child_column.attrelid = child_table.oid
           and child_column.attname = 'tenant_id'
        where constraint_row.contype = 'f'
          and child_schema.nspname = 'public'
          and child_table.relname = 'org_organization'
          and parent_schema.nspname = 'public'
          and parent_table.relname = 'platform_tenant'
          and child_column.attnum = any(constraint_row.conkey)
    )
    into tenant_foreign_key_exists;

    if not tenant_foreign_key_exists then
        raise exception 'ORG_ORGANIZATION_TENANT_CONTRACT_FOREIGN_KEY_MISSING';
    end if;
end
$$;
