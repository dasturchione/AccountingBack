alter table fa_depreciation_run_line
    add column expense_account_id integer
        references acc_chart_account (id)
        on delete restrict,
    add column accumulated_depreciation_account_id integer
        references acc_chart_account (id)
        on delete restrict;

create index ix_fa_depr_run_line_expense_account
    on fa_depreciation_run_line (expense_account_id);

create index ix_fa_depr_run_line_accum_depr_account
    on fa_depreciation_run_line (accumulated_depreciation_account_id);
