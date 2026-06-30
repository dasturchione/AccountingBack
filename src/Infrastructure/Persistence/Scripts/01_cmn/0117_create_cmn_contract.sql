-- Table: public.cmn_contract

CREATE TABLE public.cmn_contract (
    id bigint NOT NULL,
    organization_id integer NOT NULL,
    counterparty_id integer NOT NULL,
    contract_number character varying(100) NOT NULL,
    contract_date timestamp without time zone NOT NULL,
    start_date timestamp without time zone,
    end_date timestamp without time zone,
    comment character varying(1000),
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL,
    contract_type_id smallint NOT NULL
);

CREATE SEQUENCE public.cmn_contract_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.cmn_contract_id_seq OWNED BY public.cmn_contract.id;

ALTER TABLE ONLY public.cmn_contract ALTER COLUMN id SET DEFAULT nextval('public.cmn_contract_id_seq'::regclass);

CREATE SEQUENCE public.contract_number_seq
    START WITH 100000001
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

CREATE FUNCTION public.set_cmn_contract_number() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
begin
    new.contract_number := lpad(nextval('contract_number_seq')::text, 9, '0');
    return new;
end;
$$;

insert into public.cmn_contract (id, organization_id, counterparty_id, contract_number, contract_date, start_date, end_date, comment, state_id, created_date, contract_type_id) values
    ('7', '8', '16', '100000007', '2026-06-20 15:35:42', '2026-06-20 00:00:00', '2027-08-26 23:59:59.999999', '', '1', '2026-06-20 15:39:04.060566', '2'),
    ('9', '8', '18', '100000009', '2026-05-20 12:58:18.282', '2026-05-20 00:00:00', '2026-09-20 23:59:59.999999', 'string', '1', '2026-06-20 18:00:25.144259', '1'),
    ('10', '8', '18', '100000010', '2026-06-23 05:58:06', '2026-06-23 05:58:06', '2026-07-31 06:02:00', '', '1', '2026-06-23 06:02:11.011106', '2');

SELECT pg_catalog.setval('public.cmn_contract_id_seq', 10, true);

SELECT pg_catalog.setval('public.contract_number_seq', 100000010, true);

ALTER TABLE ONLY public.cmn_contract
    ADD CONSTRAINT cmn_contract_pkey PRIMARY KEY (id);

CREATE INDEX idx_cmn_contract_contract_date ON public.cmn_contract USING btree (contract_date);

CREATE INDEX idx_cmn_contract_contract_type_id ON public.cmn_contract USING btree (contract_type_id);

CREATE INDEX idx_cmn_contract_counterparty_id ON public.cmn_contract USING btree (counterparty_id);

CREATE UNIQUE INDEX idx_cmn_contract_number ON public.cmn_contract USING btree (organization_id, counterparty_id, contract_number);

CREATE INDEX idx_cmn_contract_organization_id ON public.cmn_contract USING btree (organization_id);

CREATE INDEX idx_cmn_contract_state_id ON public.cmn_contract USING btree (state_id);

CREATE TRIGGER set_cmn_contract_number_trigger BEFORE INSERT ON public.cmn_contract FOR EACH ROW EXECUTE FUNCTION public.set_cmn_contract_number();

ALTER TABLE ONLY public.cmn_contract
    ADD CONSTRAINT cmn_contract_contract_type_id_fkey FOREIGN KEY (contract_type_id) REFERENCES public.cmn_contract_type(id);

ALTER TABLE ONLY public.cmn_contract
    ADD CONSTRAINT cmn_contract_counterparty_id_fkey FOREIGN KEY (counterparty_id) REFERENCES public.counterparty_card(id);

ALTER TABLE ONLY public.cmn_contract
    ADD CONSTRAINT cmn_contract_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);

ALTER TABLE ONLY public.cmn_contract
    ADD CONSTRAINT cmn_contract_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);
