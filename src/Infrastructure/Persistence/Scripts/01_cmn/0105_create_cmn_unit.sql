create table cmn_unit 
(
    id smallint   not null,
    code character varying(20) not null,
    name character varying(100) not null,
    state_id smallint not null,
    constraint cmn_unit_pkey primary key (id),
    constraint cmn_unit_state_id_fkey foreign key (state_id) references cmn_state(id)
);

create unique index idx_cmn_unit_code on cmn_unit using btree (code);

insert into cmn_unit (id, code, name, state_id) values
    ('1', 'dona', 'Dona', '1'),
    ('2', 'kg', 'Kilogram', '1'),
    ('3', 'litr', 'Litr', '1'),
    ('4', 'metr', 'Metr', '1'),
    ('5', 'xizmat', 'Xizmat', '1');
