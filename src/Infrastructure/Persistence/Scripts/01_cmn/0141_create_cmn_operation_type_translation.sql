create table cmn_operation_type_translation
(
    operation_type_id smallint not null
        references cmn_operation_type(id),

    language_id smallint not null
        references cmn_language(id),

    name varchar(150) not null,

    primary key (operation_type_id, language_id)
);

create index ix_cmn_operation_type_translation_language_id
    on cmn_operation_type_translation (language_id);


insert into cmn_operation_type_translation
    (operation_type_id, language_id, name)
select
    operation_type.id,
    language.id,
    translation.name
from
(
    values
        ('in',            'uz', 'Kirim'),
        ('out',           'uz', 'Chiqim'),
        ('transfer',      'uz', 'O''tkazma'),
        ('debt_increase', 'uz', 'Qarz oshishi'),
        ('debt_decrease', 'uz', 'Qarz kamayishi'),

        ('in',            'ru', 'Приход'),
        ('out',           'ru', 'Расход'),
        ('transfer',      'ru', 'Перевод'),
        ('debt_increase', 'ru', 'Увеличение задолженности'),
        ('debt_decrease', 'ru', 'Уменьшение задолженности'),

        ('in',            'en', 'Inflow'),
        ('out',           'en', 'Outflow'),
        ('transfer',      'en', 'Transfer'),
        ('debt_increase', 'en', 'Debt increase'),
        ('debt_decrease', 'en', 'Debt decrease')
) as translation(operation_type_code, language_code, name)
join cmn_operation_type as operation_type
    on operation_type.code = translation.operation_type_code
join cmn_language as language
    on language.code = translation.language_code
on conflict (operation_type_id, language_id)
do update set
    name = excluded.name;
