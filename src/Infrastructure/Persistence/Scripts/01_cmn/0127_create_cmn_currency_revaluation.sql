create table cmn_currency_revaluation 
(
    id bigint not null,
    organization_id integer not null,
    revaluation_date timestamp without time zone not null,
    provider_rate_date timestamp without time zone,
    status_id smallint not null,
    state_id smallint not null,
    created_date timestamp without time zone default now() not null,
    confirmed_at timestamp without time zone,
    posted_by_user_id integer,
    cancelled_at timestamp without time zone,
    cancelled_by_user_id integer,
    constraint cmn_currency_revaluation_pkey primary key (id),
    constraint cmn_currency_revaluation_organization_id_fkey foreign key (organization_id) references org_organization(id),
    constraint cmn_currency_revaluation_state_id_fkey foreign key (state_id) references cmn_state(id)
);

create index idx_cmn_currency_revaluation_organization_id on cmn_currency_revaluation using btree (organization_id);
create index idx_cmn_currency_revaluation_revaluation_date on cmn_currency_revaluation using btree (revaluation_date);
create index idx_cmn_currency_revaluation_posted_by_user_id on cmn_currency_revaluation using btree (posted_by_user_id);
create index idx_cmn_currency_revaluation_cancelled_by_user_id on cmn_currency_revaluation using btree (cancelled_by_user_id);
create index idx_cmn_currency_revaluation_state_id on cmn_currency_revaluation using btree (state_id);
create index idx_cmn_currency_revaluation_status_id on cmn_currency_revaluation using btree (status_id);
