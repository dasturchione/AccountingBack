create table rtl_payment_method_translation
(
    payment_method_id       smallint not null references rtl_payment_method(id) on delete cascade,
    language_id 		    smallint not null references cmn_language(id) on delete cascade,
    name                    varchar(250) not null,

    primary key (payment_method_id, language_id)
);

insert into rtl_payment_method_translation
(
    payment_method_id,
    language_id,
    name
)
select
    pm.id,
    l.id,
    v.name
from (
    values
        ('CASH',     'uz', 'Naqd pul'),
        ('CASH',     'ru', 'Наличные'),
        ('CASH',     'en', 'Cash'),

        ('CARD',     'uz', 'Bank kartasi'),
        ('CARD',     'ru', 'Банковская карта'),
        ('CARD',     'en', 'Bank card'),

        ('TRANSFER', 'uz', 'Bank o''tkazmasi'),
        ('TRANSFER', 'ru', 'Банковский перевод'),
        ('TRANSFER', 'en', 'Bank transfer'),

        ('CLICK',    'uz', 'Click'),
        ('CLICK',    'ru', 'Click'),
        ('CLICK',    'en', 'Click'),

        ('PAYME',    'uz', 'Payme'),
        ('PAYME',    'ru', 'Payme'),
        ('PAYME',    'en', 'Payme'),
        
        ('MOBILE_PAYMENT', 'uz', 'Mobil to‘lov'),
        ('MOBILE_PAYMENT', 'ru', 'Мобильная оплата'),
        ('MOBILE_PAYMENT', 'en', 'Mobile payment'),

        ('OTHER',    'uz', 'Boshqa'),
        ('OTHER',    'ru', 'Прочее'),
        ('OTHER',    'en', 'Other')
) as v(payment_method_code, language_code, name)
join rtl_payment_method pm
    on pm.code = v.payment_method_code
join cmn_language l
    on l.code = v.language_code
on conflict (payment_method_id, language_id)
do update set
    name = excluded.name;