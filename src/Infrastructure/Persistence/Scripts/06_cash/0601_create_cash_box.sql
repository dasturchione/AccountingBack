-- Table: public.cash_box

CREATE TABLE public.cash_box (
    id integer NOT NULL,
    organization_id integer NOT NULL,
    branch_id integer,
    code character varying(50) NOT NULL,
    name character varying(250) NOT NULL,
    currency_id smallint NOT NULL,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL
);

CREATE SEQUENCE public.cash_box_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.cash_box_id_seq OWNED BY public.cash_box.id;

ALTER TABLE ONLY public.cash_box ALTER COLUMN id SET DEFAULT nextval('public.cash_box_id_seq'::regclass);

SELECT pg_catalog.setval('public.cash_box_id_seq', 3, true);

ALTER TABLE ONLY public.cash_box
    ADD CONSTRAINT cash_box_pkey PRIMARY KEY (id);

CREATE INDEX idx_cash_box_branch_id ON public.cash_box USING btree (branch_id);

CREATE INDEX idx_cash_box_currency_id ON public.cash_box USING btree (currency_id);

CREATE UNIQUE INDEX idx_cash_box_org_code ON public.cash_box USING btree (organization_id, code);

CREATE INDEX idx_cash_box_organization_id ON public.cash_box USING btree (organization_id);

CREATE INDEX idx_cash_box_state_id ON public.cash_box USING btree (state_id);

ALTER TABLE ONLY public.cash_box
    ADD CONSTRAINT cash_box_branch_id_fkey FOREIGN KEY (branch_id) REFERENCES public.org_branch(id);

ALTER TABLE ONLY public.cash_box
    ADD CONSTRAINT cash_box_currency_id_fkey FOREIGN KEY (currency_id) REFERENCES public.cmn_currency(id);

ALTER TABLE ONLY public.cash_box
    ADD CONSTRAINT cash_box_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);

ALTER TABLE ONLY public.cash_box
    ADD CONSTRAINT cash_box_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);
