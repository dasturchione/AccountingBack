CREATE TABLE cmn_product_type
(
    id                          smallint        NOT NULL PRIMARY KEY,
    code                        varchar(50)     NOT NULL UNIQUE,
    name                        varchar(250)    NOT NULL,
    is_service                  boolean         NOT NULL DEFAULT false,
    description                 varchar(1000)   NOT NULL
);

create table cmn_product_type_translation
(
	product_type_id				smallint not null references cmn_product_type(id),
	language_id					smallint not null references cmn_language(id),
	name						varchar(250) not null,
	description					varchar(1000) not null,

	primary key (product_type_id, language_id)
); 

INSERT INTO cmn_product_type (id, code, name, description, is_service) VALUES
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

INSERT INTO cmn_product_type_translation (product_type_id, language_id, name, description) VALUES
    -- uz
    (1,  1, 'Tovar',                          'Qayta sotish uchun sotib olingan tovar'),
    (2,  1, 'Chakana savdo tovari',           'Chakana savdo tarmog''i orqali sotiladigan tovar'),
    (3,  1, 'Boshqa tovar',                   'Ko''rgazmadagi, yo''ldagi, komissiyaga yuborilgan tovar va h.k.'),
    (4,  1, 'Xomashyo va materiallar',        'Ishlab chiqarishda ishlatiladigan asosiy xomashyo'),
    (5,  1, 'Sotib olingan yarim tayyor mahsulot va komplektlovchilar', 'Sotib olingan yarim tayyor buyumlar, konstruksiyalar va detallar'),
    (6,  1, 'Ehtiyot qismlar',                'Uskunalarni ta''mirlash va texnik xizmat ko''rsatish uchun ehtiyot qismlar'),
    (7,  1, 'Qurilish materiallari',          'Qurilish va ta''mirlash ishlari uchun materiallar'),
    (8,  1, 'Idish va idishbop materiallar',  'Qadoqlash materiallari va idishlar'),
    (9,  1, 'Boshqa materiallar',             'Boshqa guruhlarga kiritilmagan materiallar'),
    (10, 1, 'O''z ishlab chiqarishi yarim tayyor mahsuloti', 'Tashkilot tomonidan mustaqil ishlab chiqarilgan yarim tayyor mahsulot'),
    (11, 1, 'Tayyor mahsulot',                'Tashkilot ishlab chiqargan va sotishga tayyor mahsulot'),
    (12, 1, 'Xizmat (asosiy faoliyat)',       'Tashkilotning asosiy faoliyati doirasida ko''rsatiladigan xizmat'),
    (13, 1, 'Tolling xizmati',                'Mijoz xomashyosini qayta ishlash xizmati'),
    (14, 1, 'Yordamchi xizmat',               'Yordamchi ishlab chiqarish xizmati'),
    (15, 1, 'Xizmat ko''rsatuvchi xo''jalik', 'Xizmat ko''rsatuvchi bo''linmalar xizmatlari (oshxona, ijtimoiy ob''ektlar va h.k.)'),
    (16, 1, 'Ijaraga berish xizmati',         'Buyumlarni ijaraga berish');

INSERT INTO cmn_product_type_translation (product_type_id, language_id, name, description) VALUES
    -- ru
    (1,  2, 'Товар',                                    'Товар, приобретённый для перепродажи'),
    (2,  2, 'Товар в розничной торговле',               'Товар, реализуемый через розничную сеть'),
    (3,  2, 'Прочий товар',                              'Товар на выставке, в пути, отгруженный на комиссию и пр.'),
    (4,  2, 'Сырьё и материалы',                        'Основное сырьё, используемое в производстве'),
    (5,  2, 'Покупные полуфабрикаты и комплектующие',   'Комплектующие изделия, конструкции и детали'),
    (6,  2, 'Запасные части',                            'Запчасти для ремонта и обслуживания оборудования'),
    (7,  2, 'Строительные материалы',                    'Материалы для строительных и ремонтных работ'),
    (8,  2, 'Тара и тарные материалы',                   'Упаковочные материалы и тара'),
    (9,  2, 'Прочие материалы',                          'Прочие материалы, не отнесённые к другим группам'),
    (10, 2, 'Полуфабрикат собственного производства',   'Полуфабрикаты, произведённые организацией самостоятельно'),
    (11, 2, 'Готовая продукция',                         'Продукция, выпущенная организацией и готовая к реализации'),
    (12, 2, 'Услуга (основная деятельность)',            'Услуга, оказываемая в рамках основной деятельности организации'),
    (13, 2, 'Переработка давальческого сырья',           'Услуга по переработке сырья, принадлежащего заказчику'),
    (14, 2, 'Вспомогательная услуга',                    'Услуга вспомогательного производства'),
    (15, 2, 'Обслуживающее производство/хозяйство',      'Услуги обслуживающих подразделений (столовая, соцобъекты и т.п.)'),
    (16, 2, 'Услуга проката',                            'Сдача предметов в прокат');

INSERT INTO cmn_product_type_translation (product_type_id, language_id, name, description) VALUES
    -- en
    (1,  3, 'Good',                              'Item purchased for resale'),
    (2,  3, 'Retail good',                       'Good sold through a retail outlet'),
    (3,  3, 'Other good',                        'Good on display, in transit, shipped on consignment, etc.'),
    (4,  3, 'Raw material',                      'Primary material used in production'),
    (5,  3, 'Purchased component',               'Purchased semi-finished parts, assemblies and details'),
    (6,  3, 'Spare part',                        'Spare part for equipment repair and maintenance'),
    (7,  3, 'Construction material',             'Material used for construction and repair works'),
    (8,  3, 'Packaging material',                'Packaging materials and containers'),
    (9,  3, 'Other material',                    'Materials not classified into other groups'),
    (10, 3, 'Semi-finished product',             'Semi-finished product manufactured in-house'),
    (11, 3, 'Finished goods',                    'Product manufactured by the organization and ready for sale'),
    (12, 3, 'Service (core activity)',           'Service provided as part of the core business activity'),
    (13, 3, 'Toll processing service',           'Service of processing customer-supplied raw materials'),
    (14, 3, 'Auxiliary service',                 'Service of an auxiliary production unit'),
    (15, 3, 'Servicing facility',                'Services of servicing units (canteen, social facilities, etc.)'),
    (16, 3, 'Rental service',                    'Renting out items');