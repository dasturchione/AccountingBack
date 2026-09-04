begin;

create table cmn_utility_service
(
    id              smallserial primary key,
    code            varchar(50) not null unique,
    state_id        smallint not null default 1 references cmn_state(id),
    created_date    timestamp without time zone not null default now()
);

create table cmn_utility_service_translation
(
    utility_service_id    smallint not null references cmn_utility_service(id) on delete cascade,
    language_id           smallint not null references cmn_language(id),
    name                  varchar(250) not null,

    primary key (utility_service_id, language_id)
);

create index ix_cmn_utility_service_translation_language_id
    on cmn_utility_service_translation (language_id);

insert into cmn_utility_service (code, state_id)
values
    ('NATURAL_GAS', 1),
    ('HOT_WATER', 1),
    ('COLD_WATER', 1),
    ('ELECTRICITY', 1)
on conflict (code) do update
set state_id = excluded.state_id;

insert into cmn_utility_service_translation
    (utility_service_id, language_id, name)
select utility_service.id,
       language.id,
       source.name
from
(
    values
        ('NATURAL_GAS', 'uz', 'Tabiiy gaz'),
        ('NATURAL_GAS', 'uz_Cyrl', 'Табиий газ'),
        ('NATURAL_GAS', 'ru', 'Природный газ'),
        ('NATURAL_GAS', 'en', 'Natural gas'),
        ('HOT_WATER', 'uz', 'Issiq suv'),
        ('HOT_WATER', 'uz_Cyrl', 'Иссиқ сув'),
        ('HOT_WATER', 'ru', 'Горячая вода'),
        ('HOT_WATER', 'en', 'Hot water'),
        ('COLD_WATER', 'uz', 'Sovuq suv'),
        ('COLD_WATER', 'uz_Cyrl', 'Совуқ сув'),
        ('COLD_WATER', 'ru', 'Холодная вода'),
        ('COLD_WATER', 'en', 'Cold water'),
        ('ELECTRICITY', 'uz', 'Elektr energiyasi'),
        ('ELECTRICITY', 'uz_Cyrl', 'Электр энергияси'),
        ('ELECTRICITY', 'ru', 'Электроэнергия'),
        ('ELECTRICITY', 'en', 'Electricity')
) as source(utility_service_code, language_code, name)
join cmn_utility_service utility_service
    on utility_service.code = source.utility_service_code
join cmn_language language
    on language.code = source.language_code
on conflict (utility_service_id, language_id) do update
set name = excluded.name;

commit;
