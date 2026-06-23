insert into acc_posting_rule
(document_type_id, operation_type_id, code, name, state_id)
select
	dt.id,
	null,
	'purchase_service',
	'Xarid hujjati bo''yicha xizmat xarajati',
	1
from cmn_document_type dt
where dt.code = 'purchase'
  and not exists (
	select 1
	from acc_posting_rule r
	where r.document_type_id = dt.id
	  and r.operation_type_id is null
	  and r.code = 'purchase_service'
);

insert into acc_posting_rule_line
(
	rule_id,
	sort_order,
	debit_account_id,
	credit_account_id,
	debit_account_source,
	credit_account_source,
	amount_source,
	quantity_source,
	content_template,
	state_id
)
select
	r.id,
	v.sort_order,
	debit_account.id,
	credit_account.id,
	v.debit_account_source,
	v.credit_account_source,
	v.amount_source,
	v.quantity_source,
	v.content_template,
	1
from acc_posting_rule r
join (
	values
	(1, null,   '6010', 'expense_account_id', null, 'amount',     null, 'Xizmat xarajatga olindi'),
	(2, '4410', '6010', null,                 null, 'vat_amount', null, 'Xizmat bo''yicha QQS ajratildi')
) as v(sort_order, debit_account_code, credit_account_code, debit_account_source, credit_account_source, amount_source, quantity_source, content_template)
	on true
left join acc_chart_account debit_account
	on debit_account.code = v.debit_account_code
join acc_chart_account credit_account
	on credit_account.code = v.credit_account_code
where r.code = 'purchase_service'
  and not exists (
	select 1
	from acc_posting_rule_line line
	where line.rule_id = r.id
	  and line.sort_order = v.sort_order
);

