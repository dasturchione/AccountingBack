alter table fa_revaluation_doc
    add column revaluation_reserve_account_id integer
        references acc_chart_account (id)
        on delete restrict,
    add column revaluation_loss_account_id integer
        references acc_chart_account (id)
        on delete restrict;

create index ix_fa_reval_doc_reserve_account
    on fa_revaluation_doc (revaluation_reserve_account_id);

create index ix_fa_reval_doc_loss_account
    on fa_revaluation_doc (revaluation_loss_account_id);
