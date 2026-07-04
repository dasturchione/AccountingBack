insert into acc_posting_rule (id, code, name) values
    ('8', 'CURRENCY_REVALUATION_GAIN', 'Currency revaluation gain');

insert into acc_posting_rule (id, code, name) values
    ('9', 'CURRENCY_REVALUATION_LOSS', 'Currency revaluation loss');

insert into acc_posting_rule_line (id, template_id, order_number, debit_alias, credit_alias, amount_source, is_optional) values
    ('23', '8', '1', 'CurrencyAsset', 'CurrencyRevaluationGain', 'Total', 'f');

insert into acc_posting_rule_line (id, template_id, order_number, debit_alias, credit_alias, amount_source, is_optional) values
    ('24', '9', '1', 'CurrencyRevaluationLoss', 'CurrencyAsset', 'Total', 'f');
