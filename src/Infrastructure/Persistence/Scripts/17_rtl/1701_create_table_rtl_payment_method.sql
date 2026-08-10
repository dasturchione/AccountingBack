create table rtl_payment_method
(
    id              smallserial primary key,
    code            varchar(30) not null unique,
    name            varchar(250) not null
);

insert into rtl_payment_method (code, name)
values
    ('CASH',            'Naqd pul'          ),
    ('CARD',            'Bank kartasi'      ),
    ('TRANSFER',        'Bank o''tkazmasi'  ),
    ('CLICK',           'Click'             ),
    ('PAYME',           'Payme'             ),
    ('MOBILE_PAYMENT',  'Mobil to‘lov'      ),
    ('OTHER',           'Boshqa'            );