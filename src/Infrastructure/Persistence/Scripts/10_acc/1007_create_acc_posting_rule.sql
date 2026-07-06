create table acc_posting_rule 
(
    id smallint   not null,
    code character varying(50) not null,
    name character varying(250) not null,
    constraint acc_posting_rule_code_key UNIQUE (code),
    constraint acc_posting_rule_pkey primary key (id)
);

insert into acc_posting_rule (id, code, name) values
    (1, 'PURCHASE_GOODS',            'Поступление товара'),
    (2, 'PURCHASE_SERVICE',          'Получение услуги'),
    (3, 'SALE_GOODS',                'Реализация товара'),
    (4, 'SALE_SERVICE',              'Оказанная услуга'),
    (5, 'DEBIT_OPERATION',           'Банковская/кассовая операция — приход'),
    (6, 'CREDIT_OPERATION',          'Банковская/кассовая операция — расход'),
    (7, 'CASH_TRANSFER',             'Kassoviy perevod'),
    (8, 'CURRENCY_REVALUATION_GAIN', 'Currency revaluation gain'),
    (9, 'CURRENCY_REVALUATION_LOSS', 'Currency revaluation loss');
