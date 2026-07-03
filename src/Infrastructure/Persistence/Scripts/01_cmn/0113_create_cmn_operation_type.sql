
create table cmn_operation_type (
    id smallint   not null,
    code character varying(50) not null,
    name character varying(150) not null,
    state_id smallint not null,
    created_date timestamp without time zone default now() not null,
    constraint cmn_operation_type_pkey primary key (id),
    constraint cmn_operation_type_state_id_fkey foreign key (state_id) references cmn_state(id)
);

insert into cmn_operation_type (id, code, name, state_id, created_date) values
    ('1', 'in', 'Kirim', '1', '2026-06-06 16:40:19.40134'),
    ('2', 'out', 'Chiqim', '1', '2026-06-06 16:40:19.40134'),
    ('3', 'transfer', 'O''tkazma', '1', '2026-06-06 16:40:19.40134'),
    ('4', 'debt_increase', 'Qarz oshishi', '1', '2026-06-06 16:40:19.40134'),
    ('5', 'debt_decrease', 'Qarz kamayishi', '1', '2026-06-06 16:40:19.40134');

create unique index idx_cmn_operation_type_code on cmn_operation_type using btree (code);

create index idx_cmn_operation_type_state_id on cmn_operation_type using btree (state_id);

