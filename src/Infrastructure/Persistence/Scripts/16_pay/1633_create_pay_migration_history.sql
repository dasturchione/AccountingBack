begin;

create table if not exists pay_migration_history
(
    id                bigserial primary key,
    script_name       varchar(300) not null,
    checksum_sha256   char(64) not null,
    applied_at_utc    timestamp with time zone not null default now(),
    constraint ux_pay_migration_history_script unique (script_name)
);

commit;
