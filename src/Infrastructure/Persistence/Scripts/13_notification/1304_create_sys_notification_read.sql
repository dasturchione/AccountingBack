create table sys_notification_read 
(
    id bigint   not null,
    notification_id bigint not null,
    user_id integer not null,
    read_at timestamp without time zone default now() not null,
    created_date timestamp without time zone default now() not null,
    constraint sys_notification_read_pkey primary key (id),
    constraint sys_notification_read_notification_id_user_id_key unique (notification_id, user_id)
);

create index idx_sys_notification_read_user_id_notification_id on sys_notification_read using btree (user_id, notification_id);
