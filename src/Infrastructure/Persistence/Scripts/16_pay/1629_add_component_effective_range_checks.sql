begin;

alter table pay_component
    drop constraint if exists ck_pay_component_effective_range;

alter table pay_component
    add constraint ck_pay_component_effective_range
    check (effective_to is null or effective_to >= effective_from);

create index if not exists idx_pay_component_code_effective
    on pay_component (organization_id, code, effective_from, effective_to);

commit;
