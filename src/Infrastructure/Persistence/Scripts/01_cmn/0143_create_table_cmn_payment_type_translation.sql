create table cmn_payment_type_translation
(
    payment_type_id smallint not null
        references cmn_payment_type(id),

    language_id smallint not null
        references cmn_language(id),

    name varchar(100) not null,

    primary key (payment_type_id, language_id)
);

create index ix_cmn_payment_type_translation_language_id
    on cmn_payment_type_translation (language_id);


insert into cmn_payment_type_translation
    (payment_type_id, language_id, name)
select
    payment_type.id,
    language.id,
    translation.name
from
(
    values
        ('cash',     'uz', 'Naqd'),
        ('bank',     'uz', 'Bank'),
        ('card',     'uz', 'Karta'),
        ('transfer', 'uz', 'O''tkazma'),

        ('cash',     'ru', 'Наличные'),
        ('bank',     'ru', 'Банк'),
        ('card',     'ru', 'Карта'),
        ('transfer', 'ru', 'Перевод'),

        ('cash',     'en', 'Cash'),
        ('bank',     'en', 'Bank'),
        ('card',     'en', 'Card'),
        ('transfer', 'en', 'Transfer')
) as translation(payment_type_code, language_code, name)
join cmn_payment_type as payment_type
    on payment_type.code = translation.payment_type_code
join cmn_language as language
    on language.code = translation.language_code
on conflict (payment_type_id, language_id)
do update set
    name = excluded.name;
