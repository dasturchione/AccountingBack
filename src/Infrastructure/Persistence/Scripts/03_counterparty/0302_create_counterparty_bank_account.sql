
create table counterparty_bank_account (
    id integer   not null,
    organization_id integer not null,
    counterparty_id integer not null,
    bank_id integer not null,
    account_number character varying(50) not null,
    currency_id smallint not null,
    is_main boolean default false not null,
    state_id smallint not null,
    created_date timestamp without time zone default now() not null,
    constraint counterparty_bank_account_pkey primary key (id),
    constraint counterparty_bank_account_bank_id_fkey foreign key (bank_id) references cmn_bank(id),
    constraint counterparty_bank_account_counterparty_id_fkey foreign key (counterparty_id) references counterparty_card(id),
    constraint counterparty_bank_account_currency_id_fkey foreign key (currency_id) references cmn_currency(id),
    constraint counterparty_bank_account_organization_id_fkey foreign key (organization_id) references org_organization(id),
    constraint counterparty_bank_account_state_id_fkey foreign key (state_id) references cmn_state(id)
);

create index idx_counterparty_bank_account_bank_id on counterparty_bank_account using btree (bank_id);

create index idx_counterparty_bank_account_counterparty_id on counterparty_bank_account using btree (counterparty_id);

create index idx_counterparty_bank_account_currency_id on counterparty_bank_account using btree (currency_id);

create index idx_counterparty_bank_account_organization_id on counterparty_bank_account using btree (organization_id);

create index idx_counterparty_bank_account_state_id on counterparty_bank_account using btree (state_id);

