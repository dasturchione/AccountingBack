
create table inv_product_group (
    id integer   not null,
    organization_id integer not null,
    name character varying(250) not null,
    state_id smallint not null,
    created_date timestamp without time zone default now() not null,
    code character varying(100),
    parent_id integer,
    sort_order integer default 0 not null,
    constraint inv_product_group_pkey primary key (id),
    constraint inv_product_group_organization_id_fkey foreign key (organization_id) references org_organization(id),
    constraint inv_product_group_state_id_fkey foreign key (state_id) references cmn_state(id),
    constraint inv_product_group_parent_id_fkey foreign key (parent_id) references inv_product_group(id)
);

insert into inv_product_group (id, organization_id, name, state_id, created_date) values
    ('13', '8', 'Muzlatgichlar', '1', '2026-06-27 15:18:21.48753'),
    ('14', '8', 'Komunnal xizmatlar', '1', '2026-06-27 15:21:30.13086');

create index idx_inv_product_group_organization_id on inv_product_group using btree (organization_id);

create index idx_inv_product_group_state_id on inv_product_group using btree (state_id);

create index idx_inv_product_group_code on inv_product_group using btree (code);

create index idx_inv_product_group_parent_id on inv_product_group using btree (parent_id);

create index idx_inv_product_group_sort_order on inv_product_group using btree (sort_order);

create unique index uidx_inv_product_group_org_code on inv_product_group using btree (organization_id, code) WHERE (code IS not null);

