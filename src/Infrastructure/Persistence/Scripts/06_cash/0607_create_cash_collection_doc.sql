create table cash_collection_doc
(
    id                          bigserial primary key,
    organization_id             int not null references org_organization(id),
    doc_number                  varchar(100) not null,
    doc_date                    timestamp without time zone not null,
    cash_box_id                 int not null references cash_box(id),
    bank_account_id             int not null references org_bank_account(id),
    currency_id                 smallint not null references cmn_currency(id),
    amount                      numeric(18, 2) not null,
    exchange_rate               numeric(18, 6) not null default 1,
    cash_chart_account_id       int references acc_chart_account(id),
    cash_in_transit_account_id  int references acc_chart_account(id),
    bank_chart_account_id       int references acc_chart_account(id),
    status_id                   smallint not null default 1 references cmn_document_status(id),
    state_id                    smallint not null default 1 references cmn_state(id),
    comment                     varchar(1000),
    created_date                timestamp without time zone not null default now(),
    in_transit_at               timestamp without time zone,
    in_transit_by_user_id       int references sys_user(id),
    completed_at                timestamp without time zone,
    completed_by_user_id        int references sys_user(id),
    cancelled_at                timestamp without time zone,
    cancelled_by_user_id        int references sys_user(id),
    cancelled_from_status_id    smallint references cmn_document_status(id),

    constraint ck_cash_collection_doc_number_not_empty
        check (btrim(doc_number) <> ''),

    constraint ck_cash_collection_doc_amount
        check (amount > 0),

    constraint ck_cash_collection_doc_exchange_rate
        check (exchange_rate > 0),

    constraint ck_cash_collection_doc_status
        check (status_id in (1, 3, 5, 6)),

    constraint ck_cash_collection_doc_cancelled_from_status
        check (cancelled_from_status_id is null or cancelled_from_status_id in (1, 5, 6)),

    constraint ck_cash_collection_doc_timestamp_order
        check
        (
            (completed_at is null or in_transit_at is null or completed_at >= in_transit_at)
            and (cancelled_at is null or in_transit_at is null or cancelled_at >= in_transit_at)
            and (cancelled_at is null or completed_at is null or cancelled_at >= completed_at)
        ),

    constraint ck_cash_collection_doc_accounts
        check
        (
            status_id = 1
            or
            (
                (status_id = 5 or (status_id = 3 and cancelled_from_status_id = 5))
                and cash_chart_account_id is not null
                and cash_in_transit_account_id is not null
                and cash_chart_account_id <> cash_in_transit_account_id
            )
            or
            (
                (status_id = 6 or (status_id = 3 and cancelled_from_status_id = 6))
                and cash_chart_account_id is not null
                and cash_in_transit_account_id is not null
                and bank_chart_account_id is not null
                and cash_chart_account_id <> cash_in_transit_account_id
                and cash_chart_account_id <> bank_chart_account_id
                and cash_in_transit_account_id <> bank_chart_account_id
            )
            or
            (status_id = 3 and cancelled_from_status_id = 1)
        ),

    constraint ck_cash_collection_doc_lifecycle
        check
        (
            (
                status_id = 1
                and in_transit_at is null
                and in_transit_by_user_id is null
                and completed_at is null
                and completed_by_user_id is null
                and cancelled_at is null
                and cancelled_by_user_id is null
                and cancelled_from_status_id is null
            )
            or
            (
                status_id = 5
                and in_transit_at is not null
                and in_transit_by_user_id is not null
                and completed_at is null
                and completed_by_user_id is null
                and cancelled_at is null
                and cancelled_by_user_id is null
                and cancelled_from_status_id is null
            )
            or
            (
                status_id = 6
                and in_transit_at is not null
                and in_transit_by_user_id is not null
                and completed_at is not null
                and completed_by_user_id is not null
                and cancelled_at is null
                and cancelled_by_user_id is null
                and cancelled_from_status_id is null
            )
            or
            (
                status_id = 3
                and cancelled_at is not null
                and cancelled_by_user_id is not null
                and cancelled_from_status_id in (1, 5, 6)
                and
                (
                    (
                        cancelled_from_status_id = 1
                        and in_transit_at is null
                        and in_transit_by_user_id is null
                        and completed_at is null
                        and completed_by_user_id is null
                    )
                    or
                    (
                        cancelled_from_status_id = 5
                        and in_transit_at is not null
                        and in_transit_by_user_id is not null
                        and completed_at is null
                        and completed_by_user_id is null
                    )
                    or
                    (
                        cancelled_from_status_id = 6
                        and in_transit_at is not null
                        and in_transit_by_user_id is not null
                        and completed_at is not null
                        and completed_by_user_id is not null
                    )
                )
            )
        )
);

create index idx_cash_collection_doc_organization_id
    on cash_collection_doc using btree (organization_id);

create index idx_cash_collection_doc_cash_box_id
    on cash_collection_doc using btree (cash_box_id);

create index idx_cash_collection_doc_bank_account_id
    on cash_collection_doc using btree (bank_account_id);

create index idx_cash_collection_doc_currency_id
    on cash_collection_doc using btree (currency_id);

create index idx_cash_collection_doc_status_id
    on cash_collection_doc using btree (status_id);

create index idx_cash_collection_doc_state_id
    on cash_collection_doc using btree (state_id);

create index idx_cash_collection_doc_org_doc_date
    on cash_collection_doc using btree (organization_id, doc_date);

create index idx_cash_collection_doc_cash_chart_account_id
    on cash_collection_doc using btree (cash_chart_account_id);

create index idx_cash_collection_doc_cash_in_transit_account_id
    on cash_collection_doc using btree (cash_in_transit_account_id);

create index idx_cash_collection_doc_bank_chart_account_id
    on cash_collection_doc using btree (bank_chart_account_id);

create index idx_cash_collection_doc_in_transit_by_user_id
    on cash_collection_doc using btree (in_transit_by_user_id);

create index idx_cash_collection_doc_completed_by_user_id
    on cash_collection_doc using btree (completed_by_user_id);

create index idx_cash_collection_doc_cancelled_by_user_id
    on cash_collection_doc using btree (cancelled_by_user_id);

create index idx_cash_collection_doc_cancelled_from_status_id
    on cash_collection_doc using btree (cancelled_from_status_id);

create unique index ux_cash_collection_doc_org_year_doc_number
    on cash_collection_doc (organization_id, (extract(year from doc_date)), doc_number);
