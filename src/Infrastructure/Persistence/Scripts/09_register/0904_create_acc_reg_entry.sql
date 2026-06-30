-- Table: public.acc_reg_entry

CREATE TABLE public.acc_reg_entry (
    id bigint NOT NULL,
    organization_id integer NOT NULL,
    document_type_id smallint NOT NULL,
    document_id bigint NOT NULL,
    debit_account_id integer,
    credit_account_id integer,
    currency_id smallint NOT NULL,
    amount numeric(18,2) NOT NULL,
    doc_date timestamp without time zone NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL,
    operation_type_id smallint,
    debit_quantity numeric(18,3),
    credit_quantity numeric(18,3),
    content character varying(1000),
    journal_number character varying(100),
    posting_batch_id bigint,
    source_line_id bigint,
    reversal_entry_id bigint
);

CREATE SEQUENCE public.acc_reg_entry_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.acc_reg_entry_id_seq OWNED BY public.acc_reg_entry.id;

ALTER TABLE ONLY public.acc_reg_entry ALTER COLUMN id SET DEFAULT nextval('public.acc_reg_entry_id_seq'::regclass);

insert into public.acc_reg_entry (id, organization_id, document_type_id, document_id, debit_account_id, credit_account_id, currency_id, amount, doc_date, created_date, operation_type_id, debit_quantity, credit_quantity, content, journal_number) values
    ('234', '8', '1', '92', '1014', '1036', '1', '40000.00', '2026-06-27 18:05:43', '2026-06-27 13:07:18.414255', NULL, '4.000', NULL, 'Поступление товара', 'PUR-2026-000001'),
    ('235', '8', '1', '92', '1017', '1036', '1', '4800.00', '2026-06-27 18:05:43', '2026-06-27 13:07:18.419823', NULL, '4.000', NULL, 'Поступление товара', 'PUR-2026-000001'),
    ('236', '8', '1', '93', '1035', '1036', '1', '10000.00', '2026-06-27 18:07:17', '2026-06-27 13:07:50.821646', NULL, NULL, NULL, 'Получение услуги', 'PUR-2026-000001'),
    ('237', '8', '1', '93', '1017', '1036', '1', '1200.00', '2026-06-27 18:07:17', '2026-06-27 13:07:50.821707', NULL, NULL, NULL, 'Получение услуги', 'PUR-2026-000001'),
    ('238', '8', '1', '94', '1035', '1036', '1', '10000.00', '2026-06-29 10:06:32', '2026-06-29 05:13:54.422249', NULL, NULL, NULL, 'Получение услуги', 'PUR-2026-000001'),
    ('239', '8', '1', '94', '1017', '1036', '1', '1200.00', '2026-06-29 10:06:32', '2026-06-29 05:13:54.422277', NULL, NULL, NULL, 'Получение услуги', 'PUR-2026-000001'),
    ('240', '8', '1', '95', '1014', '1036', '1', '33000.00', '2026-06-15 14:58:00', '2026-06-29 09:59:15.866571', NULL, '3.000', NULL, 'Поступление товара', 'PUR-2026-000001'),
    ('241', '8', '1', '95', '1017', '1036', '1', '3960.00', '2026-06-15 14:58:00', '2026-06-29 09:59:15.87216', NULL, '3.000', NULL, 'Поступление товара', 'PUR-2026-000001'),
    ('242', '8', '1', '96', '1014', '1036', '1', '30000.00', '2026-06-29 14:59:15', '2026-06-29 10:01:54.065351', NULL, '2.000', NULL, 'Поступление товара', 'PUR-2026-000001'),
    ('243', '8', '1', '96', '1017', '1036', '1', '4500.00', '2026-06-29 14:59:15', '2026-06-29 10:01:54.065385', NULL, '2.000', NULL, 'Поступление товара', 'PUR-2026-000001'),
    ('244', '8', '1', '97', '1035', '1036', '1', '20000.00', '2026-06-29 17:55:00', '2026-06-29 12:57:52.692291', NULL, NULL, NULL, 'Получение услуги', 'PUR-2026-000001'),
    ('245', '8', '1', '97', '1017', '1036', '1', '2400.00', '2026-06-29 17:55:00', '2026-06-29 12:57:52.698531', NULL, NULL, NULL, 'Получение услуги', 'PUR-2026-000001');

SELECT pg_catalog.setval('public.acc_reg_entry_id_seq', 245, true);

ALTER TABLE ONLY public.acc_reg_entry
    ADD CONSTRAINT acc_reg_entry_pkey PRIMARY KEY (id);

CREATE INDEX idx_acc_reg_entry_credit_account_id ON public.acc_reg_entry USING btree (credit_account_id);

CREATE INDEX idx_acc_reg_entry_currency_id ON public.acc_reg_entry USING btree (currency_id);

CREATE INDEX idx_acc_reg_entry_debit_account_id ON public.acc_reg_entry USING btree (debit_account_id);

CREATE INDEX idx_acc_reg_entry_doc_date ON public.acc_reg_entry USING btree (doc_date);

CREATE INDEX idx_acc_reg_entry_document ON public.acc_reg_entry USING btree (document_type_id, document_id);

CREATE INDEX idx_acc_reg_entry_journal_number ON public.acc_reg_entry USING btree (journal_number);

CREATE INDEX idx_acc_reg_entry_operation_type_id ON public.acc_reg_entry USING btree (operation_type_id);

CREATE INDEX idx_acc_reg_entry_organization_id ON public.acc_reg_entry USING btree (organization_id);

ALTER TABLE ONLY public.acc_reg_entry
    ADD CONSTRAINT acc_reg_entry_credit_account_id_fkey FOREIGN KEY (credit_account_id) REFERENCES public.acc_chart_account(id);

ALTER TABLE ONLY public.acc_reg_entry
    ADD CONSTRAINT acc_reg_entry_currency_id_fkey FOREIGN KEY (currency_id) REFERENCES public.cmn_currency(id);

ALTER TABLE ONLY public.acc_reg_entry
    ADD CONSTRAINT acc_reg_entry_debit_account_id_fkey FOREIGN KEY (debit_account_id) REFERENCES public.acc_chart_account(id);

ALTER TABLE ONLY public.acc_reg_entry
    ADD CONSTRAINT acc_reg_entry_document_type_id_fkey FOREIGN KEY (document_type_id) REFERENCES public.cmn_document_type(id);

ALTER TABLE ONLY public.acc_reg_entry
    ADD CONSTRAINT acc_reg_entry_operation_type_id_fkey FOREIGN KEY (operation_type_id) REFERENCES public.cmn_operation_type(id);

ALTER TABLE ONLY public.acc_reg_entry
    ADD CONSTRAINT acc_reg_entry_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);

CREATE INDEX idx_acc_reg_entry_posting_batch_id ON public.acc_reg_entry USING btree (posting_batch_id);

CREATE INDEX idx_acc_reg_entry_reversal_entry_id ON public.acc_reg_entry USING btree (reversal_entry_id);

ALTER TABLE ONLY public.acc_reg_entry
    ADD CONSTRAINT acc_reg_entry_posting_batch_id_fkey FOREIGN KEY (posting_batch_id) REFERENCES public.acc_posting_batch(id);

ALTER TABLE ONLY public.acc_reg_entry
    ADD CONSTRAINT acc_reg_entry_reversal_entry_id_fkey FOREIGN KEY (reversal_entry_id) REFERENCES public.acc_reg_entry(id);
