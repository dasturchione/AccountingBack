-- Table: public.money_reg_balance

CREATE TABLE public.money_reg_balance (
    id bigint NOT NULL,
    organization_id integer NOT NULL,
    document_type_id smallint NOT NULL,
    document_id bigint NOT NULL,
    source_type character varying(20) NOT NULL,
    source_id integer NOT NULL,
    operation_type_id smallint NOT NULL,
    currency_id smallint NOT NULL,
    amount numeric(18,2) NOT NULL,
    doc_date timestamp without time zone NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL,
    posting_batch_id bigint,
    source_line_id bigint,
    reversal_entry_id bigint
);

CREATE SEQUENCE public.money_reg_balance_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.money_reg_balance_id_seq OWNED BY public.money_reg_balance.id;

ALTER TABLE ONLY public.money_reg_balance ALTER COLUMN id SET DEFAULT nextval('public.money_reg_balance_id_seq'::regclass);

insert into public.money_reg_balance (id, organization_id, document_type_id, document_id, source_type, source_id, operation_type_id, currency_id, amount, doc_date, created_date) values
    ('1', '2', '1', '1', 'KassaUzb', '1', '1', '1', '800000.00', '2026-06-08 05:56:31.616', '2026-06-08 10:59:21.788241');

SELECT pg_catalog.setval('public.money_reg_balance_id_seq', 1, true);

ALTER TABLE ONLY public.money_reg_balance
    ADD CONSTRAINT money_reg_balance_pkey PRIMARY KEY (id);

CREATE INDEX idx_money_reg_balance_currency_id ON public.money_reg_balance USING btree (currency_id);

CREATE INDEX idx_money_reg_balance_doc_date ON public.money_reg_balance USING btree (doc_date);

CREATE INDEX idx_money_reg_balance_document ON public.money_reg_balance USING btree (document_type_id, document_id);

CREATE INDEX idx_money_reg_balance_organization_id ON public.money_reg_balance USING btree (organization_id);

CREATE INDEX idx_money_reg_balance_source ON public.money_reg_balance USING btree (source_type, source_id);

ALTER TABLE ONLY public.money_reg_balance
    ADD CONSTRAINT money_reg_balance_currency_id_fkey FOREIGN KEY (currency_id) REFERENCES public.cmn_currency(id);

ALTER TABLE ONLY public.money_reg_balance
    ADD CONSTRAINT money_reg_balance_document_type_id_fkey FOREIGN KEY (document_type_id) REFERENCES public.cmn_document_type(id);

ALTER TABLE ONLY public.money_reg_balance
    ADD CONSTRAINT money_reg_balance_operation_type_id_fkey FOREIGN KEY (operation_type_id) REFERENCES public.cmn_operation_type(id);

ALTER TABLE ONLY public.money_reg_balance
    ADD CONSTRAINT money_reg_balance_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);

CREATE INDEX idx_money_reg_balance_posting_batch_id ON public.money_reg_balance USING btree (posting_batch_id);

CREATE INDEX idx_money_reg_balance_reversal_entry_id ON public.money_reg_balance USING btree (reversal_entry_id);

ALTER TABLE ONLY public.money_reg_balance
    ADD CONSTRAINT money_reg_balance_posting_batch_id_fkey FOREIGN KEY (posting_batch_id) REFERENCES public.acc_posting_batch(id);
