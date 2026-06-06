alter table org_organization
add column if not exists default_language_id smallint null references cmn_language(id);

create index if not exists idx_org_organization_default_language_id
on org_organization (default_language_id);
