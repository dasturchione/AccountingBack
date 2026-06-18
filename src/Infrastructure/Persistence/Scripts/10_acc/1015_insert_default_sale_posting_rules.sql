insert into acc_posting_rule
(document_type_id, operation_type_id, code, name, state_id)
select
    dt.id,
    null,
    'sale_goods',
    'Sotish hujjati bo''yicha tovar realizatsiyasi',
    1
from cmn_document_type dt
where dt.code = 'sale'
  and not exists (
      select 1
      from acc_posting_rule r
      where r.document_type_id = dt.id
        and r.operation_type_id is null
        and r.code = 'sale_goods'
  );

insert into acc_posting_rule_line
(
    rule_id,
    sort_order,
    debit_account_id,
    credit_account_id,
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
    v.amount_source,
    v.quantity_source,
    v.content_template,
    1
from acc_posting_rule r
join (
    values
        (1, '9120', '2910', 'cost_amount', 'quantity', 'Sotilgan tovar tannarxi hisobdan chiqarildi'),
        (2, '4010', '9020', 'amount',      null,       'Tovar sotildi'),
        (3, '4010', '6410', 'vat_amount',  null,       'QQS hisoblandi')
) as v(sort_order, debit_account_code, credit_account_code, amount_source, quantity_source, content_template)
    on true
join acc_chart_account debit_account
    on debit_account.code = v.debit_account_code
join acc_chart_account credit_account
    on credit_account.code = v.credit_account_code
where r.code = 'sale_goods'
  and not exists (
      select 1
      from acc_posting_rule_line l
      where l.rule_id = r.id
        and l.sort_order = v.sort_order
  );