create table sys_refresh_token 
(
    id bigint   not null,
    user_id integer not null,
    token_hash character varying(512) not null,
    jwt_id character varying(128),
    device_name character varying(250),
    ip_address character varying(64),
    user_agent character varying(500),
    expires_at timestamp without time zone not null,
    revoked_at timestamp without time zone,
    replaced_by_token_hash character varying(512),
    created_date timestamp without time zone default now() not null,
    constraint sys_refresh_token_pkey primary key (id),
    constraint sys_refresh_token_token_hash_key UNIQUE (token_hash),
    constraint sys_refresh_token_user_id_fkey foreign key (user_id) references sys_user(id)
);

create index idx_sys_refresh_token_expires_at on sys_refresh_token using btree (expires_at);
create index idx_sys_refresh_token_revoked_at on sys_refresh_token using btree (revoked_at);
create index idx_sys_refresh_token_user_id on sys_refresh_token using btree (user_id);

