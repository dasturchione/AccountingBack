insert into cmn_posting_operation_type 
    (id, code, name, state_id)
values
    (1, 'PURCHASE_GOODS',           'Поступление товара',        1),
    (2, 'PURCHASE_SERVICE',         'Получение услуги',          1),
    (3, 'SALE_GOODS',               'Реализация товара',         1),
    (4, 'SALE_SERVICE',             'Оказанная услуга',          1),
    (5, 'CUSTOMER_PAYMENT_ADVANCE', 'Получен аванс от клиента',  1),
    (6, 'CUSTOMER_PAYMENT',         'Оплата от клиента',         1),
    (7, 'SUPPLIER_PAYMENT_ADVANCE', 'Выдан аванс поставщику',    1),
    (8, 'SUPPLIER_PAYMENT',         'Оплата поставщику',         1);