insert into acc_chart_account
(organization_id, code, name, is_group, account_type_id, is_quantity, is_currency, state_id)
select o.id, v.code, v.name, false, at.id, v.is_quantity, v.is_currency, 1
from org_organization o
join (
	values
	('2910', 'Tovarlar', 'active', true, false),
	('4410', 'QQS bo''yicha hisob-kitoblar', 'active', false, false),
	('6010', 'Yetkazib beruvchilar bilan hisob-kitoblar', 'passive', false, true),
	('4010', 'Xaridorlar bilan hisob-kitoblar', 'active', false, true),
	('5010', 'Kassa', 'active', false, true),
	('5110', 'Hisob-kitob schyoti', 'active', false, true),
	('9020', 'Tovarlarni sotishdan tushum', 'passive', false, true),
	('9120', 'Sotilgan tovarlar tannarxi', 'active', false, true)
) as v(code, name, account_type_code, is_quantity, is_currency) on true
join acc_account_type at on at.code = v.account_type_code
where not exists (
	select 1
	from acc_chart_account a
	where a.organization_id = o.id
		and a.code = v.code
);
