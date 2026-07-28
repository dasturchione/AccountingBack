create table cmn_price_rounding_method 
(
    id smallint   not null,
    code character varying(50) not null,
    name character varying(100) not null,
    constraint cmn_price_rounding_method_code_key UNIQUE (code),
    constraint cmn_price_rounding_method_pkey primary key (id)
);

insert into cmn_price_rounding_method (id, code, name) values
    ('1', 'NONE', 'O''zgartirmasdan'),
    ('2', 'UP', 'Yuqoriga yaxlitlash'),
    ('3', 'DOWN', 'Pastga yaxlitlash'),
    ('4', 'NEAREST', 'Eng yaqin qiymatga yaxlitlash');
