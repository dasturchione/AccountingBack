insert into cmn_document_status (id, code, name, state_id)
values
    (5, 'in_transit', 'Yo''lda', 1),
    (6, 'completed',  'Yakunlangan', 1)
on conflict (id) do update set
    code = excluded.code,
    name = excluded.name,
    state_id = excluded.state_id;

insert into cmn_document_status_translation (document_status_id, language_id, name)
select
    document_status.id,
    language.id,
    translation.name
from
(
    values
        ('in_transit', 'uz', 'Yo''lda'),
        ('completed',  'uz', 'Yakunlangan'),
        ('in_transit', 'ru', 'В пути'),
        ('completed',  'ru', 'Завершён'),
        ('in_transit', 'en', 'In transit'),
        ('completed',  'en', 'Completed')
) as translation(document_status_code, language_code, name)
join cmn_document_status as document_status
    on document_status.code = translation.document_status_code
join cmn_language as language
    on language.code = translation.language_code
on conflict (document_status_id, language_id) do update set
    name = excluded.name;

insert into cmn_document_type (id, state_id, code, name)
values (24, 1, 'cash_collection', 'Naqd pulni bankka inkassatsiya qilish')
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
        ('uz', 'Naqd pulni bankka inkassatsiya qilish'),
        ('ru', 'Инкассация: перевод денег из кассы в банк'),
        ('en', 'Cash collection from cash box to bank')
) as translation(language_code, name)
join cmn_document_type as document_type
    on document_type.code = 'cash_collection'
join cmn_language as language
    on language.code = translation.language_code
on conflict (document_type_id, language_id) do update set
    name = excluded.name;
