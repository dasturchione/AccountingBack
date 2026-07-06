BEGIN;
-- Diff source: Scripts target vs Desktop\\17.sql current dump
-- NOTE: 0903_create_counterparty_reg_balance.sql line 28 is missing a semicolon; migration keeps the intended existing indexes on organization_id and posting_batch_id.

-- 1. DROP extra indexes
DROP INDEX IF EXISTS public.idx_acc_reg_entry_org_credit_docdate_id;
DROP INDEX IF EXISTS public.idx_acc_reg_entry_org_debit_docdate_id;
DROP INDEX IF EXISTS public.idx_acc_reg_entry_org_docdate_credit_account;
DROP INDEX IF EXISTS public.idx_acc_reg_entry_org_docdate_debit_account;
DROP INDEX IF EXISTS public.idx_bank_operation_contract_id;
DROP INDEX IF EXISTS public.idx_bank_operation_counterparty_bank_account_id;
DROP INDEX IF EXISTS public.idx_bank_operation_payment_purpose_id;
DROP INDEX IF EXISTS public.idx_cash_operation_payment_purpose_id;
DROP INDEX IF EXISTS public.idx_inv_inventory_count_doc_cancelled_by_user_id;
DROP INDEX IF EXISTS public.idx_inv_inventory_count_doc_count_completed_by_user_id;
DROP INDEX IF EXISTS public.idx_inv_inventory_count_doc_negative_adjustment_doc_id;
DROP INDEX IF EXISTS public.idx_inv_inventory_count_doc_positive_adjustment_doc_id;
DROP INDEX IF EXISTS public.idx_inv_inventory_count_doc_posted_by_user_id;

