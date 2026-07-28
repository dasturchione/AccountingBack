create table cmn_state 
(
    id smallint not null,
    short_name character varying(250) not null,
    full_name character varying(250) not null,
    created_date timestamp without time zone default now() not null,
    constraint cmn_state_pkey primary key (id)
);

insert into cmn_state (id, short_name, full_name, created_date) values
    ('1', 'A', 'Aktiv', '2026-06-05 16:34:55.941031'),
    ('2', 'P', 'Passiv', '2026-06-05 16:34:55.941031');

