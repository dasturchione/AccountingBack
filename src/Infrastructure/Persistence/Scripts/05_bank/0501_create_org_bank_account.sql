
create table org_bank_account (
    id integer   not null,
    organization_id integer not null,
    bank_id integer not null,
    account_number character varying(50) not null,
    currency_id smallint not null,
    is_main boolean default false not null,
    state_id smallint not null,
    created_date timestamp without time zone default now() not null,
    code character varying(100),
    name character varying(250),
    opening_balance numeric(18,2) default 0 not null,
    opening_balance_date date,
    constraint org_bank_account_pkey primary key (id),
    constraint org_bank_account_bank_id_fkey foreign key (bank_id) references cmn_bank(id),
    constraint org_bank_account_currency_id_fkey foreign key (currency_id) references cmn_currency(id),
    constraint org_bank_account_organization_id_fkey foreign key (organization_id) references org_organization(id),
    constraint org_bank_account_state_id_fkey foreign key (state_id) references cmn_state(id)
);

insert into org_bank_account (id, organization_id, bank_id, account_number, currency_id, is_main, state_id, created_date) values
    ('1', '2', '1', '064232', '4', 't', '1', '2026-06-08 10:34:07.891936'),
    ('8', '8', '4', '0099855144112', '3', 't', '1', '2026-06-24 11:59:17.881525'),
    ('9', '8', '4', '0099855144117', '3', 't', '1', '2026-06-24 15:34:17.666512'),
    ('10', '8', '3', '064232347878', '3', 't', '1', '2026-06-24 15:34:56.767807'),
    ('11', '8', '3', '23106000105157348001', '1', 't', '1', '2026-06-24 17:01:04.482009'),
    ('12', '8', '2', '20208000005157348001', '1', 't', '1', '2026-06-24 17:57:51.101968');

create index idx_org_bank_account_bank_id on org_bank_account using btree (bank_id);

create index idx_org_bank_account_currency_id on org_bank_account using btree (currency_id);

create index idx_org_bank_account_organization_id on org_bank_account using btree (organization_id);

create index idx_org_bank_account_state_id on org_bank_account using btree (state_id);

create index idx_org_bank_account_code on org_bank_account using btree (code);

create index idx_org_bank_account_name on org_bank_account using btree (name);

create unique index uidx_org_bank_account_org_code on org_bank_account using btree (organization_id, code) WHERE (code IS not null);

