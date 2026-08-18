begin;

with group_translations
(
    product_group_code,
    name_uz,
    name_ru,
    name_en
) as
(
    values
        -- Asosiy guruh
        (
            'HOME_APPLIANCES',
            'Maishiy texnika',
            'Бытовая техника',
            'Home appliances'
        ),

        -- Oraliq guruhlar
        (
            'REFRIGERATION_EQUIPMENT',
            'Sovutish texnikasi',
            'Холодильная техника',
            'Refrigeration appliances'
        ),
        (
            'LAUNDRY_EQUIPMENT',
            'Kir yuvish texnikasi',
            'Стиральная техника',
            'Laundry appliances'
        ),
        (
            'CLIMATE_EQUIPMENT',
            'Iqlim texnikasi',
            'Климатическая техника',
            'Climate control appliances'
        ),
        (
            'LARGE_KITCHEN_APPLIANCES',
            'Yirik oshxona texnikasi',
            'Крупная кухонная техника',
            'Major kitchen appliances'
        ),
        (
            'CLEANING_EQUIPMENT',
            'Tozalash texnikasi',
            'Техника для уборки',
            'Cleaning appliances'
        ),
        (
            'WATER_HEATING_EQUIPMENT',
            'Suv isitish texnikasi',
            'Водонагревательная техника',
            'Water heating appliances'
        ),
        (
            'SMALL_KITCHEN_APPLIANCES',
            'Kichik oshxona texnikasi',
            'Мелкая кухонная техника',
            'Small kitchen appliances'
        ),
        (
            'GARMENT_AND_PERSONAL_CARE',
            'Kiyim va shaxsiy parvarish texnikasi',
            'Техника для ухода за одеждой и собой',
            'Garment and personal care appliances'
        ),

        -- Televizorlar
        (
            'TELEVISIONS',
            'Televizorlar',
            'Телевизоры',
            'Televisions'
        ),

        -- Sovutish texnikasi
        (
            'HOUSEHOLD_REFRIGERATORS',
            'Maishiy sovutgichlar',
            'Бытовые холодильники',
            'Household refrigerators'
        ),
        (
            'COMMERCIAL_REFRIGERATORS',
            'Savdo sovutgichlari',
            'Торговые холодильники',
            'Commercial refrigerators'
        ),
        (
            'FREEZERS',
            'Muzlatgichlar',
            'Морозильники',
            'Freezers'
        ),

        -- Kir yuvish texnikasi
        (
            'AUTOMATIC_WASHING_MACHINES',
            'Avtomatik kir yuvish mashinalari',
            'Автоматические стиральные машины',
            'Automatic washing machines'
        ),
        (
            'SEMI_AUTOMATIC_WASHING_MACHINES',
            'Yarim avtomatik kir yuvish mashinalari',
            'Полуавтоматические стиральные машины',
            'Semi-automatic washing machines'
        ),

        -- Iqlim texnikasi
        (
            'AIR_CONDITIONERS',
            'Konditsionerlar',
            'Кондиционеры',
            'Air conditioners'
        ),
        (
            'AIR_PURIFIERS',
            'Havo tozalagichlar',
            'Очистители воздуха',
            'Air purifiers'
        ),
        (
            'AIR_HUMIDIFIERS',
            'Havo namlagichlar',
            'Увлажнители воздуха',
            'Air humidifiers'
        ),
        (
            'HEATERS',
            'Isitgichlar',
            'Обогреватели',
            'Heaters'
        ),
        (
            'FANS',
            'Ventilyatorlar',
            'Вентиляторы',
            'Fans'
        ),

        -- Yirik oshxona texnikasi
        (
            'STOVES',
            'Plitalar',
            'Плиты',
            'Stoves'
        ),
        (
            'COOKTOPS',
            'Pishirish panellari',
            'Варочные панели',
            'Cooktops'
        ),
        (
            'RANGE_HOODS',
            'Oshxona havo tortgichlari',
            'Кухонные вытяжки',
            'Range hoods'
        ),
        (
            'ELECTRIC_OVENS',
            'Elektr pechlar',
            'Электродуховки',
            'Electric ovens'
        ),
        (
            'BUILT_IN_OVENS',
            'O‘rnatiladigan duxovkalar',
            'Встраиваемые духовые шкафы',
            'Built-in ovens'
        ),
        (
            'MICROWAVE_OVENS',
            'Mikroto‘lqinli pechlar',
            'Микроволновые печи',
            'Microwave ovens'
        ),

        -- Tozalash texnikasi
        (
            'VACUUM_CLEANERS',
            'Changyutgichlar',
            'Пылесосы',
            'Vacuum cleaners'
        ),

        -- Suv isitish texnikasi
        (
            'WATER_HEATERS',
            'Suv isitgichlar',
            'Водонагреватели',
            'Water heaters'
        ),
        (
            'WATER_COOLERS',
            'Suv dispenserlari',
            'Кулеры для воды',
            'Water dispensers'
        ),

        -- Kichik oshxona texnikasi
        (
            'MEAT_GRINDERS',
            'Go‘sht maydalagichlar',
            'Мясорубки',
            'Meat grinders'
        ),
        (
            'JUICERS',
            'Sharbat chiqargichlar',
            'Соковыжималки',
            'Juicers'
        ),
        (
            'ELECTRIC_KETTLES',
            'Elektr choynaklar',
            'Электрочайники',
            'Electric kettles'
        ),
        (
            'THERMOPOTS',
            'Termopotlar',
            'Термопоты',
            'Thermopots'
        ),
        (
            'CHOPPERS',
            'Maydalagichlar',
            'Измельчители',
            'Choppers'
        ),
        (
            'MIXERS',
            'Mikserlar',
            'Миксеры',
            'Mixers'
        ),
        (
            'BLENDERS',
            'Blenderlar',
            'Блендеры',
            'Blenders'
        ),

        -- Kiyim va shaxsiy parvarish
        (
            'IRONS',
            'Dazmollar',
            'Утюги',
            'Irons'
        ),
        (
            'GARMENT_STEAMERS',
            'Kiyim bug‘lagichlar',
            'Отпариватели для одежды',
            'Garment steamers'
        ),
        (
            'HAIR_DRYERS',
            'Soch quritgichlar',
            'Фены',
            'Hair dryers'
        )
),
translations as
(
    select
        source.product_group_code,
        translation.language_code,
        translation.name
    from group_translations source
    cross join lateral
    (
        values
            ('uz', source.name_uz),
            ('ru', source.name_ru),
            ('en', source.name_en)
    ) translation(language_code, name)
)
insert into inv_product_group_translation
(
    product_group_id,
    language_id,
    name
)
select
    product_group.id,
    language.id,
    translation.name
