begin;

insert into cmn_regulated_obligation_category (code, name, state_id)
values
    ('TAX', 'Soliq', 1),
    ('CONTRIBUTION', 'Ajratma', 1)
on conflict (code) do update
set name = excluded.name,
    state_id = excluded.state_id;

insert into cmn_regulated_obligation_category_translation
    (category_id, language_id, name)
select category.id,
       language.id,
       source.name
from
(
    values
        ('TAX', 'uz', 'Soliq'),
        ('TAX', 'ru', 'Налог'),
        ('TAX', 'en', 'Tax'),
        ('CONTRIBUTION', 'uz', 'Ajratma'),
        ('CONTRIBUTION', 'ru', 'Отчисление'),
        ('CONTRIBUTION', 'en', 'Contribution')
) as source(category_code, language_code, name)
join cmn_regulated_obligation_category category
    on category.code = source.category_code
join cmn_language language
    on language.code = source.language_code
on conflict (category_id, language_id) do update
set name = excluded.name;

insert into cmn_regulated_obligation_periodicity (code, name, state_id)
values
    ('NOT_REQUIRED', 'Topshirilmaydi', 1),
    ('TEN_DAY', 'O''n kunlik', 1),
    ('MONTHLY', 'Oy', 1),
    ('QUARTERLY', 'Chorak', 1),
    ('SEMI_ANNUAL', 'Yarim yil', 1),
    ('ANNUAL', 'Yil', 1)
on conflict (code) do update
set name = excluded.name,
    state_id = excluded.state_id;

insert into cmn_regulated_obligation_periodicity_translation
    (periodicity_id, language_id, name)
select periodicity.id,
       language.id,
       source.name
from
(
    values
        ('NOT_REQUIRED', 'uz', 'Topshirilmaydi'),
        ('NOT_REQUIRED', 'ru', 'Не сдаётся'),
        ('NOT_REQUIRED', 'en', 'Not required'),
        ('TEN_DAY', 'uz', 'O''n kunlik'),
        ('TEN_DAY', 'ru', 'Декада'),
        ('TEN_DAY', 'en', 'Ten-day period'),
        ('MONTHLY', 'uz', 'Oy'),
        ('MONTHLY', 'ru', 'Месяц'),
        ('MONTHLY', 'en', 'Month'),
        ('QUARTERLY', 'uz', 'Chorak'),
        ('QUARTERLY', 'ru', 'Квартал'),
        ('QUARTERLY', 'en', 'Quarter'),
        ('SEMI_ANNUAL', 'uz', 'Yarim yil'),
        ('SEMI_ANNUAL', 'ru', 'Полугодие'),
        ('SEMI_ANNUAL', 'en', 'Half-year'),
        ('ANNUAL', 'uz', 'Yil'),
        ('ANNUAL', 'ru', 'Год'),
        ('ANNUAL', 'en', 'Year')
) as source(periodicity_code, language_code, name)
join cmn_regulated_obligation_periodicity periodicity
    on periodicity.code = source.periodicity_code
join cmn_language language
    on language.code = source.language_code
on conflict (periodicity_id, language_id) do update
set name = excluded.name;

