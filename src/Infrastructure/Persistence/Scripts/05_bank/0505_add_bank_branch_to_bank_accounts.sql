alter table cmn_bank_branch
    add constraint uq_cmn_bank_branch_id_bank_id unique (id, bank_id);

alter table org_bank_account
    add column bank_branch_id integer,
    add constraint org_bank_account_bank_branch_bank_fkey
        foreign key (bank_branch_id, bank_id)
        references cmn_bank_branch (id, bank_id);

create index idx_org_bank_account_bank_branch_id
    on org_bank_account using btree (bank_branch_id);

alter table counterparty_bank_account
    add column bank_branch_id integer,
    add constraint counterparty_bank_account_bank_branch_bank_fkey
        foreign key (bank_branch_id, bank_id)
        references cmn_bank_branch (id, bank_id);

create index idx_counterparty_bank_account_bank_branch_id
    on counterparty_bank_account using btree (bank_branch_id);
