
create table pur_doc_table (
    id bigint   not null,
    owner_id bigint not null,
    product_table_id integer not null,
    amount numeric(24,8) not null,
    vat_rate_id smallint,
    vat_amount numeric(24,8) not null,
    total_amount numeric(24,8) not null,
    constraint pur_doc_table_pkey primary key (id),
    constraint pur_doc_table_owner_id_fkey foreign key (owner_id) references pur_doc_product(id),
    constraint pur_doc_table_product_table_id_fkey foreign key (product_table_id) references inv_product_table(id),
    constraint pur_doc_table_vat_rate_id_fkey foreign key (vat_rate_id) references cmn_vat_rate(id)
);

insert into pur_doc_table (id, owner_id, product_table_id, amount, vat_rate_id, vat_amount, total_amount) values
    ('15', '14', '455', '10000.00000000', '2', '1200.00000000', '11200.00000000'),
    ('16', '14', '456', '10000.00000000', '2', '1200.00000000', '11200.00000000'),
    ('17', '14', '457', '10000.00000000', '2', '1200.00000000', '11200.00000000'),
    ('18', '14', '458', '10000.00000000', '2', '1200.00000000', '11200.00000000'),
    ('19', '17', '459', '11000.00000000', '2', '1320.00000000', '12320.00000000'),
    ('20', '17', '460', '11000.00000000', '2', '1320.00000000', '12320.00000000'),
    ('21', '17', '461', '11000.00000000', '2', '1320.00000000', '12320.00000000'),
    ('22', '18', '462', '15000.00000000', '3', '2250.00000000', '17250.00000000'),
    ('23', '18', '463', '15000.00000000', '3', '2250.00000000', '17250.00000000');

create index ix_pur_doc_table_owner_id_id on pur_doc_table using btree (owner_id, id);

create unique index ux_pur_doc_table_owner_id_product_table_id on pur_doc_table using btree (owner_id, product_table_id);

