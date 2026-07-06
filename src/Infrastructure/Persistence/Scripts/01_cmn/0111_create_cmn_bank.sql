create table cmn_bank 
(
    id integer   not null,
    code character varying(50) not null,
    name character varying(250) not null,
    inn character varying(20),
    mfo character varying(20),
    state_id smallint not null,
    created_date timestamp without time zone default now() not null,
    constraint cmn_bank_pkey primary key (id),
    constraint cmn_bank_state_id_fkey foreign key (state_id) references cmn_state(id)
);

create unique index idx_cmn_bank_code on cmn_bank using btree (code);
create index idx_cmn_bank_state_id on cmn_bank using btree (state_id);

insert into cmn_bank (id, code, name, mfo, state_id, created_date) values
    ('3', 'KAPITAL', 'Kapitalbank', null, '1', '2026-06-06 16:39:53.224198'),
    ('4', 'HAMKOR', 'Hamkorbank', null, '1', '2026-06-06 16:39:53.224198'),
    ('5', 'CODEX191315', 'Codex Test Bank Updated', '99998', '1', '2026-06-24 19:13:15.478874'),
    ('2', 'IPOTEKA', 'Ipoteka bank', null, '1', '2026-06-06 16:39:53.224198'),
    ('1', 'NBU', 'Milliy bank', null, '1', '2026-06-06 16:39:53.224198'),
    ('6', 'Agro', 'AgroBank', '1234', '1', '2026-06-25 13:53:41.945698');
