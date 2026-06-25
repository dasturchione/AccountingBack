alter table pur_doc_table
    drop constraint if exists chk_pur_doc_table_item_type_fields;

alter table pur_doc_table
    drop constraint if exists pur_doc_table_service_id_fkey;

drop index if exists idx_pur_doc_table_service_id;
drop index if exists idx_pur_doc_table_expense_account_id;

alter table pur_doc_table
    drop column if exists service_name,
    drop column if exists expense_account_id,
    drop column if exists service_id;

alter table pur_doc_table
    add column if not exists service_id bigint null references pur_service(id);

alter table pur_doc_table
    add constraint chk_pur_doc_table_item_type_fields
        check (
            (
                item_type_id = 1
                and product_table_id is not null
                and service_id is null
            )
            or
            (
                item_type_id = 2
                and product_table_id is null
                and service_id is not null
            )
        );

create index if not exists idx_pur_doc_table_service_id
    on pur_doc_table(service_id);
