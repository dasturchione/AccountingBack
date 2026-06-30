-- Table: public.org_branch

CREATE TABLE public.org_branch (
    id integer NOT NULL,
    organization_id integer NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(250) NOT NULL,
    region_id integer,
    district_id integer,
    address character varying(1000),
    phone_number character varying(50),
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL
);

CREATE SEQUENCE public.org_branch_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.org_branch_id_seq OWNED BY public.org_branch.id;

ALTER TABLE ONLY public.org_branch ALTER COLUMN id SET DEFAULT nextval('public.org_branch_id_seq'::regclass);

insert into public.org_branch (id, organization_id, code, name, region_id, district_id, address, phone_number, state_id, created_date) values
    ('6', '8', 'Malibu9876', 'Atalik', '3', '58', NULL, '+998 99 890-08-58', '1', '2026-06-20 15:38:25.066056');

SELECT pg_catalog.setval('public.org_branch_id_seq', 6, true);

ALTER TABLE ONLY public.org_branch
    ADD CONSTRAINT org_branch_pkey PRIMARY KEY (id);

CREATE INDEX idx_org_branch_district_id ON public.org_branch USING btree (district_id);

CREATE UNIQUE INDEX idx_org_branch_org_code ON public.org_branch USING btree (organization_id, code);

CREATE INDEX idx_org_branch_organization_id ON public.org_branch USING btree (organization_id);

CREATE INDEX idx_org_branch_region_id ON public.org_branch USING btree (region_id);

CREATE INDEX idx_org_branch_state_id ON public.org_branch USING btree (state_id);

ALTER TABLE ONLY public.org_branch
    ADD CONSTRAINT org_branch_district_id_fkey FOREIGN KEY (district_id) REFERENCES public.cmn_district(id);

ALTER TABLE ONLY public.org_branch
    ADD CONSTRAINT org_branch_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);

ALTER TABLE ONLY public.org_branch
    ADD CONSTRAINT org_branch_region_id_fkey FOREIGN KEY (region_id) REFERENCES public.cmn_region(id);

ALTER TABLE ONLY public.org_branch
    ADD CONSTRAINT org_branch_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);
