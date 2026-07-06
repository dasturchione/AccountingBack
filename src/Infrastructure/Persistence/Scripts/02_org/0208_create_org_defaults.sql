
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
    constraint org_defaults_pkey primary key (id),
    constraint org_defaults_bank_account_id_fkey foreign key (bank_account_id) references org_bank_account(id),
    constraint org_defaults_bank_accounting_account_id_fkey foreign key (bank_accounting_account_id) references acc_chart_account(id),
    constraint org_defaults_branch_id_fkey foreign key (branch_id) references org_branch(id),
    constraint org_defaults_cash_account_id_fkey foreign key (cash_account_id) references acc_chart_account(id),
    constraint org_defaults_cash_box_id_fkey foreign key (cash_box_id) references cash_box(id),
    constraint org_defaults_cogs_account_id_fkey foreign key (cogs_account_id) references acc_chart_account(id),
    constraint org_defaults_expense_account_id_fkey foreign key (expense_account_id) references acc_chart_account(id),
    constraint org_defaults_inventory_account_id_fkey foreign key (inventory_account_id) references acc_chart_account(id),
    constraint org_defaults_organization_id_fkey foreign key (organization_id) references org_organization(id),
    constraint org_defaults_payable_account_id_fkey foreign key (payable_account_id) references acc_chart_account(id),
    constraint org_defaults_receivable_account_id_fkey foreign key (receivable_account_id) references acc_chart_account(id),
    constraint org_defaults_revenue_account_id_fkey foreign key (revenue_account_id) references acc_chart_account(id),
    constraint org_defaults_warehouse_id_fkey foreign key (warehouse_id) references inv_warehouse(id)
);

create index idx_org_defaults_bank_account_id on org_defaults using btree (bank_account_id);
create index idx_org_defaults_branch_id on org_defaults using btree (branch_id);
create index idx_org_defaults_cash_box_id on org_defaults using btree (cash_box_id);
create index idx_org_defaults_warehouse_id on org_defaults using btree (warehouse_id);
