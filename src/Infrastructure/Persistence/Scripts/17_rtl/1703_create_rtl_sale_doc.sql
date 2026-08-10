create table rtl_sale_doc
(
    id                      bigserial primary key,

    organization_id         int not null references org_organization(id),

    doc_number              varchar(100) not null,
    doc_date                timestamp without time zone not null,

    -- В рознице покупатель может быть неизвестен
    counterparty_id         int references counterparty_card(id),

    warehouse_id            int not null references inv_warehouse(id),

    -- Касса / торговое рабочее место, через которое оформлена продажа
    cash_register_id        int not null references fiscal_cash_register(id),

    currency_id             smallint not null references cmn_currency(id),

    total_amount            numeric(24, 8) not null,
    vat_amount              numeric(24, 8) not null,
    final_amount            numeric(24, 8) not null,

    receivable_account_id   int references acc_chart_account(id),
    vat_account_id          int references acc_chart_account(id),

    status_id               smallint not null references cmn_document_status(id),

    comment                 varchar(1000),

    state_id                smallint not null references cmn_state(id),

    created_date            timestamp without time zone not null default now(),

    exchange_rate           numeric(18, 6) not null default 1,

    posted_at               timestamp without time zone,
    posted_by_user_id       int references sys_user(id),

    cancelled_at            timestamp without time zone,
    cancelled_by_user_id    int references sys_user(id),

    constraint ck_rtl_sale_doc_number_not_empty
        check (btrim(doc_number) <> ''),

    constraint ck_rtl_sale_doc_total_amount
        check (total_amount >= 0),

    constraint ck_rtl_sale_doc_vat_amount
        check (vat_amount >= 0),

    constraint ck_rtl_sale_doc_final_amount
        check (final_amount >= 0),

    constraint ck_rtl_sale_doc_exchange_rate
        check (exchange_rate > 0)
);

create index ix_rtl_sale_doc_organization_id
    on rtl_sale_doc(organization_id);

create index ix_rtl_sale_doc_counterparty_id
    on rtl_sale_doc(counterparty_id);

create index ix_rtl_sale_doc_warehouse_id
    on rtl_sale_doc(warehouse_id);

create index ix_rtl_sale_doc_cash_register_id
    on rtl_sale_doc(cash_register_id);

create index ix_rtl_sale_doc_currency_id
    on rtl_sale_doc(currency_id);

create index ix_rtl_sale_doc_receivable_account_id 
    on rtl_sale_doc(receivable_account_id);
    
create index ix_rtl_sale_doc_vat_account_id 
    on rtl_sale_doc(vat_account_id);

create index ix_rtl_sale_doc_status_id
    on rtl_sale_doc(status_id);

create index ix_rtl_sale_doc_state_id
    on rtl_sale_doc(state_id);

create index ix_rtl_sale_doc_doc_date
    on rtl_sale_doc(doc_date);

create index ix_rtl_sale_doc_org_doc_date
    on rtl_sale_doc(organization_id, doc_date);