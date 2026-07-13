create table cmn_didox_origin
(
    code smallint not null,
    name character varying(250) not null,
    state_id smallint not null,
    created_date timestamp without time zone default now() not null,
    constraint cmn_didox_origin_pkey primary key (code),
    constraint cmn_didox_origin_state_id_fkey foreign key (state_id) references cmn_state(id)
);

create index idx_cmn_didox_origin_state_id on cmn_didox_origin using btree (state_id);
