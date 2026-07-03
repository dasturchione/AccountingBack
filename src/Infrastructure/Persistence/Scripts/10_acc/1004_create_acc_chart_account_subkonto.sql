
create table acc_chart_account_subkonto (
    id integer   not null,
    organization_id integer not null,
    account_id integer not null,
    subkonto_type_id smallint not null,
    sort_order integer default 0 not null,
    is_required boolean default true not null,
    state_id smallint not null,
    created_date timestamp without time zone default now() not null,
    constraint acc_chart_account_subkonto_pkey primary key (id),
    constraint acc_chart_account_subkonto_account_id_fkey foreign key (account_id) references acc_chart_account(id) on DELETE CASCADE,
    constraint acc_chart_account_subkonto_organization_id_fkey foreign key (organization_id) references org_organization(id),
    constraint acc_chart_account_subkonto_state_id_fkey foreign key (state_id) references cmn_state(id),
    constraint acc_chart_account_subkonto_subkonto_type_id_fkey foreign key (subkonto_type_id) references acc_subkonto_type(id)
);

create index idx_acc_chart_account_subkonto_account_id on acc_chart_account_subkonto using btree (account_id);

create index idx_acc_chart_account_subkonto_organization_id on acc_chart_account_subkonto using btree (organization_id);

create index idx_acc_chart_account_subkonto_type_id on acc_chart_account_subkonto using btree (subkonto_type_id);

create unique index idx_acc_chart_account_subkonto_unique on acc_chart_account_subkonto using btree (organization_id, account_id, subkonto_type_id);

