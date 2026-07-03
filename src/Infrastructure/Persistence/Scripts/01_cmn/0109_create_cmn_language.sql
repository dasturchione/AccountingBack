
create table cmn_language (
    id smallint   not null,
    code character varying(10) not null,
    name character varying(100) not null,
    native_name character varying(100) not null,
    is_default boolean default false not null,
    sort_order integer default 0 not null,
    state_id smallint not null,
    created_date timestamp without time zone default now() not null,
    constraint cmn_language_pkey primary key (id),
    constraint cmn_language_state_id_fkey foreign key (state_id) references cmn_state(id)
);

insert into cmn_language (id, code, name, native_name, is_default, sort_order, state_id, created_date) values
    ('1', 'uz', 'Uzbek', 'O''zbekcha', 't', '1', '1', '2026-06-06 10:44:19.082498'),
    ('2', 'ru', 'Russian', 'Русский', 'f', '2', '1', '2026-06-06 10:44:19.082498'),
    ('3', 'en', 'English', 'English', 'f', '3', '1', '2026-06-06 10:44:19.082498');

create unique index idx_cmn_language_code on cmn_language using btree (code);
create unique index idx_cmn_language_default on cmn_language using btree (is_default) WHERE (is_default = true);
create index idx_cmn_language_state_id on cmn_language using btree (state_id);

