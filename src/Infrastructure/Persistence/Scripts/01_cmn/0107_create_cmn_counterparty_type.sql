create table cmn_counterparty_type 
(
    id smallint   not null,
    code character varying(50) not null,
    name character varying(100) not null,
    state_id smallint not null,
    constraint cmn_counterparty_type_pkey primary key (id),
    constraint cmn_counterparty_type_state_id_fkey foreign key (state_id) references cmn_state(id)
);

create unique index idx_cmn_counterparty_type_code on cmn_counterparty_type using btree (code);

insert into cmn_counterparty_type (id, code, name, state_id) values
    ('1', 'client', 'Mijoz', '1'),
    ('2', 'supplier', 'Yetkazib beruvchi', '1'),
    ('3', 'client_supplier', 'Mijoz va yetkazib beruvchi', '1');
