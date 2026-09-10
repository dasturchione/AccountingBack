begin;

alter table pay_component
    add column if not exists depends_on_component_id integer references pay_component(id) on delete restrict,
    add column if not exists minimum_amount numeric(18,2),
    add column if not exists maximum_amount numeric(18,2),
    add column if not exists is_taxable boolean not null default true;

alter table pay_component
    drop constraint if exists ck_pay_component_formula_amount_range;

alter table pay_component
    add constraint ck_pay_component_formula_amount_range check (
        (minimum_amount is null or minimum_amount >= 0) and
        (maximum_amount is null or maximum_amount >= 0) and
        (minimum_amount is null or maximum_amount is null or minimum_amount <= maximum_amount)
    );

create index if not exists idx_pay_component_depends_on_component_id
    on pay_component (depends_on_component_id);

commit;
