insert into acc_chart_account_subkonto
(organization_id, account_id, subkonto_type_id, sort_order, is_required, state_id)
select a.organization_id, a.id, st.id, v.sort_order, true, 1
from (
	values
	('2910', 'product', 1),
	('2910', 'warehouse', 2),
	('6010', 'counterparty', 1),
	('6010', 'document', 2),
	('4010', 'counterparty', 1),
	('4010', 'document', 2),
	('5010', 'cash_box', 1),
	('5110', 'bank_account', 1),
	('4410', 'tax', 1),
	('4410', 'document', 2)
) as v(account_code, subkonto_type_code, sort_order)
join acc_chart_account a on a.code = v.account_code
join acc_subkonto_type st on st.code = v.subkonto_type_code
where not exists (
	select 1
	from acc_chart_account_subkonto s
	where s.organization_id = a.organization_id
		and s.account_id = a.id
		and s.subkonto_type_id = st.id
);
