create table acc_account_type_translation 
(
	account_type_id				smallint not null references acc_account_type(id),
	language_id					smallint not null references cmn_language(id),
	name						varchar(150) not null,

	primary key (account_type_id, language_id)
);

create index ix_acc_account_type_translation_language_id
    on acc_account_type_translation (language_id);

insert into acc_account_type_translation
    (account_type_id, language_id, name)
select
    account_type.id,
    language.id,
    translation.name
from
(
    values
        ('active',          'uz', 'Aktiv'),
        ('passive',         'uz', 'Passiv'),
        ('active_passive',  'uz', 'Aktiv-passiv'),
        ('counter_active',  'uz', 'Kontr-aktiv'),
        ('counter_passive', 'uz', 'Kontr-passiv'),
        ('tranzit',         'uz', 'Tranzit'),
        ('off_balance',     'uz', 'Balansdan tashqari'),

        ('active',          'ru', 'Активный'),
        ('passive',         'ru', 'Пассивный'),
        ('active_passive',  'ru', 'Активно-пассивный'),
        ('counter_active',  'ru', 'Контрактивный'),
        ('counter_passive', 'ru', 'Контрпассивный'),
        ('tranzit',         'ru', 'Транзитный'),
        ('off_balance',     'ru', 'Забалансовый'),

        ('active',          'en', 'Active'),
        ('passive',         'en', 'Passive'),
        ('active_passive',  'en', 'Active-passive'),
        ('counter_active',  'en', 'Contra-asset'),
        ('counter_passive', 'en', 'Contra-liability'),
        ('tranzit',         'en', 'Transit'),
        ('off_balance',     'en', 'Off-balance')
) as translation(account_type_code, language_code, name)
join acc_account_type account_type
    on account_type.code = translation.account_type_code
join cmn_language language
    on language.code = translation.language_code
on conflict (account_type_id, language_id)
do update set
    name = excluded.name;
