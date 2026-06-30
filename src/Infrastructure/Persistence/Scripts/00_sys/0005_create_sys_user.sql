-- Table: public.sys_user

CREATE TABLE public.sys_user (
    id integer NOT NULL,
    user_name character varying(250) NOT NULL,
    password_hash character varying(250) NOT NULL,
    password_salt character varying(250) NOT NULL,
    phone_number character varying(50) NOT NULL,
    email character varying(200),
    first_name character varying(100) NOT NULL,
    last_name character varying(100) NOT NULL,
    role_id integer NOT NULL,
    last_access_time timestamp without time zone,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL,
    language_id smallint,
    organization_id integer
);

CREATE SEQUENCE public.sys_user_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.sys_user_id_seq OWNED BY public.sys_user.id;

ALTER TABLE ONLY public.sys_user ALTER COLUMN id SET DEFAULT nextval('public.sys_user_id_seq'::regclass);

insert into public.sys_user (id, user_name, password_hash, password_salt, phone_number, email, first_name, last_name, role_id, last_access_time, state_id, created_date, language_id, organization_id) values
    ('2', 'Mahmudjon Amonov', 'rCAmiQlaSG2JolJHYYEUChB89dqAQ63Sp9aY9WaKg6k=', 'biHGSYPE6sCc8w/QJ9XOkw==', '+998 99 890-08-58', 'amonovmahmudjon13@gmail.com', 'Mahmudjon', 'Amonov', '2', '2026-06-18 18:21:32.558673', '1', '2026-06-08 14:53:31.216633', '1', '2'),
    ('7', 'test_test', 'EshZvFfH7DsVO6/HnUI5E/Dj5hWCnLZeMnQO6/hpSdc=', 'v/F/y1YzCvy+hQMqc/bBIA==', '+998 99 455-55-55', 'testuser@mail.uz', 'Test', 'User', '1', NULL, '1', '2026-06-12 11:13:27.396974', NULL, NULL),
    ('3', 'Mahmudjon', '9TWnCcdvvf7Yv1m73xpzxFwuK5PY65UpQgV2iDwhNWk=', 'Y5N8oSECkMiuvFVzhARVSA==', '+998 99 890-08-58', 'amonovmahmudjon13@gmail.com', 'Mahmudjon', 'Amonov', '1', '2026-06-11 12:30:44.960698', '2', '2026-06-08 16:55:31.159039', '1', '2'),
    ('4', 'sardorbek_hafizov', 'D2lE4I3KUPIEdsYsBdTbuu/ccVuw3bieqWLYyn73OKQ=', 'l1khdk/gA0ndJP3BJyc0pQ==', '+998 99 890-08-58', 'sardorbekHafizov@gmail.com', 'Sardorbek', 'Hafizov', '1', '2026-06-29 18:57:14.477765', '1', '2026-06-11 15:51:32.389581', NULL, NULL),
    ('8', 'Amonov', 'ezzess8CQ3L1QREQqeg7iscDs08ijrWjvDPJvZCBt6A=', 'z9SXjmluYTvffYsAfBMd4A==', '+998 99 890-08-58', 'amonovmahmudjon13@gmail.com', 'Mahmudjon', 'Amonov', '3', NULL, '2', '2026-06-12 11:44:30.993097', NULL, NULL),
    ('12', 'sardorbek', 'gyObckIfXCcVCGH8wh8uLP4ORmbf7MwdDzCN8xqowBo=', 'LcOJh4Zh+ShOwvk6T3nprA==', '+998970642323', 'sardorbek@gmail.com', 'Sardorbek', 'Hafizov', '4', '2026-06-29 09:43:40.426757', '1', '2026-06-19 11:32:44.418707', NULL, NULL),
    ('10', 'test_user', '5vlocv7E6747NqLfIElKiAno7h6LhABv4dBO5AHC04A=', 'TWOahharOvv3Vu1yQWai1g==', '+998901234567', 'test@gmail.com', 'Test', 'User', '1', NULL, '1', '2026-06-13 11:48:39.245593', NULL, NULL),
    ('9', 'sarvarbek_hafizov', 'i6XKbEmXEWzLOKOmTJCL9iA8LyFEpUPk/dXKWxf9Roo=', 'EZAmUlxvpoZo4XMZ2O2E6Q==', '+998907304643', 'sarvarbekhafizov@gmail.com', 'Sarvarbek', 'Hafizov', '1', NULL, '2', '2026-06-13 11:34:36.217917', NULL, NULL),
    ('1', 'Mahmudjon Amonovvv', 'MCxmiJJhujeVeuPjiXeOGT4vQfTm7pzjwMlsGNJvsI0=', 'c5c3hf87yc3x3nlUMdO4/A==', '+998 99 890-08-58', 'amonovmahmudjon13@gmail.com', 'Mahmudjon', 'Amonov', '1', '2026-06-08 15:33:42.741805', '2', '2026-06-05 16:54:41.07541', '1', '2'),
    ('13', 'sabinahon', 'gyObckIfXCcVCGH8wh8uLP4ORmbf7MwdDzCN8xqowBo=', 'LcOJh4Zh+ShOwvk6T3nprA==', '+998 90 730-46-43', 'sabinahon13@gmail.com', 'Sabinahon', 'Soyibovna', '5', '2026-06-19 15:29:58.574366', '1', '2026-06-19 14:46:08.293094', NULL, NULL),
    ('14', 'monika', 'UFS/jbDQnrJRQqwfiKWK1aLzrKaFlCK5QCeCyTeZHFo=', 'SjRx9lE0hF0DQU2nf4kOVQ==', '+998 90 236-90-01', 'matluba13@gmail.com', 'Matluba', 'Farmonova', '5', '2026-06-19 15:32:58.193463', '1', '2026-06-19 15:31:51.265437', NULL, NULL),
    ('15', 'superadmin', 'NbX6dK2HaOHl+c5Vs2a5S75qYy2WXCG6W4HJ7UCqZnI=', '6LSuU2I2rB8GUMB51ly5rA==', '+998 99 899-89-00', 'sardorbekHafizov@gmail.com', 'Super', 'Admin', '5', '2026-06-19 16:07:02.441095', '1', '2026-06-19 15:39:50.103447', NULL, NULL);

