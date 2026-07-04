alter table inv_product
    add column product_type_id smallint not null references cmn_product_type (id) default 1;

create index idx_inv_product_product_type_id 
    on inv_product (product_type_id);

ALTER TABLE inv_product
    ADD COLUMN is_sold      boolean NOT NULL DEFAULT true,
    ADD COLUMN is_purchased boolean NOT NULL DEFAULT true,
    ADD CONSTRAINT chk_inv_product_sale_purchase_flags
        CHECK (is_sold OR is_purchased);

alter table inv_product 
drop column cogs_account_id, 
drop column expense_account_id, 
drop column income_account_id, 
drop column inventory_account_id;
