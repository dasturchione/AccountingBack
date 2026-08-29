create unique index if not exists ux_bank_operation_active_bank_document
    on bank_operation
    (
        organization_id,
        bank_account_id,
        btrim(bank_document_number),
        (doc_date::date)
    )
    where state_id = 1
      and nullif(btrim(bank_document_number), '') is not null;
