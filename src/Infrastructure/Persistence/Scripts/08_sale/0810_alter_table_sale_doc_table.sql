alter table sale_doc_product
    alter column cost_price type numeric(24, 8),
    alter column amount type numeric(24, 8),
    alter column vat_amount type numeric(24, 8),
    alter column total_amount type numeric(24, 8);
