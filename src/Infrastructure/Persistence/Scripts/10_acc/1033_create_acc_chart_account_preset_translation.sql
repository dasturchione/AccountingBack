create table acc_chart_account_preset_translation
(
    preset_id           smallint not null       references acc_chart_account_preset(id) on delete cascade,
    language_id         smallint not null       references cmn_language(id),
    name                varchar(255) not null,
    description         varchar(500) null,

    primary key (preset_id, language_id)
);

CREATE INDEX idx_acc_chart_account_preset_translation_language_id
ON acc_chart_account_preset_translation(language_id);

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
