create table cmn_contract_type_translation
(
    contract_type_id smallint not null references cmn_contract_type(id),
    language_id      smallint not null references cmn_language(id),
    name             varchar(150) not null,

    primary key (contract_type_id, language_id)
);

create index ix_cmn_contract_type_translation_language_id
    on cmn_contract_type_translation (language_id);


insert into cmn_contract_type_translation
    (contract_type_id, language_id, name)
select
    contract_type.id,
    language.id,
    translation.name
from
(
    values
        ('supplier', 'uz', 'Yetkazib beruvchi bilan shartnoma'),
        ('customer', 'uz', 'Xaridor bilan shartnoma'),

        ('supplier', 'ru', 'Договор с поставщиком'),
        ('customer', 'ru', 'Договор с покупателем'),

        ('supplier', 'en', 'Supplier contract'),
        ('customer', 'en', 'Customer contract')
) as translation(contract_type_code, language_code, name)
join cmn_contract_type as contract_type
    on contract_type.code = translation.contract_type_code
join cmn_language as language
    on language.code = translation.language_code
on conflict (contract_type_id, language_id)
do update set
    name = excluded.name;
