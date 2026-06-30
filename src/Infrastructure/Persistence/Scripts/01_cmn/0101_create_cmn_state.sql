-- Table: public.cmn_state

CREATE TABLE public.cmn_state (
    id smallint NOT NULL,
    short_name character varying(250) NOT NULL,
    full_name character varying(250) NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL
);

insert into public.cmn_state (id, short_name, full_name, created_date) values
    ('1', 'A', 'Aktiv', '2026-06-05 16:34:55.941031'),
    ('2', 'P', 'Passiv', '2026-06-05 16:34:55.941031');

ALTER TABLE ONLY public.cmn_state
    ADD CONSTRAINT cmn_state_pkey PRIMARY KEY (id);
