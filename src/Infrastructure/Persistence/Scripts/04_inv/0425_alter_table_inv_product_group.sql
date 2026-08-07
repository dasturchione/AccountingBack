alter table inv_product_group
	drop column organization_id;

alter table inv_product_group
    add column is_assignable boolean not null default false;

alter table inv_product_group
    alter column code set not null;

alter table inv_product_group
    add constraint uq_inv_product_group_code unique (code);