create table org_position 
(
    id integer   not null,
    organization_id integer not null,
    code character varying(50) not null,
    name character varying(250) not null,
    state_id smallint not null,
    created_date timestamp without time zone default now() not null,
    constraint org_position_pkey primary key (id),
    constraint org_position_organization_id_fkey foreign key (organization_id) references org_organization(id),
    constraint org_position_state_id_fkey foreign key (state_id) references cmn_state(id)
);

create unique index idx_org_position_org_code on org_position using btree (organization_id, code);
create index idx_org_position_organization_id on org_position using btree (organization_id);
create index idx_org_position_state_id on org_position using btree (state_id);
