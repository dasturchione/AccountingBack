alter table sys_user
add column if not exists language_id smallint null references cmn_language(id);

create index if not exists idx_sys_user_language_id
on sys_user (language_id);
