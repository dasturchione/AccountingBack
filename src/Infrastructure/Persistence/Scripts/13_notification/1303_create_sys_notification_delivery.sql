create table sys_notification_delivery 
(
    id bigint   not null,
    notification_id bigint not null,
    channel smallint not null,
    status smallint default 0 not null,
    error character varying(1000),
    sent_at timestamp without time zone,
    created_date timestamp without time zone default now() not null,
    constraint sys_notification_delivery_pkey primary key (id),
    constraint sys_notification_delivery_notification_id_fkey foreign key (notification_id) references sys_notification(id) on delete cascade
);

create index idx_sys_notification_delivery_notification_id on sys_notification_delivery using btree (notification_id);
create index idx_sys_notification_delivery_status on sys_notification_delivery using btree (status);
