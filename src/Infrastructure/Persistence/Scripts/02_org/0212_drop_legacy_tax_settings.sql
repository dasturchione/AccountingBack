update acc_subkonto_type
set code = 'regulated_obligations',
    name = 'Tartibga solinadigan majburiyatlar',
    source_table = 'cmn_regulated_obligation'
where id = 29;

update org_setup_state
set current_step = 'accounting-policy',
    updated_date = now()
where current_step = 'tax-settings';

alter table org_setup_state
    drop column if exists tax_completed;

drop table if exists org_tax_settings;
drop table if exists cmn_tax_type;
