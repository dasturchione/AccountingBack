begin;

-- =========================================================
-- 1. Корневая группа
-- =========================================================

insert into inv_product_group
(
    code,
    name,
    state_id,
    sort_order,
    is_assignable,
    parent_id
)
values
(
    'HOME_APPLIANCES',
    'Maishiy texnika',
    1,
    10,
    false,
    null
)
on conflict (code)
do update set
    name       = excluded.name,
    state_id   = excluded.state_id,
    sort_order = excluded.sort_order,
    is_assignable = excluded.is_assignable,
    parent_id  = excluded.parent_id;


-- =========================================================
-- 2. Промежуточные группы
-- =========================================================

insert into inv_product_group
(
    code,
    name,
    state_id,
    sort_order,
    is_assignable,
    parent_id
)
values
    (
        'REFRIGERATION_EQUIPMENT',
        'Sovutish texnikasi',
        1,
        10,
        false,
        (select id from inv_product_group where code = 'HOME_APPLIANCES')
    ),
    (
        'LAUNDRY_EQUIPMENT',
        'Kir yuvish texnikasi',
        1,
        20,
        false,
        (select id from inv_product_group where code = 'HOME_APPLIANCES')
    ),
    (
        'CLIMATE_EQUIPMENT',
        'Iqlim texnikasi',
        1,
        30,
        false,
        (select id from inv_product_group where code = 'HOME_APPLIANCES')
    ),
    (
        'LARGE_KITCHEN_APPLIANCES',
        'Yirik oshxona texnikasi',
        1,
        40,
        false,
        (select id from inv_product_group where code = 'HOME_APPLIANCES')
    ),
    (
        'CLEANING_EQUIPMENT',
        'Tozalash texnikasi',
        1,
        50,
        false,
        (select id from inv_product_group where code = 'HOME_APPLIANCES')
    ),
    (
        'WATER_HEATING_EQUIPMENT',
        'Suv isitish texnikasi',
        1,
        60,
        false,
        (select id from inv_product_group where code = 'HOME_APPLIANCES')
    ),
    (
        'SMALL_KITCHEN_APPLIANCES',
        'Kichik oshxona texnikasi',
        1,
        70,
        false,
        (select id from inv_product_group where code = 'HOME_APPLIANCES')
    ),
    (
        'GARMENT_AND_PERSONAL_CARE',
        'Kiyim va shaxsiy parvarish texnikasi',
        1,
        80,
        false,
        (select id from inv_product_group where code = 'HOME_APPLIANCES')
    )
on conflict (code)
do update set
    name       = excluded.name,
    state_id   = excluded.state_id,
    sort_order = excluded.sort_order,
    is_assignable = excluded.is_assignable,
    parent_id  = excluded.parent_id;


-- =========================================================
-- 3. Конечные группы
-- =========================================================

