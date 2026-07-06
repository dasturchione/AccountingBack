create table sys_user_organization 
(
    user_id integer not null,
    organization_id integer not null,
    role_id integer,
    is_default boolean default false not null,
    state_id smallint not null,
    created_date timestamp without time zone default now() not null,
    is_owner boolean default false not null,
    joined_at timestamp without time zone default now() not null,
    invited_by_user_id integer,
    last_access_at timestamp without time zone,
    blocked_at timestamp without time zone,
    constraint sys_user_organization_pkey primary key (user_id, organization_id),
    constraint sys_user_organization_organization_id_fkey foreign key (organization_id) references org_organization(id) on DELETE CASCADE,
    constraint sys_user_organization_role_id_fkey foreign key (role_id) references sys_role(id),
    constraint sys_user_organization_state_id_fkey foreign key (state_id) references cmn_state(id),
    constraint sys_user_organization_user_id_fkey foreign key (user_id) references sys_user(id) on DELETE CASCADE
);

create unique index idx_sys_user_organization_default_user on sys_user_organization using btree (user_id) WHERE (is_default = true);
create index idx_sys_user_organization_organization_id on sys_user_organization using btree (organization_id);
create index idx_sys_user_organization_role_id on sys_user_organization using btree (role_id);
create index idx_sys_user_organization_state_id on sys_user_organization using btree (state_id);
create index idx_sys_user_organization_is_owner on sys_user_organization using btree (is_owner);
create index idx_sys_user_organization_invited_by_user_id on sys_user_organization using btree (invited_by_user_id);

