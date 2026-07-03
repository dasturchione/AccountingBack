
create table counterparty_reg_balance (
    id bigint   not null,
    organization_id integer not null,
    document_type_id smallint not null,
    document_id bigint not null,
    counterparty_id integer not null,
    operation_type_id smallint not null,
    currency_id smallint not null,
    amount numeric(18,2) not null,
    doc_date timestamp without time zone not null,
    created_date timestamp without time zone default now() not null,
    posting_batch_id bigint,
    source_line_id bigint,
    reversal_entry_id bigint,
    constraint counterparty_reg_balance_pkey primary key (id),
    constraint counterparty_reg_balance_counterparty_id_fkey foreign key (counterparty_id) references counterparty_card(id),
    constraint counterparty_reg_balance_currency_id_fkey foreign key (currency_id) references cmn_currency(id),
    constraint counterparty_reg_balance_document_type_id_fkey foreign key (document_type_id) references cmn_document_type(id),
    constraint counterparty_reg_balance_operation_type_id_fkey foreign key (operation_type_id) references cmn_operation_type(id),
    constraint counterparty_reg_balance_organization_id_fkey foreign key (organization_id) references org_organization(id),
    constraint counterparty_reg_balance_posting_batch_id_fkey foreign key (posting_batch_id) references acc_posting_batch(id)
);

create index idx_counterparty_reg_balance_counterparty_id on counterparty_reg_balance using btree (counterparty_id);

create index idx_counterparty_reg_balance_currency_id on counterparty_reg_balance using btree (currency_id);

create index idx_counterparty_reg_balance_doc_date on counterparty_reg_balance using btree (doc_date);

create index idx_counterparty_reg_balance_document on counterparty_reg_balance using btree (document_type_id, document_id);

create index idx_counterparty_reg_balance_organization_id on counterparty_reg_balance using btree (organization_id);

create index idx_counterparty_reg_balance_posting_batch_id on counterparty_reg_balance using btree (posting_batch_id);

create index idx_counterparty_reg_balance_reversal_entry_id on counterparty_reg_balance using btree (reversal_entry_id);

