insert into acc_account_resolve_rule (policy_id, alias, dimension_key, dimension_value, account_id, priority)
select 1, 'CurrencyAsset', '_none', '_default', id, 100 from acc_chart_account where code = '5020';

insert into acc_account_resolve_rule (policy_id, alias, dimension_key, dimension_value, account_id, priority)
select 1, 'CurrencyRevaluationGain', '_none', '_default', id, 100 from acc_chart_account where code = '9030.1';

insert into acc_account_resolve_rule (policy_id, alias, dimension_key, dimension_value, account_id, priority)
select 1, 'CurrencyRevaluationLoss', '_none', '_default', id, 100 from acc_chart_account where code = '9430';
