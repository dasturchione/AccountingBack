create table if not exists fa_disposal_type_translation
(
    disposal_type_id            smallint not null references fa_disposal_type (id),
    language_id                 smallint not null references cmn_language (id),
    name                        varchar(100) not null,

    primary key (disposal_type_id, language_id)
);

create index if not exists ix_fa_disposal_type_translation_language_id
    on fa_disposal_type_translation (language_id);

INSERT INTO fa_disposal_type_translation
(
    disposal_type_id,
    language_id,
    name
)
SELECT
    dt.id,
    l.id,
    v.name
FROM (
    VALUES
        ('SALE',      'uz', 'Sotish'),
        ('SALE',      'ru', 'Продажа'),
        ('SALE',      'en', 'Sale'),

        ('WRITEOFF',  'uz', 'Hisobdan chiqarish'),
        ('WRITEOFF',  'ru', 'Списание'),
        ('WRITEOFF',  'en', 'Write-off'),

        ('BREAKDOWN', 'uz', 'Buzilish'),
        ('BREAKDOWN', 'ru', 'Поломка'),
        ('BREAKDOWN', 'en', 'Breakdown')
) AS v(disposal_type_code, language_code, name)
JOIN fa_disposal_type dt
    ON dt.code = v.disposal_type_code
JOIN cmn_language l
    ON l.code = v.language_code
ON CONFLICT (disposal_type_id, language_id)
DO UPDATE SET
    name = EXCLUDED.name;