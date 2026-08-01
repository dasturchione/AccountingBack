alter table fa_receipt_doc_asset
    add column asset_account_id integer
        references acc_chart_account (id)
        on delete restrict,
    add column accumulated_depreciation_account_id integer
        references acc_chart_account (id)
        on delete restrict,
    add column depreciation_expense_account_id integer
        references acc_chart_account (id)
        on delete restrict;

create index ix_fa_receipt_asset_asset_account
    on fa_receipt_doc_asset (asset_account_id);

create index ix_fa_receipt_asset_accum_depr_account
    on fa_receipt_doc_asset (accumulated_depreciation_account_id);

create index ix_fa_receipt_asset_depr_exp_account
    on fa_receipt_doc_asset (depreciation_expense_account_id);
