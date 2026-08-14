begin;

alter table fa_receipt_doc_asset
    rename column owner_id to receipt_doc_line_id;

drop index if exists idx_fa_receipt_doc_asset_depreciation_method_id;
drop index if exists idx_fa_receipt_doc_asset_department_id;
drop index if exists idx_fa_receipt_doc_asset_responsible_user_id;
drop index if exists ix_fa_receipt_asset_accum_depr_account;
drop index if exists ix_fa_receipt_asset_depr_exp_account;

alter table fa_receipt_doc_asset
    drop constraint fa_receipt_doc_asset_owner_id_fkey,
    drop constraint fa_receipt_doc_asset_asset_account_id_fkey,
    drop constraint if exists fa_receipt_doc_asset_accumulated_depreciation_account_id_fkey,
    drop constraint if exists fa_receipt_doc_asset_depreciation_expense_account_id_fkey,
    drop constraint if exists fa_receipt_doc_asset_depreciation_method_id_fkey,
    drop constraint if exists fa_receipt_doc_asset_department_id_fkey,
    drop constraint if exists fa_receipt_doc_asset_responsible_user_id_fkey,
    drop constraint if exists ck_fa_receipt_doc_asset_useful_life_positive,
    drop constraint if exists ck_fa_receipt_doc_asset_salvage_nonnegative,
    drop constraint if exists ck_fa_receipt_doc_asset_salvage_le_initial_cost,
    drop constraint if exists ck_fa_receipt_doc_asset_planned_units_positive,
    add constraint fa_receipt_doc_asset_receipt_doc_line_id_fkey
        foreign key (receipt_doc_line_id) references fa_receipt_doc_line (id),
    add constraint fa_receipt_doc_asset_asset_account_id_fkey
        foreign key (asset_account_id) references acc_chart_account (id),
    alter column name type varchar(500),
    alter column initial_cost type numeric(24,8),
    alter column asset_account_id set not null,
    drop column salvage_value,
    drop column useful_life_months,
    drop column depreciation_method_id,
    drop column commissioning_date,
    drop column depr_start_date,
    drop column planned_units_total,
    drop column department_id,
    drop column responsible_user_id,
    drop column accumulated_depreciation_account_id,
    drop column depreciation_expense_account_id;

alter table fa_receipt_doc_asset
    rename constraint fa_receipt_doc_asset_pkey to pk_fa_receipt_doc_asset;

alter table fa_receipt_doc_asset
    rename constraint ck_fa_receipt_doc_asset_initial_cost_nonnegative to ck_fa_receipt_doc_asset_initial_cost;

alter index idx_fa_receipt_doc_asset_owner_id rename to ix_fa_receipt_doc_asset_receipt_doc_line_id;
alter index idx_fa_receipt_doc_asset_fa_asset_id rename to ix_fa_receipt_doc_asset_fa_asset_id;
alter index idx_fa_receipt_doc_asset_fa_group_id rename to ix_fa_receipt_doc_asset_fa_group_id;
alter index idx_fa_receipt_doc_asset_okof_id rename to ix_fa_receipt_doc_asset_okof_id;
alter index ix_fa_receipt_asset_asset_account rename to ix_fa_receipt_doc_asset_asset_account_id;

create unique index uq_fa_receipt_doc_asset_fa_asset_id
    on fa_receipt_doc_asset (fa_asset_id)
    where fa_asset_id is not null;

commit;
