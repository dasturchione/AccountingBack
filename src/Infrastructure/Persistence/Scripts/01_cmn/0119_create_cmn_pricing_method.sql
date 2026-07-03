-- Table: public.cmn_pricing_method

CREATE TABLE public.cmn_pricing_method (
    id smallint NOT NULL,
    code character varying(20) NOT NULL,
    name character varying(200) NOT NULL,
    CONSTRAINT cmn_pricing_method_code_key UNIQUE (code),
    CONSTRAINT cmn_pricing_method_pkey PRIMARY KEY (id)
);

insert into public.cmn_pricing_method (id, code, name) values
    ('1', 'COST_PLUS_PERCENT', 'Tannarx + foizli marja'),
    ('2', 'COST_PLUS_AMOUNT', 'Tannarx + belgilangan summa'),
    ('3', 'FIXED_PRICE', 'Belgilangan narx');
