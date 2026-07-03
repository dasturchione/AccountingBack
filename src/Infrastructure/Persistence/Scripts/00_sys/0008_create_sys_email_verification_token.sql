
create table sys_email_verification_token (
    id bigint   not null,
    user_id integer not null,
    token_hash character varying(512) not null,
    email character varying(200) not null,
    expires_at timestamp without time zone not null,
    verified_at timestamp without time zone,
    created_date timestamp without time zone default now() not null,
    constraint sys_email_verification_token_pkey primary key (id),
    constraint sys_email_verification_token_token_hash_key UNIQUE (token_hash),
    constraint sys_email_verification_token_user_id_fkey foreign key (user_id) references sys_user(id)
);

create index idx_sys_email_verification_token_email on sys_email_verification_token using btree (email);
create index idx_sys_email_verification_token_user_id on sys_email_verification_token using btree (user_id);

