-- Table: public.counterparty_contact

CREATE TABLE public.counterparty_contact (
    id integer NOT NULL,
    organization_id integer NOT NULL,
    counterparty_id integer NOT NULL,
    full_name character varying(250) NOT NULL,
    phone_number character varying(50),
    email character varying(250),
    "position" character varying(250),
    comment character varying(1000),
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL
);

CREATE SEQUENCE public.counterparty_contact_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.counterparty_contact_id_seq OWNED BY public.counterparty_contact.id;

ALTER TABLE ONLY public.counterparty_contact ALTER COLUMN id SET DEFAULT nextval('public.counterparty_contact_id_seq'::regclass);

SELECT pg_catalog.setval('public.counterparty_contact_id_seq', 6, true);

ALTER TABLE ONLY public.counterparty_contact
    ADD CONSTRAINT counterparty_contact_pkey PRIMARY KEY (id);

CREATE INDEX idx_counterparty_contact_counterparty_id ON public.counterparty_contact USING btree (counterparty_id);

CREATE INDEX idx_counterparty_contact_organization_id ON public.counterparty_contact USING btree (organization_id);

CREATE INDEX idx_counterparty_contact_state_id ON public.counterparty_contact USING btree (state_id);

ALTER TABLE ONLY public.counterparty_contact
    ADD CONSTRAINT counterparty_contact_counterparty_id_fkey FOREIGN KEY (counterparty_id) REFERENCES public.counterparty_card(id);

ALTER TABLE ONLY public.counterparty_contact
    ADD CONSTRAINT counterparty_contact_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);

ALTER TABLE ONLY public.counterparty_contact
    ADD CONSTRAINT counterparty_contact_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);
