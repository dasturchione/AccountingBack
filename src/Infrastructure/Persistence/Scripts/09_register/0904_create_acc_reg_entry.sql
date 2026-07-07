create table acc_reg_entry 
(
    id bigint generated always as identity not null,
    organization_id integer not null,
    document_type_id smallint not null,
    document_id bigint not null,
    debit_account_id integer,
    credit_account_id integer,
    currency_id smallint not null,
    amount numeric(18,2) not null,
    doc_date timestamp without time zone not null,
    created_date timestamp without time zone default now() not null,
    operation_type_id smallint,
    debit_quantity numeric(18,3),
    credit_quantity numeric(18,3),
    content character varying(1000),
    journal_number character varying(100),
    posting_batch_id bigint,
    source_line_id bigint,
    reversal_entry_id bigint,
    constraint acc_reg_entry_pkey primary key (id),
    constraint acc_reg_entry_credit_account_id_fkey foreign key (credit_account_id) references acc_chart_account(id),
    constraint acc_reg_entry_currency_id_fkey foreign key (currency_id) references cmn_currency(id),
    constraint acc_reg_entry_debit_account_id_fkey foreign key (debit_account_id) references acc_chart_account(id),
    constraint acc_reg_entry_document_type_id_fkey foreign key (document_type_id) references cmn_document_type(id),
    constraint acc_reg_entry_operation_type_id_fkey foreign key (operation_type_id) references cmn_operation_type(id),
    constraint acc_reg_entry_organization_id_fkey foreign key (organization_id) references org_organization(id)
);

create index idx_acc_reg_entry_credit_account_id on acc_reg_entry using btree (credit_account_id);
create index idx_acc_reg_entry_currency_id on acc_reg_entry using btree (currency_id);
create index idx_acc_reg_entry_debit_account_id on acc_reg_entry using btree (debit_account_id);
create index idx_acc_reg_entry_doc_date on acc_reg_entry using btree (doc_date);
create index idx_acc_reg_entry_document on acc_reg_entry using btree (document_type_id, document_id);
create index idx_acc_reg_entry_journal_number on acc_reg_entry using btree (journal_number);
create index idx_acc_reg_entry_operation_type_id on acc_reg_entry using btree (operation_type_id);
create index idx_acc_reg_entry_organization_id on acc_reg_entry using btree (organization_id);
create index idx_acc_reg_entry_posting_batch_id on acc_reg_entry using btree (posting_batch_id);
create index idx_acc_reg_entry_reversal_entry_id on acc_reg_entry using btree (reversal_entry_id);

insert into acc_reg_entry (id, organization_id, document_type_id, document_id, debit_account_id, credit_account_id, currency_id, amount, doc_date, created_date, operation_type_id, debit_quantity, credit_quantity, content, journal_number) values
    ('234', '8', '1', '92', '1014', '1036', '1', '40000.00', '2026-06-27 18:05:43', '2026-06-27 13:07:18.414255', null, '4.000', null, 'Поступление товара', 'PUR-2026-000001'),
    ('235', '8', '1', '92', '1017', '1036', '1', '4800.00', '2026-06-27 18:05:43', '2026-06-27 13:07:18.419823', null, '4.000', null, 'Поступление товара', 'PUR-2026-000001'),
    ('236', '8', '1', '93', '1035', '1036', '1', '10000.00', '2026-06-27 18:07:17', '2026-06-27 13:07:50.821646', null, null, null, 'Получение услуги', 'PUR-2026-000001'),
    ('237', '8', '1', '93', '1017', '1036', '1', '1200.00', '2026-06-27 18:07:17', '2026-06-27 13:07:50.821707', null, null, null, 'Получение услуги', 'PUR-2026-000001'),
    ('238', '8', '1', '94', '1035', '1036', '1', '10000.00', '2026-06-29 10:06:32', '2026-06-29 05:13:54.422249', null, null, null, 'Получение услуги', 'PUR-2026-000001'),
    ('239', '8', '1', '94', '1017', '1036', '1', '1200.00', '2026-06-29 10:06:32', '2026-06-29 05:13:54.422277', null, null, null, 'Получение услуги', 'PUR-2026-000001'),
    ('240', '8', '1', '95', '1014', '1036', '1', '33000.00', '2026-06-15 14:58:00', '2026-06-29 09:59:15.866571', null, '3.000', null, 'Поступление товара', 'PUR-2026-000001'),
    ('241', '8', '1', '95', '1017', '1036', '1', '3960.00', '2026-06-15 14:58:00', '2026-06-29 09:59:15.87216', null, '3.000', null, 'Поступление товара', 'PUR-2026-000001'),
    ('242', '8', '1', '96', '1014', '1036', '1', '30000.00', '2026-06-29 14:59:15', '2026-06-29 10:01:54.065351', null, '2.000', null, 'Поступление товара', 'PUR-2026-000001'),
    ('243', '8', '1', '96', '1017', '1036', '1', '4500.00', '2026-06-29 14:59:15', '2026-06-29 10:01:54.065385', null, '2.000', null, 'Поступление товара', 'PUR-2026-000001'),
    ('244', '8', '1', '97', '1035', '1036', '1', '20000.00', '2026-06-29 17:55:00', '2026-06-29 12:57:52.692291', null, null, null, 'Получение услуги', 'PUR-2026-000001'),
    ('245', '8', '1', '97', '1017', '1036', '1', '2400.00', '2026-06-29 17:55:00', '2026-06-29 12:57:52.698531', null, null, null, 'Получение услуги', 'PUR-2026-000001');
