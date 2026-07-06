
create table org_defaults 
(
    id integer   not null,
    organization_id integer not null,
    branch_id integer,
    warehouse_id integer,
    cash_box_id integer,
    bank_account_id integer,
    receivable_account_id integer,
    payable_account_id integer,
    inventory_account_id integer,
    cash_account_id integer,
    bank_accounting_account_id integer,
    revenue_account_id integer,
    expense_account_id integer,
    cogs_account_id integer,
    created_date timestamp without time zone default now() not null,
    constraint org_defaults_organization_id_key UNIQUE (organization_id),
    constraint org_defaults_pkey primary key (id)
);

create index idx_org_defaults_bank_account_id on org_defaults using btree (bank_account_id);
create index idx_org_defaults_branch_id on org_defaults using btree (branch_id);
create index idx_org_defaults_cash_box_id on org_defaults using btree (cash_box_id);
create index idx_org_defaults_warehouse_id on org_defaults using btree (warehouse_id);
