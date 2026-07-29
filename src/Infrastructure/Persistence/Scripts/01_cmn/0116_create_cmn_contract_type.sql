create table cmn_contract_type 
(
    id smallint   not null,
    code character varying(50) not null,
    name character varying(150) not null,
    state_id smallint not null,
    created_date timestamp without time zone default now() not null,
    constraint cmn_contract_type_pkey primary key (id),
    constraint cmn_contract_type_state_id_fkey foreign key (state_id) references cmn_state(id)
);

create unique index idx_cmn_contract_type_code on cmn_contract_type using btree (code);
create index idx_cmn_contract_type_state_id on cmn_contract_type using btree (state_id);

insert into cmn_contract_type (id, code, name, state_id, created_date) values
    ('1', 'supplier', 'Yetkazib beruvchi bilan shartnoma', '1', '2026-06-17 17:35:47.346557'),
    ('2', 'customer', 'Xaridor bilan shartnoma', '1', '2026-06-17 17:35:47.346557');
