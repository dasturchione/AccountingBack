
create table org_organization (
    id integer   not null,
    short_name character varying(250) not null,
    full_name character varying(500) not null,
    inn character varying(20) not null,
    phone_number character varying(50),
    region_id integer not null,
    district_id integer,
    address character varying(1000),
    director character varying(250),
    is_parent boolean default false not null,
    state_id smallint not null,
    created_date timestamp without time zone default now() not null,
    default_language_id smallint,
    tenant_id integer,
    setup_status character varying(30) default 'not_started'::character varying not null,
    setup_completed_at timestamp without time zone,
    email character varying(200),
    website character varying(250),
    oked character varying(20),
    constraint org_organization_pkey primary key (id),
    constraint org_organization_default_language_id_fkey foreign key (default_language_id) references cmn_language(id),
    constraint org_organization_district_id_fkey foreign key (district_id) references cmn_district(id),
    constraint org_organization_region_id_fkey foreign key (region_id) references cmn_region(id),
    constraint org_organization_state_id_fkey foreign key (state_id) references cmn_state(id),
    constraint org_organization_tenant_id_fkey foreign key (tenant_id) references platform_tenant(id)
);

insert into org_organization (id, short_name, full_name, inn, phone_number, region_id, district_id, address, director, is_parent, state_id, created_date, default_language_id) values
    ('8', 'baraka_market', 'Baraka market', '1599789', '+998 99 897-06-42', '8', '127', 'Alisher Navoiy 17', 'Hafizov Sardorbek', 'f', '1', '2026-06-12 11:24:38.218896', '1'),
    ('2', 'Najot Ta''lim', 'Najot Ta''lim Xususiy', '310540000', '+998 99 871-23-12', '8', '125', 'Toshkent shahar, Mirzo Ulug''bek tumani, Amir Temur ko''chasi 1-uy test', 'Karimov Jasur test', 't', '1', '2026-06-05 16:49:46.331959', '2');

create index idx_org_organization_default_language_id on org_organization using btree (default_language_id);

create index idx_org_organization_district_id on org_organization using btree (district_id);

create index idx_org_organization_full_name on org_organization using btree (full_name);

create index idx_org_organization_inn on org_organization using btree (inn);

create index idx_org_organization_region_id on org_organization using btree (region_id);

create index idx_org_organization_short_name on org_organization using btree (short_name);

create index idx_org_organization_state_id on org_organization using btree (state_id);

create index idx_org_organization_tenant_id on org_organization using btree (tenant_id);

create index idx_org_organization_setup_status on org_organization using btree (setup_status);

