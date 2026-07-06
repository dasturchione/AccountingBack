create table cmn_product_price_type 
(
    id smallint   not null,
    code character varying(50) not null,
    name character varying(150) not null,
    constraint cmn_product_price_type_code_key UNIQUE (code),
    constraint cmn_product_price_type_pkey primary key (id)
);

insert into cmn_product_price_type (id, code, name) values
    ('1', 'AVERAGE_COST_PRICE', 'Hisoblangan o''rtacha tannarx'),
    ('2', 'FIXED_SALE_PRICE', 'Belgilangan sotuv narxi');
