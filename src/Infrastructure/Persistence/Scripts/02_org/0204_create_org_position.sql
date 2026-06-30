-- Table: public.org_position

CREATE TABLE public.org_position (
    id integer NOT NULL,
    organization_id integer NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(250) NOT NULL,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL
);

CREATE SEQUENCE public.org_position_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.org_position_id_seq OWNED BY public.org_position.id;

ALTER TABLE ONLY public.org_position ALTER COLUMN id SET DEFAULT nextval('public.org_position_id_seq'::regclass);

SELECT pg_catalog.setval('public.org_position_id_seq', 4, true);

ALTER TABLE ONLY public.org_position
    ADD CONSTRAINT org_position_pkey PRIMARY KEY (id);

CREATE UNIQUE INDEX idx_org_position_org_code ON public.org_position USING btree (organization_id, code);

CREATE INDEX idx_org_position_organization_id ON public.org_position USING btree (organization_id);

CREATE INDEX idx_org_position_state_id ON public.org_position USING btree (state_id);

ALTER TABLE ONLY public.org_position
    ADD CONSTRAINT org_position_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);

ALTER TABLE ONLY public.org_position
    ADD CONSTRAINT org_position_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);
