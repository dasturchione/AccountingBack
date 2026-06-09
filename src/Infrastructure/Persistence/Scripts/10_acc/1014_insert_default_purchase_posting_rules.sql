insert into acc_posting_rule
(organization_id, document_type_id, operation_type_id, code, name, state_id)
select o.id, dt.id, null, 'purchase_goods', 'Xarid hujjati bo''yicha tovar kirimi', 1
from org_organization o
join cmn_document_type dt on dt.code = 'purchase'
where not exists (
	select 1
	from acc_posting_rule r
	where r.organization_id = o.id
		and r.document_type_id = dt.id
		and r.operation_type_id is null
		and r.code = 'purchase_goods'
);

insert into acc_posting_rule_line
(rule_id, sort_order, debit_account_id, credit_account_id, amount_source, quantity_source, content_template, state_id)
select r.id, v.sort_order, debit_account.id, credit_account.id, v.amount_source, v.quantity_source, v.content_template, 1
from acc_posting_rule r
join cmn_document_type dt on dt.id = r.document_type_id and dt.code = 'purchase'
join (
	values
	(1, '2910', '6010', 'amount', 'quantity', 'Tovarlar kirim qilindi'),
	(2, '4410', '6010', 'vat_amount', null, 'QQS ajratildi')
) as v(sort_order, debit_account_code, credit_account_code, amount_source, quantity_source, content_template) on true
join acc_chart_account debit_account on debit_account.organization_id = r.organization_id
	and debit_account.code = v.debit_account_code
join acc_chart_account credit_account on credit_account.organization_id = r.organization_id
	and credit_account.code = v.credit_account_code
where r.code = 'purchase_goods'
	and r.organization_id is not null
	and not exists (
		select 1
		from acc_posting_rule_line line
		where line.rule_id = r.id
			and line.sort_order = v.sort_order
	);
