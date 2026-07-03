create table inv_inventory_adjustment_doc
(
    id                          bigint primary key,
    organization_id             integer not null references org_organization(id),
    doc_number                  varchar(100) not null,
    doc_date                    timestamp without time zone not null,
    warehouse_id                integer not null references inv_warehouse(id),
    adjustment_type             varchar(50) not null,
    status_id                   smallint not null references cmn_document_status(id),
    comment                     varchar(1000),
    state_id                    smallint not null references cmn_state(id),
    created_date                timestamp without time zone not null default now(),
    posted_at                   timestamp without time zone,
    posted_by_user_id           integer references sys_user(id),
    cancelled_at                timestamp without time zone,
    cancelled_by_user_id        integer references sys_user(id));

create index idx_inv_inventory_adjustment_doc_organization_id on inv_inventory_adjustment_doc (organization_id);
create index idx_inv_inventory_adjustment_doc_doc_date on inv_inventory_adjustment_doc (doc_date);
create index idx_inv_inventory_adjustment_doc_warehouse_id on inv_inventory_adjustment_doc (warehouse_id);
create index idx_inv_inventory_adjustment_doc_adjustment_type on inv_inventory_adjustment_doc (adjustment_type);
create unique index ux_inv_inventory_adjustment_doc_org_doc_number on inv_inventory_adjustment_doc (organization_id, doc_number);
create index idx_inv_inventory_adjustment_doc_status_id on inv_inventory_adjustment_doc (status_id);
create index idx_inv_inventory_adjustment_doc_state_id on inv_inventory_adjustment_doc (state_id);
create index idx_inv_inventory_adjustment_doc_posted_by_user_id on inv_inventory_adjustment_doc (posted_by_user_id);
create index idx_inv_inventory_adjustment_doc_cancelled_by_user_id on inv_inventory_adjustment_doc (cancelled_by_user_id);

