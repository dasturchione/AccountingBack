insert into cmn_document_type(id, state_id, code, name)
values (27, 1, 'hr_order', 'Kadr buyrug''i')
on conflict (id) do update set
    state_id = excluded.state_id,
    code = excluded.code,
    name = excluded.name;

insert into cmn_document_type_translation(document_type_id, language_id, name)
select document_type.id, language.id, translation.name
from
(
    values
        ('uz', 'Kadr buyrug''i'),
        ('ru', 'Кадровый приказ'),
        ('en', 'HR order')
) as translation(language_code, name)
join cmn_document_type document_type on document_type.code = 'hr_order'
join cmn_language language on language.code = translation.language_code
on conflict (document_type_id, language_id) do update set
    name = excluded.name;
