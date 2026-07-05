
create table cmn_notification_type (
    id smallint   not null,
    code character varying(50) not null,
    name character varying(150) not null,
    state_id smallint not null,
    created_date timestamp without time zone default now() not null,
    constraint cmn_notification_type_pkey primary key (id),
    constraint cmn_notification_type_state_id_fkey foreign key (state_id) references cmn_state(id)
);

insert into cmn_notification_type (id, code, name, state_id, created_date) values
    ('1', 'info', 'Ma''lumot', '1', now()),
    ('2', 'success', 'Muvaffaqiyat', '1', now()),
    ('3', 'warning', 'Ogohlantirish', '1', now()),
    ('4', 'error', 'Xato', '1', now()),
    ('5', 'doc_approved', 'Hujjat tasdiqlandi', '1', now()),
    ('6', 'payment_due', 'To''lov muddati', '1', now());

create unique index idx_cmn_notification_type_code on cmn_notification_type using btree (code);

create index idx_cmn_notification_type_state_id on cmn_notification_type using btree (state_id);
