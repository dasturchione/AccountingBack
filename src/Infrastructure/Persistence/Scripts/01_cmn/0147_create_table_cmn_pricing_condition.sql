create table cmn_pricing_condition
(
    id                      bigserial primary key,
    organization_id         int not null references org_organization(id),

    pricing_method_id       smallint not null references cmn_pricing_method(id),
    pricing_value           numeric(18,2) not null,

    rounding_method_id      smallint not null references cmn_price_rounding_method(id),
    rounding_precision      numeric(12,2) not null default 1,

    start_date              timestamp without time zone default now() not null,
    end_date                timestamp without time zone null,

    state_id                smallint not null references cmn_state(id),

    created_date            timestamp without time zone default now() not null,

    constraint ck_cmn_pricing_condition_pricing_value 
        check (pricing_value >= 0),

    constraint ck_cmn_pricing_condition_rounding_precision 
        check (rounding_precision > 0),

    constraint ck_cmn_pricing_condition_dates check
    (
        end_date is null or end_date >= start_date
    )
);

create index idx_cmn_pricing_condition_organization_id
    on cmn_pricing_condition (organization_id);

create index idx_cmn_pricing_condition_pricing_method_id
    on cmn_pricing_condition (pricing_method_id);

create index idx_cmn_pricing_condition_rounding_method_id
    on cmn_pricing_condition (rounding_method_id);

create index idx_cmn_pricing_condition_dates
    on cmn_pricing_condition (start_date, end_date);