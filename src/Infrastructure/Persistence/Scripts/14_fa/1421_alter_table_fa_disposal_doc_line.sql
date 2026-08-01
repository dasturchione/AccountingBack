alter table fa_disposal_doc_line
    add column asset_account_id integer
        references acc_chart_account (id)
        on delete restrict,
    add column accumulated_depreciation_account_id integer
        references acc_chart_account (id)
        on delete restrict;

create index ix_fa_disposal_line_asset_account
    on fa_disposal_doc_line (asset_account_id);

create index ix_fa_disposal_line_accum_depr_account
    on fa_disposal_doc_line (accumulated_depreciation_account_id);