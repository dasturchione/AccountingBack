alter table fa_receipt_doc_line
    add column capital_investment_account_id integer
        references acc_chart_account (id)
        on delete restrict,
    add column vat_account_id integer
        references acc_chart_account (id)
        on delete restrict;

create index ix_fa_receipt_line_capital_account
    on fa_receipt_doc_line (capital_investment_account_id);

create index ix_fa_receipt_line_vat_account
    on fa_receipt_doc_line (vat_account_id);
