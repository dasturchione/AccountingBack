create table cmn_costing_method 
(
    id smallint   not null,
    code character varying(50) not null,
    name character varying(100) not null,
    constraint cmn_costing_method_code_key UNIQUE (code),
    constraint cmn_costing_method_pkey primary key (id)
);

insert into cmn_costing_method (id, code, name) values
    ('1', 'FIFO', 'FIFO - birinchi kirgan birinchi chiqadi'),
    ('2', 'LIFO', 'LIFO - oxirgi kirgan birinchi chiqadi'),
    ('3', 'AVERAGE', 'O''rtacha tannarx');
