insert into acc_chart_account_preset 
    (id, code, name, description, state_id, created_date)
values
    (
        1,
        'default_chart_account',
        'Standart hisobvaraqlar rejasi',
        'Tashkilot uchun standart hisobvaraqlar rejasi andozasi',
        1,
        now()
    );

insert into acc_chart_account_preset_translation
    (preset_id, language_id, name, description)
values
    (
        1,
        1,
        'Standart hisobvaraqlar rejasi',
        'Tashkilot uchun standart hisobvaraqlar rejasi andozasi'
    ),
    (
        1,
        2,
        'Базовый план счетов',
        'Стандартная заготовка плана счетов для организации'
    ),
    (
        1,
        3,
        'Basic chart of accounts',
        'Standard chart of accounts preset for an organization'
    );