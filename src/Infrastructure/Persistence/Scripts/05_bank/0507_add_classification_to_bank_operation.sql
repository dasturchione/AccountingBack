alter table bank_operation
    add column classification_category_id smallint references cmn_bank_operation_category(id),
    add column classification_rule_id integer references cmn_bank_operation_classification_rule(id);

create index ix_bank_operation_classification_category_id
    on bank_operation (classification_category_id);

create index ix_bank_operation_classification_rule_id
    on bank_operation (classification_rule_id);
