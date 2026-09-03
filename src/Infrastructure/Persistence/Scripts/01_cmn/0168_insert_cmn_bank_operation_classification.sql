insert into cmn_bank_operation_category (code, name)
values
    ('BANK_COMMISSION', 'Bank commission'),
    ('BANK_SERVICE', 'Bank service'),
    ('ACQUIRING', 'Terminal / acquiring'),
    ('CASH_COLLECTION', 'Cash collection'),
    ('CAPITAL_CONTRIBUTION', 'Capital contribution'),
    ('PERSONAL_CARD', 'Personal card'),
    ('COUNTERPARTY', 'Counterparty'),
    ('REVIEW_REQUIRED', 'Review required');

insert into cmn_bank_operation_category_translation (category_id, language_id, name)
select category.id, language.id, translation.name
from
(
    values
        ('BANK_COMMISSION', 'uz', 'Bank komissiyasi'),
        ('BANK_COMMISSION', 'ru', 'Комиссия банка'),
        ('BANK_COMMISSION', 'en', 'Bank commission'),
        ('BANK_SERVICE', 'uz', 'Bank xizmati'),
        ('BANK_SERVICE', 'ru', 'Услуга банка'),
        ('BANK_SERVICE', 'en', 'Bank service'),
        ('ACQUIRING', 'uz', 'Terminal / ekvayring'),
        ('ACQUIRING', 'ru', 'Терминал / эквайринг'),
        ('ACQUIRING', 'en', 'Terminal / acquiring'),
        ('CASH_COLLECTION', 'uz', 'Inkassatsiya'),
        ('CASH_COLLECTION', 'ru', 'Инкассация'),
        ('CASH_COLLECTION', 'en', 'Cash collection'),
        ('CAPITAL_CONTRIBUTION', 'uz', 'Ustav kapitaliga badal'),
        ('CAPITAL_CONTRIBUTION', 'ru', 'Взнос в уставный капитал'),
        ('CAPITAL_CONTRIBUTION', 'en', 'Capital contribution'),
        ('PERSONAL_CARD', 'uz', 'Jismoniy shaxs kartasi'),
        ('PERSONAL_CARD', 'ru', 'Карта физического лица'),
        ('PERSONAL_CARD', 'en', 'Personal card'),
        ('COUNTERPARTY', 'uz', 'Kontragent'),
        ('COUNTERPARTY', 'ru', 'Контрагент'),
        ('COUNTERPARTY', 'en', 'Counterparty'),
        ('REVIEW_REQUIRED', 'uz', 'Tekshirish talab etiladi'),
        ('REVIEW_REQUIRED', 'ru', 'Требуется проверка'),
        ('REVIEW_REQUIRED', 'en', 'Review required')
) as translation(category_code, language_code, name)
join cmn_bank_operation_category as category
    on category.code = translation.category_code
join cmn_language as language
    on language.code = translation.language_code;

insert into cmn_bank_operation_classification_rule_set
(
    bank_id,
    code,
    name,
    version
)
values
(
    (select id from cmn_bank where code = 'UZSANOATQURILISHBANK'),
    'UZSANOATQURILISHBANK_CLASSIFICATION_V1',
    'Uzsanoatqurilishbank operation classification V1',
    1
);

