create table fa_receipt_type
(
	id					smallserial primary key,
	code  				varchar(30) not null unique,
	name				varchar(100) not null
);

insert into fa_receipt_type (id, code, name)
values
    (1, 'PURCHASE',     'Sotib olish'),
    (2, 'CONSTRUCTION', 'Qurilish'),
    (3, 'OTHER',        'Boshqa')
on conflict (code) do nothing;
