create table sale_condition 
(
    id bigint   not null,
    organization_id integer not null,
    costing_method_id smallint not null,
    vat_rate_id smallint not null,
    start_date timestamp without time zone default now() not null,
    end_date timestamp without time zone,
    state_id smallint not null,
    created_date timestamp without time zone default now() not null,
    constraint ck_sale_condition_dates CHECK (((end_date IS null) OR (end_date >= start_date))),
    constraint sale_condition_pkey primary key (id),
    constraint sale_condition_costing_method_id_fkey foreign key (costing_method_id) references cmn_costing_method(id),
    constraint sale_condition_organization_id_fkey foreign key (organization_id) references org_organization(id),
    constraint sale_condition_state_id_fkey foreign key (state_id) references cmn_state(id),
    constraint sale_condition_vat_rate_id_fkey foreign key (vat_rate_id) references cmn_vat_rate(id)
);

create index idx_sale_condition_costing_method_id on sale_condition using btree (costing_method_id);
create index idx_sale_condition_dates on sale_condition using btree (start_date, end_date);
create index idx_sale_condition_org_state_dates on sale_condition using btree (organization_id, state_id, start_date, end_date);
create index idx_sale_condition_organization_id on sale_condition using btree (organization_id);
create index idx_sale_condition_state_id on sale_condition using btree (state_id);
create index idx_sale_condition_vat_rate_id on sale_condition using btree (vat_rate_id);

insert into sale_condition (id, organization_id, costing_method_id, vat_rate_id, start_date, end_date, state_id, created_date) values
    ('1', '8', '1', '2', '2026-04-01 00:00:00', '2026-12-31 23:59:59.999999', '1', '2026-06-27 16:03:42.262514'),
    ('2', '8', '3', '4', '2026-06-15 17:02:00', null, '1', '2026-06-27 17:04:40.566346'),
    ('3', '8', '1', '2', '2026-06-29 14:50:40', null, '2', '2026-06-29 16:09:34.054396'),
    ('4', '8', '1', '2', '2026-06-25 16:09:00', null, '1', '2026-06-29 16:10:04.471712');s