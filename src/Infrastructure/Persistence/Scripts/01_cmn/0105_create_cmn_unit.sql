-- Table: public.cmn_unit

CREATE TABLE public.cmn_unit (
    id smallint NOT NULL,
    code character varying(20) NOT NULL,
    name character varying(100) NOT NULL,
    state_id smallint NOT NULL
);

CREATE SEQUENCE public.cmn_unit_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.cmn_unit_id_seq OWNED BY public.cmn_unit.id;

ALTER TABLE ONLY public.cmn_unit ALTER COLUMN id SET DEFAULT nextval('public.cmn_unit_id_seq'::regclass);

insert into public.cmn_unit (id, code, name, state_id) values
    ('1', 'dona', 'Dona', '1'),
    ('2', 'kg', 'Kilogram', '1'),
    ('3', 'litr', 'Litr', '1'),
    ('4', 'metr', 'Metr', '1'),
    ('5', 'xizmat', 'Xizmat', '1');

SELECT pg_catalog.setval('public.cmn_unit_id_seq', 5, true);

ALTER TABLE ONLY public.cmn_unit
    ADD CONSTRAINT cmn_unit_pkey PRIMARY KEY (id);

CREATE UNIQUE INDEX idx_cmn_unit_code ON public.cmn_unit USING btree (code);

ALTER TABLE ONLY public.cmn_unit
    ADD CONSTRAINT cmn_unit_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);
