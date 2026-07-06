create table org_branch 
(
    id integer   not null,
    organization_id integer not null,
    code character varying(50) not null,
    name character varying(250) not null,
    region_id integer,
    district_id integer,
    address character varying(1000),
    phone_number character varying(50),
    state_id smallint not null,
    created_date timestamp without time zone default now() not null,
    constraint org_branch_pkey primary key (id),
    constraint org_branch_district_id_fkey foreign key (district_id) references cmn_district(id),
    constraint org_branch_organization_id_fkey foreign key (organization_id) references org_organization(id),
    constraint org_branch_region_id_fkey foreign key (region_id) references cmn_region(id),
    constraint org_branch_state_id_fkey foreign key (state_id) references cmn_state(id)
);

create index idx_org_branch_district_id on org_branch using btree (district_id);
create unique index idx_org_branch_org_code on org_branch using btree (organization_id, code);
create index idx_org_branch_organization_id on org_branch using btree (organization_id);
create index idx_org_branch_region_id on org_branch using btree (region_id);
create index idx_org_branch_state_id on org_branch using btree (state_id);
