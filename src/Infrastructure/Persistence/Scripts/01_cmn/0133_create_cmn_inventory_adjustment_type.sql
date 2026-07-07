create table cmn_inventory_adjustment_type
(
    id smallint   not null,
    code character varying(50) not null,
    name character varying(100) not null,
    state_id smallint not null,
    constraint cmn_inventory_adjustment_type_pkey primary key (id),
    constraint cmn_inventory_adjustment_type_state_id_fkey foreign key (state_id) references cmn_state(id)
);

create unique index idx_cmn_inventory_adjustment_type_code on cmn_inventory_adjustment_type using btree (code);

insert into cmn_inventory_adjustment_type (id, code, name, state_id) values
    ('1', 'POSITIVE_ADJUSTMENT', 'Ortdi', '1'),
    ('2', 'NEGATIVE_ADJUSTMENT', 'Kamaydi', '1'),
    ('3', 'WRITE_OFF', 'Hisobdan chiqarish', '1'),
    ('4', 'DAMAGE', 'Buzilgan', '1'),
    ('5', 'LOSS', 'Yo''qolgan', '1'),
    ('6', 'FOUND_STOCK', 'Topilgan', '1'),
    ('7', 'CORRECTION', 'Tuzatish', '1');
