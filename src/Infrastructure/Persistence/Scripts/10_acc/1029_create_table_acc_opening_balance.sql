create table acc_opening_balance
(
    id bigserial primary key,

    organization_id integer not null unique references org_organization(id),

    balance_date date not null,

    description character varying(500),

    state_id smallint not null references cmn_state(id),

    created_date timestamp without time zone not null default now()
);

create index ix_acc_opening_balance_organization
    on acc_opening_balance(organization_id);
