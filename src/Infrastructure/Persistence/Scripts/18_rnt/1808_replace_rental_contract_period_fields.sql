begin;

alter table rnt_contract_object
    drop constraint if exists ck_rnt_contract_object_contract_amount;

alter table rnt_contract_object
    rename column contract_amount to period_amount;

alter table rnt_contract_object
    drop column period_value;

alter table rnt_contract_object
    add constraint ck_rnt_contract_object_period_amount
        check (period_amount >= 0);

commit;