from translations translation
join inv_product_group product_group
    on product_group.code = translation.product_group_code
join cmn_language language
    on language.code = translation.language_code
on conflict (product_group_id, language_id)
do update set
    name = excluded.name;

commit;


begin;

with group_translations
(
    product_group_code,
    name_uz,
    name_ru,
    name_en
) as
(
    values
        -- Xizmatlar
        (
            'SERVICES',
            'Xizmatlar',
            'Услуги',
            'Services'
        ),

        -- Dasturiy ta''minot xizmatlari
        (
            'SOFTWARE_SERVICES',
            'Dasturiy ta''minot xizmatlari',
            'Услуги по разработке программного обеспечения',
            'Software services'
        ),

        -- Bank xizmatlari
        (
            'BANK_SERVICES',
            'Bank xizmatlari',
            'Банковские услуги',
            'Banking services'
        ),

        -- Internet xizmatlari
        (
            'INTERNET_SERVICES',
            'Internet xizmatlari',
            'Интернет-услуги',
            'Internet services'
        ),

        -- Elektron hujjat aylanishi xizmatlari
        (
            'E_DOCUMENT_SERVICES',
            'Elektron hujjat aylanishi xizmatlari',
            'Услуги электронного документооборота',
            'Electronic document management services'
        )
),
translations as
(
    select
        source.product_group_code,
        translation.language_code,
        translation.name
    from group_translations source
    cross join lateral
    (
        values
            ('uz', source.name_uz),
            ('ru', source.name_ru),
            ('en', source.name_en)
    ) translation(language_code, name)
)
insert into inv_product_group_translation
(
    product_group_id,
    language_id,
    name
)
select
    product_group.id,
    language.id,
    translation.name
from translations translation
join inv_product_group product_group
    on product_group.code = translation.product_group_code
join cmn_language language
    on language.code = translation.language_code
on conflict (product_group_id, language_id)
do update set
    name = excluded.name;

commit;