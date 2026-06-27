create table cmn_price_rounding_method
(
    id      smallserial primary key,
    code    varchar(50) not null unique,
    name    varchar(100) not null
);

insert into cmn_price_rounding_method
    (id, code, name)
values
    (1, 'NONE', 'O''zgartirmasdan'),
    (2, 'UP', 'Yuqoriga yaxlitlash'),
    (3, 'DOWN', 'Pastga yaxlitlash'),
    (4, 'NEAREST', 'Eng yaqin qiymatga yaxlitlash');