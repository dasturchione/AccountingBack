insert into cmn_document_type (id, state_id, code, name)
values (23, 1, 'cash_fiscal_transfer', 'Fiskal va asosiy kassa o''rtasida pul ko''chirish')
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
        ('uz', 'Fiskal va asosiy kassa o''rtasida pul ko''chirish'),
        ('ru', 'Перемещение денег между фискальной и основной кассой'),
        ('en', 'Cash transfer between fiscal register and cash box')
) as translation(language_code, name)
join cmn_document_type as document_type
    on document_type.code = 'cash_fiscal_transfer'
join cmn_language as language
    on language.code = translation.language_code
on conflict (document_type_id, language_id) do update set
    name = excluded.name;
