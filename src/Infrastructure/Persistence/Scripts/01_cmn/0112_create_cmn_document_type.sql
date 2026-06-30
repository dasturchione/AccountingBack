-- Table: public.cmn_document_type

CREATE TABLE public.cmn_document_type (
    id smallint NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(150) NOT NULL,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL
);

CREATE SEQUENCE public.cmn_document_type_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.cmn_document_type_id_seq OWNED BY public.cmn_document_type.id;

ALTER TABLE ONLY public.cmn_document_type ALTER COLUMN id SET DEFAULT nextval('public.cmn_document_type_id_seq'::regclass);

insert into public.cmn_document_type (id, code, name, state_id, created_date) values
    ('1', 'purchase', 'Xarid / Kirim', '1', '2026-06-06 16:40:07.223429'),
    ('2', 'sale', 'Sotuv', '1', '2026-06-06 16:40:07.223429'),
    ('3', 'bank_operation', 'Bank operatsiyasi', '1', '2026-06-06 16:40:07.223429'),
    ('4', 'cash_operation', 'Kassa operatsiyasi', '1', '2026-06-06 16:40:07.223429'),
    ('5', 'salary', 'Ish haqi', '1', '2026-06-06 16:40:07.223429'),
    ('6', 'expense', 'Xarajat', '1', '2026-06-06 16:40:07.223429');

SELECT pg_catalog.setval('public.cmn_document_type_id_seq', 6, true);

ALTER TABLE ONLY public.cmn_document_type
    ADD CONSTRAINT cmn_document_type_pkey PRIMARY KEY (id);

CREATE UNIQUE INDEX idx_cmn_document_type_code ON public.cmn_document_type USING btree (code);

CREATE INDEX idx_cmn_document_type_state_id ON public.cmn_document_type USING btree (state_id);

ALTER TABLE ONLY public.cmn_document_type
    ADD CONSTRAINT cmn_document_type_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);
