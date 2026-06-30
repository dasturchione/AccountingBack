-- Table: public.cmn_document_status

CREATE TABLE public.cmn_document_status (
    id smallint NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(100) NOT NULL,
    state_id smallint NOT NULL
);

CREATE SEQUENCE public.cmn_document_status_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.cmn_document_status_id_seq OWNED BY public.cmn_document_status.id;

ALTER TABLE ONLY public.cmn_document_status ALTER COLUMN id SET DEFAULT nextval('public.cmn_document_status_id_seq'::regclass);

insert into public.cmn_document_status (id, code, name, state_id) values
    ('1', 'draft', 'Qoralama', '1'),
    ('2', 'posted', 'O''tkazilgan', '1'),
    ('3', 'cancelled', 'Bekor qilingan', '1'),
    ('4', 'pending', 'Kutilmoqda', '1');

SELECT pg_catalog.setval('public.cmn_document_status_id_seq', 4, true);

ALTER TABLE ONLY public.cmn_document_status
    ADD CONSTRAINT cmn_document_status_pkey PRIMARY KEY (id);

CREATE UNIQUE INDEX idx_cmn_document_status_code ON public.cmn_document_status USING btree (code);

ALTER TABLE ONLY public.cmn_document_status
    ADD CONSTRAINT cmn_document_status_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);
