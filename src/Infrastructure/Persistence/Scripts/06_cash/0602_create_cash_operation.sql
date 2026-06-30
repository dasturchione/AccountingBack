-- Table: public.cash_operation

CREATE TABLE public.cash_operation (
    id bigint NOT NULL,
    organization_id integer NOT NULL,
    cash_box_id integer NOT NULL,
    operation_type_id smallint NOT NULL,
    payment_type_id smallint,
    counterparty_id integer,
    doc_number character varying(100) NOT NULL,
    doc_date timestamp without time zone NOT NULL,
    currency_id smallint NOT NULL,
    amount numeric(18,2) NOT NULL,
    comment character varying(1000),
    status_id smallint NOT NULL,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL,
    exchange_rate numeric(18,6) DEFAULT 1 NOT NULL,
    posted_at timestamp without time zone,
    posted_by_user_id integer,
    cancelled_at timestamp without time zone,
    cancelled_by_user_id integer
);

CREATE SEQUENCE public.cash_operation_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.cash_operation_id_seq OWNED BY public.cash_operation.id;

ALTER TABLE ONLY public.cash_operation ALTER COLUMN id SET DEFAULT nextval('public.cash_operation_id_seq'::regclass);

CREATE SEQUENCE public.doc_number_cash_operation_seq
    START WITH 100000001
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

CREATE FUNCTION public.set_cash_operation_doc_number() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
begin
    new.doc_number := lpad(nextval('doc_number_cash_operation_seq')::text, 9, '0');
    return new;
end;
$$;

SELECT pg_catalog.setval('public.cash_operation_id_seq', 2, true);

SELECT pg_catalog.setval('public.doc_number_cash_operation_seq', 100000000, true);

ALTER TABLE ONLY public.cash_operation
    ADD CONSTRAINT cash_operation_pkey PRIMARY KEY (id);

CREATE INDEX idx_cash_operation_cash_box_id ON public.cash_operation USING btree (cash_box_id);

CREATE INDEX idx_cash_operation_counterparty_id ON public.cash_operation USING btree (counterparty_id);

CREATE INDEX idx_cash_operation_doc_date ON public.cash_operation USING btree (doc_date);

CREATE INDEX idx_cash_operation_operation_type_id ON public.cash_operation USING btree (operation_type_id);

CREATE INDEX idx_cash_operation_organization_id ON public.cash_operation USING btree (organization_id);

CREATE INDEX idx_cash_operation_state_id ON public.cash_operation USING btree (state_id);

CREATE INDEX idx_cash_operation_status_id ON public.cash_operation USING btree (status_id);

CREATE TRIGGER set_cash_operation_doc_number_trigger BEFORE INSERT ON public.cash_operation FOR EACH ROW EXECUTE FUNCTION public.set_cash_operation_doc_number();

ALTER TABLE ONLY public.cash_operation
    ADD CONSTRAINT cash_operation_cash_box_id_fkey FOREIGN KEY (cash_box_id) REFERENCES public.cash_box(id);

ALTER TABLE ONLY public.cash_operation
    ADD CONSTRAINT cash_operation_counterparty_id_fkey FOREIGN KEY (counterparty_id) REFERENCES public.counterparty_card(id);

ALTER TABLE ONLY public.cash_operation
    ADD CONSTRAINT cash_operation_currency_id_fkey FOREIGN KEY (currency_id) REFERENCES public.cmn_currency(id);

ALTER TABLE ONLY public.cash_operation
    ADD CONSTRAINT cash_operation_operation_type_id_fkey FOREIGN KEY (operation_type_id) REFERENCES public.cmn_operation_type(id);

ALTER TABLE ONLY public.cash_operation
    ADD CONSTRAINT cash_operation_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);

ALTER TABLE ONLY public.cash_operation
    ADD CONSTRAINT cash_operation_payment_type_id_fkey FOREIGN KEY (payment_type_id) REFERENCES public.cmn_payment_type(id);

ALTER TABLE ONLY public.cash_operation
    ADD CONSTRAINT cash_operation_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);

ALTER TABLE ONLY public.cash_operation
    ADD CONSTRAINT cash_operation_status_id_fkey FOREIGN KEY (status_id) REFERENCES public.cmn_document_status(id);

CREATE INDEX idx_cash_operation_posted_by_user_id ON public.cash_operation USING btree (posted_by_user_id);

CREATE INDEX idx_cash_operation_cancelled_by_user_id ON public.cash_operation USING btree (cancelled_by_user_id);

ALTER TABLE ONLY public.cash_operation
    ADD CONSTRAINT cash_operation_posted_by_user_id_fkey FOREIGN KEY (posted_by_user_id) REFERENCES public.sys_user(id);

ALTER TABLE ONLY public.cash_operation
    ADD CONSTRAINT cash_operation_cancelled_by_user_id_fkey FOREIGN KEY (cancelled_by_user_id) REFERENCES public.sys_user(id);