-- 2. DROP extra constraints
ALTER TABLE public.acc_accounting_period DROP CONSTRAINT IF EXISTS acc_accounting_period_closed_by_user_id_fkey;
ALTER TABLE public.acc_accounting_period DROP CONSTRAINT IF EXISTS acc_accounting_period_organization_id_fkey;
ALTER TABLE public.acc_chart_account DROP CONSTRAINT IF EXISTS acc_chart_account_parent_id_fkey;
ALTER TABLE public.acc_posting_batch DROP CONSTRAINT IF EXISTS acc_posting_batch_document_type_id_fkey;
ALTER TABLE public.acc_posting_batch DROP CONSTRAINT IF EXISTS acc_posting_batch_organization_id_fkey;
ALTER TABLE public.acc_posting_batch DROP CONSTRAINT IF EXISTS acc_posting_batch_posted_by_user_id_fkey;
ALTER TABLE public.acc_posting_batch DROP CONSTRAINT IF EXISTS acc_posting_batch_reversed_by_user_id_fkey;
ALTER TABLE public.acc_reg_entry DROP CONSTRAINT IF EXISTS acc_reg_entry_posting_batch_id_fkey;
ALTER TABLE public.acc_reg_entry DROP CONSTRAINT IF EXISTS acc_reg_entry_reversal_entry_id_fkey;
ALTER TABLE public.bank_operation DROP CONSTRAINT IF EXISTS bank_operation_cancelled_by_user_id_fkey;
ALTER TABLE public.bank_operation DROP CONSTRAINT IF EXISTS bank_operation_posted_by_user_id_fkey;
ALTER TABLE public.cash_box DROP CONSTRAINT IF EXISTS cash_box_responsible_user_id_fkey;
ALTER TABLE public.cash_operation DROP CONSTRAINT IF EXISTS cash_operation_cancelled_by_user_id_fkey;
ALTER TABLE public.cash_operation DROP CONSTRAINT IF EXISTS cash_operation_posted_by_user_id_fkey;
ALTER TABLE public.cmn_document_sequence DROP CONSTRAINT IF EXISTS cmn_document_sequence_document_type_id_fkey;
ALTER TABLE public.cmn_document_sequence DROP CONSTRAINT IF EXISTS cmn_document_sequence_organization_id_fkey;
ALTER TABLE public.cmn_document_sequence DROP CONSTRAINT IF EXISTS cmn_document_sequence_state_id_fkey;
ALTER TABLE public.counterparty_reg_balance DROP CONSTRAINT IF EXISTS counterparty_reg_balance_posting_batch_id_fkey;
ALTER TABLE public.inv_inventory_adjustment_doc DROP CONSTRAINT IF EXISTS inv_inventory_adjustment_doc_cancelled_by_user_id_fkey;
ALTER TABLE public.inv_inventory_adjustment_doc DROP CONSTRAINT IF EXISTS inv_inventory_adjustment_doc_posted_by_user_id_fkey;
ALTER TABLE public.inv_inventory_count_doc DROP CONSTRAINT IF EXISTS inv_inventory_count_doc_cancelled_by_user_id_fkey;
ALTER TABLE public.inv_inventory_count_doc DROP CONSTRAINT IF EXISTS inv_inventory_count_doc_count_completed_by_user_id_fkey;
ALTER TABLE public.inv_inventory_count_doc DROP CONSTRAINT IF EXISTS inv_inventory_count_doc_posted_by_user_id_fkey;
ALTER TABLE public.inv_product_group DROP CONSTRAINT IF EXISTS inv_product_group_parent_id_fkey;
ALTER TABLE public.inv_product DROP CONSTRAINT IF EXISTS inv_product_default_vat_rate_id_fkey;
ALTER TABLE public.inv_reg_balance DROP CONSTRAINT IF EXISTS inv_reg_balance_posting_batch_id_fkey;
ALTER TABLE public.inv_transfer_doc_table DROP CONSTRAINT IF EXISTS inv_transfer_doc_table_destination_warehouse_id_fkey;
ALTER TABLE public.inv_transfer_doc_table DROP CONSTRAINT IF EXISTS inv_transfer_doc_table_source_warehouse_id_fkey;
ALTER TABLE public.inv_transfer_doc DROP CONSTRAINT IF EXISTS inv_transfer_doc_cancelled_by_user_id_fkey;
ALTER TABLE public.inv_transfer_doc DROP CONSTRAINT IF EXISTS inv_transfer_doc_posted_by_user_id_fkey;
ALTER TABLE public.money_reg_balance DROP CONSTRAINT IF EXISTS money_reg_balance_posting_batch_id_fkey;
ALTER TABLE public.org_claim_request DROP CONSTRAINT IF EXISTS org_claim_request_organization_id_fkey;
ALTER TABLE public.org_claim_request DROP CONSTRAINT IF EXISTS org_claim_request_requested_by_user_id_fkey;
ALTER TABLE public.org_claim_request DROP CONSTRAINT IF EXISTS org_claim_request_reviewed_by_user_id_fkey;
ALTER TABLE public.org_defaults DROP CONSTRAINT IF EXISTS org_defaults_bank_account_id_fkey;
ALTER TABLE public.org_defaults DROP CONSTRAINT IF EXISTS org_defaults_bank_accounting_account_id_fkey;
ALTER TABLE public.org_defaults DROP CONSTRAINT IF EXISTS org_defaults_branch_id_fkey;
ALTER TABLE public.org_defaults DROP CONSTRAINT IF EXISTS org_defaults_cash_account_id_fkey;
ALTER TABLE public.org_defaults DROP CONSTRAINT IF EXISTS org_defaults_cash_box_id_fkey;
ALTER TABLE public.org_defaults DROP CONSTRAINT IF EXISTS org_defaults_cogs_account_id_fkey;
ALTER TABLE public.org_defaults DROP CONSTRAINT IF EXISTS org_defaults_expense_account_id_fkey;
ALTER TABLE public.org_defaults DROP CONSTRAINT IF EXISTS org_defaults_inventory_account_id_fkey;
ALTER TABLE public.org_defaults DROP CONSTRAINT IF EXISTS org_defaults_organization_id_fkey;
ALTER TABLE public.org_defaults DROP CONSTRAINT IF EXISTS org_defaults_payable_account_id_fkey;
ALTER TABLE public.org_defaults DROP CONSTRAINT IF EXISTS org_defaults_receivable_account_id_fkey;
ALTER TABLE public.org_defaults DROP CONSTRAINT IF EXISTS org_defaults_revenue_account_id_fkey;
ALTER TABLE public.org_defaults DROP CONSTRAINT IF EXISTS org_defaults_warehouse_id_fkey;
ALTER TABLE public.org_organization_config DROP CONSTRAINT IF EXISTS org_organization_config_accounting_policy_id_fkey;
ALTER TABLE public.org_organization_config DROP CONSTRAINT IF EXISTS org_organization_config_base_currency_id_fkey;
ALTER TABLE public.org_organization DROP CONSTRAINT IF EXISTS org_organization_tenant_id_fkey;
ALTER TABLE public.org_setup_state DROP CONSTRAINT IF EXISTS org_setup_state_organization_id_fkey;
ALTER TABLE public.org_tax_settings DROP CONSTRAINT IF EXISTS org_tax_settings_organization_id_fkey;
ALTER TABLE public.org_tax_settings DROP CONSTRAINT IF EXISTS org_tax_settings_state_id_fkey;
ALTER TABLE public.org_tax_settings DROP CONSTRAINT IF EXISTS org_tax_settings_tax_type_id_fkey;
ALTER TABLE public.org_user_invitation DROP CONSTRAINT IF EXISTS org_user_invitation_accepted_by_user_id_fkey;
ALTER TABLE public.org_user_invitation DROP CONSTRAINT IF EXISTS org_user_invitation_invited_by_user_id_fkey;
ALTER TABLE public.org_user_invitation DROP CONSTRAINT IF EXISTS org_user_invitation_organization_id_fkey;
ALTER TABLE public.org_user_invitation DROP CONSTRAINT IF EXISTS org_user_invitation_role_id_fkey;
ALTER TABLE public.org_user_invitation DROP CONSTRAINT IF EXISTS org_user_invitation_state_id_fkey;
ALTER TABLE public.platform_tenant DROP CONSTRAINT IF EXISTS platform_tenant_owner_user_id_fkey;
ALTER TABLE public.platform_tenant DROP CONSTRAINT IF EXISTS platform_tenant_state_id_fkey;
ALTER TABLE public.pur_doc DROP CONSTRAINT IF EXISTS pur_doc_cancelled_by_user_id_fkey;
ALTER TABLE public.pur_doc DROP CONSTRAINT IF EXISTS pur_doc_posted_by_user_id_fkey;
ALTER TABLE public.sale_doc DROP CONSTRAINT IF EXISTS sale_doc_cancelled_by_user_id_fkey;
ALTER TABLE public.sale_doc DROP CONSTRAINT IF EXISTS sale_doc_posted_by_user_id_fkey;
ALTER TABLE public.sys_email_verification_token DROP CONSTRAINT IF EXISTS sys_email_verification_token_user_id_fkey;
ALTER TABLE public.sys_module DROP CONSTRAINT IF EXISTS sys_module_parent_id_fkey;
ALTER TABLE public.sys_password_reset_token DROP CONSTRAINT IF EXISTS sys_password_reset_token_user_id_fkey;
ALTER TABLE public.sys_refresh_token DROP CONSTRAINT IF EXISTS sys_refresh_token_user_id_fkey;
ALTER TABLE public.sys_user_organization DROP CONSTRAINT IF EXISTS sys_user_organization_invited_by_user_id_fkey;
ALTER TABLE public.acc_chart_account DROP CONSTRAINT IF EXISTS uq_acc_chart_account_code;

