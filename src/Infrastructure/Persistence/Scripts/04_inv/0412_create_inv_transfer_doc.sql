create table inv_transfer_doc
(
    id                          bigint primary key,
    organization_id             integer not null references org_organization(id),
    doc_number                  varchar(100) not null,
    doc_date                    timestamp without time zone not null,
    source_warehouse_id         integer not null references inv_warehouse(id),
    destination_warehouse_id    integer not null references inv_warehouse(id),
    status_id                   smallint not null references cmn_document_status(id),
    comment                     varchar(1000),
    state_id                    smallint not null references cmn_state(id),
    created_date                timestamp without time zone not null default now(),
    posted_at                   timestamp without time zone,
    posted_by_user_id           integer references sys_user(id),
    cancelled_at                timestamp without time zone,
    cancelled_by_user_id        integer references sys_user(id));

create index idx_inv_transfer_doc_organization_id on inv_transfer_doc (organization_id);
create index idx_inv_transfer_doc_doc_date on inv_transfer_doc (doc_date);
create index idx_inv_transfer_doc_source_warehouse_id on inv_transfer_doc (source_warehouse_id);
create index idx_inv_transfer_doc_destination_warehouse_id on inv_transfer_doc (destination_warehouse_id);
create unique index ux_inv_transfer_doc_doc_number_org on inv_transfer_doc (organization_id, doc_number);
create index idx_inv_transfer_doc_status_id on inv_transfer_doc (status_id);
create index idx_inv_transfer_doc_state_id on inv_transfer_doc (state_id);
create index idx_inv_transfer_doc_posted_by_user_id on inv_transfer_doc (posted_by_user_id);
create index idx_inv_transfer_doc_cancelled_by_user_id on inv_transfer_doc (cancelled_by_user_id);

