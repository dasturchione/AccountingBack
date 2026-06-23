insert into acc_chart_account
(organization_id, code, name, is_group, account_type_id, is_quantity, is_currency, state_id)
select o.id, v.code, v.name, false, at.id, v.is_quantity, v.is_currency, 1
from org_organization o
join (
	values
	('2010', 'Asosiy ishlab chiqarish', 'active', false, true),
	('9420', 'Ma''muriy xarajatlar', 'active', false, true),
	('9430', 'Boshqa operatsion xarajatlar', 'active', false, true)
) as v(code, name, account_type_code, is_quantity, is_currency) on true
join acc_account_type at on at.code = v.account_type_code
where not exists (
	select 1
	from acc_chart_account a
	where a.organization_id = o.id
		and a.code = v.code
);

