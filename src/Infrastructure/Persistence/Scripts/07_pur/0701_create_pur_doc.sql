-- Table: public.pur_doc

CREATE TABLE public.pur_doc (
    id bigint NOT NULL,
    organization_id integer NOT NULL,
    doc_number character varying(100) NOT NULL,
    doc_date timestamp without time zone NOT NULL,
    counterparty_id integer NOT NULL,
    warehouse_id integer NOT NULL,
    currency_id smallint NOT NULL,
    total_amount numeric(24,8) DEFAULT 0 NOT NULL,
    vat_amount numeric(24,8) DEFAULT 0 NOT NULL,
    final_amount numeric(24,8) DEFAULT 0 NOT NULL,
    status_id smallint NOT NULL,
    comment character varying(1000),
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL,
    contract_id bigint
);

CREATE SEQUENCE public.pur_doc_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.pur_doc_id_seq OWNED BY public.pur_doc.id;

ALTER TABLE ONLY public.pur_doc ALTER COLUMN id SET DEFAULT nextval('public.pur_doc_id_seq'::regclass);

CREATE SEQUENCE public.doc_number_purchase_seq
    START WITH 100000001
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

CREATE FUNCTION public.set_pur_doc_number() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
begin
    new.doc_number := lpad(nextval('doc_number_purchase_seq')::text, 9, '0');
    return new;
end;
$$;

insert into public.pur_doc (id, organization_id, doc_number, doc_date, counterparty_id, warehouse_id, currency_id, total_amount, vat_amount, final_amount, status_id, comment, state_id, created_date, contract_id) values
    ('92', '8', '100000074', '2026-06-27 18:05:43', '18', '7', '1', '40000.00000000', '4800.00000000', '44800.00000000', '1', NULL, '1', '2026-06-27 18:07:17.501272', '9'),
    ('93', '8', '100000075', '2026-06-27 18:07:17', '18', '7', '1', '10000.00000000', '1200.00000000', '11200.00000000', '1', NULL, '1', '2026-06-27 18:07:50.767417', '10'),
    ('94', '8', '100000076', '2026-06-29 10:06:32', '18', '7', '1', '10000.00000000', '1200.00000000', '11200.00000000', '1', NULL, '1', '2026-06-29 10:13:54.392765', '9'),
    ('95', '8', '100000077', '2026-06-15 14:58:00', '18', '7', '1', '33000.00000000', '3960.00000000', '36960.00000000', '1', NULL, '1', '2026-06-29 14:59:14.796206', '9'),
    ('96', '8', '100000078', '2026-06-29 14:59:15', '18', '7', '1', '30000.00000000', '4500.00000000', '34500.00000000', '1', NULL, '1', '2026-06-29 15:01:54.008454', '10'),
    ('97', '8', '100000079', '2026-06-29 17:55:00', '18', '7', '1', '20000.00000000', '2400.00000000', '22400.00000000', '1', NULL, '1', '2026-06-29 17:57:51.597146', '9');

SELECT pg_catalog.setval('public.doc_number_purchase_seq', 100000079, true);

SELECT pg_catalog.setval('public.pur_doc_id_seq', 97, true);

ALTER TABLE ONLY public.pur_doc
    ADD CONSTRAINT pur_doc_pkey PRIMARY KEY (id);

CREATE INDEX idx_pur_doc_contract_id ON public.pur_doc USING btree (contract_id);

CREATE INDEX idx_pur_doc_counterparty_id ON public.pur_doc USING btree (counterparty_id);

CREATE INDEX idx_pur_doc_doc_date ON public.pur_doc USING btree (doc_date);

CREATE INDEX idx_pur_doc_organization_id ON public.pur_doc USING btree (organization_id);

CREATE INDEX idx_pur_doc_state_id ON public.pur_doc USING btree (state_id);

CREATE INDEX idx_pur_doc_status_id ON public.pur_doc USING btree (status_id);

CREATE INDEX idx_pur_doc_warehouse_id ON public.pur_doc USING btree (warehouse_id);

CREATE TRIGGER set_pur_doc_number_trigger BEFORE INSERT ON public.pur_doc FOR EACH ROW EXECUTE FUNCTION public.set_pur_doc_number();

ALTER TABLE ONLY public.pur_doc
    ADD CONSTRAINT pur_doc_contract_id_fkey FOREIGN KEY (contract_id) REFERENCES public.cmn_contract(id);

ALTER TABLE ONLY public.pur_doc
    ADD CONSTRAINT pur_doc_counterparty_id_fkey FOREIGN KEY (counterparty_id) REFERENCES public.counterparty_card(id);

ALTER TABLE ONLY public.pur_doc
    ADD CONSTRAINT pur_doc_currency_id_fkey FOREIGN KEY (currency_id) REFERENCES public.cmn_currency(id);

ALTER TABLE ONLY public.pur_doc
    ADD CONSTRAINT pur_doc_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);

ALTER TABLE ONLY public.pur_doc
    ADD CONSTRAINT pur_doc_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);

ALTER TABLE ONLY public.pur_doc
    ADD CONSTRAINT pur_doc_status_id_fkey FOREIGN KEY (status_id) REFERENCES public.cmn_document_status(id);

ALTER TABLE ONLY public.pur_doc
    ADD CONSTRAINT pur_doc_warehouse_id_fkey FOREIGN KEY (warehouse_id) REFERENCES public.inv_warehouse(id);
