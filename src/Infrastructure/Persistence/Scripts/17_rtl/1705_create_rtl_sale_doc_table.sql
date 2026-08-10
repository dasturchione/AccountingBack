create table rtl_sale_doc_table
(
    id                  bigserial primary key,

    product_table_id    int not null
        references inv_product_table(id),

    amount              numeric(24, 8) not null,

    vat_rate_id         smallint
        references cmn_vat_rate(id),

    vat_amount          numeric(24, 8) not null,

    total_amount        numeric(24, 8) not null,

    cost_price          numeric(24, 8) not null,

    owner_id            bigint not null
        references rtl_sale_doc_product(id) on delete cascade,

    constraint ck_rtl_sale_doc_table_amount
        check (amount >= 0),

    constraint ck_rtl_sale_doc_table_vat_amount
        check (vat_amount >= 0),

    constraint ck_rtl_sale_doc_table_total_amount
        check (total_amount >= 0),

    constraint ck_rtl_sale_doc_table_cost_price
        check (cost_price >= 0)
);

create index idx_rtl_sale_doc_table_product_id
    on rtl_sale_doc_table(product_table_id);

create index idx_rtl_sale_doc_table_vat_rate_id
    on rtl_sale_doc_table(vat_rate_id);

create index ix_rtl_sale_doc_table_owner_id
    on rtl_sale_doc_table(owner_id);