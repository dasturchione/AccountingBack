create table sale_doc_product 
(
    id bigint not null,
    owner_id bigint not null,
    product_id integer not null,
    quantity numeric(19,6) not null,
    unit_price numeric(24,8) not null,
    cost_price numeric(24,8) default 0 not null,
    amount numeric(24,8) not null,
    vat_rate_id smallint,
    vat_amount numeric(24,8) default 0 not null,
    total_amount numeric(24,8) not null,
    unit_id smallint default 1 not null,
    constraint sale_doc_product_pkey primary key (id),
    constraint sale_doc_product_owner_id_fkey foreign key (owner_id) references sale_doc(id) on DELETE CASCADE,
    constraint sale_doc_product_product_id_fkey foreign key (product_id) references inv_product(id),
    constraint sale_doc_product_unit_id_fkey foreign key (unit_id) references cmn_unit(id),
    constraint sale_doc_product_vat_rate_id_fkey foreign key (vat_rate_id) references cmn_vat_rate(id)
);

create index ix_sale_doc_product_owner_id on sale_doc_product using btree (owner_id);
create index ix_sale_doc_product_product_id on sale_doc_product using btree (product_id);

insert into sale_doc_product (id, owner_id, product_id, quantity, unit_price, cost_price, amount, vat_rate_id, vat_amount, total_amount, unit_id) values
    ('26', '78', '24', '1.000000', '18975.00000000', '17250.00000000', '18975.00000000', '2', '2277.00000000', '21252.00000000', '1'),
    ('28', '80', '23', '5.000000', '13059.20000000', '11872.00000000', '65296.00000000', '2', '7835.52000000', '73131.52000000', '1');