-- 3. CREATE missing tables from target scripts
-- source: D:\Projects\backend\accounting_back\src\Infrastructure\Persistence\Scripts\00_sys\0010_create_sys_setting.sql
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
    constraint sys_setting_pkey primary key (id)
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

-- source: D:\Projects\backend\accounting_back\src\Infrastructure\Persistence\Scripts\01_cmn\0125_create_cmn_currency_rate.sql
create table cmn_currency_rate 
(
    id bigint not null,
    base_currency_id smallint not null,
    target_currency_id smallint not null,
    effective_date timestamp without time zone default now() not null,
    buy_rate numeric(18,6) not null,
    sell_rate numeric(18,6) not null,
    official_rate numeric(18,6) not null,
    rate_source character varying(100),
    is_active boolean default true not null,
    state_id smallint not null,
    created_date timestamp without time zone default now() not null,
    constraint ck_cmn_currency_rate_base_target_diff check (base_currency_id <> target_currency_id),
    constraint ck_cmn_currency_rate_rates_positive check (buy_rate > 0 and sell_rate > 0 and official_rate > 0),
    constraint ck_cmn_currency_rate_effective_date check (effective_date is not null),
    constraint cmn_currency_rate_pkey primary key (id),
    constraint cmn_currency_rate_base_currency_id_fkey foreign key (base_currency_id) references cmn_currency(id),
    constraint cmn_currency_rate_target_currency_id_fkey foreign key (target_currency_id) references cmn_currency(id),
    constraint cmn_currency_rate_state_id_fkey foreign key (state_id) references cmn_state(id)
);

