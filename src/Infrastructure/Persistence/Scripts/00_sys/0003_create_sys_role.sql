create table sys_role 
(
    id integer   not null,
    short_name character varying(100) not null,
    full_name character varying(255) not null,
    state_id smallint not null,
    created_date timestamp without time zone default now() not null,
    organization_id integer,
    has_global_access boolean default false not null,
    code character varying(100),
    description character varying(500),
    is_system boolean default false not null,
    is_owner_role boolean default false not null,
    sort_order integer default 0 not null,
    constraint sys_role_pkey primary key (id),
    constraint sys_role_organization_id_fkey foreign key (organization_id) references org_organization(id),
    constraint sys_role_state_id_fkey foreign key (state_id) references cmn_state(id)
);
create index idx_sys_role_organization_id on sys_role using btree (organization_id);
create index idx_sys_role_code on sys_role using btree (code);
create index idx_sys_role_is_system on sys_role using btree (is_system);
create index idx_sys_role_sort_order on sys_role using btree (sort_order);
create unique index uidx_sys_role_org_code on sys_role using btree (organization_id, code) WHERE (code IS not null);

insert into sys_role (id, short_name, full_name, state_id, created_date, organization_id, has_global_access) values
    ('3', 'Kamilahon', 'Kamilichka', '1', '2026-06-11 18:23:06.708355', null, 'f'),
    ('2', 'AsadbekBux', 'AsadbekBux', '1', '2026-06-11 18:18:43.452469', null, 'f'),
    ('4', 'super_admin', 'Super Admin', '1', '2026-06-19 11:15:16.697264', null, 't'),
    ('5', 'adminka', 'Adminka', '1', '2026-06-19 14:37:38.409257', null, 'f'),
    ('1', 'admin', 'Tashkilot admini', '1', '2026-06-05 16:52:31.75491', null, 'f');
