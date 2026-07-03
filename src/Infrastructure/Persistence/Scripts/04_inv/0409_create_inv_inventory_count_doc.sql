create table inv_inventory_count_doc
(
    id                          bigint primary key,
    organization_id             integer not null references org_organization(id),
    doc_number                  varchar(100) not null,
    doc_date                    timestamp without time zone not null,
    warehouse_id                integer not null references inv_warehouse(id),
    status_id                   smallint not null references cmn_document_status(id),
    comment                     varchar(1000),
    state_id                    smallint not null references cmn_state(id),
    created_date                timestamp without time zone not null default now(),
    count_completed_at          timestamp without time zone,
    count_completed_by_user_id   integer references sys_user(id),
    positive_adjustment_doc_id   bigint references inv_inventory_adjustment_doc(id),
    negative_adjustment_doc_id   bigint references inv_inventory_adjustment_doc(id),
    posted_at                   timestamp without time zone,
    posted_by_user_id           integer references sys_user(id),
    cancelled_at                timestamp without time zone,
    cancelled_by_user_id        integer references sys_user(id));

create index idx_inv_inventory_count_doc_organization_id on inv_inventory_count_doc (organization_id);
create index idx_inv_inventory_count_doc_doc_date on inv_inventory_count_doc (doc_date);
create index idx_inv_inventory_count_doc_warehouse_id on inv_inventory_count_doc (warehouse_id);
create unique index ux_inv_inventory_count_doc_org_doc_number on inv_inventory_count_doc (organization_id, doc_number);
create index idx_inv_inventory_count_doc_status_id on inv_inventory_count_doc (status_id);
create index idx_inv_inventory_count_doc_state_id on inv_inventory_count_doc (state_id);