insert into cmn_bank_operation_classification_rule
(
    rule_set_id,
    category_id,
    code,
    name,
    priority,
    direction_id,
    is_fallback
)
values
    (
        (select id from cmn_bank_operation_classification_rule_set where code = 'UZSANOATQURILISHBANK_CLASSIFICATION_V1'),
        (select id from cmn_bank_operation_category where code = 'BANK_COMMISSION'),
        'BANK_COMMISSION', 'Bank commission', 10,
        (select id from cmn_movement_direction where code = 'OUT'), false
    ),
    (
        (select id from cmn_bank_operation_classification_rule_set where code = 'UZSANOATQURILISHBANK_CLASSIFICATION_V1'),
        (select id from cmn_bank_operation_category where code = 'BANK_SERVICE'),
        'BANK_SERVICE', 'Bank service', 20,
        (select id from cmn_movement_direction where code = 'OUT'), false
    ),
    (
        (select id from cmn_bank_operation_classification_rule_set where code = 'UZSANOATQURILISHBANK_CLASSIFICATION_V1'),
        (select id from cmn_bank_operation_category where code = 'REVIEW_REQUIRED'),
        'REVIEW_BANK_ACCRUAL', 'Unknown bank accrual', 30,
        (select id from cmn_movement_direction where code = 'OUT'), false
    ),
    (
        (select id from cmn_bank_operation_classification_rule_set where code = 'UZSANOATQURILISHBANK_CLASSIFICATION_V1'),
        (select id from cmn_bank_operation_category where code = 'CAPITAL_CONTRIBUTION'),
        'CAPITAL_CONTRIBUTION', 'Capital contribution', 40,
        (select id from cmn_movement_direction where code = 'IN'), false
    ),
    (
        (select id from cmn_bank_operation_classification_rule_set where code = 'UZSANOATQURILISHBANK_CLASSIFICATION_V1'),
        (select id from cmn_bank_operation_category where code = 'CASH_COLLECTION'),
        'CASH_COLLECTION', 'Cash collection', 50,
        (select id from cmn_movement_direction where code = 'IN'), false
    ),
    (
        (select id from cmn_bank_operation_classification_rule_set where code = 'UZSANOATQURILISHBANK_CLASSIFICATION_V1'),
        (select id from cmn_bank_operation_category where code = 'ACQUIRING'),
        'ACQUIRING', 'Terminal / acquiring', 60,
        (select id from cmn_movement_direction where code = 'IN'), false
    ),
    (
        (select id from cmn_bank_operation_classification_rule_set where code = 'UZSANOATQURILISHBANK_CLASSIFICATION_V1'),
        (select id from cmn_bank_operation_category where code = 'PERSONAL_CARD'),
        'PERSONAL_CARD', 'Personal card', 70, null, false
    ),
    (
        (select id from cmn_bank_operation_classification_rule_set where code = 'UZSANOATQURILISHBANK_CLASSIFICATION_V1'),
        (select id from cmn_bank_operation_category where code = 'COUNTERPARTY'),
        'COUNTERPARTY', 'Counterparty', 999, null, true
    );

insert into cmn_bank_operation_classification_condition
(
    rule_id,
    condition_order,
    field_code,
    operator_code,
    value_source_code,
    compare_value,
    normalization_code
)
select rule.id, condition.condition_order, condition.field_code, condition.operator_code,
       condition.value_source_code, condition.compare_value, condition.normalization_code
