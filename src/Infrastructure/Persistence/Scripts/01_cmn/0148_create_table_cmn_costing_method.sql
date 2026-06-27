create table cmn_costing_method
(
    id      smallserial primary key,
    code    varchar(50) not null unique,
    name    varchar(100) not null
);

insert into cmn_costing_method
    (id, code, name)
values
    (1, 'FIFO',         'FIFO - birinchi kirgan birinchi chiqadi'),
    (2, 'LIFO',         'LIFO - oxirgi kirgan birinchi chiqadi'),
    (3, 'AVERAGE',      'O''rtacha tannarx'); 