with source(category_code, code, name_uz, name_ru, name_en) as
(
    values
        ('TAX', 'EXCISE_TAX', 'Aksiz solig''i',
            'Акцизный налог',
            'Excise tax'),
        ('TAX', 'ECONOMIC_COURT_STATE_DUTY', 'Xo''jalik sudlarida ko''rib chiqiladigan ishlar bo''yicha davlat boji',
            'Государственная пошлина по делам, рассматриваемым хозяйственными судами',
            'State duty on cases considered by economic courts'),
        ('TAX', 'UNIFIED_LAND_TAX', 'Yagona yer solig''i',
            'Единый земельный налог',
            'Unified land tax'),
        ('TAX', 'UNIFIED_IMPUTED_INCOME_TAX', 'Hisoblangan daromadga yagona soliq',
            'Единый налог на вмененный доход',
            'Unified tax on imputed income'),
        ('TAX', 'SUBSOIL_USE_TAX', 'Yer qa''ridan foydalanganlik uchun soliq',
            'Налог за пользование недрами',
            'Subsoil use tax'),
        ('TAX', 'SOCIAL_INFRASTRUCTURE_DEVELOPMENT_TAX', 'Obodonlashtirish va ijtimoiy infratuzilmani rivojlantirish solig''i',
            'Налог на благоустройство и развитие социальной инфраструктуры',
            'Tax for improvement and social infrastructure development'),
        ('TAX', 'VAT', 'Qo''shilgan qiymat solig''i',
            'Налог на добавленную стоимость',
            'Value added tax'),
        ('TAX', 'PROPERTY_RENTAL_INCOME_TAX', 'Jismoniy shaxslarning mol-mulkni ijaraga berishdan olgan daromad solig''i',
            'Налог на доходы физических лиц от сдачи имущества в аренду',
            'Tax on individual income from property rental'),
        ('TAX', 'AGRICULTURAL_LAND_TAX', 'Qishloq xo''jaligi korxonalarining yer solig''i',
            'Налог на землю с сельхоз предприятий',
            'Land tax on agricultural enterprises'),
        ('TAX', 'LEGAL_ENTITY_LAND_TAX', 'Yuridik shaxslarning yer solig''i',
            'Налог на землю с юридических лиц',
            'Land tax on legal entities'),
        ('TAX', 'LEGAL_ENTITY_PROPERTY_TAX', 'Yuridik shaxslarning mol-mulk solig''i',
            'Налог на имущество с юридических лиц',
            'Property tax on legal entities'),
        ('TAX', 'CORPORATE_INCOME_TAX', 'Boshqa korxona va tashkilotlarning foyda (daromad) solig''i',
            'Налог на прибыль (доходы) других предприятий и организаций',
            'Profit (income) tax of other enterprises and organizations'),
        ('TAX', 'WATER_USE_FEE', 'Suv resurslaridan foydalanganlik uchun to''lov',
            'Плата за пользование водными ресурсами',
            'Water resources use fee'),
        ('TAX', 'PERSONAL_INCOME_TAX', 'Ishchilar, xizmatchilar va kolxozchilardan olinadigan daromad solig''i',
            'Подоходный налог с рабочих, служащих и колхозников',
            'Income tax on workers, employees and collective farmers'),
        ('TAX', 'TAX_PENALTY', 'Jarima sanksiyalari (FRNO)',
            'Штрафные санкции (ФРНО)',
            'Penalty sanctions (FRNO)'),
        ('TAX', 'ENVIRONMENTAL_TAX', 'Ekologiya solig''i',
            'Экологический налог',
            'Environmental tax'),

        ('CONTRIBUTION', 'TRADE_UNION_SALARY_CONTRIBUTION', 'Ish haqidan kasaba uyushmasiga badallar',
            'Взносы в профсоюз из заработной платы',
            'Trade union contributions from salary'),
        ('CONTRIBUTION', 'TRADE_UNION_PAYROLL_CONTRIBUTION', 'Mehnatga haq to''lash fondidan kasaba uyushmasiga badallar',
            'Взносы в профсоюз от фонда оплаты труда',
            'Trade union contributions from the payroll fund'),
        ('CONTRIBUTION', 'UNIFIED_SOCIAL_PAYMENT', 'Yagona ijtimoiy to''lov',
            'Единый социальный платеж',
            'Unified social payment'),
        ('CONTRIBUTION', 'STATE_SPECIAL_FUNDS_CONTRIBUTION', 'Davlat maqsadli jamg''armalariga majburiy ajratmalar',
            'Обязательные отчисления в Государственные целевые фонды',
            'Mandatory contributions to state special-purpose funds'),
        ('CONTRIBUTION', 'PENSION_FUND_TURNOVER_CONTRIBUTION', 'Tovar aylanmasidan Pensiya jamg''armasiga majburiy ajratmalar',
            'Обязательные отчисления в Пенсионный фонд от товарооборота',
            'Mandatory contributions to the Pension Fund from turnover'),
        ('CONTRIBUTION', 'EDUCATION_RECONSTRUCTION_FUND_CONTRIBUTION', 'Ta''lim muassasalarini rekonstruksiya qilish jamg''armasiga majburiy ajratmalar',
            'Обязательные отчисления в фонд реконструкции образовательных учреждений',
            'Mandatory contributions to the Educational Institutions Reconstruction Fund'),
        ('CONTRIBUTION', 'STATE_PENSION_INSURANCE_CONTRIBUTION', 'Davlat pensiya jamg''armasiga majburiy sug''urta badallari',
            'Обязательные страховые взносы в ГПФ',
            'Mandatory insurance contributions to the State Pension Fund'),
        ('CONTRIBUTION', 'INPS_CONTRIBUTION', 'INPSga ajratmalar',
            'Отчисления в ИНПС',
            'Contributions to individual accumulative pension accounts'),
        ('CONTRIBUTION', 'KAMOLOT_YOUTH_FUND_CONTRIBUTION', '"Kamolot" yoshlar jamg''armasiga ajratmalar',
            'Отчисления в молодежный фонд "Камолот"',
            'Contributions to the Kamolot Youth Fund'),
        ('CONTRIBUTION', 'ROAD_FUND_OTHER_PAYERS_CONTRIBUTION', 'Boshqa to''lovchilardan Yo''l jamg''armasiga ajratmalar',
            'Отчисления ДФ от остальных плательщиков',
            'Road Fund contributions from other payers')
),
upserted as
(
    insert into cmn_regulated_obligation (category_id, code, name, state_id)
    select category.id,
           source.code,
           source.name_uz,
           1
    from source
    join cmn_regulated_obligation_category category
        on category.code = source.category_code
    on conflict (code) do update
    set category_id = excluded.category_id,
        name = excluded.name,
        state_id = excluded.state_id
    returning id, code
)
insert into cmn_regulated_obligation_translation
    (regulated_obligation_id, language_id, name)
