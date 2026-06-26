alter table pur_doc
alter column total_amount type numeric(24, 8);

alter table pur_doc
alter column vat_amount type numeric(24, 8);

alter table pur_doc 
alter column final_amount type numeric(24, 8);