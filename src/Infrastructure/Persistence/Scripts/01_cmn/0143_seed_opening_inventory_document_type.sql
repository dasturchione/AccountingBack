insert into cmn_document_type (id, code, name, state_id, created_date)
values (16, 'opening_inventory', 'Boshlang''ich tovar qoldig''i', 1, now())
on conflict (id) do update set
    code = excluded.code,
    name = excluded.name,
    state_id = excluded.state_id;

insert into cmn_document_type_translation (document_type_id, language_id, name)
select 16, language.id, values_to_insert.name
from
(
    values
        ('uz', 'Boshlang''ich tovar qoldig''i'),
        ('ru', 'Ввод начальных остатков товаров'),
        ('en', 'Opening inventory')
) as values_to_insert(language_code, name)
join cmn_language language on language.code = values_to_insert.language_code
on conflict (document_type_id, language_id) do update set
    name = excluded.name;
