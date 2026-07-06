create table acc_posting_rule_line
(
    id integer not null,
    template_id smallint not null,
    order_number smallint not null,
    debit_alias_id smallint not null,
    credit_alias_id smallint not null,
    amount_source character varying(20),
    is_optional boolean default true not null,
    constraint acc_posting_rule_line_pkey primary key (id),
    constraint acc_posting_rule_line_template_id_fkey foreign key (template_id) references acc_posting_rule(id),
    constraint fk_acc_posting_rule_line_debit_alias foreign key (debit_alias_id) references acc_posting_alias(id),
    constraint fk_acc_posting_rule_line_credit_alias foreign key (credit_alias_id) references acc_posting_alias(id)
);

insert into acc_posting_rule_line (id, template_id, order_number, debit_alias_id, credit_alias_id, amount_source, 
    is_optional
) values
    (1,  1, 1, (select id from acc_posting_alias where code = 'Inventory'), (select id from acc_posting_alias where code = 'Supplier'), 'Base',  false),
    (2,  1, 2, (select id from acc_posting_alias where code = 'VATIn'), (select id from acc_posting_alias where code = 'Supplier'), 'VAT',   false),
    (3,  2, 1, (select id from acc_posting_alias where code = 'Expense'), (select id from acc_posting_alias where code = 'Supplier'), 'Base',  false),
    (4,  2, 2, (select id from acc_posting_alias where code = 'VATIn'), (select id from acc_posting_alias where code = 'Supplier'), 'VAT',   false),
    (5,  3, 1, (select id from acc_posting_alias where code = 'Customer'), (select id from acc_posting_alias where code = 'SalesRevenue'), 'Base',  false),
    (6,  3, 2, (select id from acc_posting_alias where code = 'Customer'), (select id from acc_posting_alias where code = 'VATOut'), 'VAT',   false),
    (7,  3, 3, (select id from acc_posting_alias where code = 'CostOfGoods'), (select id from acc_posting_alias where code = 'Inventory'), 'Cost',  false),
    (8,  4, 1, (select id from acc_posting_alias where code = 'Customer'), (select id from acc_posting_alias where code = 'ServiceRevenue'), 'Base',  false),
    (9,  4, 2, (select id from acc_posting_alias where code = 'Customer'), (select id from acc_posting_alias where code = 'VATOut'), 'VAT',   false),
    (10, 4, 3, (select id from acc_posting_alias where code = 'CostOfService'), (select id from acc_posting_alias where code = 'AssetWriteOff'), 'Cost',  true),
    (11, 5, 1, (select id from acc_posting_alias where code = 'PaymentAccount'), (select id from acc_posting_alias where code = 'Customer'), 'Total', true),
    (12, 5, 1, (select id from acc_posting_alias where code = 'PaymentAccount'), (select id from acc_posting_alias where code = 'CustomerAdvance'), 'Total', true),
    (13, 5, 1, (select id from acc_posting_alias where code = 'PaymentAccount'), (select id from acc_posting_alias where code = 'LoanReceived'), 'Total', true),
    (14, 5, 1, (select id from acc_posting_alias where code = 'PaymentAccount'), (select id from acc_posting_alias where code = 'EmployeeAdvance'), 'Total', true),
    (15, 6, 1, (select id from acc_posting_alias where code = 'Supplier'), (select id from acc_posting_alias where code = 'PaymentAccount'), 'Total', true),
    (16, 6, 1, (select id from acc_posting_alias where code = 'SupplierAdvance'), (select id from acc_posting_alias where code = 'PaymentAccount'), 'Total', true),
    (17, 6, 1, (select id from acc_posting_alias where code = 'Employee'), (select id from acc_posting_alias where code = 'PaymentAccount'), 'Total', true),
    (18, 6, 1, (select id from acc_posting_alias where code = 'EmployeeAdvance'), (select id from acc_posting_alias where code = 'PaymentAccount'), 'Total', true),
    (19, 6, 1, (select id from acc_posting_alias where code = 'Founder'), (select id from acc_posting_alias where code = 'PaymentAccount'), 'Total', true),
    (21, 6, 1, (select id from acc_posting_alias where code = 'LoanGiven'), (select id from acc_posting_alias where code = 'PaymentAccount'), 'Total', true),
    (22, 7, 1, (select id from acc_posting_alias where code = 'CashBoxSource'), (select id from acc_posting_alias where code = 'CashBoxDestination'), 'Total', false),
    (23, 8, 1, (select id from acc_posting_alias where code = 'CurrencyAsset'), (select id from acc_posting_alias where code = 'CurrencyRevaluationGain'), 'Total', false),
    (24, 9, 1, (select id from acc_posting_alias where code = 'CurrencyRevaluationLoss'), (select id from acc_posting_alias where code = 'CurrencyAsset'), 'Total', false);