SELECT pg_catalog.setval('public.sys_user_id_seq', 15, true);

ALTER TABLE ONLY public.sys_user
    ADD CONSTRAINT sys_user_pkey PRIMARY KEY (id);

CREATE INDEX idx_sys_user_language_id ON public.sys_user USING btree (language_id);

CREATE INDEX idx_sys_user_organization_id ON public.sys_user USING btree (organization_id);

CREATE INDEX idx_sys_user_phone ON public.sys_user USING btree (phone_number);

CREATE INDEX idx_sys_user_role_id ON public.sys_user USING btree (role_id);

CREATE UNIQUE INDEX uidx_sys_user_user_name ON public.sys_user USING btree (user_name);

ALTER TABLE ONLY public.sys_user
    ADD CONSTRAINT sys_user_language_id_fkey FOREIGN KEY (language_id) REFERENCES public.cmn_language(id);

ALTER TABLE ONLY public.sys_user
    ADD CONSTRAINT sys_user_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);

ALTER TABLE ONLY public.sys_user
    ADD CONSTRAINT sys_user_role_id_fkey FOREIGN KEY (role_id) REFERENCES public.sys_role(id);

ALTER TABLE ONLY public.sys_user
    ADD CONSTRAINT sys_user_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- PostgreSQL database dump complete
--

\unrestrict ECw6Seh1B4Oet0luhL1bPdG19lSQmbvNuJBeCmzsUhbvhzX4wOwsiyCfA4Cf4VN;
