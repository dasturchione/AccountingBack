create table sale_shipment_doc
(
    id                  bigserial primary key,
    organization_id     int not null references org_organization(id),
    warehouse_id        int not null references inv_warehouse(id),

    sale_doc_id         bigint references sale_doc(id),
    counterparty_id     int references counterparty_card(id),

    doc_number          varchar(50),
    doc_date            timestamp without time zone not null default now(),
    comment             varchar(1000),

    status_id           smallint not null references cmn_document_status(id),

    submitted_at        timestamp without time zone,
    submitted_user_id   int references sys_user(id),

    accepted_at         timestamp without time zone,
    accepted_user_id    int references sys_user(id),

    cancelled_at        timestamp without time zone,
    cancelled_user_id   int references sys_user(id),

    created_user_id     int not null references sys_user(id),
    created_date        timestamp without time zone not null default now()
);

-- Основной список складских документов организации
create index idx_sale_shipment_doc_organization_status_date
    on sale_shipment_doc
    (
        organization_id,
        status_id,
        doc_date desc
    );

-- Документы конкретного склада
create index idx_sale_shipment_doc_organization_warehouse_date
    on sale_shipment_doc
    (
        organization_id,
        warehouse_id,
        doc_date desc
    );

-- Поиск складского документа по документу продажи
create index idx_sale_shipment_doc_sale_doc_id
    on sale_shipment_doc (sale_doc_id)
    where sale_doc_id is not null;

-- История отгрузок контрагента
create index idx_sale_shipment_doc_organization_counterparty_date
    on sale_shipment_doc
    (
        organization_id,
        counterparty_id,
        doc_date desc
    )
    where counterparty_id is not null;

-- Поиск по номеру документа внутри организации
create index idx_sale_shipment_doc_organization_doc_number
    on sale_shipment_doc
    (
        organization_id,
        doc_number
    )
    where doc_number is not null;

-- Документы, переданные конкретным пользователем
create index idx_sale_shipment_doc_submitted_user_id
    on sale_shipment_doc (submitted_user_id)
    where submitted_user_id is not null;

-- Документы, принятые конкретным пользователем
create index idx_sale_shipment_doc_accepted_user_id
    on sale_shipment_doc (accepted_user_id)
    where accepted_user_id is not null;

-- Документы, созданные пользователем
create index idx_sale_shipment_doc_created_user_id
    on sale_shipment_doc (created_user_id);