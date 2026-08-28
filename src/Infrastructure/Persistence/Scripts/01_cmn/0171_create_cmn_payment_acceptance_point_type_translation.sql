create table cmn_payment_acceptance_point_type_translation
(
    payment_acceptance_point_type_id smallint not null references cmn_payment_acceptance_point_type(id) on delete cascade,
    language_id                      smallint not null references cmn_language(id),
    name                             varchar(150) not null,
    primary key (payment_acceptance_point_type_id, language_id)
);

insert into cmn_payment_acceptance_point_type_translation
    (payment_acceptance_point_type_id, language_id, name)
select point_type.id, language.id, translation.name
from
(
    values
        ('POS', 'uz', 'POS terminal'),
        ('POS', 'ru', 'POS-терминал'),
        ('POS', 'en', 'POS terminal'),
        ('QR', 'uz', 'QR to''lov'),
        ('QR', 'ru', 'QR-платёж'),
        ('QR', 'en', 'QR payment'),
        ('PAYMENT_LINK', 'uz', 'To''lov havolasi'),
        ('PAYMENT_LINK', 'ru', 'Платёжная ссылка'),
        ('PAYMENT_LINK', 'en', 'Payment link'),
        ('MARKETPLACE', 'uz', 'Marketpleys'),
        ('MARKETPLACE', 'ru', 'Маркетплейс'),
        ('MARKETPLACE', 'en', 'Marketplace'),
        ('MOBILE_APP', 'uz', 'Mobil ilova'),
        ('MOBILE_APP', 'ru', 'Мобильное приложение'),
        ('MOBILE_APP', 'en', 'Mobile application'),
        ('OTHER', 'uz', 'Boshqa'),
        ('OTHER', 'ru', 'Другое'),
        ('OTHER', 'en', 'Other')
) as translation(type_code, language_code, name)
join cmn_payment_acceptance_point_type point_type on point_type.code = translation.type_code
join cmn_language language on language.code = translation.language_code
on conflict (payment_acceptance_point_type_id, language_id) do update set
    name = excluded.name;
