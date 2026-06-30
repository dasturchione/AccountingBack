-- Table: public.acc_chart_account_subkonto

CREATE TABLE public.acc_chart_account_subkonto (
    id integer NOT NULL,
    organization_id integer NOT NULL,
    account_id integer NOT NULL,
    subkonto_type_id smallint NOT NULL,
    sort_order integer DEFAULT 0 NOT NULL,
    is_required boolean DEFAULT true NOT NULL,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL
);

CREATE SEQUENCE public.acc_chart_account_subkonto_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.acc_chart_account_subkonto_id_seq OWNED BY public.acc_chart_account_subkonto.id;

ALTER TABLE ONLY public.acc_chart_account_subkonto ALTER COLUMN id SET DEFAULT nextval('public.acc_chart_account_subkonto_id_seq'::regclass);

SELECT pg_catalog.setval('public.acc_chart_account_subkonto_id_seq', 30, true);

ALTER TABLE ONLY public.acc_chart_account_subkonto
    ADD CONSTRAINT acc_chart_account_subkonto_pkey PRIMARY KEY (id);

CREATE INDEX idx_acc_chart_account_subkonto_account_id ON public.acc_chart_account_subkonto USING btree (account_id);

CREATE INDEX idx_acc_chart_account_subkonto_organization_id ON public.acc_chart_account_subkonto USING btree (organization_id);

CREATE INDEX idx_acc_chart_account_subkonto_type_id ON public.acc_chart_account_subkonto USING btree (subkonto_type_id);

CREATE UNIQUE INDEX idx_acc_chart_account_subkonto_unique ON public.acc_chart_account_subkonto USING btree (organization_id, account_id, subkonto_type_id);

ALTER TABLE ONLY public.acc_chart_account_subkonto
    ADD CONSTRAINT acc_chart_account_subkonto_account_id_fkey FOREIGN KEY (account_id) REFERENCES public.acc_chart_account(id) ON DELETE CASCADE;

ALTER TABLE ONLY public.acc_chart_account_subkonto
    ADD CONSTRAINT acc_chart_account_subkonto_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);

ALTER TABLE ONLY public.acc_chart_account_subkonto
    ADD CONSTRAINT acc_chart_account_subkonto_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);

ALTER TABLE ONLY public.acc_chart_account_subkonto
    ADD CONSTRAINT acc_chart_account_subkonto_subkonto_type_id_fkey FOREIGN KEY (subkonto_type_id) REFERENCES public.acc_subkonto_type(id);
