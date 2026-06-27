create table cmn_product_price_type
(
    id      smallserial primary key,
    code    varchar(50) not null unique,
    name    varchar(150) not null
);

insert into cmn_product_price_type
    (id, code, name)
values
    (1, 'AVERAGE_COST_PRICE', 'Hisoblangan o''rtacha tannarx'),
    (2, 'FIXED_SALE_PRICE', 'Belgilangan sotuv narxi');
