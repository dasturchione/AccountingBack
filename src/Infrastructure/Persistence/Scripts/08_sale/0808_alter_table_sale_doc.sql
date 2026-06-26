alter table sale_doc 
add column contract_id bigint null references cmn_contract (id);

create index idx_sale_doc_contract_id
    on sale_doc (contract_id)
    where contract_id is not null;

alter table sale_doc
alter column total_amount type numeric(24, 8);

alter table sale_doc
alter column vat_amount type numeric(24, 8);

alter table sale_doc 
alter column final_amount type numeric(24, 8);