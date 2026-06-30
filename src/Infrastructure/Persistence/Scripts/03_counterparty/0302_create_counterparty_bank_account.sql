-- Table: public.counterparty_bank_account

CREATE TABLE public.counterparty_bank_account (
    id integer NOT NULL,
    organization_id integer NOT NULL,
    counterparty_id integer NOT NULL,
    bank_id integer NOT NULL,
    account_number character varying(50) NOT NULL,
    currency_id smallint NOT NULL,
    is_main boolean DEFAULT false NOT NULL,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL
);

CREATE SEQUENCE public.counterparty_bank_account_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.counterparty_bank_account_id_seq OWNED BY public.counterparty_bank_account.id;

ALTER TABLE ONLY public.counterparty_bank_account ALTER COLUMN id SET DEFAULT nextval('public.counterparty_bank_account_id_seq'::regclass);

SELECT pg_catalog.setval('public.counterparty_bank_account_id_seq', 1, false);

ALTER TABLE ONLY public.counterparty_bank_account
    ADD CONSTRAINT counterparty_bank_account_pkey PRIMARY KEY (id);

CREATE INDEX idx_counterparty_bank_account_bank_id ON public.counterparty_bank_account USING btree (bank_id);

CREATE INDEX idx_counterparty_bank_account_counterparty_id ON public.counterparty_bank_account USING btree (counterparty_id);

CREATE INDEX idx_counterparty_bank_account_currency_id ON public.counterparty_bank_account USING btree (currency_id);

CREATE INDEX idx_counterparty_bank_account_organization_id ON public.counterparty_bank_account USING btree (organization_id);

CREATE INDEX idx_counterparty_bank_account_state_id ON public.counterparty_bank_account USING btree (state_id);

ALTER TABLE ONLY public.counterparty_bank_account
    ADD CONSTRAINT counterparty_bank_account_bank_id_fkey FOREIGN KEY (bank_id) REFERENCES public.cmn_bank(id);

ALTER TABLE ONLY public.counterparty_bank_account
    ADD CONSTRAINT counterparty_bank_account_counterparty_id_fkey FOREIGN KEY (counterparty_id) REFERENCES public.counterparty_card(id);

ALTER TABLE ONLY public.counterparty_bank_account
    ADD CONSTRAINT counterparty_bank_account_currency_id_fkey FOREIGN KEY (currency_id) REFERENCES public.cmn_currency(id);

ALTER TABLE ONLY public.counterparty_bank_account
    ADD CONSTRAINT counterparty_bank_account_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);

ALTER TABLE ONLY public.counterparty_bank_account
    ADD CONSTRAINT counterparty_bank_account_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);
