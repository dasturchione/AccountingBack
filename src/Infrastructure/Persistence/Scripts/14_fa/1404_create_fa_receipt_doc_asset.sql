create table fa_receipt_doc_asset
(
    id bigint generated always as identity
        constraint fa_receipt_doc_asset_pkey primary key,
    owner_id bigint not null
        constraint fa_receipt_doc_asset_owner_id_fkey references fa_receipt_doc_line (id) on delete cascade,
    fa_asset_id bigint
        constraint fa_receipt_doc_asset_fa_asset_id_fkey references fa_asset (id),
    inventory_number character varying(100) not null,
    name character varying(250) not null,
    initial_cost numeric(18, 2) not null,
    salvage_value numeric(18, 2) default 0 not null,
    useful_life_months integer not null,
    depreciation_method_id smallint not null
        constraint fa_receipt_doc_asset_depreciation_method_id_fkey references cmn_fa_depreciation_method (id),
    fa_group_id integer not null
        constraint fa_receipt_doc_asset_fa_group_id_fkey references cmn_fa_group (id),
    okof_id smallint
        constraint fa_receipt_doc_asset_okof_id_fkey references cmn_fa_okof (id),
    commissioning_date timestamp without time zone,
    depr_start_date timestamp without time zone,
    planned_units_total numeric(18, 3),
    department_id integer
        constraint fa_receipt_doc_asset_department_id_fkey references org_department (id),
    responsible_user_id integer
        constraint fa_receipt_doc_asset_responsible_user_id_fkey references sys_user (id),
    constraint ck_fa_receipt_doc_asset_useful_life_positive check (useful_life_months > 0),
    constraint ck_fa_receipt_doc_asset_initial_cost_nonnegative check (initial_cost >= 0),
    constraint ck_fa_receipt_doc_asset_salvage_nonnegative check (salvage_value >= 0),
    constraint ck_fa_receipt_doc_asset_salvage_le_initial_cost check (salvage_value <= initial_cost),
    constraint ck_fa_receipt_doc_asset_planned_units_positive check (planned_units_total is null or planned_units_total > 0)
);

create index idx_fa_receipt_doc_asset_owner_id on fa_receipt_doc_asset using btree (owner_id);
create index idx_fa_receipt_doc_asset_fa_asset_id on fa_receipt_doc_asset using btree (fa_asset_id);
create index idx_fa_receipt_doc_asset_fa_group_id on fa_receipt_doc_asset using btree (fa_group_id);
create index idx_fa_receipt_doc_asset_okof_id on fa_receipt_doc_asset using btree (okof_id);
create index idx_fa_receipt_doc_asset_depreciation_method_id on fa_receipt_doc_asset using btree (depreciation_method_id);
create index idx_fa_receipt_doc_asset_department_id on fa_receipt_doc_asset using btree (department_id);
create index idx_fa_receipt_doc_asset_responsible_user_id on fa_receipt_doc_asset using btree (responsible_user_id);
