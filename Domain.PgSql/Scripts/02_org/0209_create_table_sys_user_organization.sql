create table sys_user_organization
(
	user_id int not null references sys_user(id) on delete cascade,
	organization_id int not null references org_organization(id) on delete cascade,
	role_id int null references sys_role(id),
	is_default boolean default false not null,
	state_id smallint not null references cmn_state(id),
	created_date timestamp without time zone default now() not null,
	primary key (user_id, organization_id)
);

create index idx_sys_user_organization_organization_id
on sys_user_organization (organization_id);

create index idx_sys_user_organization_role_id
on sys_user_organization (role_id);

create index idx_sys_user_organization_state_id
on sys_user_organization (state_id);

create unique index idx_sys_user_organization_default_user
on sys_user_organization (user_id)
where is_default = true;
