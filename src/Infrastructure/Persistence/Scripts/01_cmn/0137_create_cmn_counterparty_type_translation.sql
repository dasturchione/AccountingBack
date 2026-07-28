create table cmn_counterparty_type_translation
(
    counterparty_type_id smallint not null
        references cmn_counterparty_type(id),

    language_id smallint not null
        references cmn_language(id),

    name varchar(100) not null,

    primary key (counterparty_type_id, language_id)
);

create index ix_cmn_counterparty_type_translation_language_id
    on cmn_counterparty_type_translation (language_id);


insert into cmn_counterparty_type_translation
    (counterparty_type_id, language_id, name)
select
    counterparty_type.id,
    language.id,
    translation.name
from
(
    values
        ('client',          'uz', 'Mijoz'),
        ('supplier',        'uz', 'Yetkazib beruvchi'),
        ('client_supplier', 'uz', 'Mijoz va yetkazib beruvchi'),

        ('client',          'ru', 'Клиент'),
        ('supplier',        'ru', 'Поставщик'),
        ('client_supplier', 'ru', 'Клиент и поставщик'),

        ('client',          'en', 'Client'),
        ('supplier',        'en', 'Supplier'),
        ('client_supplier', 'en', 'Client and supplier')
) as translation(counterparty_type_code, language_code, name)
join cmn_counterparty_type as counterparty_type
    on counterparty_type.code = translation.counterparty_type_code
join cmn_language as language
    on language.code = translation.language_code
on conflict (counterparty_type_id, language_id)
do update set
    name = excluded.name;
