create table cmn_costing_method_translation
(
    costing_method_id smallint not null references cmn_costing_method(id),
    language_id       smallint not null references cmn_language(id),
    name              varchar(100) not null,

    primary key (costing_method_id, language_id)
);

create index ix_cmn_costing_method_translation_language_id
    on cmn_costing_method_translation (language_id);


insert into cmn_costing_method_translation
    (costing_method_id, language_id, name)
select
    costing_method.id,
    language.id,
    translation.name
from
(
    values
        ('FIFO',    'uz', 'FIFO - birinchi kirgan birinchi chiqadi'),
        ('LIFO',    'uz', 'LIFO - oxirgi kirgan birinchi chiqadi'),
        ('AVERAGE', 'uz', 'O''rtacha tannarx'),

        ('FIFO',    'ru', 'FIFO - первым поступил, первым выбыл'),
        ('LIFO',    'ru', 'LIFO - последним поступил, первым выбыл'),
        ('AVERAGE', 'ru', 'Средняя себестоимость'),

        ('FIFO',    'en', 'FIFO - first in, first out'),
        ('LIFO',    'en', 'LIFO - last in, first out'),
        ('AVERAGE', 'en', 'Average cost')
) as translation(costing_method_code, language_code, name)
join cmn_costing_method as costing_method
    on costing_method.code = translation.costing_method_code
join cmn_language as language
    on language.code = translation.language_code
on conflict (costing_method_id, language_id)
do update set
    name = excluded.name;
