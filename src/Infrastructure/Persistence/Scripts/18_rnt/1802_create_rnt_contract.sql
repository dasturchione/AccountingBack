create table rnt_contract
(
    id                           bigserial primary key,
    organization_id              int not null references org_organization(id),
    lessor_full_name             varchar(500) not null,
    lessor_inn                   varchar(20),
    lessor_pinfl                 varchar(14),
    contract_number              varchar(100) not null,
    contract_date                date not null,
    start_date                   date not null,
    end_date                     date not null,
    currency_id                  smallint not null references cmn_currency(id),
    lessor_payable_account_id    int references acc_chart_account(id),
    tax_payable_account_id       int references acc_chart_account(id),
    status_id                    smallint not null references cmn_document_status(id),
    comment                      varchar(1000),
    state_id                     smallint not null default 1 references cmn_state(id),
    created_date                 timestamp without time zone not null default now(),
    created_by_user_id           int,
    updated_date                 timestamp without time zone,
    updated_by_user_id           int,
    posted_at                    timestamp without time zone,
    posted_by_user_id            int,
    cancelled_at                 timestamp without time zone,
    cancelled_by_user_id         int,

    check (end_date >= start_date),
    check
    (
        nullif(btrim(lessor_inn), '') is not null
        or nullif(btrim(lessor_pinfl), '') is not null
    )
);

create index ix_rnt_contract_organization_status
    on rnt_contract (organization_id, status_id, state_id);

create index ix_rnt_contract_dates
    on rnt_contract (start_date, end_date);

create unique index ux_rnt_contract_org_year_contract_number
    on rnt_contract (organization_id, (extract(year from contract_date)), contract_number)
    where state_id = 1;

create table rnt_contract_object
(
    id                      bigserial primary key,
    contract_id             bigint not null references rnt_contract(id),
    rental_object_type_id   smallint not null references rnt_rental_object_type(id),
    object_name             varchar(500) not null,
    object_identifier       varchar(250),
    object_address          varchar(1000),
    start_date              date not null,
    end_date                date not null,
    period_unit             varchar(20) not null,
    period_value            int not null default 1,
    next_accrual_date       date not null,
    contract_amount         numeric(24, 8) not null,
    tax_base_amount         numeric(24, 8) not null,
    tax_rate                numeric(9, 6) not null,
    expense_account_id      int references acc_chart_account(id),
    state_id                smallint not null default 1 references cmn_state(id),
    created_date            timestamp without time zone not null default now(),
    updated_date            timestamp without time zone,

    check (end_date >= start_date),
    check (next_accrual_date >= start_date),
    check (period_unit in ('DAY', 'MONTH')),
    check (period_value > 0),
    check (contract_amount > 0),
    check (tax_base_amount >= contract_amount),
    check (tax_rate >= 0 and tax_rate <= 100)
);

create index ix_rnt_contract_object_contract_id
    on rnt_contract_object (contract_id);

create index ix_rnt_contract_object_due
    on rnt_contract_object (next_accrual_date, state_id);
