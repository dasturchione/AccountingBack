create table fiscal_cash_register_type_translation
(
    cash_register_type_id  smallint not null
        references fiscal_cash_register_type(id) on delete cascade,

    language_id            smallint not null
        references cmn_language(id) on delete cascade,

    name                   varchar(250) not null,

    primary key (cash_register_type_id, language_id)
);

create index ix_fiscal_cash_register_type_translation_language_id
    on fiscal_cash_register_type_translation(language_id);


insert into fiscal_cash_register_type_translation
(
    cash_register_type_id,
    language_id,
    name
)
select
    t.id,
    l.id,
    v.name
from (
    values
        ('ONLINE_KKM',             'uz', 'Onlayn-NKM'),
        ('ONLINE_KKM',             'ru', 'Онлайн-ККМ'),
        ('ONLINE_KKM',             'en', 'Online cash register'),

        ('VIRTUAL_CASH_REGISTER',  'uz', 'Virtual kassa'),
        ('VIRTUAL_CASH_REGISTER',  'ru', 'Виртуальная касса'),
        ('VIRTUAL_CASH_REGISTER',  'en', 'Virtual cash register')
) as v(type_code, language_code, name)
join fiscal_cash_register_type t
    on t.code = v.type_code
join cmn_language l
    on l.code = v.language_code
on conflict (cash_register_type_id, language_id)
do update set
    name = excluded.name;