
create table inv_warehouse (
    id integer   not null,
    organization_id integer not null,
    branch_id integer,
    name character varying(250) not null,
    responsible_user_id integer,
    state_id smallint not null,
    created_date timestamp without time zone default now() not null,
    code character varying(100),
    address character varying(1000),
    is_main boolean default false not null,
    constraint inv_warehouse_pkey primary key (id),
    constraint inv_warehouse_branch_id_fkey foreign key (branch_id) references org_branch(id),
    constraint inv_warehouse_organization_id_fkey foreign key (organization_id) references org_organization(id),
    constraint inv_warehouse_responsible_user_id_fkey foreign key (responsible_user_id) references sys_user(id),
    constraint inv_warehouse_state_id_fkey foreign key (state_id) references cmn_state(id)
);

insert into inv_warehouse (id, organization_id, branch_id, name, responsible_user_id, state_id, created_date) values
    ('7', '8', '6', 'amonov', '15', '1', '2026-06-20 15:38:43.144691');

create index idx_inv_warehouse_branch_id on inv_warehouse using btree (branch_id);

create index idx_inv_warehouse_organization_id on inv_warehouse using btree (organization_id);

create index idx_inv_warehouse_responsible_user_id on inv_warehouse using btree (responsible_user_id);

create index idx_inv_warehouse_state_id on inv_warehouse using btree (state_id);

create index idx_inv_warehouse_code on inv_warehouse using btree (code);

create index idx_inv_warehouse_is_main on inv_warehouse using btree (is_main);

create unique index uidx_inv_warehouse_org_code on inv_warehouse using btree (organization_id, code) WHERE (code IS not null);

