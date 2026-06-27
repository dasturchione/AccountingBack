create table sale_condition
(
    id                          bigserial primary key,
    organization_id             int not null references org_organization(id),
    costing_method_id           smallint not null references cmn_costing_method(id),
    vat_rate_id                 smallint not null references cmn_vat_rate(id),

    start_date                  timestamp without time zone default now() not null,
    end_date                    timestamp without time zone null,

    state_id                    smallint not null references cmn_state(id),

    created_date                timestamp without time zone default now() not null,

    constraint ck_sale_condition_dates check
    (
        end_date is null or end_date >= start_date
    )
);

create index idx_sale_condition_organization_id 
    on sale_condition (organization_id);

create index idx_sale_condition_costing_method_id 
    on sale_condition (costing_method_id);

create index idx_sale_condition_vat_rate_id 
    on sale_condition (vat_rate_id);

create index idx_sale_condition_state_id 
    on sale_condition (state_id);

create index idx_sale_condition_dates 
    on sale_condition (start_date, end_date);

create index idx_sale_condition_org_state_dates 
    on sale_condition (organization_id, state_id, start_date, end_date);
