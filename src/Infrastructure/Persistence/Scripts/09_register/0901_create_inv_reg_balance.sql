
create table inv_reg_balance (
    id bigint   not null,
    organization_id integer not null,
    document_type_id smallint not null,
    document_id bigint not null,
    warehouse_id integer not null,
    product_id integer not null,
    product_table_id integer,
    operation_type_id smallint not null,
    quantity numeric(18,3) not null,
    amount numeric(18,2) not null,
    doc_date timestamp without time zone not null,
    created_date timestamp without time zone default now() not null,
    posting_batch_id bigint,
    source_line_id bigint,
    reversal_entry_id bigint,
    constraint inv_reg_balance_pkey primary key (id),
    constraint inv_reg_balance_document_type_id_fkey foreign key (document_type_id) references cmn_document_type(id),
    constraint inv_reg_balance_operation_type_id_fkey foreign key (operation_type_id) references cmn_operation_type(id),
    constraint inv_reg_balance_organization_id_fkey foreign key (organization_id) references org_organization(id),
    constraint inv_reg_balance_product_id_fkey foreign key (product_id) references inv_product(id),
    constraint inv_reg_balance_product_table_id_fkey foreign key (product_table_id) references inv_product_table(id),
    constraint inv_reg_balance_warehouse_id_fkey foreign key (warehouse_id) references inv_warehouse(id),
    constraint inv_reg_balance_posting_batch_id_fkey foreign key (posting_batch_id) references acc_posting_batch(id)
);

insert into inv_reg_balance (id, organization_id, document_type_id, document_id, warehouse_id, product_id, operation_type_id, quantity, amount, doc_date, created_date) values
    ('203', '8', '1', '92', '7', '23', '1', '1.000', '11200.00', '2026-06-27 18:05:43', '2026-06-27 18:07:18.670817'),
    ('204', '8', '1', '92', '7', '23', '1', '1.000', '11200.00', '2026-06-27 18:05:43', '2026-06-27 18:07:18.670939'),
    ('205', '8', '1', '92', '7', '23', '1', '1.000', '11200.00', '2026-06-27 18:05:43', '2026-06-27 18:07:18.67094'),
    ('206', '8', '1', '92', '7', '23', '1', '1.000', '11200.00', '2026-06-27 18:05:43', '2026-06-27 18:07:18.67094'),
    ('207', '8', '1', '95', '7', '23', '1', '1.000', '12320.00', '2026-06-15 14:58:00', '2026-06-29 14:59:16.180774'),
    ('208', '8', '1', '95', '7', '23', '1', '1.000', '12320.00', '2026-06-15 14:58:00', '2026-06-29 14:59:16.180929'),
    ('209', '8', '1', '95', '7', '23', '1', '1.000', '12320.00', '2026-06-15 14:58:00', '2026-06-29 14:59:16.180931'),
    ('210', '8', '1', '96', '7', '24', '1', '1.000', '17250.00', '2026-06-29 14:59:15', '2026-06-29 15:01:54.095025'),
    ('211', '8', '1', '96', '7', '24', '1', '1.000', '17250.00', '2026-06-29 14:59:15', '2026-06-29 15:01:54.095028');

create index idx_inv_reg_balance_doc_date on inv_reg_balance using btree (doc_date);

create index idx_inv_reg_balance_document on inv_reg_balance using btree (document_type_id, document_id);

create index idx_inv_reg_balance_organization_id on inv_reg_balance using btree (organization_id);

create index idx_inv_reg_balance_product_id on inv_reg_balance using btree (product_id);

create index idx_inv_reg_balance_product_table_id on inv_reg_balance using btree (product_table_id);

create index idx_inv_reg_balance_warehouse_id on inv_reg_balance using btree (warehouse_id);

create index idx_inv_reg_balance_posting_batch_id on inv_reg_balance using btree (posting_batch_id);

create index idx_inv_reg_balance_reversal_entry_id on inv_reg_balance using btree (reversal_entry_id);

