-- Table: public.org_department

CREATE TABLE public.org_department (
    id integer NOT NULL,
    organization_id integer NOT NULL,
    branch_id integer,
    code character varying(50) NOT NULL,
    name character varying(250) NOT NULL,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL
);

CREATE SEQUENCE public.org_department_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.org_department_id_seq OWNED BY public.org_department.id;

ALTER TABLE ONLY public.org_department ALTER COLUMN id SET DEFAULT nextval('public.org_department_id_seq'::regclass);

SELECT pg_catalog.setval('public.org_department_id_seq', 2, true);

ALTER TABLE ONLY public.org_department
    ADD CONSTRAINT org_department_pkey PRIMARY KEY (id);

CREATE INDEX idx_org_department_branch_id ON public.org_department USING btree (branch_id);

CREATE UNIQUE INDEX idx_org_department_org_code ON public.org_department USING btree (organization_id, code);

CREATE INDEX idx_org_department_organization_id ON public.org_department USING btree (organization_id);

CREATE INDEX idx_org_department_state_id ON public.org_department USING btree (state_id);

ALTER TABLE ONLY public.org_department
    ADD CONSTRAINT org_department_branch_id_fkey FOREIGN KEY (branch_id) REFERENCES public.org_branch(id);

ALTER TABLE ONLY public.org_department
    ADD CONSTRAINT org_department_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);

ALTER TABLE ONLY public.org_department
    ADD CONSTRAINT org_department_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);
