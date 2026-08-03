create table fa_receipt_type_translation
(
	receipt_type_id		smallint not null references fa_receipt_type (id),
	language_id			smallint not null references cmn_language (id),
	name				varchar(100) not null,

	primary key (receipt_type_id, language_id)
);

CREATE INDEX ix_fa_receipt_type_translation_language_id
    ON fa_receipt_type_translation (language_id);

INSERT INTO fa_receipt_type_translation
(
    receipt_type_id,
    language_id,
    name
)
SELECT
    rt.id,
    l.id,
    t.name
FROM (
    VALUES
        ('PURCHASE',     'uz', 'Sotib olish'),
        ('PURCHASE',     'ru', 'Покупка'),
        ('PURCHASE',     'en', 'Purchase'),

        ('CONSTRUCTION', 'uz', 'Qurilish'),
        ('CONSTRUCTION', 'ru', 'Строительство'),
        ('CONSTRUCTION', 'en', 'Construction'),

        ('OTHER',        'uz', 'Boshqa'),
        ('OTHER',        'ru', 'Прочее'),
        ('OTHER',        'en', 'Other')
) AS t(receipt_type_code, language_code, name)
JOIN fa_receipt_type rt
    ON rt.code = t.receipt_type_code
JOIN cmn_language l
    ON l.code = t.language_code
ON CONFLICT (receipt_type_id, language_id)
DO UPDATE SET
    name = EXCLUDED.name;