create index idx_cmn_currency_rate_pair_effective_date on cmn_currency_rate using btree (base_currency_id, target_currency_id, effective_date desc);
create index idx_cmn_currency_rate_base_currency_id on cmn_currency_rate using btree (base_currency_id);
create index idx_cmn_currency_rate_target_currency_id on cmn_currency_rate using btree (target_currency_id);
create index idx_cmn_currency_rate_effective_date on cmn_currency_rate using btree (effective_date desc);
create index idx_cmn_currency_rate_is_active on cmn_currency_rate using btree (is_active);
create index idx_cmn_currency_rate_state_id on cmn_currency_rate using btree (state_id);

-- source: D:\Projects\backend\accounting_back\src\Infrastructure\Persistence\Scripts\01_cmn\0127_create_cmn_currency_revaluation.sql
create table cmn_currency_revaluation 
(
    id bigint not null,
    organization_id integer not null,
    revaluation_date timestamp without time zone not null,
    provider_rate_date timestamp without time zone,
    status_id smallint not null,
    state_id smallint not null,
    created_date timestamp without time zone default now() not null,
    confirmed_at timestamp without time zone,
    posted_by_user_id integer,
    cancelled_at timestamp without time zone,
    cancelled_by_user_id integer,
    constraint cmn_currency_revaluation_pkey primary key (id),
    constraint cmn_currency_revaluation_organization_id_fkey foreign key (organization_id) references org_organization(id),
    constraint cmn_currency_revaluation_state_id_fkey foreign key (state_id) references cmn_state(id)
);

create index idx_cmn_currency_revaluation_organization_id on cmn_currency_revaluation using btree (organization_id);
create index idx_cmn_currency_revaluation_revaluation_date on cmn_currency_revaluation using btree (revaluation_date);
create index idx_cmn_currency_revaluation_posted_by_user_id on cmn_currency_revaluation using btree (posted_by_user_id);
create index idx_cmn_currency_revaluation_cancelled_by_user_id on cmn_currency_revaluation using btree (cancelled_by_user_id);
create index idx_cmn_currency_revaluation_state_id on cmn_currency_revaluation using btree (state_id);
create index idx_cmn_currency_revaluation_status_id on cmn_currency_revaluation using btree (status_id);

-- source: D:\Projects\backend\accounting_back\src\Infrastructure\Persistence\Scripts\01_cmn\0128_create_cmn_currency_revaluation_line.sql
create table cmn_currency_revaluation_line 
(
    id bigint not null,
    revaluation_id bigint not null,
    base_currency_id smallint not null,
    target_currency_id smallint not null,
    balance_amount numeric(18,2) not null,
    opening_rate numeric(18,6) not null,
    current_rate numeric(18,6) not null,
    difference_amount numeric(18,2) not null,
    state_id smallint not null,
    created_date timestamp without time zone default now() not null,
    constraint cmn_currency_revaluation_line_pkey primary key (id),
    constraint cmn_currency_revaluation_line_revaluation_id_fkey foreign key (revaluation_id) references cmn_currency_revaluation(id),
    constraint cmn_currency_revaluation_line_base_currency_id_fkey foreign key (base_currency_id) references cmn_currency(id),
    constraint cmn_currency_revaluation_line_target_currency_id_fkey foreign key (target_currency_id) references cmn_currency(id),
    constraint cmn_currency_revaluation_line_state_id_fkey foreign key (state_id) references cmn_state(id)
);

create index idx_cmn_currency_revaluation_line_revaluation_id on cmn_currency_revaluation_line using btree (revaluation_id);
create index idx_cmn_currency_revaluation_line_target_currency_id on cmn_currency_revaluation_line using btree (target_currency_id);

