-- Table: public.inv_warehouse

CREATE TABLE public.inv_warehouse (
    id integer NOT NULL,
    organization_id integer NOT NULL,
    branch_id integer,
    name character varying(250) NOT NULL,
    responsible_user_id integer,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL,
    code character varying(100),
    address character varying(1000),
    is_main boolean DEFAULT false NOT NULL
);

CREATE SEQUENCE public.inv_warehouse_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.inv_warehouse_id_seq OWNED BY public.inv_warehouse.id;

ALTER TABLE ONLY public.inv_warehouse ALTER COLUMN id SET DEFAULT nextval('public.inv_warehouse_id_seq'::regclass);

insert into public.inv_warehouse (id, organization_id, branch_id, name, responsible_user_id, state_id, created_date) values
    ('7', '8', '6', 'amonov', '15', '1', '2026-06-20 15:38:43.144691');

SELECT pg_catalog.setval('public.inv_warehouse_id_seq', 7, true);

ALTER TABLE ONLY public.inv_warehouse
    ADD CONSTRAINT inv_warehouse_pkey PRIMARY KEY (id);

CREATE INDEX idx_inv_warehouse_branch_id ON public.inv_warehouse USING btree (branch_id);

CREATE INDEX idx_inv_warehouse_organization_id ON public.inv_warehouse USING btree (organization_id);

CREATE INDEX idx_inv_warehouse_responsible_user_id ON public.inv_warehouse USING btree (responsible_user_id);

CREATE INDEX idx_inv_warehouse_state_id ON public.inv_warehouse USING btree (state_id);

ALTER TABLE ONLY public.inv_warehouse
    ADD CONSTRAINT inv_warehouse_branch_id_fkey FOREIGN KEY (branch_id) REFERENCES public.org_branch(id);

ALTER TABLE ONLY public.inv_warehouse
    ADD CONSTRAINT inv_warehouse_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);

ALTER TABLE ONLY public.inv_warehouse
    ADD CONSTRAINT inv_warehouse_responsible_user_id_fkey FOREIGN KEY (responsible_user_id) REFERENCES public.sys_user(id);

ALTER TABLE ONLY public.inv_warehouse
    ADD CONSTRAINT inv_warehouse_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);

CREATE INDEX idx_inv_warehouse_code ON public.inv_warehouse USING btree (code);

CREATE INDEX idx_inv_warehouse_is_main ON public.inv_warehouse USING btree (is_main);

CREATE UNIQUE INDEX uidx_inv_warehouse_org_code ON public.inv_warehouse USING btree (organization_id, code) WHERE (code IS NOT NULL);
