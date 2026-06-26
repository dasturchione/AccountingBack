alter table inv_product
add column mxik varchar(17);

alter table inv_product
add constraint ck_inv_product_mxik
    check (mxik is null or mxik ~ '^[A-Za-z0-9]{17}$');

create index ix_inv_product_mxik
    on inv_product (mxik)
    where mxik is not null;
