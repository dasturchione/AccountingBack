create table cmn_product_type
(
    id smallint not null primary key,
    code varchar(50) not null unique,
    name varchar(250) not null,
    is_service boolean not null default false,
    description varchar(1000) not null
);

insert into cmn_product_type (id, code, name, description, is_service) values
    (1,  'good',                 'Tovar',                          'Qayta sotish uchun sotib olingan tovar', false),
    (2,  'good_retail',          'Chakana savdo tovari',           'Chakana savdo tarmog''i orqali sotiladigan tovar', false),
    (3,  'good_other',           'Boshqa tovar',                   'Ko''rgazmadagi, yo''ldagi, komissiyaga yuborilgan tovar va h.k.', false),
    (4,  'material_raw',         'Xomashyo va materiallar',        'Ishlab chiqarishda ishlatiladigan asosiy xomashyo', false),
    (5,  'material_component',   'Sotib olingan yarim tayyor mahsulot va komplektlovchilar', 'Sotib olingan yarim tayyor buyumlar, konstruksiyalar va detallar', false),
    (6,  'material_spare_part',  'Ehtiyot qismlar',                'Uskunalarni ta''mirlash va texnik xizmat ko''rsatish uchun ehtiyot qismlar', false),
    (7,  'material_construction','Qurilish materiallari',          'Qurilish va ta''mirlash ishlari uchun materiallar', false),
    (8,  'material_packaging',   'Idish va idishbop materiallar',  'Qadoqlash materiallari va idishlar', false),
    (9,  'material_other',       'Boshqa materiallar',             'Boshqa guruhlarga kiritilmagan materiallar', false),
    (10, 'semi_finished',        'O''z ishlab chiqarishi yarim tayyor mahsuloti', 'Tashkilot tomonidan mustaqil ishlab chiqarilgan yarim tayyor mahsulot', false),
    (11, 'finished_goods',       'Tayyor mahsulot',                'Tashkilot ishlab chiqargan va sotishga tayyor mahsulot', false),
    (12, 'service_main',         'Xizmat (asosiy faoliyat)',       'Tashkilotning asosiy faoliyati doirasida ko''rsatiladigan xizmat', true),
    (13, 'service_toll',         'Tolling xizmati',                'Mijoz xomashyosini qayta ishlash xizmati', true),
    (14, 'service_auxiliary',    'Yordamchi xizmat',               'Yordamchi ishlab chiqarish xizmati', true),
    (15, 'service_maintenance',  'Xizmat ko''rsatuvchi xo''jalik',  'Xizmat ko''rsatuvchi bo''linmalar xizmatlari (oshxona, ijtimoiy ob''ektlar va h.k.)', true),
    (16, 'service_rental',       'Ijaraga berish xizmati',         'Buyumlarni ijaraga berish', true);
