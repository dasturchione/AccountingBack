create table pur_doc_product 
(
    id bigint   not null,
    owner_id bigint not null,
    product_id integer not null,
    quantity numeric(19,6) not null,
    unit_id smallint not null,
    amount numeric(24,8) not null,
    vat_rate_id smallint,
    vat_amount numeric(24,8) not null,
    total_amount numeric(24,8) not null,
    unit_price numeric(24,8) not null,
    constraint pur_doc_product_pkey primary key (id),
    constraint pur_doc_product_owner_id_fkey foreign key (owner_id) references pur_doc(id),
    constraint pur_doc_product_product_id_fkey foreign key (product_id) references inv_product(id),
    constraint pur_doc_product_unit_id_fkey foreign key (unit_id) references cmn_unit(id),
    constraint pur_doc_product_vat_rate_id_fkey foreign key (vat_rate_id) references cmn_vat_rate(id)
);

create index ix_pur_doc_product_owner_id on pur_doc_product using btree (owner_id);
create index ix_pur_doc_product_product_id on pur_doc_product using btree (product_id) WHERE (product_id IS not null);

insert into pur_doc_product (id, owner_id, product_id, quantity, unit_id, amount, vat_rate_id, vat_amount, total_amount, unit_price) values
    ('14', '92', '23', '4.000000', '1', '40000.00000000', '2', '4800.00000000', '44800.00000000', '10000.00000000'),
    ('15', '93', '25', '2.000000', '5', '10000.00000000', '2', '1200.00000000', '11200.00000000', '5000.00000000'),
    ('16', '94', '25', '1.000000', '5', '10000.00000000', '2', '1200.00000000', '11200.00000000', '10000.00000000'),
    ('17', '95', '23', '3.000000', '1', '33000.00000000', '2', '3960.00000000', '36960.00000000', '11000.00000000'),
    ('18', '96', '24', '2.000000', '1', '30000.00000000', '3', '4500.00000000', '34500.00000000', '15000.00000000'),
    ('19', '97', '25', '2.000000', '5', '20000.00000000', '2', '2400.00000000', '22400.00000000', '10000.00000000');
