create table rnt_rental_object_type
(
    id            smallserial primary key,
    code          varchar(50) not null unique,
    state_id      smallint not null default 1 references cmn_state(id),
    created_date  timestamp without time zone not null default now()
);

create table rnt_rental_object_type_translation
(
    rental_object_type_id  smallint not null references rnt_rental_object_type(id),
    language_id            smallint not null references cmn_language(id),
    name                   varchar(250) not null,

    primary key (rental_object_type_id, language_id)
);

create index ix_rnt_rental_object_type_translation_language_id
    on rnt_rental_object_type_translation (language_id);

insert into rnt_rental_object_type(code, state_id)
values
    ('REAL_ESTATE', 1),
    ('VEHICLE', 1),
    ('EQUIPMENT', 1),
    ('OTHER', 1)
on conflict (code) do update set
    state_id = excluded.state_id;

insert into rnt_rental_object_type_translation(rental_object_type_id, language_id, name)
select object_type.id, language.id, translation.name
from
(
    values
        ('REAL_ESTATE', 'uz', 'Ko''chmas mulk'),
        ('VEHICLE', 'uz', 'Transport'),
        ('EQUIPMENT', 'uz', 'Uskuna'),
        ('OTHER', 'uz', 'Boshqa'),
        ('REAL_ESTATE', 'uz_Cyrl', 'Кўчмас мулк'),
        ('VEHICLE', 'uz_Cyrl', 'Транспорт'),
        ('EQUIPMENT', 'uz_Cyrl', 'Ускуна'),
        ('OTHER', 'uz_Cyrl', 'Бошқа'),
        ('REAL_ESTATE', 'ru', 'Недвижимость'),
        ('VEHICLE', 'ru', 'Транспорт'),
        ('EQUIPMENT', 'ru', 'Оборудование'),
        ('OTHER', 'ru', 'Другое'),
        ('REAL_ESTATE', 'en', 'Real estate'),
        ('VEHICLE', 'en', 'Vehicle'),
        ('EQUIPMENT', 'en', 'Equipment'),
        ('OTHER', 'en', 'Other')
) as translation(object_type_code, language_code, name)
join rnt_rental_object_type object_type on object_type.code = translation.object_type_code
join cmn_language language on language.code = translation.language_code
on conflict (rental_object_type_id, language_id) do update set
    name = excluded.name;
