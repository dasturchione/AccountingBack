-- Table: public.counterparty_card

CREATE TABLE public.counterparty_card (
    id integer NOT NULL,
    organization_id integer NOT NULL,
    counterparty_type_id smallint NOT NULL,
    short_name character varying(250) NOT NULL,
    full_name character varying(500),
    inn character varying(20),
    phone_number character varying(50),
    email character varying(250),
    region_id integer,
    district_id integer,
    address character varying(1000),
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL
);

CREATE SEQUENCE public.counterparty_card_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.counterparty_card_id_seq OWNED BY public.counterparty_card.id;

ALTER TABLE ONLY public.counterparty_card ALTER COLUMN id SET DEFAULT nextval('public.counterparty_card_id_seq'::regclass);

insert into public.counterparty_card (id, organization_id, counterparty_type_id, short_name, full_name, inn, phone_number, email, region_id, district_id, address, state_id, created_date) values
    ('16', '8', '1', 'as', 'Asta', '12132145631', '+998 99 890-08-58', NULL, '4', '77', 'Navoiy, Uzbekistan', '1', '2026-06-20 15:37:24.758641'),
    ('17', '8', '2', 'aaaa', 'Shaxriddinbek', '12132145631', '+998 99 890-08-58', NULL, '3', '58', 'Navoiy, Uzbekistan', '1', '2026-06-20 16:03:26.590196'),
    ('18', '8', '2', 'Artel', 'Artel', '222222222', '+998 00 222-00-22', NULL, '2', '51', '', '1', '2026-06-20 17:54:14.649893'),
    ('19', '8', '1', 'Farrux Tech', 'Farrux Tech', '999888777', '+998 00 111-44-11', NULL, '8', '129', '', '1', '2026-06-20 18:20:49.024257'),
    ('20', '8', '3', 'Ava', 'Avalon', '22618000562088110001', '+998 99 556-56-88', NULL, '4', '78', 'Navoiy, Uzbekistan', '1', '2026-06-24 18:15:21.1843'),
    ('21', '8', '1', 'Aval', 'Avaloncha', '20208000005157348001', '+998 99 890-08-58', NULL, '3', '59', 'Navoiy, Uzbekistan', '1', '2026-06-25 11:14:04.759077');

SELECT pg_catalog.setval('public.counterparty_card_id_seq', 21, true);

ALTER TABLE ONLY public.counterparty_card
    ADD CONSTRAINT counterparty_card_pkey PRIMARY KEY (id);

CREATE INDEX idx_counterparty_card_district_id ON public.counterparty_card USING btree (district_id);

CREATE INDEX idx_counterparty_card_inn ON public.counterparty_card USING btree (inn);

CREATE INDEX idx_counterparty_card_organization_id ON public.counterparty_card USING btree (organization_id);

CREATE INDEX idx_counterparty_card_region_id ON public.counterparty_card USING btree (region_id);

CREATE INDEX idx_counterparty_card_short_name ON public.counterparty_card USING btree (short_name);

CREATE INDEX idx_counterparty_card_state_id ON public.counterparty_card USING btree (state_id);

CREATE INDEX idx_counterparty_card_type_id ON public.counterparty_card USING btree (counterparty_type_id);

ALTER TABLE ONLY public.counterparty_card
    ADD CONSTRAINT counterparty_card_counterparty_type_id_fkey FOREIGN KEY (counterparty_type_id) REFERENCES public.cmn_counterparty_type(id);

ALTER TABLE ONLY public.counterparty_card
    ADD CONSTRAINT counterparty_card_district_id_fkey FOREIGN KEY (district_id) REFERENCES public.cmn_district(id);

ALTER TABLE ONLY public.counterparty_card
    ADD CONSTRAINT counterparty_card_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);

ALTER TABLE ONLY public.counterparty_card
    ADD CONSTRAINT counterparty_card_region_id_fkey FOREIGN KEY (region_id) REFERENCES public.cmn_region(id);

ALTER TABLE ONLY public.counterparty_card
    ADD CONSTRAINT counterparty_card_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);
