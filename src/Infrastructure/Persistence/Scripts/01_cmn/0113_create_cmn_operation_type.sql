-- Table: public.cmn_operation_type

CREATE TABLE public.cmn_operation_type (
    id smallint NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(150) NOT NULL,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL
);

CREATE SEQUENCE public.cmn_operation_type_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.cmn_operation_type_id_seq OWNED BY public.cmn_operation_type.id;

ALTER TABLE ONLY public.cmn_operation_type ALTER COLUMN id SET DEFAULT nextval('public.cmn_operation_type_id_seq'::regclass);

insert into public.cmn_operation_type (id, code, name, state_id, created_date) values
    ('1', 'in', 'Kirim', '1', '2026-06-06 16:40:19.40134'),
    ('2', 'out', 'Chiqim', '1', '2026-06-06 16:40:19.40134'),
    ('3', 'transfer', 'O''tkazma', '1', '2026-06-06 16:40:19.40134'),
    ('4', 'debt_increase', 'Qarz oshishi', '1', '2026-06-06 16:40:19.40134'),
    ('5', 'debt_decrease', 'Qarz kamayishi', '1', '2026-06-06 16:40:19.40134');

SELECT pg_catalog.setval('public.cmn_operation_type_id_seq', 5, true);

ALTER TABLE ONLY public.cmn_operation_type
    ADD CONSTRAINT cmn_operation_type_pkey PRIMARY KEY (id);

CREATE UNIQUE INDEX idx_cmn_operation_type_code ON public.cmn_operation_type USING btree (code);

CREATE INDEX idx_cmn_operation_type_state_id ON public.cmn_operation_type USING btree (state_id);

ALTER TABLE ONLY public.cmn_operation_type
    ADD CONSTRAINT cmn_operation_type_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);
