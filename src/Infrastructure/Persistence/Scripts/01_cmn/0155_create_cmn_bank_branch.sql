create table cmn_bank_branch
(
    id                          serial primary key,
    bank_id                     int not null            references cmn_bank(id),
    mfo                         varchar(5) not null unique,
    branch_type                 smallint not null,
    name                        varchar(500) not null,
    address                     varchar(500),
    opened_date                 date,
    source_updated_date         date,
    region_id                   int                     references cmn_region(id),
    district_id                 int                     references cmn_district(id),
    city                        varchar(250),
    inn                         varchar(20),
    website                     varchar(250),
    latitude                    numeric(9,6),
    longitude                   numeric(9,6),
    state_id                    smallint not null       references cmn_state(id),
    created_date                timestamp without time zone not null default now(),

    constraint ck_cmn_bank_branch_mfo check (mfo ~ '^[0-9]{5}$'),
    constraint ck_cmn_bank_branch_type check (branch_type in (0, 1, 2, 3)),
    constraint ck_cmn_bank_branch_latitude check (latitude is null or latitude between -90 and 90),
    constraint ck_cmn_bank_branch_longitude check (longitude is null or longitude between -180 and 180)
);

comment on column cmn_bank_branch.branch_type is
    'Source unit type: 0 - Central Bank unit, 1 - commercial bank head office, 2 - payment center, 3 - branch';

create index idx_cmn_bank_branch_bank_id
    on cmn_bank_branch using btree (bank_id);

create index idx_cmn_bank_branch_branch_type
    on cmn_bank_branch using btree (branch_type);

create index idx_cmn_bank_branch_region_id
    on cmn_bank_branch using btree (region_id);

create index idx_cmn_bank_branch_district_id
    on cmn_bank_branch using btree (district_id);

create index idx_cmn_bank_branch_inn
    on cmn_bank_branch using btree (inn);
