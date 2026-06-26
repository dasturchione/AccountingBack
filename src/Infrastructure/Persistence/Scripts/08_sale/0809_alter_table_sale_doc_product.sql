alter table sale_doc_product
    alter column quantity type numeric(19, 6),
    alter column unit_price type numeric(24, 8),
    alter column cost_price type numeric(24, 8),
    alter column amount type numeric(24, 8),
    alter column vat_amount type numeric(24, 8),
    alter column total_amount type numeric(24, 8);

alter table sale_doc_product
add column unit_id small_int not null references cmn_unit (id);