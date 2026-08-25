create table if not exists cmn_movement_direction
(
    id smallint not null,
    code character varying(10) not null,
    name character varying(150) not null,
    constraint cmn_movement_direction_pkey primary key (id),
    constraint ck_cmn_movement_direction_id check (id in (-1, 1))
);

create unique index if not exists ux_cmn_movement_direction_code
    on cmn_movement_direction using btree (code);

insert into cmn_movement_direction (id, code, name)
values
    (-1, 'OUT', 'Chiqim'),
    ( 1, 'IN',  'Kirim')
on conflict (id) do update set
    code = excluded.code,
    name = excluded.name;
