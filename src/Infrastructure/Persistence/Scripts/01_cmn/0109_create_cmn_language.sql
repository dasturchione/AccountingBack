-- Table: public.cmn_language

CREATE TABLE public.cmn_language (
    id smallint NOT NULL,
    code character varying(10) NOT NULL,
    name character varying(100) NOT NULL,
    native_name character varying(100) NOT NULL,
    is_default boolean DEFAULT false NOT NULL,
    sort_order integer DEFAULT 0 NOT NULL,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL
);

CREATE SEQUENCE public.cmn_language_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.cmn_language_id_seq OWNED BY public.cmn_language.id;

ALTER TABLE ONLY public.cmn_language ALTER COLUMN id SET DEFAULT nextval('public.cmn_language_id_seq'::regclass);

insert into public.cmn_language (id, code, name, native_name, is_default, sort_order, state_id, created_date) values
    ('1', 'uz', 'Uzbek', 'O''zbekcha', 't', '1', '1', '2026-06-06 10:44:19.082498'),
    ('2', 'ru', 'Russian', 'Русский', 'f', '2', '1', '2026-06-06 10:44:19.082498'),
    ('3', 'en', 'English', 'English', 'f', '3', '1', '2026-06-06 10:44:19.082498');

SELECT pg_catalog.setval('public.cmn_language_id_seq', 3, true);

ALTER TABLE ONLY public.cmn_language
    ADD CONSTRAINT cmn_language_pkey PRIMARY KEY (id);

CREATE UNIQUE INDEX idx_cmn_language_code ON public.cmn_language USING btree (code);

CREATE UNIQUE INDEX idx_cmn_language_default ON public.cmn_language USING btree (is_default) WHERE (is_default = true);

CREATE INDEX idx_cmn_language_state_id ON public.cmn_language USING btree (state_id);

ALTER TABLE ONLY public.cmn_language
    ADD CONSTRAINT cmn_language_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);
