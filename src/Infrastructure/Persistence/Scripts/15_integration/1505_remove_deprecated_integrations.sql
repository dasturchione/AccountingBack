-- Remove persistence objects owned exclusively by deprecated external providers.
-- The statements are idempotent so the script is safe for environments at different schema levels.

drop table if exists inv_product_table_didox_origin;
drop table if exists inv_product_didox_profile;
drop table if exists counterparty_didox_profile;
drop table if exists cmn_unit_didox_package;
drop table if exists int_eimzo_challenge;
drop table if exists int_provider_session;
drop table if exists int_provider_operation;
drop table if exists int_provider_credential;
drop table if exists cmn_didox_origin;
drop table if exists cmn_didox_vat_reg_status;
drop table if exists cmn_mxik_catalog;
