begin;

update fa_receipt_doc_asset ra
set asset_account_id = a.asset_account_id
from fa_asset a
where ra.fa_asset_id = a.id
  and ra.asset_account_id is null;

update fa_asset a
set asset_account_id = ra.asset_account_id
from fa_receipt_doc_asset ra
where ra.fa_asset_id = a.id
  and a.asset_account_id is null;

create table fa_asset_accounting
(
    asset_id bigint not null
        constraint fa_asset_accounting_asset_id_fkey references fa_asset (id),
    initial_cost numeric(24,8) not null,
    asset_account_id integer not null
        constraint fa_asset_accounting_asset_account_id_fkey references acc_chart_account (id),
    salvage_value numeric(24,8),
    depreciation_method_id smallint
        constraint fa_asset_accounting_depreciation_method_id_fkey references cmn_fa_depreciation_method (id),
    useful_life_months integer,
    depr_start_date timestamp without time zone,
    planned_units_total numeric(24,8),
    accumulated_depreciation_account_id integer
        constraint fa_asset_accounting_accumulated_depreciation_account_id_fkey references acc_chart_account (id),
    depreciation_expense_account_id integer
        constraint fa_asset_accounting_depreciation_expense_account_id_fkey references acc_chart_account (id),
    created_date timestamp without time zone default now() not null,
    updated_date timestamp without time zone default now() not null,
    constraint pk_fa_asset_accounting primary key (asset_id),
    constraint ck_fa_asset_accounting_initial_cost check (initial_cost >= 0),
    constraint ck_fa_asset_accounting_salvage_value check
    (
        salvage_value is null
        or (salvage_value >= 0 and salvage_value <= initial_cost)
    ),
    constraint ck_fa_asset_accounting_useful_life check
    (
        useful_life_months is null or useful_life_months > 0
    ),
    constraint ck_fa_asset_accounting_planned_units check
    (
        planned_units_total is null or planned_units_total > 0
    )
);

insert into fa_asset_accounting
(
    asset_id,
    initial_cost,
    asset_account_id,
    salvage_value,
    depreciation_method_id,
    useful_life_months,
    depr_start_date,
    planned_units_total,
    accumulated_depreciation_account_id,
    depreciation_expense_account_id,
    created_date,
    updated_date
)
select
    id,
    initial_cost,
    asset_account_id,
    salvage_value,
    depreciation_method_id,
    useful_life_months,
    depr_start_date,
    planned_units_total,
    accumulated_depreciation_account_id,
    depreciation_expense_account_id,
    created_date,
    updated_date
from fa_asset;

create index ix_fa_asset_accounting_asset_account_id
    on fa_asset_accounting (asset_account_id);

create index ix_fa_asset_accounting_depreciation_method_id
    on fa_asset_accounting (depreciation_method_id);

create index ix_fa_asset_accounting_accumulated_depreciation_account_id
    on fa_asset_accounting (accumulated_depreciation_account_id);

create index ix_fa_asset_accounting_depreciation_expense_account_id
    on fa_asset_accounting (depreciation_expense_account_id);

commit;
