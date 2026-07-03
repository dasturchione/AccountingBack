
create table platform_tenant (
    id integer   not null,
    name character varying(250) not null,
    slug character varying(150) not null,
    owner_user_id integer,
    state_id smallint default 1 not null,
    created_date timestamp without time zone default now() not null,
    updated_date timestamp without time zone,
    constraint platform_tenant_pkey primary key (id),
    constraint platform_tenant_slug_key UNIQUE (slug),
    constraint platform_tenant_owner_user_id_fkey foreign key (owner_user_id) references sys_user(id),
    constraint platform_tenant_state_id_fkey foreign key (state_id) references cmn_state(id)
);

create index idx_platform_tenant_owner_user_id on platform_tenant using btree (owner_user_id);

create index idx_platform_tenant_state_id on platform_tenant using btree (state_id);

