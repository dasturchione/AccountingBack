
create table acc_account_type (
    id smallint   not null,
    code character varying(50) not null,
    name character varying(150) not null,
    state_id smallint not null,
    created_date timestamp without time zone default now() not null,
    constraint acc_account_type_pkey primary key (id),
    constraint acc_account_type_state_id_fkey foreign key (state_id) references cmn_state(id)
);

insert into acc_account_type (id, code, name, state_id, created_date) values
    ('1', 'active', 'Aktiv', '1', '2026-06-09 15:28:20.198084'),
    ('2', 'passive', 'Passiv', '1', '2026-06-09 15:28:20.198084'),
    ('3', 'active_passive', 'Aktiv-passiv', '1', '2026-06-09 15:28:20.198084'),
    ('4', 'off_balance', 'Balansdan tashqari', '1', '2026-06-09 15:28:20.198084');

create unique index idx_acc_account_type_code on acc_account_type using btree (code);

create index idx_acc_account_type_state_id on acc_account_type using btree (state_id);

