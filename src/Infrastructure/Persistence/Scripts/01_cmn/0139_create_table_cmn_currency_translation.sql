create table cmn_currency_translation
(
    currency_id smallint not null
        references cmn_currency(id),

    language_id smallint not null
        references cmn_language(id),

    name varchar(100) not null,

    primary key (currency_id, language_id)
);

-- PK уже создаёт индекс по (currency_id, language_id)
create index ix_cmn_currency_translation_language_id
    on cmn_currency_translation (language_id);


insert into cmn_currency_translation
    (currency_id, language_id, name)
select
    currency.id,
    language.id,
    translation.name
from
(
    values
        ('UZS', 'uz', 'O''zbek so''mi'),
        ('USD', 'uz', 'AQSh dollari'),
        ('RUB', 'uz', 'Rossiya rubli'),
        ('EUR', 'uz', 'Yevro'),

        ('UZS', 'ru', 'Узбекский сум'),
        ('USD', 'ru', 'Доллар США'),
        ('RUB', 'ru', 'Российский рубль'),
        ('EUR', 'ru', 'Евро'),

        ('UZS', 'en', 'Uzbekistani som'),
        ('USD', 'en', 'US dollar'),
        ('RUB', 'en', 'Russian ruble'),
        ('EUR', 'en', 'Euro')
) as translation(currency_code, language_code, name)
join cmn_currency as currency
    on currency.code = translation.currency_code
join cmn_language as language
    on language.code = translation.language_code
on conflict (currency_id, language_id)
do update set
    name = excluded.name;
