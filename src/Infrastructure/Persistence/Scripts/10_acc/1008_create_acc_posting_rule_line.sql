
create table acc_posting_rule_line (
    id integer   not null,
    template_id smallint not null,
    order_number smallint not null,
    debit_alias character varying(250) not null,
    credit_alias character varying(250) not null,
    amount_source character varying(20),
    is_optional boolean default true not null,
    constraint acc_posting_rule_line_pkey primary key (id),
    constraint acc_posting_rule_line_template_id_fkey foreign key (template_id) references acc_posting_rule(id)
);

insert into acc_posting_rule_line (id, template_id, order_number, debit_alias, credit_alias, amount_source, is_optional) values
    ('1', '1', '1', 'Inventory', 'Supplier', 'Base', 'f'),
    ('2', '1', '2', 'VATIn', 'Supplier', 'VAT', 'f'),
    ('3', '2', '1', 'Expense', 'Supplier', 'Base', 'f'),
    ('4', '2', '2', 'VATIn', 'Supplier', 'VAT', 'f'),
    ('5', '3', '1', 'Customer', 'SalesRevenue', 'Base', 'f'),
    ('6', '3', '2', 'Customer', 'VATOut', 'VAT', 'f'),
    ('7', '3', '3', 'CostOfGoods', 'Inventory', 'Cost', 'f'),
    ('8', '4', '1', 'Customer', 'ServiceRevenue', 'Base', 'f'),
    ('9', '4', '2', 'Customer', 'VATOut', 'VAT', 'f'),
    ('10', '4', '3', 'CostOfService', 'AssetWriteOff', 'Cost', 't'),
    ('11', '5', '1', 'PaymentAccount', 'Customer', 'Total', 't'),
    ('12', '5', '1', 'PaymentAccount', 'CustomerAdvance', 'Total', 't'),
    ('13', '5', '1', 'PaymentAccount', 'LoanReceived', 'Total', 't'),
    ('14', '5', '1', 'PaymentAccount', 'EmployeeAdvance', 'Total', 't'),
    ('15', '6', '1', 'Supplier', 'PaymentAccount', 'Total', 't'),
    ('16', '6', '1', 'SupplierAdvance', 'PaymentAccount', 'Total', 't'),
    ('17', '6', '1', 'Employee', 'PaymentAccount', 'Total', 't'),
    ('18', '6', '1', 'EmployeeAdvance', 'PaymentAccount', 'Total', 't'),
    ('19', '6', '1', 'Founder', 'PaymentAccount', 'Total', 't'),
    ('20', '6', '1', 'TaxAuthority', 'PaymentAccount', 'Total', 't'),
    ('21', '6', '1', 'LoanGiven', 'PaymentAccount', 'Total', 't');

insert into acc_posting_rule_line (id, template_id, order_number, debit_alias, credit_alias, amount_source, is_optional) values ('22', '7', '1', 'CashBoxSource', 'CashBoxDestination', 'Total', 'f');

