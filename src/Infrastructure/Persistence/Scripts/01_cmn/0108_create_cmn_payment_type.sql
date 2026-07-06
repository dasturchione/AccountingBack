create table cmn_payment_type 
(
    id smallint   not null,
    code character varying(50) not null,
    name character varying(100) not null,
    state_id smallint not null,
    constraint cmn_payment_type_pkey primary key (id),
    constraint cmn_payment_type_state_id_fkey foreign key (state_id) references cmn_state(id)
);

create unique index idx_cmn_payment_type_code on cmn_payment_type using btree (code);

insert into cmn_payment_type (id, code, name, state_id) values
    ('1', 'cash', 'Naqd', '1'),
    ('2', 'bank', 'Bank', '1'),
    ('3', 'card', 'Karta', '1'),
    ('4', 'transfer', 'O''tkazma', '1');
