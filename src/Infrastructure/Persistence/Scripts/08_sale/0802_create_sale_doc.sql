-- Table: public.sale_doc

CREATE TABLE public.sale_doc (
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

CREATE SEQUENCE public.sale_doc_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.sale_doc_id_seq OWNED BY public.sale_doc.id;

ALTER TABLE ONLY public.sale_doc ALTER COLUMN id SET DEFAULT nextval('public.sale_doc_id_seq'::regclass);

CREATE SEQUENCE public.doc_number_sale_seq
    START WITH 100000001
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

CREATE FUNCTION public.set_sale_doc_number() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
begin
    new.doc_number := lpad(nextval('doc_number_sale_seq')::text, 9, '0');
    return new;
end;
$$;


SET default_tablespace = '';

SET default_table_access_method = heap;

insert into public.sale_doc (id, organization_id, doc_number, doc_date, counterparty_id, warehouse_id, currency_id, total_amount, vat_amount, final_amount, status_id, comment, state_id, created_date, contract_id) values
    ('78', '8', '100000076', '2026-06-29 16:42:30.843443', '18', '7', '1', '18975.00000000', '2277.00000000', '21252.00000000', '4', NULL, '1', '2026-06-29 16:42:30.843443', '9'),
    ('79', '8', '100000077', '2026-06-29 17:21:54.01643', '18', '7', '1', '65296.00000000', '7835.52000000', '73131.52000000', '1', 'FIFO bo''yicha sotilyapti', '2', '2026-06-29 17:21:54.01643', '9'),
    ('80', '8', '100000078', '2026-06-29 17:36:29.055026', '18', '7', '1', '65296.00000000', '7835.52000000', '73131.52000000', '1', NULL, '1', '2026-06-29 17:36:29.055026', '10');

SELECT pg_catalog.setval('public.doc_number_sale_seq', 100000078, true);

SELECT pg_catalog.setval('public.sale_doc_id_seq', 80, true);

ALTER TABLE ONLY public.sale_doc
    ADD CONSTRAINT sale_doc_pkey PRIMARY KEY (id);

CREATE INDEX idx_sale_doc_contract_id ON public.sale_doc USING btree (contract_id) WHERE (contract_id IS NOT NULL);

CREATE INDEX idx_sale_doc_counterparty_id ON public.sale_doc USING btree (counterparty_id);

CREATE INDEX idx_sale_doc_doc_date ON public.sale_doc USING btree (doc_date);

CREATE INDEX idx_sale_doc_organization_id ON public.sale_doc USING btree (organization_id);

CREATE INDEX idx_sale_doc_state_id ON public.sale_doc USING btree (state_id);

CREATE INDEX idx_sale_doc_status_id ON public.sale_doc USING btree (status_id);

CREATE INDEX idx_sale_doc_warehouse_id ON public.sale_doc USING btree (warehouse_id);

CREATE TRIGGER set_sale_doc_number_trigger BEFORE INSERT ON public.sale_doc FOR EACH ROW EXECUTE FUNCTION public.set_sale_doc_number();

ALTER TABLE ONLY public.sale_doc
    ADD CONSTRAINT sale_doc_contract_id_fkey FOREIGN KEY (contract_id) REFERENCES public.cmn_contract(id);

ALTER TABLE ONLY public.sale_doc
    ADD CONSTRAINT sale_doc_counterparty_id_fkey FOREIGN KEY (counterparty_id) REFERENCES public.counterparty_card(id);

ALTER TABLE ONLY public.sale_doc
    ADD CONSTRAINT sale_doc_currency_id_fkey FOREIGN KEY (currency_id) REFERENCES public.cmn_currency(id);

ALTER TABLE ONLY public.sale_doc
    ADD CONSTRAINT sale_doc_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);

ALTER TABLE ONLY public.sale_doc
    ADD CONSTRAINT sale_doc_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);

ALTER TABLE ONLY public.sale_doc
    ADD CONSTRAINT sale_doc_status_id_fkey FOREIGN KEY (status_id) REFERENCES public.cmn_document_status(id);

ALTER TABLE ONLY public.sale_doc
    ADD CONSTRAINT sale_doc_warehouse_id_fkey FOREIGN KEY (warehouse_id) REFERENCES public.inv_warehouse(id);
