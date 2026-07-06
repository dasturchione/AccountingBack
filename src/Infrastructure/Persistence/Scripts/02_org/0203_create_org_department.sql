create table org_department 
(
    id integer   not null,
    organization_id integer not null,
    branch_id integer,
    code character varying(50) not null,
    name character varying(250) not null,
    state_id smallint not null,
    created_date timestamp without time zone default now() not null,
    constraint org_department_pkey primary key (id),
    constraint org_department_branch_id_fkey foreign key (branch_id) references org_branch(id),
    constraint org_department_organization_id_fkey foreign key (organization_id) references org_organization(id),
    constraint org_department_state_id_fkey foreign key (state_id) references cmn_state(id)
);

create index idx_org_department_branch_id on org_department using btree (branch_id);
create unique index idx_org_department_org_code on org_department using btree (organization_id, code);
create index idx_org_department_organization_id on org_department using btree (organization_id);
create index idx_org_department_state_id on org_department using btree (state_id);
