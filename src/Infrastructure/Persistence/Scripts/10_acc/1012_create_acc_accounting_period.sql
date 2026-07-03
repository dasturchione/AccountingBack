
create table acc_accounting_period (
    id integer   not null,
    organization_id integer not null,
    year smallint not null,
    month smallint not null,
    start_date date not null,
    end_date date not null,
    is_closed boolean default false not null,
    closed_at timestamp without time zone,
    closed_by_user_id integer,
    created_date timestamp without time zone default now() not null,
    constraint ck_acc_accounting_period_dates CHECK ((end_date >= start_date)),
    constraint ck_acc_accounting_period_month CHECK (((month >= 1) AND (month <= 12))),
    constraint acc_accounting_period_pkey primary key (id),
    constraint acc_accounting_period_unique_period UNIQUE (organization_id, year, month),
    constraint acc_accounting_period_closed_by_user_id_fkey foreign key (closed_by_user_id) references sys_user(id),
    constraint acc_accounting_period_organization_id_fkey foreign key (organization_id) references org_organization(id)
);

create index idx_acc_accounting_period_is_closed on acc_accounting_period using btree (is_closed);

create index idx_acc_accounting_period_organization_id on acc_accounting_period using btree (organization_id);

