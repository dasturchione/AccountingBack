-- Table: public.sys_user_organization

CREATE TABLE public.sys_user_organization (
    user_id integer NOT NULL,
    organization_id integer NOT NULL,
    role_id integer,
    is_default boolean DEFAULT false NOT NULL,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL,
    is_owner boolean DEFAULT false NOT NULL,
    joined_at timestamp without time zone DEFAULT now() NOT NULL,
    invited_by_user_id integer,
    last_access_at timestamp without time zone,
    blocked_at timestamp without time zone,
    CONSTRAINT sys_user_organization_pkey PRIMARY KEY (user_id, organization_id),
    CONSTRAINT sys_user_organization_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id) ON DELETE CASCADE,
    CONSTRAINT sys_user_organization_role_id_fkey FOREIGN KEY (role_id) REFERENCES public.sys_role(id),
    CONSTRAINT sys_user_organization_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id),
    CONSTRAINT sys_user_organization_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.sys_user(id) ON DELETE CASCADE,
    CONSTRAINT sys_user_organization_invited_by_user_id_fkey FOREIGN KEY (invited_by_user_id) REFERENCES public.sys_user(id)
);

insert into public.sys_user_organization (user_id, organization_id, role_id, is_default, state_id, created_date) values
    ('7', '2', '1', 't', '1', '2026-06-12 11:13:27.42408'),
    ('4', '8', NULL, 'f', '1', '2026-06-13 11:02:26.046266'),
    ('2', '2', '1', 'f', '1', '2026-06-13 11:23:17.800494'),
    ('2', '8', '2', 'f', '1', '2026-06-13 11:23:17.800494'),
    ('12', '2', '4', 't', '1', '2026-06-19 12:03:57.914819'),
    ('12', '8', '4', 'f', '1', '2026-06-19 12:03:57.917009'),
    ('15', '8', NULL, 't', '1', '2026-06-19 15:39:50.109461');

CREATE UNIQUE INDEX idx_sys_user_organization_default_user ON public.sys_user_organization USING btree (user_id) WHERE (is_default = true);

CREATE INDEX idx_sys_user_organization_organization_id ON public.sys_user_organization USING btree (organization_id);

CREATE INDEX idx_sys_user_organization_role_id ON public.sys_user_organization USING btree (role_id);

CREATE INDEX idx_sys_user_organization_state_id ON public.sys_user_organization USING btree (state_id);

CREATE INDEX idx_sys_user_organization_is_owner ON public.sys_user_organization USING btree (is_owner);

CREATE INDEX idx_sys_user_organization_invited_by_user_id ON public.sys_user_organization USING btree (invited_by_user_id);
