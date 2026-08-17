insert into cmn_document_type (id, state_id, code, name)
values
    ( 8, 1, 'inventory_adjustment', 'Inventar tuzatish'          ),
    ( 9, 1, 'inventory_count',      'Inventarizatsiya'           ),
    (18, 1, 'warehouse_transfer',   'Omborlararo ko''chirish'    ),
    (19, 1, 'sale_shipment',        'Sotuv bo''yicha jo''natish' ),
    (20, 1, 'payroll_timesheet',    'Ish vaqti tabeli'           ),
    (21, 1, 'payroll_payment',      'Ish haqi to''lovi'          ),
    (22, 1, 'hr_absence',           'Xodim yo''qligi'            )
on conflict (id) do update set
    state_id = excluded.state_id,
    code = excluded.code,
    name = excluded.name;

insert into cmn_document_type_translation (document_type_id, language_id, name)
select
    document_type.id,
    language.id,
    translation.name
from
(
    values
        ('inventory_adjustment', 'uz', 'Inventar tuzatish'),
        ('inventory_count',      'uz', 'Inventarizatsiya'),
        ('warehouse_transfer',   'uz', 'Omborlararo ko''chirish'),
        ('sale_shipment',        'uz', 'Sotuv bo''yicha jo''natish'),
        ('payroll_timesheet',    'uz', 'Ish vaqti tabeli'),
        ('payroll_payment',      'uz', 'Ish haqi to''lovi'),
        ('hr_absence',           'uz', 'Xodim yo''qligi'),

        ('inventory_adjustment', 'ru', 'Корректировка запасов'),
        ('inventory_count',      'ru', 'Инвентаризация'),
        ('warehouse_transfer',   'ru', 'Перемещение между складами'),
        ('sale_shipment',        'ru', 'Отгрузка по продаже'),
        ('payroll_timesheet',    'ru', 'Табель рабочего времени'),
        ('payroll_payment',      'ru', 'Выплата заработной платы'),
        ('hr_absence',           'ru', 'Отсутствие сотрудника'),

        ('inventory_adjustment', 'en', 'Inventory adjustment'),
        ('inventory_count',      'en', 'Inventory count'),
        ('warehouse_transfer',   'en', 'Warehouse transfer'),
        ('sale_shipment',        'en', 'Sale shipment'),
        ('payroll_timesheet',    'en', 'Payroll timesheet'),
        ('payroll_payment',      'en', 'Payroll payment'),
        ('hr_absence',           'en', 'Employee absence')
) as translation(document_type_code, language_code, name)
join cmn_document_type as document_type
    on document_type.code = translation.document_type_code
join cmn_language as language
    on language.code = translation.language_code
on conflict (document_type_id, language_id) do update set
    name = excluded.name;

drop function if exists set_bank_operation_doc_number() cascade;
drop function if exists set_cash_operation_doc_number() cascade;
drop function if exists set_pur_doc_number() cascade;
drop function if exists set_sale_doc_number() cascade;
drop sequence if exists doc_number_bank_operation_seq;
drop sequence if exists doc_number_cash_operation_seq;
drop sequence if exists doc_number_purchase_seq;
drop sequence if exists doc_number_sale_seq;