-- source: D:\Projects\backend\accounting_back\src\Infrastructure\Persistence\Scripts\13_notification\1301_create_cmn_notification_type.sql
create table cmn_notification_type 
(
    id smallint   not null,
    code character varying(50) not null,
    name character varying(150) not null,
    state_id smallint not null,
    created_date timestamp without time zone default now() not null,
    constraint cmn_notification_type_pkey primary key (id)
);

create unique index idx_cmn_notification_type_code on cmn_notification_type using btree (code);
create index idx_cmn_notification_type_state_id on cmn_notification_type using btree (state_id);

insert into cmn_notification_type (id, code, name, state_id, created_date) values
    ('1', 'info', 'Ma''lumot', '1', now()),
    ('2', 'success', 'Muvaffaqiyat', '1', now()),
    ('3', 'warning', 'Ogohlantirish', '1', now()),
    ('4', 'error', 'Xato', '1', now()),
    ('5', 'doc_approved', 'Hujjat tasdiqlandi', '1', now()),
    ('6', 'payment_due', 'To''lov muddati', '1', now());

-- source: D:\Projects\backend\accounting_back\src\Infrastructure\Persistence\Scripts\13_notification\1302_create_sys_notification.sql
create table sys_notification
(
    id bigint   not null,
    organization_id integer,
    user_id integer,
    type_id smallint not null,
    title character varying(300) not null,
    body text not null,
    link character varying(500),
    entity_type character varying(100),
    entity_id bigint,
    state_id smallint default 1 not null,
    created_date timestamp without time zone default now() not null,
    constraint sys_notification_pkey primary key (id),
    constraint sys_notification_type_id_key check (type_id > 0)
);

create index idx_sys_notification_organization_id on sys_notification using btree (organization_id);
create index idx_sys_notification_user_created_date on sys_notification using btree (user_id, created_date desc);
create index idx_sys_notification_type_id on sys_notification using btree (type_id);

-- source: D:\Projects\backend\accounting_back\src\Infrastructure\Persistence\Scripts\13_notification\1303_create_sys_notification_delivery.sql
create table sys_notification_delivery 
(
    id bigint   not null,
    notification_id bigint not null,
    channel smallint not null,
    status smallint default 0 not null,
    error character varying(1000),
    sent_at timestamp without time zone,
    created_date timestamp without time zone default now() not null,
    constraint sys_notification_delivery_pkey primary key (id)
);

create index idx_sys_notification_delivery_notification_id on sys_notification_delivery using btree (notification_id);
create index idx_sys_notification_delivery_status on sys_notification_delivery using btree (status);

-- source: D:\Projects\backend\accounting_back\src\Infrastructure\Persistence\Scripts\13_notification\1304_create_sys_notification_read.sql
create table sys_notification_read 
(
    id bigint   not null,
    notification_id bigint not null,
    user_id integer not null,
    read_at timestamp without time zone default now() not null,
    created_date timestamp without time zone default now() not null,
    constraint sys_notification_read_pkey primary key (id),
    constraint sys_notification_read_notification_id_user_id_key unique (notification_id, user_id)
);

create index idx_sys_notification_read_user_id_notification_id on sys_notification_read using btree (user_id, notification_id);

