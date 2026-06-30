-- Table: public.sys_role

CREATE TABLE public.sys_role (
    id integer NOT NULL,
    short_name character varying(100) NOT NULL,
    full_name character varying(255) NOT NULL,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL,
    organization_id integer,
    has_global_access boolean DEFAULT false NOT NULL,
    code character varying(100),
    description character varying(500),
    is_system boolean DEFAULT false NOT NULL,
    is_owner_role boolean DEFAULT false NOT NULL,
    sort_order integer DEFAULT 0 NOT NULL
);

CREATE SEQUENCE public.sys_role_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.sys_role_id_seq OWNED BY public.sys_role.id;

ALTER TABLE ONLY public.sys_role ALTER COLUMN id SET DEFAULT nextval('public.sys_role_id_seq'::regclass);

insert into public.sys_role (id, short_name, full_name, state_id, created_date, organization_id, has_global_access) values
    ('3', 'Kamilahon', 'Kamilichka', '1', '2026-06-11 18:23:06.708355', NULL, 'f'),
    ('2', 'AsadbekBux', 'AsadbekBux', '1', '2026-06-11 18:18:43.452469', NULL, 'f'),
    ('4', 'super_admin', 'Super Admin', '1', '2026-06-19 11:15:16.697264', NULL, 't'),
    ('5', 'adminka', 'Adminka', '1', '2026-06-19 14:37:38.409257', NULL, 'f'),
    ('1', 'admin', 'Tashkilot admini', '1', '2026-06-05 16:52:31.75491', NULL, 'f');

SELECT pg_catalog.setval('public.sys_role_id_seq', 5, true);

ALTER TABLE ONLY public.sys_role
    ADD CONSTRAINT sys_role_pkey PRIMARY KEY (id);

CREATE INDEX idx_sys_role_organization_id ON public.sys_role USING btree (organization_id);

ALTER TABLE ONLY public.sys_role
    ADD CONSTRAINT sys_role_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);

ALTER TABLE ONLY public.sys_role
    ADD CONSTRAINT sys_role_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);

CREATE INDEX idx_sys_role_code ON public.sys_role USING btree (code);

CREATE INDEX idx_sys_role_is_system ON public.sys_role USING btree (is_system);

CREATE INDEX idx_sys_role_sort_order ON public.sys_role USING btree (sort_order);

CREATE UNIQUE INDEX uidx_sys_role_org_code ON public.sys_role USING btree (organization_id, code) WHERE (code IS NOT NULL);
