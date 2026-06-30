-- Table: public.counterparty_reg_balance

CREATE TABLE public.counterparty_reg_balance (
    id bigint NOT NULL,
    organization_id integer NOT NULL,
    document_type_id smallint NOT NULL,
    document_id bigint NOT NULL,
    counterparty_id integer NOT NULL,
    operation_type_id smallint NOT NULL,
    currency_id smallint NOT NULL,
    amount numeric(18,2) NOT NULL,
    doc_date timestamp without time zone NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL,
    posting_batch_id bigint,
    source_line_id bigint,
    reversal_entry_id bigint
);

CREATE SEQUENCE public.counterparty_reg_balance_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.counterparty_reg_balance_id_seq OWNED BY public.counterparty_reg_balance.id;

ALTER TABLE ONLY public.counterparty_reg_balance ALTER COLUMN id SET DEFAULT nextval('public.counterparty_reg_balance_id_seq'::regclass);

SELECT pg_catalog.setval('public.counterparty_reg_balance_id_seq', 1, true);

ALTER TABLE ONLY public.counterparty_reg_balance
    ADD CONSTRAINT counterparty_reg_balance_pkey PRIMARY KEY (id);

CREATE INDEX idx_counterparty_reg_balance_counterparty_id ON public.counterparty_reg_balance USING btree (counterparty_id);

CREATE INDEX idx_counterparty_reg_balance_currency_id ON public.counterparty_reg_balance USING btree (currency_id);

CREATE INDEX idx_counterparty_reg_balance_doc_date ON public.counterparty_reg_balance USING btree (doc_date);

CREATE INDEX idx_counterparty_reg_balance_document ON public.counterparty_reg_balance USING btree (document_type_id, document_id);

CREATE INDEX idx_counterparty_reg_balance_organization_id ON public.counterparty_reg_balance USING btree (organization_id);

ALTER TABLE ONLY public.counterparty_reg_balance
    ADD CONSTRAINT counterparty_reg_balance_counterparty_id_fkey FOREIGN KEY (counterparty_id) REFERENCES public.counterparty_card(id);

ALTER TABLE ONLY public.counterparty_reg_balance
    ADD CONSTRAINT counterparty_reg_balance_currency_id_fkey FOREIGN KEY (currency_id) REFERENCES public.cmn_currency(id);

ALTER TABLE ONLY public.counterparty_reg_balance
    ADD CONSTRAINT counterparty_reg_balance_document_type_id_fkey FOREIGN KEY (document_type_id) REFERENCES public.cmn_document_type(id);

ALTER TABLE ONLY public.counterparty_reg_balance
    ADD CONSTRAINT counterparty_reg_balance_operation_type_id_fkey FOREIGN KEY (operation_type_id) REFERENCES public.cmn_operation_type(id);

ALTER TABLE ONLY public.counterparty_reg_balance
    ADD CONSTRAINT counterparty_reg_balance_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);

CREATE INDEX idx_counterparty_reg_balance_posting_batch_id ON public.counterparty_reg_balance USING btree (posting_batch_id);

CREATE INDEX idx_counterparty_reg_balance_reversal_entry_id ON public.counterparty_reg_balance USING btree (reversal_entry_id);

ALTER TABLE ONLY public.counterparty_reg_balance
    ADD CONSTRAINT counterparty_reg_balance_posting_batch_id_fkey FOREIGN KEY (posting_batch_id) REFERENCES public.acc_posting_batch(id);
