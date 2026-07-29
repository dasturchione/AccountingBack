create table cmn_product_table_status 
(
    id smallint   not null,
    code character varying(50) not null,
    name character varying(100) not null,
    state_id smallint not null,
    constraint cmn_product_table_status_code_key UNIQUE (code),
    constraint cmn_product_table_status_pkey primary key (id),
    constraint cmn_product_table_status_state_id_fkey foreign key (state_id) references cmn_state(id)
);

insert into cmn_product_table_status (id, code, name, state_id) values
    ('1', 'IN_STOCK', 'На складе', '1'),
    ('2', 'RESERVED', 'Зарезервирован', '1'),
    ('3', 'SOLD', 'Продан', '1'),
    ('4', 'RETURNED_TO_SUPPLIER', 'Возвращен поставщику', '1'),
    ('5', 'RETURNED_FROM_CUSTOMER', 'Возвращен покупателем', '1'),
    ('6', 'WRITTEN_OFF', 'Списан', '1'),
    ('7', 'LOST', 'Утерян', '1'),
    ('8', 'BLOCKED', 'Заблокирован', '1');