select upserted.id,
       language.id,
       case language.code
           when 'uz' then source.name_uz
           when 'ru' then source.name_ru
           when 'en' then source.name_en
       end
from source
join upserted
    on upserted.code = source.code
join cmn_language language
    on language.code in ('uz', 'ru', 'en')
on conflict (regulated_obligation_id, language_id) do update
set name = excluded.name;

do $$
declare
    item              record;
    legacy_id         smallint;
    current_id        smallint;
begin
    for item in
        select *
        from
        (
            values
                ('000000003', 'EXCISE_TAX'),
                ('000000039', 'ECONOMIC_COURT_STATE_DUTY'),
                ('000000004', 'UNIFIED_LAND_TAX'),
                ('000000005', 'UNIFIED_IMPUTED_INCOME_TAX'),
                ('000000010', 'SUBSOIL_USE_TAX'),
                ('000000006', 'SOCIAL_INFRASTRUCTURE_DEVELOPMENT_TAX'),
                ('000000007', 'VAT'),
                ('000000041', 'PROPERTY_RENTAL_INCOME_TAX'),
                ('000000008', 'AGRICULTURAL_LAND_TAX'),
                ('000000040', 'LEGAL_ENTITY_LAND_TAX'),
                ('000000009', 'LEGAL_ENTITY_PROPERTY_TAX'),
                ('000000011', 'CORPORATE_INCOME_TAX'),
                ('000000012', 'WATER_USE_FEE'),
                ('000000013', 'PERSONAL_INCOME_TAX'),
                ('000000036', 'TAX_PENALTY'),
                ('000000014', 'ENVIRONMENTAL_TAX'),
                ('000000016', 'TRADE_UNION_SALARY_CONTRIBUTION'),
                ('000000017', 'TRADE_UNION_PAYROLL_CONTRIBUTION'),
                ('000000019', 'UNIFIED_SOCIAL_PAYMENT'),
                ('000000018', 'UNIFIED_SOCIAL_PAYMENT'),
                ('000000034', 'UNIFIED_SOCIAL_PAYMENT'),
                ('000000035', 'STATE_SPECIAL_FUNDS_CONTRIBUTION'),
                ('000000021', 'PENSION_FUND_TURNOVER_CONTRIBUTION'),
                ('000000031', 'EDUCATION_RECONSTRUCTION_FUND_CONTRIBUTION'),
                ('000000032', 'STATE_PENSION_INSURANCE_CONTRIBUTION'),
                ('000000020', 'INPS_CONTRIBUTION'),
                ('000000015', 'KAMOLOT_YOUTH_FUND_CONTRIBUTION'),
                ('000000033', 'ROAD_FUND_OTHER_PAYERS_CONTRIBUTION')
        ) as mapping(legacy_code, current_code)
    loop
        select id
        into legacy_id
        from cmn_regulated_obligation
        where code = item.legacy_code;

        if legacy_id is null then
            continue;
        end if;

        select id
        into current_id
        from cmn_regulated_obligation
        where code = item.current_code;

        if to_regclass('org_regulated_obligation_setting') is not null then
            execute
                'update org_regulated_obligation_setting
                 set regulated_obligation_id = $1
                 where regulated_obligation_id = $2'
            using current_id, legacy_id;
        end if;

        delete from cmn_regulated_obligation_translation
        where regulated_obligation_id = legacy_id;

        delete from cmn_regulated_obligation
        where id = legacy_id;
    end loop;
end
$$;

commit;
