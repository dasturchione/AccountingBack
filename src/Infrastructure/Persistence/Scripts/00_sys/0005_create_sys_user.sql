create table sys_user 
(
    id integer   not null,
    user_name character varying(250) not null,
    password_hash character varying(250) not null,
    password_salt character varying(250) not null,
    phone_number character varying(50) not null,
    email character varying(200),
    first_name character varying(100) not null,
    last_name character varying(100) not null,
    role_id integer not null,
    last_access_time timestamp without time zone,
    state_id smallint not null,
    created_date timestamp without time zone default now() not null,
    language_id smallint,
    organization_id integer,
    email_verified boolean default false not null,
    email_verified_at timestamp without time zone,
    last_login_ip character varying(64),
    is_platform_admin boolean default false not null,
    timezone character varying(100),
    constraint sys_user_pkey primary key (id),
    constraint sys_user_language_id_fkey foreign key (language_id) references cmn_language(id),
    constraint sys_user_organization_id_fkey foreign key (organization_id) references org_organization(id),
    constraint sys_user_role_id_fkey foreign key (role_id) references sys_role(id),
    constraint sys_user_state_id_fkey foreign key (state_id) references cmn_state(id)
);

create index idx_sys_user_language_id on sys_user using btree (language_id);
create index idx_sys_user_organization_id on sys_user using btree (organization_id);
create index idx_sys_user_phone on sys_user using btree (phone_number);
create index idx_sys_user_role_id on sys_user using btree (role_id);
create unique index uidx_sys_user_user_name on sys_user using btree (user_name);
create index idx_sys_user_email on sys_user using btree (email);
create index idx_sys_user_email_verified on sys_user using btree (email_verified);
create index idx_sys_user_is_platform_admin on sys_user using btree (is_platform_admin);

