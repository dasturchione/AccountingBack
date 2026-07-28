create table cmn_document_status_translation
(
    document_status_id smallint not null
        references cmn_document_status(id),

    language_id smallint not null
        references cmn_language(id),

    name varchar(100) not null,

    primary key (document_status_id, language_id)
);

create index ix_cmn_document_status_translation_language_id
    on cmn_document_status_translation (language_id);


insert into cmn_document_status_translation
    (document_status_id, language_id, name)
select
    document_status.id,
    language.id,
    translation.name
from
(
    values
        ('draft',     'uz', 'Qoralama'),
        ('posted',    'uz', 'O''tkazilgan'),
        ('cancelled', 'uz', 'Bekor qilingan'),
        ('pending',   'uz', 'Kutilmoqda'),

        ('draft',     'ru', 'Черновик'),
        ('posted',    'ru', 'Проведён'),
        ('cancelled', 'ru', 'Отменён'),
        ('pending',   'ru', 'Ожидает'),

        ('draft',     'en', 'Draft'),
        ('posted',    'en', 'Posted'),
        ('cancelled', 'en', 'Cancelled'),
        ('pending',   'en', 'Pending')
) as translation(document_status_code, language_code, name)
join cmn_document_status as document_status
    on document_status.code = translation.document_status_code
join cmn_language as language
    on language.code = translation.language_code
on conflict (document_status_id, language_id)
do update set
    name = excluded.name;