insert into inv_product_group
(
    code,
    name,
    state_id,
    sort_order,
    is_assignable,
    parent_id
)
values
    -- Maishiy texnika
    (
        'TELEVISIONS',
        'Televizorlar',
        1,
        10,
        true,
        (select id from inv_product_group where code = 'HOME_APPLIANCES')
    ),

    -- Sovutish texnikasi
    (
        'HOUSEHOLD_REFRIGERATORS',
        'Maishiy sovutgichlar',
        1,
        10,
        true,
        (select id from inv_product_group where code = 'REFRIGERATION_EQUIPMENT')
    ),
    (
        'COMMERCIAL_REFRIGERATORS',
        'Savdo sovutgichlari',
        1,
        20,
        true,
        (select id from inv_product_group where code = 'REFRIGERATION_EQUIPMENT')
    ),
    (
        'FREEZERS',
        'Muzlatgichlar',
        1,
        30,
        true,
        (select id from inv_product_group where code = 'REFRIGERATION_EQUIPMENT')
    ),

    -- Kir yuvish texnikasi
    (
        'AUTOMATIC_WASHING_MACHINES',
        'Avtomatik kir yuvish mashinalari',
        1,
        10,
        true,
        (select id from inv_product_group where code = 'LAUNDRY_EQUIPMENT')
    ),
    (
        'SEMI_AUTOMATIC_WASHING_MACHINES',
        'Yarim avtomatik kir yuvish mashinalari',
        1,
        20,
        true,
        (select id from inv_product_group where code = 'LAUNDRY_EQUIPMENT')
    ),

    -- Iqlim texnikasi
    (
        'AIR_CONDITIONERS',
        'Konditsionerlar',
        1,
        10,
        true,
        (select id from inv_product_group where code = 'CLIMATE_EQUIPMENT')
    ),
    (
        'AIR_PURIFIERS',
        'Havo tozalagichlar',
        1,
        20,
        true,
        (select id from inv_product_group where code = 'CLIMATE_EQUIPMENT')
    ),
    (
        'AIR_HUMIDIFIERS',
        'Havo namlagichlar',
        1,
        30,
        true,
        (select id from inv_product_group where code = 'CLIMATE_EQUIPMENT')
    ),
    (
        'HEATERS',
        'Isitgichlar',
        1,
        40,
        true,
        (select id from inv_product_group where code = 'CLIMATE_EQUIPMENT')
    ),
    (
        'FANS',
        'Ventilyatorlar',
        1, 
        50,
        true,
        (select id from inv_product_group where code = 'CLIMATE_EQUIPMENT'
    ),

    -- Yirik oshxona texnikasi
    (
        'STOVES',
        'Plitalar',
        1,
        10,
        true,
        (select id from inv_product_group where code = 'LARGE_KITCHEN_APPLIANCES')
    ),
    (
        'COOKTOPS',
        'Pishirish panellari',
        1,
        20,
        true,
        (select id from inv_product_group where code = 'LARGE_KITCHEN_APPLIANCES')
    ),
    (
        'RANGE_HOODS',
        'Oshxona havo tortgichlari',
        1,
        30,
        true,
        (select id from inv_product_group where code = 'LARGE_KITCHEN_APPLIANCES')
    ),
    (
        'ELECTRIC_OVENS',
        'Elektr pechlar',
        1,
        40,
        true,
        (select id from inv_product_group where code = 'LARGE_KITCHEN_APPLIANCES')
    ),
    (
        'BUILT_IN_OVENS',
        'O‘rnatiladigan duxovkalar',
        1,
        50,
        true,
        (select id from inv_product_group where code = 'LARGE_KITCHEN_APPLIANCES')
    ),
    (
        'MICROWAVE_OVENS',
        'Mikroto‘lqinli pechlar',
        1,
        60,
        true,
        (select id from inv_product_group where code = 'LARGE_KITCHEN_APPLIANCES')
    ),

    -- Tozalash texnikasi
    (
        'VACUUM_CLEANERS',
        'Changyutgichlar',
        1,
        10,
        true,
        (select id from inv_product_group where code = 'CLEANING_EQUIPMENT')
    ),

    -- Suv isitish texnikasi
    (
        'WATER_HEATERS',
        'Suv isitgichlar',
        1,
        10,
        true,
        (select id from inv_product_group where code = 'WATER_HEATING_EQUIPMENT')
    ),
    (
        'WATER_COOLERS',
        'Suv dispenserlari',
        1,
        20,
        true,
        (select id from inv_product_group where code = 'WATER_HEATING_EQUIPMENT')
    ),

    -- Kichik oshxona texnikasi
    (
        'MEAT_GRINDERS',
        'Go‘sht maydalagichlar',
        1,
        10,
        true,
        (select id from inv_product_group where code = 'SMALL_KITCHEN_APPLIANCES')
    ),
    (
        'JUICERS',
        'Sharbat chiqargichlar',
        1,
        20,
        true,
        (select id from inv_product_group where code = 'SMALL_KITCHEN_APPLIANCES')
    ),
    (
        'ELECTRIC_KETTLES',
        'Elektr choynaklar',
        1,
        30,
        true,
        (select id from inv_product_group where code = 'SMALL_KITCHEN_APPLIANCES')
    ),
    (
        'THERMOPOTS',
        'Termopotlar',
        1,
        40,
        true,
        (select id from inv_product_group where code = 'SMALL_KITCHEN_APPLIANCES')
    ),
    (
        'CHOPPERS',
        'Maydalagichlar',
        1,
        50,
        true,
        (select id from inv_product_group where code = 'SMALL_KITCHEN_APPLIANCES')
    ),
    (
        'MIXERS',
        'Mikserlar',
        1,
        60,
        true,
        (select id from inv_product_group where code = 'SMALL_KITCHEN_APPLIANCES')
    ),
    (
        'BLENDERS',
        'Blenderlar',
        1,
        70,
        true,
        (select id from inv_product_group where code = 'SMALL_KITCHEN_APPLIANCES')
    ),

    -- Kiyim va shaxsiy parvarish texnikasi
    (
        'IRONS',
        'Dazmollar',
        1,
        10,
        true,
        (select id from inv_product_group where code = 'GARMENT_AND_PERSONAL_CARE')
    ),
    (
        'GARMENT_STEAMERS',
        'Kiyim bug‘lagichlar',
        1,
        20,
        true,
        (select id from inv_product_group where code = 'GARMENT_AND_PERSONAL_CARE')
    ),
    (
        'HAIR_DRYERS',
        'Soch quritgichlar',
        1,
        30,
        true,
        (select id from inv_product_group where code = 'GARMENT_AND_PERSONAL_CARE')
    )
on conflict (code)
do update set
    name       = excluded.name,
    state_id   = excluded.state_id,
    sort_order = excluded.sort_order,
    is_assignable = excluded.is_assignable,
    parent_id  = excluded.parent_id;

commit;


begin;

-- Root: Xizmatlar
insert into inv_product_group
(
    name,
    state_id,
    created_date,
    code,
    parent_id,
    sort_order,
    is_assignable
)
select
    'Xizmatlar',
    1,
    now(),
    'SERVICES',
    null,
    20,
    false
where not exists (
    select 1
    from inv_product_group
    where code = 'SERVICES'
);


-- Dasturiy ta'minot xizmatlari
insert into inv_product_group
(
    name,
    state_id,
    created_date,
    code,
    parent_id,
    sort_order,
    is_assignable
)
select
    'Dasturiy ta''minot xizmatlari',
    1,
    now(),
    'SOFTWARE_SERVICES',
    (select id from inv_product_group where code = 'SERVICES'),
    10,
    true
where not exists (
    select 1
    from inv_product_group
    where code = 'SOFTWARE_SERVICES'
);


-- Bank xizmatlari
insert into inv_product_group
(
    name,
    state_id,
    created_date,
    code,
    parent_id,
    sort_order,
    is_assignable
)
select
    'Bank xizmatlari',
    1,
    now(),
    'BANK_SERVICES',
    (select id from inv_product_group where code = 'SERVICES'),
    20,
    true
where not exists (
    select 1
    from inv_product_group
    where code = 'BANK_SERVICES'
);


-- Internet xizmatlari
insert into inv_product_group
(
    name,
    state_id,
    created_date,
    code,
    parent_id,
    sort_order,
    is_assignable
)
select
    'Internet xizmatlari',
    1,
    now(),
    'INTERNET_SERVICES',
    (select id from inv_product_group where code = 'SERVICES'),
    30,
    true
where not exists (
    select 1
    from inv_product_group
    where code = 'INTERNET_SERVICES'
);


-- Elektron hujjat aylanishi xizmatlari
insert into inv_product_group
(
    name,
    state_id,
    created_date,
    code,
    parent_id,
    sort_order,
    is_assignable
)
select
    'Elektron hujjat aylanishi xizmatlari',
    1,
    now(),
    'E_DOCUMENT_SERVICES',
    (select id from inv_product_group where code = 'SERVICES'),
    40,
    true
where not exists (
    select 1
    from inv_product_group
    where code = 'E_DOCUMENT_SERVICES'
);

commit;