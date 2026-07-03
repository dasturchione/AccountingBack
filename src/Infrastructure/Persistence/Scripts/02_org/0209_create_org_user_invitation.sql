
create table org_user_invitation (
    id bigint   not null,
    organization_id integer not null,
    email character varying(200) not null,
    role_id integer not null,
    invited_by_user_id integer,
    token_hash character varying(512) not null,
    expires_at timestamp without time zone not null,
    accepted_at timestamp without time zone,
    accepted_by_user_id integer,
    state_id smallint default 1 not null,
    created_date timestamp without time zone default now() not null,
    constraint org_user_invitation_pkey primary key (id),
    constraint org_user_invitation_token_hash_key UNIQUE (token_hash),
    constraint org_user_invitation_accepted_by_user_id_fkey foreign key (accepted_by_user_id) references sys_user(id),
    constraint org_user_invitation_invited_by_user_id_fkey foreign key (invited_by_user_id) references sys_user(id),
    constraint org_user_invitation_organization_id_fkey foreign key (organization_id) references org_organization(id),
    constraint org_user_invitation_role_id_fkey foreign key (role_id) references sys_role(id),
    constraint org_user_invitation_state_id_fkey foreign key (state_id) references cmn_state(id)
);

create index idx_org_user_invitation_email on org_user_invitation using btree (email);

create index idx_org_user_invitation_organization_id on org_user_invitation using btree (organization_id);

create index idx_org_user_invitation_role_id on org_user_invitation using btree (role_id);

