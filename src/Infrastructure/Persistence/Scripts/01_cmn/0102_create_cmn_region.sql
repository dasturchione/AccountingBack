-- Table: public.cmn_region

CREATE TABLE public.cmn_region (
    id integer NOT NULL,
    short_name character varying(250) NOT NULL,
    full_name character varying(250) NOT NULL,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL
);

CREATE SEQUENCE public.cmn_region_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.cmn_region_id_seq OWNED BY public.cmn_region.id;

ALTER TABLE ONLY public.cmn_region ALTER COLUMN id SET DEFAULT nextval('public.cmn_region_id_seq'::regclass);

insert into public.cmn_region (id, short_name, full_name, state_id, created_date) values
    ('1', 'Toshkent shahri', 'Toshkent shahri', '1', '2026-06-05 16:36:13.009757'),
    ('2', 'Toshkent', 'Toshkent', '1', '2026-06-05 16:36:13.009757'),
    ('3', 'Andijon', 'Andijon', '1', '2026-06-05 16:36:13.009757'),
    ('4', 'Buxoro', 'Buxoro', '1', '2026-06-05 16:36:13.009757'),
    ('5', 'Jizzax', 'Jizzax', '1', '2026-06-05 16:36:13.009757'),
    ('6', 'Qoraqalpog‘iston Respublikasi', 'Qoraqalpog‘iston Respublikasi', '1', '2026-06-05 16:36:13.009757'),
    ('7', 'Qashqadaryo', 'Qashqadaryo', '1', '2026-06-05 16:36:13.009757'),
    ('8', 'Navoiy', 'Navoiy', '1', '2026-06-05 16:36:13.009757'),
    ('9', 'Namangan', 'Namangan', '1', '2026-06-05 16:36:13.009757'),
    ('10', 'Samarqand', 'Samarqand', '1', '2026-06-05 16:36:13.009757'),
    ('11', 'Surxondaryo', 'Surxondaryo', '1', '2026-06-05 16:36:13.009757'),
    ('12', 'Sirdaryo', 'Sirdaryo', '1', '2026-06-05 16:36:13.009757'),
    ('13', 'Farg‘ona', 'Farg‘ona', '1', '2026-06-05 16:36:13.009757'),
    ('14', 'Xorazm', 'Xorazm', '1', '2026-06-05 16:36:13.009757');

SELECT pg_catalog.setval('public.cmn_region_id_seq', 14, true);

ALTER TABLE ONLY public.cmn_region
    ADD CONSTRAINT cmn_region_pkey PRIMARY KEY (id);

ALTER TABLE ONLY public.cmn_region
    ADD CONSTRAINT cmn_region_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);
