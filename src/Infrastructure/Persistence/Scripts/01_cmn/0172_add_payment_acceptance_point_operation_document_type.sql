insert into cmn_document_type(id, state_id, code, name)
values (25, 1, 'payment_acceptance_point_operation', 'To''lov qabul qilish nuqtasi operatsiyasi')
on conflict (id) do update set
    state_id = excluded.state_id,
    code = excluded.code,
    name = excluded.name;

insert into cmn_document_type_translation(document_type_id, language_id, name)
select document_type.id, language.id, translation.name
from
(
    values
        ('uz', 'To''lov qabul qilish nuqtasi operatsiyasi'),
        ('ru', 'Операция точки приёма платежей'),
        ('en', 'Payment acceptance point operation')
) as translation(language_code, name)
join cmn_document_type document_type on document_type.code = 'payment_acceptance_point_operation'
join cmn_language language on language.code = translation.language_code
on conflict (document_type_id, language_id) do update set
    name = excluded.name;