-- 4. ALTER existing columns to match target scripts
ALTER TABLE public.acc_account_type ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.acc_account_type ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.acc_accounting_period ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.acc_accounting_policy ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.acc_accounting_policy ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.acc_chart_account ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.acc_chart_account ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.acc_chart_account_subkonto ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.acc_chart_account_subkonto ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.acc_posting_alias ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.acc_posting_alias ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.acc_posting_batch ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.acc_posting_rule ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.acc_posting_rule ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.acc_posting_rule_line ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.acc_posting_rule_line ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.acc_reg_entry ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.acc_reg_entry ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.acc_reg_entry_subkonto ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.acc_reg_entry_subkonto ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.acc_subkonto_type ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.acc_subkonto_type ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.bank_operation ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.bank_operation ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.bank_operation ALTER COLUMN payment_purpose_id SET DEFAULT 1;
ALTER TABLE public.cash_box ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.cash_box ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.cash_operation ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.cash_operation ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.cmn_bank ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.cmn_bank ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.cmn_contract ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.cmn_contract ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.cmn_contract_type ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.cmn_contract_type ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.cmn_costing_method ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.cmn_costing_method ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.cmn_counterparty_type ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.cmn_counterparty_type ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.cmn_currency ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.cmn_currency ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.cmn_district ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.cmn_district ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.cmn_document_sequence ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.cmn_document_status ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.cmn_document_status ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.cmn_document_type ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.cmn_document_type ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.cmn_language ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.cmn_language ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.cmn_operation_type ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.cmn_operation_type ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.cmn_payment_type ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.cmn_payment_type ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.cmn_price_rounding_method ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.cmn_price_rounding_method ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.cmn_pricing_condition ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.cmn_pricing_condition ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.cmn_product_price_type ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.cmn_product_price_type ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.cmn_product_table_status ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.cmn_product_table_status ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.cmn_region ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.cmn_region ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.cmn_tax_type ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.cmn_tax_type ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.cmn_translation ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.cmn_translation ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.cmn_unit ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.cmn_unit ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.cmn_vat_rate ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.cmn_vat_rate ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.counterparty_bank_account ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.counterparty_bank_account ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.counterparty_card ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.counterparty_card ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.counterparty_contact ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.counterparty_contact ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.counterparty_reg_balance ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.counterparty_reg_balance ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.inv_product ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.inv_product ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.inv_product_group ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.inv_product_group ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.inv_product_price ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.inv_product_price ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.inv_product_table ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.inv_product_table ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.inv_reg_balance ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.inv_reg_balance ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.inv_warehouse ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.inv_warehouse ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.money_reg_balance ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.money_reg_balance ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.org_bank_account ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.org_bank_account ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.org_branch ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.org_branch ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.org_claim_request ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.org_defaults ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.org_department ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.org_department ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.org_organization ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.org_organization ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.org_position ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.org_position ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.org_setup_state ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.org_tax_settings ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.org_user_invitation ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.platform_tenant ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.pur_doc ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.pur_doc ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.pur_doc_product ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.pur_doc_product ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.pur_doc_table ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.pur_doc_table ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.sale_condition ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.sale_condition ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.sale_doc ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.sale_doc ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.sale_doc_product ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.sale_doc_product ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.sale_doc_table ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.sale_doc_table ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.sys_audit_log ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.sys_audit_log ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.sys_email_verification_token ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.sys_module ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.sys_module ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.sys_module_sub_group ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.sys_module_sub_group ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.sys_password_reset_token ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.sys_refresh_token ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.sys_role ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.sys_role ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.sys_user ALTER COLUMN id DROP IDENTITY IF EXISTS;
ALTER TABLE public.sys_user ALTER COLUMN id DROP DEFAULT;

-- 5. ADD missing constraints
ALTER TABLE public.bank_operation ADD CONSTRAINT bank_operation_cancelled_at_check CHECK (cancelled_by_user_id is null or cancelled_at is not null);
ALTER TABLE public.cash_operation ADD CONSTRAINT cash_operation_cancelled_at_check CHECK (cancelled_by_user_id is null or cancelled_at is not null);
ALTER TABLE public.acc_chart_account ADD CONSTRAINT acc_chart_account_parent_id_fkey FOREIGN KEY (parent_id) REFERENCES acc_chart_account(id) DEFERRABLE INITIALLY DEFERRED;
ALTER TABLE public.sys_module ADD CONSTRAINT sys_module_parent_id_fkey FOREIGN KEY (parent_id) REFERENCES sys_module(id) DEFERRABLE INITIALLY DEFERRED;

-- 6. ADD missing indexes
CREATE INDEX idx_cmn_contract_contract_type_id ON public.cmn_contract using btree (contract_type_id);
CREATE UNIQUE INDEX ux_acc_posting_batch_document_posted ON public.acc_posting_batch using btree (document_type_id, document_id) WHERE (status = 'POSTED');
CREATE UNIQUE INDEX ux_acc_posting_batch_document_reversal ON public.acc_posting_batch using btree (document_type_id, document_id) WHERE (status = 'REVERSAL');
CREATE UNIQUE INDEX ux_sale_doc_table_owner_product_table ON public.sale_doc_table using btree (owner_id, product_table_id);
COMMIT;
