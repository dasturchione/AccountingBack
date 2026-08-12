begin;

alter table fa_asset
    alter column name type varchar(500);

drop index if exists idx_fa_asset_state_id;
drop index if exists idx_fa_asset_depreciation_method_id;
drop index if exists idx_fa_asset_commissioning_date;
drop index if exists ix_fa_asset_asset_account;
drop index if exists ix_fa_asset_accum_depr_account;
drop index if exists ix_fa_asset_depr_exp_account;

alter table fa_asset
    drop constraint if exists fa_asset_depreciation_method_id_fkey,
    drop constraint if exists fa_asset_asset_account_id_fkey,
    drop constraint if exists fa_asset_accumulated_depreciation_account_id_fkey,
    drop constraint if exists fa_asset_depreciation_expense_account_id_fkey,
    drop constraint if exists ck_fa_asset_useful_life_positive,
    drop constraint if exists ck_fa_asset_initial_cost_nonnegative,
    drop constraint if exists ck_fa_asset_salvage_value_nonnegative,
    drop constraint if exists ck_fa_asset_salvage_value_not_gt_initial,
    drop constraint if exists ck_fa_asset_planned_units_positive,
    drop column depreciation_method_id,
    drop column useful_life_months,
    drop column initial_cost,
    drop column salvage_value,
    drop column commissioning_date,
    drop column depr_start_date,
    drop column planned_units_total,
    drop column asset_account_id,
    drop column accumulated_depreciation_account_id,
    drop column depreciation_expense_account_id;

alter table fa_asset
    rename constraint fa_asset_pkey to pk_fa_asset;

alter index uidx_fa_asset_org_inventory_number
    rename to uq_fa_asset_org_inventory_number;

alter table fa_asset
    add constraint uq_fa_asset_org_inventory_number
        unique using index uq_fa_asset_org_inventory_number;

alter index idx_fa_asset_status_id rename to ix_fa_asset_status_id;
alter index idx_fa_asset_fa_group_id rename to ix_fa_asset_fa_group_id;
alter index idx_fa_asset_okof_id rename to ix_fa_asset_okof_id;
alter index idx_fa_asset_department_id rename to ix_fa_asset_department_id;
alter index idx_fa_asset_responsible_user_id rename to ix_fa_asset_responsible_user_id;

create index ix_fa_asset_organization_id
    on fa_asset (organization_id);

commit;
