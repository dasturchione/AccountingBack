begin;

set local lock_timeout = '5s';
set local statement_timeout = '60s';

alter table pay_component
    add column if not exists proration_basis varchar(10) not null default 'DAYS';

update pay_component
set proration_basis = 'DAYS'
where proration_basis is null;

do $$
begin
    if not exists (
        select 1
        from pg_constraint
        where conname = 'ck_pay_component_proration_basis'
          and conrelid = 'pay_component'::regclass
    ) then
        alter table pay_component
            add constraint ck_pay_component_proration_basis
            check (proration_basis in ('DAYS', 'HOURS'));
    end if;
end $$;

commit;
