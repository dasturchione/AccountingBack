create table cmn_document_type_translation
(
    document_type_id smallint not null
        references cmn_document_type(id),

    language_id smallint not null
        references cmn_language(id),

    name varchar(150) not null,

    primary key (document_type_id, language_id)
);

create index ix_cmn_document_type_translation_language_id
    on cmn_document_type_translation (language_id);


insert into cmn_document_type_translation
    (document_type_id, language_id, name)
select
    document_type.id,
    language.id,
    translation.name
from
(
    values
        ('purchase',            'uz', 'Xarid / Kirim'),
        ('sale',                'uz', 'Sotuv'),
        ('bank_operation',      'uz', 'Bank operatsiyasi'),
        ('cash_operation',      'uz', 'Kassa operatsiyasi'),
        ('salary',              'uz', 'Ish haqi'),
        ('expense',             'uz', 'Xarajat'),
        ('retail_sale',         'uz', 'Chakana sotuv'),
        ('inventory_adjustment','uz', 'Inventar tuzatish'),
        ('inventory_count',     'uz', 'Inventarizatsiya'),
        ('fa_receipt',          'uz', 'Asosiy vosita qabuli'),
        ('fa_movement',         'uz', 'Asosiy vosita ko''chirilishi'),
        ('fa_depreciation',     'uz', 'Asosiy vosita amortizatsiyasi'),
        ('fa_disposal',         'uz', 'Asosiy vosita chiqib ketishi'),
        ('fa_revaluation',      'uz', 'Asosiy vositani qayta baholash'),
        ('opening_inventory',   'uz', 'Boshlang''ich tovar qoldig''i'),
        ('fa_commissioning',    'uz', 'Asosiy vositani ishga tushirish'),
        ('warehouse_transfer',  'uz', 'Omborlararo ko''chirish'),
        ('sale_shipment',       'uz', 'Sotuv bo''yicha jo''natish'),
        ('payroll_timesheet',   'uz', 'Ish vaqti tabeli'),
        ('payroll_payment',     'uz', 'Ish haqi to''lovi'),
        ('hr_absence',          'uz', 'Xodim yo''qligi'),

        ('purchase',            'ru', 'Покупка / Поступление'),
        ('sale',                'ru', 'Продажа'),
        ('bank_operation',      'ru', 'Банковская операция'),
        ('cash_operation',      'ru', 'Кассовая операция'),
        ('salary',              'ru', 'Заработная плата'),
        ('expense',             'ru', 'Расход'),
        ('retail_sale',         'ru', 'Розничная продажа'),
        ('inventory_adjustment','ru', 'Корректировка запасов'),
        ('inventory_count',     'ru', 'Инвентаризация'),
        ('fa_receipt',          'ru', 'Поступление основного средства'),
        ('fa_movement',         'ru', 'Перемещение основного средства'),
        ('fa_depreciation',     'ru', 'Амортизация основного средства'),
        ('fa_disposal',         'ru', 'Выбытие основного средства'),
        ('fa_revaluation',      'ru', 'Переоценка основного средства'),
        ('opening_inventory',   'ru', 'Ввод начальных остатков товаров'),
        ('fa_commissioning',    'ru', 'Ввод в эксплуатацию основного средства'),
        ('warehouse_transfer',  'ru', 'Перемещение между складами'),
        ('sale_shipment',       'ru', 'Отгрузка по продаже'),
        ('payroll_timesheet',   'ru', 'Табель рабочего времени'),
        ('payroll_payment',     'ru', 'Выплата заработной платы'),
        ('hr_absence',          'ru', 'Отсутствие сотрудника'),

        ('purchase',            'en', 'Purchase / Receipt'),
        ('sale',                'en', 'Sale'),
        ('bank_operation',      'en', 'Bank operation'),
        ('cash_operation',      'en', 'Cash operation'),
        ('salary',              'en', 'Salary'),
        ('expense',             'en', 'Expense'),
        ('retail_sale',         'en', 'Retail sale'),
        ('inventory_adjustment','en', 'Inventory adjustment'),
        ('inventory_count',     'en', 'Inventory count'),
        ('fa_receipt',          'en', 'Fixed asset receipt'),
        ('fa_movement',         'en', 'Fixed asset movement'),
        ('fa_depreciation',     'en', 'Fixed asset depreciation'),
        ('fa_disposal',         'en', 'Fixed asset disposal'),
        ('fa_revaluation',      'en', 'Fixed asset revaluation'),
        ('opening_inventory',   'en', 'Opening inventory'),
        ('fa_commissioning',    'en', 'Fixed asset commissioning'),
        ('warehouse_transfer',  'en', 'Warehouse transfer'),
        ('sale_shipment',       'en', 'Sale shipment'),
        ('payroll_timesheet',   'en', 'Payroll timesheet'),
        ('payroll_payment',     'en', 'Payroll payment'),
        ('hr_absence',          'en', 'Employee absence')
) as translation(document_type_code, language_code, name)
join cmn_document_type as document_type
    on document_type.code = translation.document_type_code
join cmn_language as language
    on language.code = translation.language_code
on conflict (document_type_id, language_id)
do update set
    name = excluded.name;
