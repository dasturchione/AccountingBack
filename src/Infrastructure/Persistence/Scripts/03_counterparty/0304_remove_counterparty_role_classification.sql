alter table counterparty_card
    drop constraint if exists counterparty_card_counterparty_type_id_fkey;

drop index if exists idx_counterparty_card_type_id;

alter table counterparty_card
    drop column if exists counterparty_type_id,
    drop column if exists is_customer,
    drop column if exists is_supplier;

drop table if exists cmn_counterparty_type_translation;
drop table if exists cmn_counterparty_type;

delete from cmn_translation
where table_name = 'cmn_counterparty_type';

delete from sys_role_module
where module_id in
(
    select id
    from sys_module
    where code in
    (
        'MANUAL_GET_COUNTERPARTY_TYPES',
        'MANUAL_GET_SUPPLIERS',
        'MANUAL_GET_CLIENTS'
    )
);

delete from sys_module
where code in
(
    'MANUAL_GET_COUNTERPARTY_TYPES',
    'MANUAL_GET_SUPPLIERS',
    'MANUAL_GET_CLIENTS'
);
