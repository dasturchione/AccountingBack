update acc_subkonto_type
set code = 'regulated_obligations',
    name = 'Tartibga solinadigan majburiyatlar',
    source_table = 'cmn_regulated_obligation'
where id = 29;

drop table if exists org_tax_settings;
drop table if exists cmn_tax_type;
