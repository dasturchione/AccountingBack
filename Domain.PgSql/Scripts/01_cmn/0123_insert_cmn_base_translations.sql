insert into cmn_translation (language_id, table_name, record_id, column_name, value)
select l.id, 'cmn_currency', c.id, 'name', v.value
from (values
	('uz', 'UZS', 'So''m'),
	('ru', 'UZS', 'Узбекский сум'),
	('en', 'UZS', 'Uzbek sum'),
	('uz', 'USD', 'AQSH dollari'),
	('ru', 'USD', 'Доллар США'),
	('en', 'USD', 'US Dollar'),
	('uz', 'RUB', 'Rossiya rubli'),
	('ru', 'RUB', 'Российский рубль'),
	('en', 'RUB', 'Russian Ruble'),
	('uz', 'EUR', 'Yevro'),
	('ru', 'EUR', 'Евро'),
	('en', 'EUR', 'Euro')
) as v(language_code, code, value)
join cmn_language l on l.code = v.language_code
join cmn_currency c on c.code = v.code
on conflict (language_id, table_name, record_id, column_name)
do update set value = excluded.value;

insert into cmn_translation (language_id, table_name, record_id, column_name, value)
select l.id, 'cmn_unit', u.id, 'name', v.value
from (values
	('uz', 'dona', 'Dona'),
	('ru', 'dona', 'Штука'),
	('en', 'dona', 'Piece'),
	('uz', 'kg', 'Kilogram'),
	('ru', 'kg', 'Килограмм'),
	('en', 'kg', 'Kilogram'),
	('uz', 'litr', 'Litr'),
	('ru', 'litr', 'Литр'),
	('en', 'litr', 'Liter'),
	('uz', 'metr', 'Metr'),
	('ru', 'metr', 'Метр'),
	('en', 'metr', 'Meter'),
	('uz', 'xizmat', 'Xizmat'),
	('ru', 'xizmat', 'Услуга'),
	('en', 'xizmat', 'Service')
) as v(language_code, code, value)
join cmn_language l on l.code = v.language_code
join cmn_unit u on u.code = v.code
on conflict (language_id, table_name, record_id, column_name)
do update set value = excluded.value;

insert into cmn_translation (language_id, table_name, record_id, column_name, value)
select l.id, 'cmn_document_status', s.id, 'name', v.value
from (values
	('uz', 'draft', 'Qoralama'),
	('ru', 'draft', 'Черновик'),
	('en', 'draft', 'Draft'),
	('uz', 'posted', 'O''tkazilgan'),
	('ru', 'posted', 'Проведён'),
	('en', 'posted', 'Posted'),
	('uz', 'cancelled', 'Bekor qilingan'),
	('ru', 'cancelled', 'Отменён'),
	('en', 'cancelled', 'Cancelled')
) as v(language_code, code, value)
join cmn_language l on l.code = v.language_code
join cmn_document_status s on s.code = v.code
on conflict (language_id, table_name, record_id, column_name)
do update set value = excluded.value;

insert into cmn_translation (language_id, table_name, record_id, column_name, value)
select l.id, 'cmn_counterparty_type', t.id, 'name', v.value
from (values
	('uz', 'client', 'Mijoz'),
	('ru', 'client', 'Клиент'),
	('en', 'client', 'Client'),
	('uz', 'supplier', 'Yetkazib beruvchi'),
	('ru', 'supplier', 'Поставщик'),
	('en', 'supplier', 'Supplier'),
	('uz', 'client_supplier', 'Mijoz va yetkazib beruvchi'),
	('ru', 'client_supplier', 'Клиент и поставщик'),
	('en', 'client_supplier', 'Client and supplier')
) as v(language_code, code, value)
join cmn_language l on l.code = v.language_code
join cmn_counterparty_type t on t.code = v.code
on conflict (language_id, table_name, record_id, column_name)
do update set value = excluded.value;

insert into cmn_translation (language_id, table_name, record_id, column_name, value)
select l.id, 'cmn_payment_type', p.id, 'name', v.value
from (values
	('uz', 'cash', 'Naqd'),
	('ru', 'cash', 'Наличные'),
	('en', 'cash', 'Cash'),
	('uz', 'bank', 'Bank'),
	('ru', 'bank', 'Банк'),
	('en', 'bank', 'Bank'),
	('uz', 'card', 'Karta'),
	('ru', 'card', 'Карта'),
	('en', 'card', 'Card'),
	('uz', 'transfer', 'O''tkazma'),
	('ru', 'transfer', 'Перевод'),
	('en', 'transfer', 'Transfer')
) as v(language_code, code, value)
join cmn_language l on l.code = v.language_code
join cmn_payment_type p on p.code = v.code
on conflict (language_id, table_name, record_id, column_name)
do update set value = excluded.value;
