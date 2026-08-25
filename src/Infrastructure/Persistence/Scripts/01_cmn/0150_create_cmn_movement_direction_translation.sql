create table if not exists cmn_movement_direction_translation
(
    movement_direction_id smallint not null
        references cmn_movement_direction(id),

    language_id smallint not null
        references cmn_language(id),

    name character varying(150) not null,

    constraint cmn_movement_direction_translation_pkey
        primary key (movement_direction_id, language_id)
);

create index if not exists ix_cmn_movement_direction_translation_language_id
    on cmn_movement_direction_translation (language_id);

insert into cmn_movement_direction_translation
    (movement_direction_id, language_id, name)
select
    direction.id,
    language.id,
    translation.name
from
(
    values
        ('IN',  'uz', 'Kirim'),
        ('OUT', 'uz', 'Chiqim'),

        ('IN',  'ru', 'Поступление / приход'),
        ('OUT', 'ru', 'Выбытие / расход'),

        ('IN',  'en', 'Inflow / receipt'),
        ('OUT', 'en', 'Outflow / issue')
) as translation(direction_code, language_code, name)
join cmn_movement_direction as direction
    on direction.code = translation.direction_code
join cmn_language as language
    on language.code = translation.language_code
on conflict (movement_direction_id, language_id) do update set
    name = excluded.name;
