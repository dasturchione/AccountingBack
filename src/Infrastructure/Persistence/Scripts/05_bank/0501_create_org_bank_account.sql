-- Table: public.org_bank_account

CREATE TABLE public.org_bank_account (
    id integer NOT NULL,
    organization_id integer NOT NULL,
    bank_id integer NOT NULL,
    account_number character varying(50) NOT NULL,
    currency_id smallint NOT NULL,
    is_main boolean DEFAULT false NOT NULL,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL
);

CREATE SEQUENCE public.org_bank_account_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.org_bank_account_id_seq OWNED BY public.org_bank_account.id;

ALTER TABLE ONLY public.org_bank_account ALTER COLUMN id SET DEFAULT nextval('public.org_bank_account_id_seq'::regclass);

insert into public.org_bank_account (id, organization_id, bank_id, account_number, currency_id, is_main, state_id, created_date) values
    ('1', '2', '1', '064232', '4', 't', '1', '2026-06-08 10:34:07.891936'),
    ('8', '8', '4', '0099855144112', '3', 't', '1', '2026-06-24 11:59:17.881525'),
    ('9', '8', '4', '0099855144117', '3', 't', '1', '2026-06-24 15:34:17.666512'),
    ('10', '8', '3', '064232347878', '3', 't', '1', '2026-06-24 15:34:56.767807'),
    ('11', '8', '3', '23106000105157348001', '1', 't', '1', '2026-06-24 17:01:04.482009'),
    ('12', '8', '2', '20208000005157348001', '1', 't', '1', '2026-06-24 17:57:51.101968');

SELECT pg_catalog.setval('public.org_bank_account_id_seq', 12, true);

ALTER TABLE ONLY public.org_bank_account
    ADD CONSTRAINT org_bank_account_pkey PRIMARY KEY (id);

CREATE INDEX idx_org_bank_account_bank_id ON public.org_bank_account USING btree (bank_id);

CREATE INDEX idx_org_bank_account_currency_id ON public.org_bank_account USING btree (currency_id);

CREATE INDEX idx_org_bank_account_organization_id ON public.org_bank_account USING btree (organization_id);

CREATE INDEX idx_org_bank_account_state_id ON public.org_bank_account USING btree (state_id);

ALTER TABLE ONLY public.org_bank_account
    ADD CONSTRAINT org_bank_account_bank_id_fkey FOREIGN KEY (bank_id) REFERENCES public.cmn_bank(id);

ALTER TABLE ONLY public.org_bank_account
    ADD CONSTRAINT org_bank_account_currency_id_fkey FOREIGN KEY (currency_id) REFERENCES public.cmn_currency(id);

ALTER TABLE ONLY public.org_bank_account
    ADD CONSTRAINT org_bank_account_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);

ALTER TABLE ONLY public.org_bank_account
    ADD CONSTRAINT org_bank_account_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);
