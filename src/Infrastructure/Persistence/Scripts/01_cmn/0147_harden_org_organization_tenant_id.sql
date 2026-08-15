begin;

do $$
begin
    if exists (
        select 1
        from public.org_organization
        where tenant_id is null
    ) then
        raise exception 'ORG_ORGANIZATION_TENANT_ID_NULL_ROWS';
    end if;
end
$$;

alter table public.org_organization
    alter column tenant_id set not null;

commit;
