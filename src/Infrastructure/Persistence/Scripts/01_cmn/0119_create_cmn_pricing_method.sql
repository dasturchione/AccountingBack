create table cmn_pricing_method 
(
    id smallint not null,
    code character varying(20) not null,
    name character varying(200) not null,
    constraint cmn_pricing_method_code_key UNIQUE (code),
    constraint cmn_pricing_method_pkey primary key (id)
);

insert into cmn_pricing_method (id, code, name) values
    ('1', 'COST_PLUS_PERCENT', 'Tannarx + foizli marja'),
    ('2', 'COST_PLUS_AMOUNT', 'Tannarx + belgilangan summa'),
    ('3', 'FIXED_PRICE', 'Belgilangan narx');
