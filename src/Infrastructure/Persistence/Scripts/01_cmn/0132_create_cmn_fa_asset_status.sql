create table cmn_fa_asset_status
(
    id smallint not null,
    code character varying(50) not null,
    name character varying(100) not null,
    state_id smallint not null,

    constraint cmn_fa_asset_status_pkey primary key (id),
    constraint cmn_fa_asset_status_code_key unique (code),
    constraint cmn_fa_asset_status_state_id_fkey foreign key (state_id) references cmn_state(id)
);

insert into cmn_fa_asset_status
(
    id,
    code,
    name,
    state_id
)
values
    (1, 'NOT_COMMISSIONED', 'Foydalanishga topshirilmagan', 1),
    (2, 'ACTIVE',           'Ekspluatatsiyada',             1),
    (3, 'CONSERVATION',     'Konservatsiyada',              1),
    (4, 'DISPOSED',         'Hisobdan chiqarilgan',         1);