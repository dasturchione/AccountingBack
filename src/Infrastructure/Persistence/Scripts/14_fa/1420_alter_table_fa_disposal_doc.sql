alter table fa_disposal_doc
    add column disposal_account_id integer
        references acc_chart_account (id)
        on delete restrict,
    add column customer_account_id integer
        references acc_chart_account (id)
        on delete restrict,
    add column vat_account_id integer
        references acc_chart_account (id)
        on delete restrict,
    add column gain_account_id integer
        references acc_chart_account (id)
        on delete restrict,
    add column loss_account_id integer
        references acc_chart_account (id)
        on delete restrict;

create index ix_fa_disposal_doc_disposal_account
    on fa_disposal_doc (disposal_account_id);

create index ix_fa_disposal_doc_customer_account
    on fa_disposal_doc (customer_account_id);

create index ix_fa_disposal_doc_vat_account
    on fa_disposal_doc (vat_account_id);

create index ix_fa_disposal_doc_gain_account
    on fa_disposal_doc (gain_account_id);

create index ix_fa_disposal_doc_loss_account
    on fa_disposal_doc (loss_account_id);
