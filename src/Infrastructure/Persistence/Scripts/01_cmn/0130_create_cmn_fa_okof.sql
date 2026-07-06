create table cmn_fa_okof
(
    id smallint not null,
    code character varying(50) not null,
    name character varying(250) not null,
    state_id smallint not null,
    constraint cmn_fa_okof_pkey primary key (id),
    constraint cmn_fa_okof_state_id_fkey foreign key (state_id) references cmn_state(id)
);

create unique index idx_cmn_fa_okof_code on cmn_fa_okof using btree (code);
create index idx_cmn_fa_okof_state_id on cmn_fa_okof using btree (state_id);

insert into cmn_fa_okof (id, code, name, state_id) values
    ('1', '210.00.00.00.000', 'Binolar', '1'),
    ('2', '310.29.10.000', 'Transport vositalari', '1'),
    ('3', '320.26.20.11.110', 'Kompyuter texnikasi', '1'),
    ('4', '330.31.01.1', 'Mebel', '1');
