create table rnt_accrual_doc
(
    id                           bigserial primary key,
    organization_id              int not null references org_organization(id),
    contract_id                  bigint not null references rnt_contract(id),
    doc_number                   varchar(100) not null,
    doc_date                     timestamp without time zone not null,
    currency_id                  smallint not null references cmn_currency(id),
    exchange_rate                numeric(24, 8) not null default 1,
    contract_amount              numeric(24, 8) not null default 0,
    tax_base_amount              numeric(24, 8) not null default 0,
    tax_amount                   numeric(24, 8) not null default 0,
    payable_amount               numeric(24, 8) not null default 0,
    amount                       numeric(24, 8) not null default 0,
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

    check (exchange_rate > 0),
    check (contract_amount > 0),
    check (tax_base_amount >= contract_amount),
    check (tax_amount >= 0),
    check (payable_amount >= 0),
    check (amount = payable_amount + tax_amount)
);

create index ix_rnt_accrual_doc_organization_date
    on rnt_accrual_doc (organization_id, doc_date desc);

create unique index ux_rnt_accrual_doc_org_year_doc_number
    on rnt_accrual_doc (organization_id, (extract(year from doc_date)), doc_number);

create index ix_rnt_accrual_doc_contract_id
    on rnt_accrual_doc (contract_id);

create index ix_rnt_accrual_doc_status_id
    on rnt_accrual_doc (status_id);

create table rnt_accrual_doc_item
(
    id                    bigserial primary key,
    accrual_doc_id        bigint not null references rnt_accrual_doc(id) on delete cascade,
    contract_object_id    bigint not null references rnt_contract_object(id),
    period_from           date not null,
    period_to             date not null,
    contract_amount       numeric(24, 8) not null,
    tax_base_amount       numeric(24, 8) not null,
    tax_rate              numeric(9, 6) not null,
    tax_amount            numeric(24, 8) not null,
    payable_amount        numeric(24, 8) not null,
    amount                numeric(24, 8) not null,
    expense_account_id    int references acc_chart_account(id),

    unique (contract_object_id, period_from, period_to),
    check (period_to >= period_from),
    check (contract_amount > 0),
    check (tax_base_amount >= contract_amount),
    check (tax_rate >= 0 and tax_rate <= 100),
    check (tax_amount >= 0),
    check (payable_amount >= 0),
    check (amount = payable_amount + tax_amount)
);

create index ix_rnt_accrual_doc_item_doc_id
    on rnt_accrual_doc_item (accrual_doc_id);