from
(
    values
        ('BANK_COMMISSION', 1, 'COUNTERPARTY_INN', 'EQUALS', 'ORGANIZATION_INN', null, 'NORMALIZE_KEY'),
        ('BANK_COMMISSION', 2, 'COUNTERPARTY_NAME', 'STARTS_WITH', 'LITERAL', 'Начисленные', 'NORMALIZE_WHITESPACE'),
        ('BANK_COMMISSION', 3, 'PURPOSE', 'CONTAINS', 'LITERAL', 'от суммы', 'NORMALIZE_WHITESPACE'),

        ('BANK_SERVICE', 1, 'COUNTERPARTY_INN', 'EQUALS', 'ORGANIZATION_INN', null, 'NORMALIZE_KEY'),
        ('BANK_SERVICE', 2, 'COUNTERPARTY_NAME', 'STARTS_WITH', 'LITERAL', 'Начисленные', 'NORMALIZE_WHITESPACE'),
        ('BANK_SERVICE', 3, 'PURPOSE', 'NOT_CONTAINS', 'LITERAL', 'от суммы', 'NORMALIZE_WHITESPACE'),
        ('BANK_SERVICE', 4, 'PURPOSE', 'CONTAINS_ANY', 'LITERAL', null, 'LOWER'),

        ('REVIEW_BANK_ACCRUAL', 1, 'COUNTERPARTY_INN', 'EQUALS', 'ORGANIZATION_INN', null, 'NORMALIZE_KEY'),
        ('REVIEW_BANK_ACCRUAL', 2, 'COUNTERPARTY_NAME', 'STARTS_WITH', 'LITERAL', 'Начисленные', 'NORMALIZE_WHITESPACE'),

        ('CAPITAL_CONTRIBUTION', 1, 'COUNTERPARTY_NAME', 'EQUALS', 'LITERAL', 'Айланма кассадаги накд пуллар', 'NORMALIZE_WHITESPACE'),
        ('CAPITAL_CONTRIBUTION', 2, 'COUNTERPARTY_ACCOUNT', 'EQUALS', 'LITERAL', '10101000800010900101', 'NORMALIZE_KEY'),
        ('CAPITAL_CONTRIBUTION', 3, 'PURPOSE', 'CONTAINS_ANY', 'LITERAL', null, 'LOWER'),

        ('CASH_COLLECTION', 1, 'COUNTERPARTY_NAME', 'EQUALS', 'LITERAL', 'Айланма кассадаги накд пуллар', 'NORMALIZE_WHITESPACE'),
        ('CASH_COLLECTION', 2, 'COUNTERPARTY_ACCOUNT', 'EQUALS', 'LITERAL', '10101000800010900101', 'NORMALIZE_KEY'),

        ('ACQUIRING', 1, 'COUNTERPARTY_INN', 'EQUALS', 'ORGANIZATION_INN', null, 'NORMALIZE_KEY'),
        ('ACQUIRING', 2, 'COUNTERPARTY_NAME', 'NOT_STARTS_WITH', 'LITERAL', 'Начисленные', 'NORMALIZE_WHITESPACE'),

        ('PERSONAL_CARD', 1, 'PURPOSE', 'CONTAINS_ANY', 'LITERAL', null, 'LOWER')
) as condition(rule_code, condition_order, field_code, operator_code, value_source_code, compare_value, normalization_code)
join cmn_bank_operation_classification_rule as rule
    on rule.rule_set_id = (select id from cmn_bank_operation_classification_rule_set where code = 'UZSANOATQURILISHBANK_CLASSIFICATION_V1')
   and rule.code = condition.rule_code;

insert into cmn_bank_operation_classification_condition_value
(
    condition_id,
    value_order,
    compare_value
)
select condition.id, condition_value.value_order, condition_value.compare_value
from
(
    values
        ('BANK_SERVICE', 4, 1, 'выпуск'),
        ('BANK_SERVICE', 4, 2, 'карт'),
        ('BANK_SERVICE', 4, 3, 'терминал'),
        ('BANK_SERVICE', 4, 4, 'terminal'),
        ('BANK_SERVICE', 4, 5, 'тариф'),
        ('BANK_SERVICE', 4, 6, 'абонент'),

        ('CAPITAL_CONTRIBUTION', 3, 1, 'устав'),
        ('CAPITAL_CONTRIBUTION', 3, 2, 'ustav'),
        ('CAPITAL_CONTRIBUTION', 3, 3, 'капитал'),
        ('CAPITAL_CONTRIBUTION', 3, 4, 'kapital'),

        ('PERSONAL_CARD', 1, 1, 'kartasiga'),
        ('PERSONAL_CARD', 1, 2, 'картасига')
) as condition_value(rule_code, condition_order, value_order, compare_value)
join cmn_bank_operation_classification_rule as rule
    on rule.rule_set_id = (select id from cmn_bank_operation_classification_rule_set where code = 'UZSANOATQURILISHBANK_CLASSIFICATION_V1')
   and rule.code = condition_value.rule_code
join cmn_bank_operation_classification_condition as condition
    on condition.rule_id = rule.id
   and condition.condition_order = condition_value.condition_order;
