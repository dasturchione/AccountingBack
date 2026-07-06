create table sys_setting (
    id bigint not null,
    code character varying(100) not null,
    value text,
    value_type smallint not null,
    category character varying(100),
    description character varying(500),
    is_editable boolean default true not null,
    organization_id integer,
    state_id smallint default 1 not null,
    created_date timestamp without time zone default now() not null,
    updated_date timestamp without time zone,
    constraint sys_setting_pkey primary key (id),
    constraint sys_setting_organization_id_fkey foreign key (organization_id) references org_organization(id),
    constraint sys_setting_state_id_fkey foreign key (state_id) references cmn_state(id)
);

create unique index ux_sys_setting_global_code on sys_setting using btree (code) where organization_id is null;
create unique index ux_sys_setting_org_code on sys_setting using btree (organization_id, code) where organization_id is not null;
create index idx_sys_setting_category on sys_setting using btree (category);
create index idx_sys_setting_organization_id on sys_setting using btree (organization_id);

insert into sys_setting ( id, code, value, value_type, category, description, is_editable, organization_id, state_id, created_date ) values
    (1, 'SYSTEM_NAME', 'Accounting Back', 0, 'general', 'Displayed system name.', true, null, 1, '2026-07-05 00:00:00'),
    (2, 'DEFAULT_CURRENCY', 'UZS', 0, 'general', 'Default currency code.', true, null, 1, '2026-07-05 00:00:00'),
    (3, 'DEFAULT_LANGUAGE', 'uz', 0, 'localization', 'Default interface language code.', true, null, 1, '2026-07-05 00:00:00'),
    (4, 'SUPPORT_EMAIL', 'support@example.com', 0, 'email', 'Support email reference.', true, null, 1, '2026-07-05 00:00:00'),
    (5, 'EMAIL_PROVIDER', 'smtp', 0, 'email', 'Configured outbound email provider reference.', false, null, 1, '2026-07-05 00:00:00');
