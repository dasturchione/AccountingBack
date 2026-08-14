create table rtl_sale_doc_product
(
    id                      bigserial primary key,

    owner_id                bigint not null
        references rtl_sale_doc(id) on delete cascade,

    product_id              int not null
        references inv_product(id),

    quantity                numeric(19, 6) not null,

    unit_price              numeric(24, 8) not null,

    -- Себестоимость единицы товара на момент продажи
    cost_price              numeric(24, 8) not null,

    -- quantity * unit_price, без НДС
    amount                  numeric(24, 8) not null,

    vat_rate_id             smallint
        references cmn_vat_rate(id),

    vat_amount              numeric(24, 8) not null,

    -- amount + vat_amount
    total_amount            numeric(24, 8) not null,

    unit_id                 smallint not null
        references cmn_unit(id),

    -- Например 2920
    inventory_account_id    int
        references acc_chart_account(id),

    -- Например 9020
    income_account_id       int
        references acc_chart_account(id),

    -- Например 9120
    cost_account_id         int
        references acc_chart_account(id),

    constraint ck_rtl_sale_doc_product_quantity
        check (quantity > 0),

    constraint ck_rtl_sale_doc_product_unit_price
        check (unit_price >= 0),

    constraint ck_rtl_sale_doc_product_cost_price
        check (cost_price >= 0),

    constraint ck_rtl_sale_doc_product_amount
        check (amount >= 0),

    constraint ck_rtl_sale_doc_product_vat_amount
        check (vat_amount >= 0),

    constraint ck_rtl_sale_doc_product_total_amount
        check (total_amount >= 0)
);

create index ix_rtl_sale_doc_product_owner_id
    on rtl_sale_doc_product(owner_id);

create index ix_rtl_sale_doc_product_product_id
    on rtl_sale_doc_product(product_id);

create index ix_rtl_sale_doc_product_unit_id
    on rtl_sale_doc_product(unit_id);

create index ix_rtl_sale_doc_product_vat_rate_id
    on rtl_sale_doc_product(vat_rate_id);

create index ix_rtl_sale_doc_product_inventory_account_id
    on rtl_sale_doc_product(inventory_account_id);

create index ix_rtl_sale_doc_product_income_account_id
    on rtl_sale_doc_product(income_account_id);

create index ix_rtl_sale_doc_product_cost_account_id
    on rtl_sale_doc_product(cost_account_id);