--
-- PostgreSQL database dump
--

\restrict dHKCrvK9c7j3jVKobcEFvhE4FoIfEfGX7OnpzYTpHSRckBob0CsBq22rKTrmmBt

-- Dumped from database version 16.14 (Ubuntu 16.14-0ubuntu0.24.04.1)
-- Dumped by pg_dump version 18.4

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Name: set_bank_operation_doc_number(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.set_bank_operation_doc_number() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
begin
    new.doc_number := lpad(nextval('doc_number_bank_operation_seq')::text, 9, '0');
    return new;
end;
$$;


ALTER FUNCTION public.set_bank_operation_doc_number() OWNER TO postgres;

--
-- Name: set_cash_operation_doc_number(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.set_cash_operation_doc_number() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
begin
    new.doc_number := lpad(nextval('doc_number_cash_operation_seq')::text, 9, '0');
    return new;
end;
$$;


ALTER FUNCTION public.set_cash_operation_doc_number() OWNER TO postgres;

--
-- Name: set_cmn_contract_number(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.set_cmn_contract_number() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
begin
    new.contract_number := lpad(nextval('contract_number_seq')::text, 9, '0');
    return new;
end;
$$;


ALTER FUNCTION public.set_cmn_contract_number() OWNER TO postgres;

--
-- Name: set_pur_doc_number(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.set_pur_doc_number() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
begin
    new.doc_number := lpad(nextval('doc_number_purchase_seq')::text, 9, '0');
    return new;
end;
$$;


ALTER FUNCTION public.set_pur_doc_number() OWNER TO postgres;

--
-- Name: set_sale_doc_number(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.set_sale_doc_number() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
begin
    new.doc_number := lpad(nextval('doc_number_sale_seq')::text, 9, '0');
    return new;
end;
$$;


ALTER FUNCTION public.set_sale_doc_number() OWNER TO postgres;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: acc_account_resolve_rule; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.acc_account_resolve_rule (
    id integer NOT NULL,
    policy_id smallint NOT NULL,
    alias character varying(250) NOT NULL,
    dimension_key character varying(250) NOT NULL,
    dimension_value character varying(250) NOT NULL,
    account_id integer NOT NULL,
    priority integer NOT NULL
);


ALTER TABLE public.acc_account_resolve_rule OWNER TO postgres;

--
-- Name: acc_account_resolve_rule_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.acc_account_resolve_rule_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.acc_account_resolve_rule_id_seq OWNER TO postgres;

--
-- Name: acc_account_resolve_rule_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.acc_account_resolve_rule_id_seq OWNED BY public.acc_account_resolve_rule.id;


--
-- Name: acc_account_type; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.acc_account_type (
    id smallint NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(150) NOT NULL,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.acc_account_type OWNER TO postgres;

--
-- Name: acc_account_type_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.acc_account_type_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.acc_account_type_id_seq OWNER TO postgres;

--
-- Name: acc_account_type_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.acc_account_type_id_seq OWNED BY public.acc_account_type.id;


--
-- Name: acc_accounting_period; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.acc_accounting_period (
    id integer NOT NULL,
    organization_id integer NOT NULL,
    year smallint NOT NULL,
    month smallint NOT NULL,
    start_date date NOT NULL,
    end_date date NOT NULL,
    is_closed boolean DEFAULT false NOT NULL,
    closed_at timestamp without time zone,
    closed_by_user_id integer,
    created_date timestamp without time zone DEFAULT now() NOT NULL,
    CONSTRAINT ck_acc_accounting_period_dates CHECK ((end_date >= start_date)),
    CONSTRAINT ck_acc_accounting_period_month CHECK (((month >= 1) AND (month <= 12)))
);


ALTER TABLE public.acc_accounting_period OWNER TO postgres;

--
-- Name: acc_accounting_period_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.acc_accounting_period ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.acc_accounting_period_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: acc_accounting_policy; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.acc_accounting_policy (
    id smallint NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(250) NOT NULL,
    state_id smallint NOT NULL
);


ALTER TABLE public.acc_accounting_policy OWNER TO postgres;

--
-- Name: acc_accounting_policy_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.acc_accounting_policy_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.acc_accounting_policy_id_seq OWNER TO postgres;

--
-- Name: acc_accounting_policy_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.acc_accounting_policy_id_seq OWNED BY public.acc_accounting_policy.id;


--
-- Name: acc_chart_account; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.acc_chart_account (
    id integer NOT NULL,
    parent_id integer,
    code character varying(50) NOT NULL,
    name character varying(250) NOT NULL,
    is_group boolean DEFAULT false NOT NULL,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL,
    account_type_id smallint,
    is_quantity boolean DEFAULT false NOT NULL,
    is_currency boolean DEFAULT false NOT NULL
);


ALTER TABLE public.acc_chart_account OWNER TO postgres;

--
-- Name: acc_chart_account_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.acc_chart_account_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.acc_chart_account_id_seq OWNER TO postgres;

--
-- Name: acc_chart_account_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.acc_chart_account_id_seq OWNED BY public.acc_chart_account.id;


--
-- Name: acc_chart_account_subkonto; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.acc_chart_account_subkonto (
    id integer NOT NULL,
    organization_id integer NOT NULL,
    account_id integer NOT NULL,
    subkonto_type_id smallint NOT NULL,
    sort_order integer DEFAULT 0 NOT NULL,
    is_required boolean DEFAULT true NOT NULL,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.acc_chart_account_subkonto OWNER TO postgres;

--
-- Name: acc_chart_account_subkonto_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.acc_chart_account_subkonto_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.acc_chart_account_subkonto_id_seq OWNER TO postgres;

--
-- Name: acc_chart_account_subkonto_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.acc_chart_account_subkonto_id_seq OWNED BY public.acc_chart_account_subkonto.id;


--
-- Name: acc_payment_purpose; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.acc_payment_purpose (
    id smallint NOT NULL,
    code character varying(50) NOT NULL,
    alias_id smallint NOT NULL,
    name character varying(250) NOT NULL,
    requires_counterparty boolean DEFAULT true NOT NULL,
    operation_type_id smallint NOT NULL
);


ALTER TABLE public.acc_payment_purpose OWNER TO postgres;

--
-- Name: acc_payment_purpose_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.acc_payment_purpose_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.acc_payment_purpose_id_seq OWNER TO postgres;

--
-- Name: acc_payment_purpose_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.acc_payment_purpose_id_seq OWNED BY public.acc_payment_purpose.id;


--
-- Name: acc_payment_purpose_translation; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.acc_payment_purpose_translation (
    payment_purpose_id smallint NOT NULL,
    language_id smallint NOT NULL,
    name character varying(250) NOT NULL
);


ALTER TABLE public.acc_payment_purpose_translation OWNER TO postgres;

--
-- Name: acc_posting_alias; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.acc_posting_alias (
    id smallint NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(250) NOT NULL
);


ALTER TABLE public.acc_posting_alias OWNER TO postgres;

--
-- Name: acc_posting_alias_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.acc_posting_alias_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.acc_posting_alias_id_seq OWNER TO postgres;

--
-- Name: acc_posting_alias_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.acc_posting_alias_id_seq OWNED BY public.acc_posting_alias.id;


--
-- Name: acc_posting_alias_translation; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.acc_posting_alias_translation (
    posting_alias_id smallint NOT NULL,
    language_id smallint NOT NULL,
    name character varying(250) NOT NULL
);


ALTER TABLE public.acc_posting_alias_translation OWNER TO postgres;

--
-- Name: acc_posting_batch; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.acc_posting_batch (
    id bigint NOT NULL,
    organization_id integer NOT NULL,
    document_type_id smallint NOT NULL,
    document_id bigint NOT NULL,
    status character varying(30) DEFAULT 'posted'::character varying NOT NULL,
    posted_by_user_id integer,
    posted_at timestamp without time zone DEFAULT now() NOT NULL,
    reversed_by_user_id integer,
    reversed_at timestamp without time zone,
    comment character varying(1000)
);


ALTER TABLE public.acc_posting_batch OWNER TO postgres;

--
-- Name: acc_posting_batch_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.acc_posting_batch ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.acc_posting_batch_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: acc_posting_rule; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.acc_posting_rule (
    id smallint NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(250) NOT NULL
);


ALTER TABLE public.acc_posting_rule OWNER TO postgres;

--
-- Name: acc_posting_rule_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.acc_posting_rule_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.acc_posting_rule_id_seq OWNER TO postgres;

--
-- Name: acc_posting_rule_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.acc_posting_rule_id_seq OWNED BY public.acc_posting_rule.id;


--
-- Name: acc_posting_rule_line; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.acc_posting_rule_line (
    id integer NOT NULL,
    template_id smallint NOT NULL,
    order_number smallint NOT NULL,
    amount_source character varying(20),
    is_optional boolean DEFAULT true NOT NULL,
    debit_alias_id smallint NOT NULL,
    credit_alias_id smallint NOT NULL
);


ALTER TABLE public.acc_posting_rule_line OWNER TO postgres;

--
-- Name: acc_posting_rule_line_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.acc_posting_rule_line_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.acc_posting_rule_line_id_seq OWNER TO postgres;

--
-- Name: acc_posting_rule_line_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.acc_posting_rule_line_id_seq OWNED BY public.acc_posting_rule_line.id;


--
-- Name: acc_reg_entry; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.acc_reg_entry (
    id bigint NOT NULL,
    organization_id integer NOT NULL,
    document_type_id smallint NOT NULL,
    document_id bigint NOT NULL,
    debit_account_id integer,
    credit_account_id integer,
    currency_id smallint NOT NULL,
    amount numeric(18,2) NOT NULL,
    doc_date timestamp without time zone NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL,
    operation_type_id smallint,
    debit_quantity numeric(18,3),
    credit_quantity numeric(18,3),
    content character varying(1000),
    journal_number character varying(100),
    posting_batch_id bigint,
    source_line_id bigint,
    reversal_entry_id bigint
);


ALTER TABLE public.acc_reg_entry OWNER TO postgres;

--
-- Name: acc_reg_entry_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.acc_reg_entry_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.acc_reg_entry_id_seq OWNER TO postgres;

--
-- Name: acc_reg_entry_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.acc_reg_entry_id_seq OWNED BY public.acc_reg_entry.id;


--
-- Name: acc_reg_entry_subkonto; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.acc_reg_entry_subkonto (
    id bigint NOT NULL,
    entry_id bigint NOT NULL,
    side character varying(2) NOT NULL,
    subkonto_type_id smallint NOT NULL,
    sort_order integer DEFAULT 0 NOT NULL,
    entity_id bigint,
    display_value character varying(500),
    created_date timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.acc_reg_entry_subkonto OWNER TO postgres;

--
-- Name: acc_reg_entry_subkonto_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.acc_reg_entry_subkonto_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.acc_reg_entry_subkonto_id_seq OWNER TO postgres;

--
-- Name: acc_reg_entry_subkonto_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.acc_reg_entry_subkonto_id_seq OWNED BY public.acc_reg_entry_subkonto.id;


--
-- Name: acc_subkonto_type; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.acc_subkonto_type (
    id smallint NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(150) NOT NULL,
    source_table character varying(100) NOT NULL,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.acc_subkonto_type OWNER TO postgres;

--
-- Name: acc_subkonto_type_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.acc_subkonto_type_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.acc_subkonto_type_id_seq OWNER TO postgres;

--
-- Name: acc_subkonto_type_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.acc_subkonto_type_id_seq OWNED BY public.acc_subkonto_type.id;


--
-- Name: bank_operation; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.bank_operation (
    id bigint NOT NULL,
    organization_id integer NOT NULL,
    bank_account_id integer NOT NULL,
    operation_type_id smallint NOT NULL,
    payment_type_id smallint,
    counterparty_id integer,
    doc_number character varying(100) NOT NULL,
    doc_date timestamp without time zone NOT NULL,
    currency_id smallint NOT NULL,
    amount numeric(18,2) NOT NULL,
    comment character varying(1000),
    status_id smallint NOT NULL,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL,
    exchange_rate numeric(18,6) DEFAULT 1 NOT NULL,
    posted_at timestamp without time zone,
    posted_by_user_id integer,
    cancelled_at timestamp without time zone,
    cancelled_by_user_id integer,
    counterparty_bank_account_id integer,
    contract_id bigint,
    payment_purpose_id smallint NOT NULL
);


ALTER TABLE public.bank_operation OWNER TO postgres;

--
-- Name: bank_operation_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.bank_operation_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.bank_operation_id_seq OWNER TO postgres;

--
-- Name: bank_operation_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.bank_operation_id_seq OWNED BY public.bank_operation.id;


--
-- Name: bank_operation_line; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.bank_operation_line (
    id bigint NOT NULL,
    bank_operation_id bigint NOT NULL,
    order_number smallint NOT NULL,
    payment_purpose_id smallint NOT NULL,
    counterparty_id integer,
    amount numeric(18,2) NOT NULL,
    comment character varying(500)
);


ALTER TABLE public.bank_operation_line OWNER TO postgres;

--
-- Name: bank_operation_line_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.bank_operation_line_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.bank_operation_line_id_seq OWNER TO postgres;

--
-- Name: bank_operation_line_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.bank_operation_line_id_seq OWNED BY public.bank_operation_line.id;


--
-- Name: cash_box; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.cash_box (
    id integer NOT NULL,
    organization_id integer NOT NULL,
    branch_id integer,
    code character varying(50) NOT NULL,
    name character varying(250) NOT NULL,
    currency_id smallint NOT NULL,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL,
    is_main boolean DEFAULT false NOT NULL,
    responsible_user_id integer,
    opening_balance numeric(18,2) DEFAULT 0 NOT NULL,
    opening_balance_date date
);


ALTER TABLE public.cash_box OWNER TO postgres;

--
-- Name: cash_box_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.cash_box_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.cash_box_id_seq OWNER TO postgres;

--
-- Name: cash_box_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.cash_box_id_seq OWNED BY public.cash_box.id;


--
-- Name: cash_operation; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.cash_operation (
    id bigint NOT NULL,
    organization_id integer NOT NULL,
    cash_box_id integer NOT NULL,
    operation_type_id smallint NOT NULL,
    payment_type_id smallint,
    counterparty_id integer,
    doc_number character varying(100) NOT NULL,
    doc_date timestamp without time zone NOT NULL,
    currency_id smallint NOT NULL,
    amount numeric(18,2) NOT NULL,
    comment character varying(1000),
    status_id smallint NOT NULL,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL,
    exchange_rate numeric(18,6) DEFAULT 1 NOT NULL,
    posted_at timestamp without time zone,
    posted_by_user_id integer,
    cancelled_at timestamp without time zone,
    cancelled_by_user_id integer,
    destination_cash_box_id integer,
    payment_purpose_id smallint NOT NULL
);


ALTER TABLE public.cash_operation OWNER TO postgres;

--
-- Name: cash_operation_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.cash_operation_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.cash_operation_id_seq OWNER TO postgres;

--
-- Name: cash_operation_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.cash_operation_id_seq OWNED BY public.cash_operation.id;


--
-- Name: cmn_bank; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.cmn_bank (
    id integer NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(250) NOT NULL,
    mfo character varying(20),
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL,
    inn character varying(20)
);


ALTER TABLE public.cmn_bank OWNER TO postgres;

--
-- Name: cmn_bank_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.cmn_bank_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.cmn_bank_id_seq OWNER TO postgres;

--
-- Name: cmn_bank_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.cmn_bank_id_seq OWNED BY public.cmn_bank.id;


--
-- Name: cmn_contract; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.cmn_contract (
    id bigint NOT NULL,
    organization_id integer NOT NULL,
    counterparty_id integer NOT NULL,
    contract_number character varying(100) NOT NULL,
    contract_date timestamp without time zone NOT NULL,
    start_date timestamp without time zone,
    end_date timestamp without time zone,
    comment character varying(1000),
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL,
    contract_type_id smallint NOT NULL
);


ALTER TABLE public.cmn_contract OWNER TO postgres;

--
-- Name: cmn_contract_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.cmn_contract_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.cmn_contract_id_seq OWNER TO postgres;

--
-- Name: cmn_contract_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.cmn_contract_id_seq OWNED BY public.cmn_contract.id;


--
-- Name: cmn_contract_type; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.cmn_contract_type (
    id smallint NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(150) NOT NULL,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.cmn_contract_type OWNER TO postgres;

--
-- Name: cmn_contract_type_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.cmn_contract_type_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.cmn_contract_type_id_seq OWNER TO postgres;

--
-- Name: cmn_contract_type_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.cmn_contract_type_id_seq OWNED BY public.cmn_contract_type.id;


--
-- Name: cmn_costing_method; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.cmn_costing_method (
    id smallint NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(100) NOT NULL
);


ALTER TABLE public.cmn_costing_method OWNER TO postgres;

--
-- Name: cmn_costing_method_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.cmn_costing_method_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.cmn_costing_method_id_seq OWNER TO postgres;

--
-- Name: cmn_costing_method_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.cmn_costing_method_id_seq OWNED BY public.cmn_costing_method.id;


--
-- Name: cmn_counterparty_type; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.cmn_counterparty_type (
    id smallint NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(100) NOT NULL,
    state_id smallint NOT NULL
);


ALTER TABLE public.cmn_counterparty_type OWNER TO postgres;

--
-- Name: cmn_counterparty_type_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.cmn_counterparty_type_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.cmn_counterparty_type_id_seq OWNER TO postgres;

--
-- Name: cmn_counterparty_type_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.cmn_counterparty_type_id_seq OWNED BY public.cmn_counterparty_type.id;


--
-- Name: cmn_currency; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.cmn_currency (
    id smallint NOT NULL,
    code character varying(10) NOT NULL,
    name character varying(100) NOT NULL,
    symbol character varying(10),
    state_id smallint NOT NULL
);


ALTER TABLE public.cmn_currency OWNER TO postgres;

--
-- Name: cmn_currency_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.cmn_currency_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.cmn_currency_id_seq OWNER TO postgres;

--
-- Name: cmn_currency_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.cmn_currency_id_seq OWNED BY public.cmn_currency.id;


--
-- Name: cmn_district; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.cmn_district (
    id integer NOT NULL,
    short_name character varying(250) NOT NULL,
    full_name character varying(250) NOT NULL,
    region_id integer NOT NULL,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.cmn_district OWNER TO postgres;

--
-- Name: cmn_district_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.cmn_district_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.cmn_district_id_seq OWNER TO postgres;

--
-- Name: cmn_district_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.cmn_district_id_seq OWNED BY public.cmn_district.id;


--
-- Name: cmn_document_sequence; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.cmn_document_sequence (
    id integer NOT NULL,
    organization_id integer NOT NULL,
    document_type_id smallint NOT NULL,
    prefix character varying(50),
    suffix character varying(50),
    current_number bigint DEFAULT 0 NOT NULL,
    padding smallint DEFAULT 5 NOT NULL,
    year smallint,
    month smallint,
    reset_period character varying(20) DEFAULT 'yearly'::character varying NOT NULL,
    state_id smallint DEFAULT 1 NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.cmn_document_sequence OWNER TO postgres;

--
-- Name: cmn_document_sequence_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.cmn_document_sequence ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.cmn_document_sequence_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: cmn_document_status; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.cmn_document_status (
    id smallint NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(100) NOT NULL,
    state_id smallint NOT NULL
);


ALTER TABLE public.cmn_document_status OWNER TO postgres;

--
-- Name: cmn_document_status_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.cmn_document_status_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.cmn_document_status_id_seq OWNER TO postgres;

--
-- Name: cmn_document_status_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.cmn_document_status_id_seq OWNED BY public.cmn_document_status.id;


--
-- Name: cmn_document_type; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.cmn_document_type (
    id smallint NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(150) NOT NULL,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.cmn_document_type OWNER TO postgres;

--
-- Name: cmn_document_type_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.cmn_document_type_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.cmn_document_type_id_seq OWNER TO postgres;

--
-- Name: cmn_document_type_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.cmn_document_type_id_seq OWNED BY public.cmn_document_type.id;


--
-- Name: cmn_language; Type: TABLE; Schema: public; Owner: postgres
--

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


ALTER TABLE public.cmn_language OWNER TO postgres;

--
-- Name: cmn_language_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.cmn_language_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.cmn_language_id_seq OWNER TO postgres;

--
-- Name: cmn_language_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.cmn_language_id_seq OWNED BY public.cmn_language.id;


--
-- Name: cmn_operation_type; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.cmn_operation_type (
    id smallint NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(150) NOT NULL,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.cmn_operation_type OWNER TO postgres;

--
-- Name: cmn_operation_type_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.cmn_operation_type_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.cmn_operation_type_id_seq OWNER TO postgres;

--
-- Name: cmn_operation_type_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.cmn_operation_type_id_seq OWNED BY public.cmn_operation_type.id;


--
-- Name: cmn_payment_type; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.cmn_payment_type (
    id smallint NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(100) NOT NULL,
    state_id smallint NOT NULL
);


ALTER TABLE public.cmn_payment_type OWNER TO postgres;

--
-- Name: cmn_payment_type_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.cmn_payment_type_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.cmn_payment_type_id_seq OWNER TO postgres;

--
-- Name: cmn_payment_type_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.cmn_payment_type_id_seq OWNED BY public.cmn_payment_type.id;


--
-- Name: cmn_price_rounding_method; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.cmn_price_rounding_method (
    id smallint NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(100) NOT NULL
);


ALTER TABLE public.cmn_price_rounding_method OWNER TO postgres;

--
-- Name: cmn_price_rounding_method_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.cmn_price_rounding_method_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.cmn_price_rounding_method_id_seq OWNER TO postgres;

--
-- Name: cmn_price_rounding_method_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.cmn_price_rounding_method_id_seq OWNED BY public.cmn_price_rounding_method.id;


--
-- Name: cmn_pricing_condition; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.cmn_pricing_condition (
    id bigint NOT NULL,
    organization_id integer NOT NULL,
    pricing_method_id smallint NOT NULL,
    pricing_value numeric(18,2) NOT NULL,
    rounding_method_id smallint NOT NULL,
    rounding_precision numeric(12,2) DEFAULT 1 NOT NULL,
    start_date timestamp without time zone DEFAULT now() NOT NULL,
    end_date timestamp without time zone,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL,
    CONSTRAINT ck_cmn_pricing_condition_dates CHECK (((end_date IS NULL) OR (end_date >= start_date))),
    CONSTRAINT ck_cmn_pricing_condition_pricing_value CHECK ((pricing_value >= (0)::numeric)),
    CONSTRAINT ck_cmn_pricing_condition_rounding_precision CHECK ((rounding_precision > (0)::numeric))
);


ALTER TABLE public.cmn_pricing_condition OWNER TO postgres;

--
-- Name: cmn_pricing_condition_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.cmn_pricing_condition_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.cmn_pricing_condition_id_seq OWNER TO postgres;

--
-- Name: cmn_pricing_condition_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.cmn_pricing_condition_id_seq OWNED BY public.cmn_pricing_condition.id;


--
-- Name: cmn_pricing_method; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.cmn_pricing_method (
    id smallint NOT NULL,
    code character varying(20) NOT NULL,
    name character varying(200) NOT NULL
);


ALTER TABLE public.cmn_pricing_method OWNER TO postgres;

--
-- Name: cmn_product_price_type; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.cmn_product_price_type (
    id smallint NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(150) NOT NULL
);


ALTER TABLE public.cmn_product_price_type OWNER TO postgres;

--
-- Name: cmn_product_price_type_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.cmn_product_price_type_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.cmn_product_price_type_id_seq OWNER TO postgres;

--
-- Name: cmn_product_price_type_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.cmn_product_price_type_id_seq OWNED BY public.cmn_product_price_type.id;


--
-- Name: cmn_product_table_status; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.cmn_product_table_status (
    id smallint NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(100) NOT NULL,
    state_id smallint NOT NULL
);


ALTER TABLE public.cmn_product_table_status OWNER TO postgres;

--
-- Name: cmn_product_table_status_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.cmn_product_table_status_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.cmn_product_table_status_id_seq OWNER TO postgres;

--
-- Name: cmn_product_table_status_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.cmn_product_table_status_id_seq OWNED BY public.cmn_product_table_status.id;


--
-- Name: cmn_product_type; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.cmn_product_type (
    id smallint NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(250) NOT NULL,
    is_service boolean DEFAULT false NOT NULL,
    description character varying(1000) NOT NULL
);


ALTER TABLE public.cmn_product_type OWNER TO postgres;

--
-- Name: cmn_product_type_translation; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.cmn_product_type_translation (
    product_type_id smallint NOT NULL,
    language_id smallint NOT NULL,
    name character varying(250) NOT NULL,
    description character varying(1000) NOT NULL
);


ALTER TABLE public.cmn_product_type_translation OWNER TO postgres;

--
-- Name: cmn_region; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.cmn_region (
    id integer NOT NULL,
    short_name character varying(250) NOT NULL,
    full_name character varying(250) NOT NULL,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.cmn_region OWNER TO postgres;

--
-- Name: cmn_region_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.cmn_region_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.cmn_region_id_seq OWNER TO postgres;

--
-- Name: cmn_region_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.cmn_region_id_seq OWNED BY public.cmn_region.id;


--
-- Name: cmn_state; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.cmn_state (
    id smallint NOT NULL,
    short_name character varying(250) NOT NULL,
    full_name character varying(250) NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.cmn_state OWNER TO postgres;

--
-- Name: cmn_tax_type; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.cmn_tax_type (
    id smallint NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(150) NOT NULL,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.cmn_tax_type OWNER TO postgres;

--
-- Name: cmn_tax_type_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.cmn_tax_type_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.cmn_tax_type_id_seq OWNER TO postgres;

--
-- Name: cmn_tax_type_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.cmn_tax_type_id_seq OWNED BY public.cmn_tax_type.id;


--
-- Name: cmn_translation; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.cmn_translation (
    id bigint NOT NULL,
    language_id smallint NOT NULL,
    table_name character varying(100) NOT NULL,
    record_id bigint NOT NULL,
    column_name character varying(100) NOT NULL,
    value text NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.cmn_translation OWNER TO postgres;

--
-- Name: cmn_translation_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.cmn_translation_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.cmn_translation_id_seq OWNER TO postgres;

--
-- Name: cmn_translation_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.cmn_translation_id_seq OWNED BY public.cmn_translation.id;


--
-- Name: cmn_unit; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.cmn_unit (
    id smallint NOT NULL,
    code character varying(20) NOT NULL,
    name character varying(100) NOT NULL,
    state_id smallint NOT NULL
);


ALTER TABLE public.cmn_unit OWNER TO postgres;

--
-- Name: cmn_unit_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.cmn_unit_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.cmn_unit_id_seq OWNER TO postgres;

--
-- Name: cmn_unit_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.cmn_unit_id_seq OWNED BY public.cmn_unit.id;


--
-- Name: cmn_vat_rate; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.cmn_vat_rate (
    id smallint NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(150) NOT NULL,
    rate numeric(5,2) NOT NULL,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL,
    effective_from date,
    effective_to date,
    CONSTRAINT cmn_vat_rate_effective_dates_check CHECK (((effective_to IS NULL) OR (effective_from IS NULL) OR (effective_to >= effective_from)))
);


ALTER TABLE public.cmn_vat_rate OWNER TO postgres;

--
-- Name: cmn_vat_rate_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.cmn_vat_rate_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.cmn_vat_rate_id_seq OWNER TO postgres;

--
-- Name: cmn_vat_rate_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.cmn_vat_rate_id_seq OWNED BY public.cmn_vat_rate.id;


--
-- Name: contract_number_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.contract_number_seq
    START WITH 100000001
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.contract_number_seq OWNER TO postgres;

--
-- Name: counterparty_account_payment_purpose_hint; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.counterparty_account_payment_purpose_hint (
    counterparty_bank_account_id integer NOT NULL,
    payment_purpose_id smallint NOT NULL,
    usage_count integer DEFAULT 1 NOT NULL,
    last_used_date timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.counterparty_account_payment_purpose_hint OWNER TO postgres;

--
-- Name: counterparty_bank_account; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.counterparty_bank_account (
    id integer NOT NULL,
    organization_id integer NOT NULL,
    counterparty_id integer NOT NULL,
    bank_id integer NOT NULL,
    account_number character varying(50) NOT NULL,
    currency_id smallint NOT NULL,
    is_main boolean DEFAULT false NOT NULL,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.counterparty_bank_account OWNER TO postgres;

--
-- Name: counterparty_bank_account_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.counterparty_bank_account_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.counterparty_bank_account_id_seq OWNER TO postgres;

--
-- Name: counterparty_bank_account_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.counterparty_bank_account_id_seq OWNED BY public.counterparty_bank_account.id;


--
-- Name: counterparty_card; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.counterparty_card (
    id integer NOT NULL,
    organization_id integer NOT NULL,
    counterparty_type_id smallint NOT NULL,
    short_name character varying(250) NOT NULL,
    full_name character varying(500),
    inn character varying(20),
    phone_number character varying(50),
    email character varying(250),
    region_id integer,
    district_id integer,
    address character varying(1000),
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL,
    code character varying(100),
    is_customer boolean DEFAULT true NOT NULL,
    is_supplier boolean DEFAULT true NOT NULL,
    is_vat_payer boolean DEFAULT false NOT NULL,
    oked character varying(20),
    external_id character varying(100)
);


ALTER TABLE public.counterparty_card OWNER TO postgres;

--
-- Name: counterparty_card_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.counterparty_card_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.counterparty_card_id_seq OWNER TO postgres;

--
-- Name: counterparty_card_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.counterparty_card_id_seq OWNED BY public.counterparty_card.id;


--
-- Name: counterparty_contact; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.counterparty_contact (
    id integer NOT NULL,
    organization_id integer NOT NULL,
    counterparty_id integer NOT NULL,
    full_name character varying(250) NOT NULL,
    phone_number character varying(50),
    email character varying(250),
    "position" character varying(250),
    comment character varying(1000),
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.counterparty_contact OWNER TO postgres;

--
-- Name: counterparty_contact_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.counterparty_contact_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.counterparty_contact_id_seq OWNER TO postgres;

--
-- Name: counterparty_contact_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.counterparty_contact_id_seq OWNED BY public.counterparty_contact.id;


--
-- Name: counterparty_reg_balance; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.counterparty_reg_balance (
    id bigint NOT NULL,
    organization_id integer NOT NULL,
    document_type_id smallint NOT NULL,
    document_id bigint NOT NULL,
    counterparty_id integer NOT NULL,
    operation_type_id smallint NOT NULL,
    currency_id smallint NOT NULL,
    amount numeric(18,2) NOT NULL,
    doc_date timestamp without time zone NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL,
    posting_batch_id bigint,
    source_line_id bigint,
    reversal_entry_id bigint
);


ALTER TABLE public.counterparty_reg_balance OWNER TO postgres;

--
-- Name: counterparty_reg_balance_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.counterparty_reg_balance_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.counterparty_reg_balance_id_seq OWNER TO postgres;

--
-- Name: counterparty_reg_balance_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.counterparty_reg_balance_id_seq OWNED BY public.counterparty_reg_balance.id;


--
-- Name: doc_number_bank_operation_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.doc_number_bank_operation_seq
    START WITH 100000001
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.doc_number_bank_operation_seq OWNER TO postgres;

--
-- Name: doc_number_cash_operation_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.doc_number_cash_operation_seq
    START WITH 100000001
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.doc_number_cash_operation_seq OWNER TO postgres;

--
-- Name: doc_number_purchase_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.doc_number_purchase_seq
    START WITH 100000001
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.doc_number_purchase_seq OWNER TO postgres;

--
-- Name: doc_number_sale_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.doc_number_sale_seq
    START WITH 100000001
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.doc_number_sale_seq OWNER TO postgres;

--
-- Name: inv_inventory_adjustment_doc; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.inv_inventory_adjustment_doc (
    id bigint NOT NULL,
    organization_id integer NOT NULL,
    doc_number character varying(100) NOT NULL,
    doc_date timestamp without time zone NOT NULL,
    warehouse_id integer NOT NULL,
    adjustment_type character varying(50) NOT NULL,
    status_id smallint NOT NULL,
    comment character varying(1000),
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL,
    posted_at timestamp without time zone,
    posted_by_user_id integer,
    cancelled_at timestamp without time zone,
    cancelled_by_user_id integer
);


ALTER TABLE public.inv_inventory_adjustment_doc OWNER TO postgres;

--
-- Name: inv_inventory_adjustment_doc_table; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.inv_inventory_adjustment_doc_table (
    id bigint NOT NULL,
    owner_id bigint NOT NULL,
    product_table_id integer,
    cost_price numeric(24,8) NOT NULL,
    original_status_id smallint,
    original_state_id smallint,
    original_warehouse_id integer,
    was_created boolean DEFAULT false NOT NULL
);


ALTER TABLE public.inv_inventory_adjustment_doc_table OWNER TO postgres;

--
-- Name: inv_inventory_adjustment_line; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.inv_inventory_adjustment_line (
    id bigint NOT NULL,
    owner_id bigint NOT NULL,
    product_id integer NOT NULL,
    unit_id smallint NOT NULL,
    quantity numeric(24,8) NOT NULL,
    comment character varying(1000)
);


ALTER TABLE public.inv_inventory_adjustment_line OWNER TO postgres;

--
-- Name: inv_inventory_count_doc; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.inv_inventory_count_doc (
    id bigint NOT NULL,
    organization_id integer NOT NULL,
    doc_number character varying(100) NOT NULL,
    doc_date timestamp without time zone NOT NULL,
    warehouse_id integer NOT NULL,
    status_id smallint NOT NULL,
    comment character varying(1000),
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL,
    count_completed_at timestamp without time zone,
    count_completed_by_user_id integer,
    positive_adjustment_doc_id bigint,
    negative_adjustment_doc_id bigint,
    posted_at timestamp without time zone,
    posted_by_user_id integer,
    cancelled_at timestamp without time zone,
    cancelled_by_user_id integer
);


ALTER TABLE public.inv_inventory_count_doc OWNER TO postgres;

--
-- Name: inv_inventory_count_doc_table; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.inv_inventory_count_doc_table (
    id bigint NOT NULL,
    owner_id bigint NOT NULL,
    product_table_id integer,
    barcode character varying(100),
    serial_number character varying(250),
    marking_number character varying(250),
    cost_price numeric(24,8) NOT NULL
);


ALTER TABLE public.inv_inventory_count_doc_table OWNER TO postgres;

--
-- Name: inv_inventory_count_line; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.inv_inventory_count_line (
    id bigint NOT NULL,
    owner_id bigint NOT NULL,
    product_id integer NOT NULL,
    unit_id smallint NOT NULL,
    counted_quantity numeric(24,8) NOT NULL,
    default_cost_price numeric(24,8) NOT NULL,
    comment character varying(1000)
);


ALTER TABLE public.inv_inventory_count_line OWNER TO postgres;

--
-- Name: inv_product; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.inv_product (
    id integer NOT NULL,
    organization_id integer NOT NULL,
    product_group_id integer,
    unit_id smallint NOT NULL,
    barcode character varying(100),
    name character varying(250) NOT NULL,
    description character varying(1000),
    is_service boolean DEFAULT false NOT NULL,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL,
    mxik character varying(17),
    is_piece_tracked boolean DEFAULT false NOT NULL,
    code character varying(100),
    sku character varying(100),
    article character varying(100),
    default_vat_rate_id smallint,
    min_stock numeric(18,3),
    product_type_id smallint DEFAULT 1 NOT NULL,
    is_sold boolean DEFAULT true NOT NULL,
    is_purchased boolean DEFAULT true NOT NULL,
    CONSTRAINT chk_inv_product_sale_purchase_flags CHECK ((is_sold OR is_purchased)),
    CONSTRAINT ck_inv_product_mxik CHECK (((mxik IS NULL) OR ((mxik)::text ~ '^[A-Za-z0-9]{17}$'::text)))
);


ALTER TABLE public.inv_product OWNER TO postgres;

--
-- Name: inv_product_group; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.inv_product_group (
    id integer NOT NULL,
    organization_id integer NOT NULL,
    name character varying(250) NOT NULL,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL,
    code character varying(100),
    parent_id integer,
    sort_order integer DEFAULT 0 NOT NULL
);


ALTER TABLE public.inv_product_group OWNER TO postgres;

--
-- Name: inv_product_group_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.inv_product_group_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.inv_product_group_id_seq OWNER TO postgres;

--
-- Name: inv_product_group_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.inv_product_group_id_seq OWNED BY public.inv_product_group.id;


--
-- Name: inv_product_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.inv_product_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.inv_product_id_seq OWNER TO postgres;

--
-- Name: inv_product_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.inv_product_id_seq OWNED BY public.inv_product.id;


--
-- Name: inv_product_price; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.inv_product_price (
    id bigint NOT NULL,
    organization_id integer NOT NULL,
    product_id integer NOT NULL,
    currency_id smallint NOT NULL,
    price_type_id smallint NOT NULL,
    unit_id smallint NOT NULL,
    price numeric(24,8) NOT NULL,
    start_date timestamp without time zone DEFAULT now() NOT NULL,
    end_date timestamp without time zone,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL,
    CONSTRAINT ck_inv_product_price_dates CHECK (((end_date IS NULL) OR (end_date >= start_date))),
    CONSTRAINT ck_inv_product_price_price CHECK ((price >= (0)::numeric))
);


ALTER TABLE public.inv_product_price OWNER TO postgres;

--
-- Name: inv_product_price_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.inv_product_price_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.inv_product_price_id_seq OWNER TO postgres;

--
-- Name: inv_product_price_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.inv_product_price_id_seq OWNED BY public.inv_product_price.id;


--
-- Name: inv_product_table; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.inv_product_table (
    id integer NOT NULL,
    product_id integer NOT NULL,
    organization_id integer NOT NULL,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL,
    serial_number character varying(250),
    marking_number character varying(250),
    status_id smallint DEFAULT 1 NOT NULL,
    current_warehouse_id integer
);


ALTER TABLE public.inv_product_table OWNER TO postgres;

--
-- Name: inv_product_table_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.inv_product_table_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.inv_product_table_id_seq OWNER TO postgres;

--
-- Name: inv_product_table_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.inv_product_table_id_seq OWNED BY public.inv_product_table.id;


--
-- Name: inv_reg_balance; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.inv_reg_balance (
    id bigint NOT NULL,
    organization_id integer NOT NULL,
    document_type_id smallint NOT NULL,
    document_id bigint NOT NULL,
    warehouse_id integer NOT NULL,
    product_id integer NOT NULL,
    operation_type_id smallint NOT NULL,
    quantity numeric(18,3) NOT NULL,
    amount numeric(18,2) NOT NULL,
    doc_date timestamp without time zone NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL,
    posting_batch_id bigint,
    source_line_id bigint,
    reversal_entry_id bigint,
    product_table_id integer
);


ALTER TABLE public.inv_reg_balance OWNER TO postgres;

--
-- Name: inv_reg_balance_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.inv_reg_balance_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.inv_reg_balance_id_seq OWNER TO postgres;

--
-- Name: inv_reg_balance_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.inv_reg_balance_id_seq OWNED BY public.inv_reg_balance.id;


--
-- Name: inv_transfer_doc; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.inv_transfer_doc (
    id bigint NOT NULL,
    organization_id integer NOT NULL,
    doc_number character varying(100) NOT NULL,
    doc_date timestamp without time zone NOT NULL,
    source_warehouse_id integer NOT NULL,
    destination_warehouse_id integer NOT NULL,
    status_id smallint NOT NULL,
    comment character varying(1000),
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL,
    posted_at timestamp without time zone,
    posted_by_user_id integer,
    cancelled_at timestamp without time zone,
    cancelled_by_user_id integer
);


ALTER TABLE public.inv_transfer_doc OWNER TO postgres;

--
-- Name: inv_transfer_doc_table; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.inv_transfer_doc_table (
    id bigint NOT NULL,
    owner_id bigint NOT NULL,
    product_table_id integer NOT NULL,
    source_warehouse_id integer NOT NULL,
    destination_warehouse_id integer NOT NULL,
    cost_price numeric(24,8) NOT NULL
);


ALTER TABLE public.inv_transfer_doc_table OWNER TO postgres;

--
-- Name: inv_transfer_line; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.inv_transfer_line (
    id bigint NOT NULL,
    owner_id bigint NOT NULL,
    product_id integer NOT NULL,
    unit_id smallint NOT NULL,
    quantity numeric(24,8) NOT NULL,
    comment character varying(1000)
);


ALTER TABLE public.inv_transfer_line OWNER TO postgres;

--
-- Name: inv_warehouse; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.inv_warehouse (
    id integer NOT NULL,
    organization_id integer NOT NULL,
    branch_id integer,
    name character varying(250) NOT NULL,
    responsible_user_id integer,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL,
    code character varying(100),
    address character varying(1000),
    is_main boolean DEFAULT false NOT NULL
);


ALTER TABLE public.inv_warehouse OWNER TO postgres;

--
-- Name: inv_warehouse_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.inv_warehouse_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.inv_warehouse_id_seq OWNER TO postgres;

--
-- Name: inv_warehouse_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.inv_warehouse_id_seq OWNED BY public.inv_warehouse.id;


--
-- Name: money_reg_balance; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.money_reg_balance (
    id bigint NOT NULL,
    organization_id integer NOT NULL,
    document_type_id smallint NOT NULL,
    document_id bigint NOT NULL,
    source_type character varying(20) NOT NULL,
    source_id integer NOT NULL,
    operation_type_id smallint NOT NULL,
    currency_id smallint NOT NULL,
    amount numeric(18,2) NOT NULL,
    doc_date timestamp without time zone NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL,
    posting_batch_id bigint,
    source_line_id bigint,
    reversal_entry_id bigint
);


ALTER TABLE public.money_reg_balance OWNER TO postgres;

--
-- Name: money_reg_balance_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.money_reg_balance_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.money_reg_balance_id_seq OWNER TO postgres;

--
-- Name: money_reg_balance_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.money_reg_balance_id_seq OWNED BY public.money_reg_balance.id;


--
-- Name: org_bank_account; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.org_bank_account (
    id integer NOT NULL,
    organization_id integer NOT NULL,
    bank_id integer NOT NULL,
    account_number character varying(50) NOT NULL,
    currency_id smallint NOT NULL,
    is_main boolean DEFAULT false NOT NULL,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL,
    code character varying(100),
    name character varying(250),
    opening_balance numeric(18,2) DEFAULT 0 NOT NULL,
    opening_balance_date date
);


ALTER TABLE public.org_bank_account OWNER TO postgres;

--
-- Name: org_bank_account_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.org_bank_account_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.org_bank_account_id_seq OWNER TO postgres;

--
-- Name: org_bank_account_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.org_bank_account_id_seq OWNED BY public.org_bank_account.id;


--
-- Name: org_branch; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.org_branch (
    id integer NOT NULL,
    organization_id integer NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(250) NOT NULL,
    region_id integer,
    district_id integer,
    address character varying(1000),
    phone_number character varying(50),
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.org_branch OWNER TO postgres;

--
-- Name: org_branch_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.org_branch_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.org_branch_id_seq OWNER TO postgres;

--
-- Name: org_branch_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.org_branch_id_seq OWNED BY public.org_branch.id;


--
-- Name: org_claim_request; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.org_claim_request (
    id bigint NOT NULL,
    organization_id integer,
    requested_by_user_id integer NOT NULL,
    inn character varying(20) NOT NULL,
    organization_name character varying(500),
    status character varying(30) DEFAULT 'pending'::character varying NOT NULL,
    review_comment character varying(1000),
    reviewed_by_user_id integer,
    reviewed_at timestamp without time zone,
    created_date timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.org_claim_request OWNER TO postgres;

--
-- Name: org_claim_request_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.org_claim_request ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.org_claim_request_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: org_defaults; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.org_defaults (
    id integer NOT NULL,
    organization_id integer NOT NULL,
    branch_id integer,
    warehouse_id integer,
    cash_box_id integer,
    bank_account_id integer,
    receivable_account_id integer,
    payable_account_id integer,
    inventory_account_id integer,
    cash_account_id integer,
    bank_accounting_account_id integer,
    revenue_account_id integer,
    expense_account_id integer,
    cogs_account_id integer,
    created_date timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.org_defaults OWNER TO postgres;

--
-- Name: org_defaults_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.org_defaults ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.org_defaults_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: org_department; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.org_department (
    id integer NOT NULL,
    organization_id integer NOT NULL,
    branch_id integer,
    code character varying(50) NOT NULL,
    name character varying(250) NOT NULL,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.org_department OWNER TO postgres;

--
-- Name: org_department_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.org_department_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.org_department_id_seq OWNER TO postgres;

--
-- Name: org_department_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.org_department_id_seq OWNED BY public.org_department.id;


--
-- Name: org_organization; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.org_organization (
    id integer NOT NULL,
    short_name character varying(250) NOT NULL,
    full_name character varying(500) NOT NULL,
    inn character varying(20) NOT NULL,
    phone_number character varying(50),
    region_id integer NOT NULL,
    district_id integer,
    address character varying(1000),
    director character varying(250),
    is_parent boolean DEFAULT false NOT NULL,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL,
    default_language_id smallint,
    tenant_id integer,
    setup_status character varying(30) DEFAULT 'not_started'::character varying NOT NULL,
    setup_completed_at timestamp without time zone,
    email character varying(200),
    website character varying(250),
    oked character varying(20)
);


ALTER TABLE public.org_organization OWNER TO postgres;

--
-- Name: org_organization_config; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.org_organization_config (
    organization_id integer NOT NULL,
    inventory_valuation_method character varying(20) DEFAULT 'fifo'::character varying NOT NULL,
    accounting_policy_id smallint,
    base_currency_id smallint,
    accounting_start_date date,
    fiscal_year_start_month smallint DEFAULT 1 NOT NULL,
    CONSTRAINT org_organization_config_fiscal_year_start_month_check CHECK (((fiscal_year_start_month >= 1) AND (fiscal_year_start_month <= 12))),
    CONSTRAINT org_organization_config_inventory_valuation_method_check CHECK (((inventory_valuation_method)::text = ANY ((ARRAY['fifo'::character varying, 'lifo'::character varying, 'average'::character varying])::text[])))
);


ALTER TABLE public.org_organization_config OWNER TO postgres;

--
-- Name: org_organization_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.org_organization_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.org_organization_id_seq OWNER TO postgres;

--
-- Name: org_organization_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.org_organization_id_seq OWNED BY public.org_organization.id;


--
-- Name: org_position; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.org_position (
    id integer NOT NULL,
    organization_id integer NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(250) NOT NULL,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.org_position OWNER TO postgres;

--
-- Name: org_position_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.org_position_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.org_position_id_seq OWNER TO postgres;

--
-- Name: org_position_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.org_position_id_seq OWNED BY public.org_position.id;


--
-- Name: org_setup_state; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.org_setup_state (
    id integer NOT NULL,
    organization_id integer NOT NULL,
    current_step character varying(100) DEFAULT 'organization'::character varying NOT NULL,
    organization_completed boolean DEFAULT false NOT NULL,
    tax_completed boolean DEFAULT false NOT NULL,
    accounting_completed boolean DEFAULT false NOT NULL,
    defaults_completed boolean DEFAULT false NOT NULL,
    users_completed boolean DEFAULT false NOT NULL,
    is_completed boolean DEFAULT false NOT NULL,
    completed_at timestamp without time zone,
    updated_date timestamp without time zone DEFAULT now() NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.org_setup_state OWNER TO postgres;

--
-- Name: org_setup_state_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.org_setup_state ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.org_setup_state_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: org_tax_settings; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.org_tax_settings (
    id integer NOT NULL,
    organization_id integer NOT NULL,
    tax_type_id smallint NOT NULL,
    is_vat_payer boolean DEFAULT false NOT NULL,
    vat_registration_number character varying(100),
    effective_from date DEFAULT CURRENT_DATE NOT NULL,
    effective_to date,
    state_id smallint DEFAULT 1 NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL,
    CONSTRAINT ck_org_tax_settings_dates CHECK (((effective_to IS NULL) OR (effective_to >= effective_from)))
);


ALTER TABLE public.org_tax_settings OWNER TO postgres;

--
-- Name: org_tax_settings_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.org_tax_settings ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.org_tax_settings_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: org_user_invitation; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.org_user_invitation (
    id bigint NOT NULL,
    organization_id integer NOT NULL,
    email character varying(200) NOT NULL,
    role_id integer NOT NULL,
    invited_by_user_id integer,
    token_hash character varying(512) NOT NULL,
    expires_at timestamp without time zone NOT NULL,
    accepted_at timestamp without time zone,
    accepted_by_user_id integer,
    state_id smallint DEFAULT 1 NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.org_user_invitation OWNER TO postgres;

--
-- Name: org_user_invitation_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.org_user_invitation ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.org_user_invitation_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: platform_tenant; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.platform_tenant (
    id integer NOT NULL,
    name character varying(250) NOT NULL,
    slug character varying(150) NOT NULL,
    owner_user_id integer,
    state_id smallint DEFAULT 1 NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL,
    updated_date timestamp without time zone
);


ALTER TABLE public.platform_tenant OWNER TO postgres;

--
-- Name: platform_tenant_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.platform_tenant ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.platform_tenant_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: pur_doc; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.pur_doc (
    id bigint NOT NULL,
    organization_id integer NOT NULL,
    doc_number character varying(100) NOT NULL,
    doc_date timestamp without time zone NOT NULL,
    counterparty_id integer NOT NULL,
    warehouse_id integer NOT NULL,
    currency_id smallint NOT NULL,
    total_amount numeric(24,8) DEFAULT 0 NOT NULL,
    vat_amount numeric(24,8) DEFAULT 0 NOT NULL,
    final_amount numeric(24,8) DEFAULT 0 NOT NULL,
    status_id smallint NOT NULL,
    comment character varying(1000),
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL,
    contract_id bigint,
    exchange_rate numeric(18,6) DEFAULT 1 NOT NULL,
    posted_at timestamp without time zone,
    posted_by_user_id integer,
    cancelled_at timestamp without time zone,
    cancelled_by_user_id integer
);


ALTER TABLE public.pur_doc OWNER TO postgres;

--
-- Name: pur_doc_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.pur_doc_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.pur_doc_id_seq OWNER TO postgres;

--
-- Name: pur_doc_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.pur_doc_id_seq OWNED BY public.pur_doc.id;


--
-- Name: pur_doc_product; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.pur_doc_product (
    id bigint NOT NULL,
    owner_id bigint NOT NULL,
    product_id integer NOT NULL,
    quantity numeric(19,6) NOT NULL,
    unit_id smallint NOT NULL,
    amount numeric(24,8) NOT NULL,
    vat_rate_id smallint,
    vat_amount numeric(24,8) NOT NULL,
    total_amount numeric(24,8) NOT NULL,
    unit_price numeric(24,8) NOT NULL
);


ALTER TABLE public.pur_doc_product OWNER TO postgres;

--
-- Name: pur_doc_product_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.pur_doc_product_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.pur_doc_product_id_seq OWNER TO postgres;

--
-- Name: pur_doc_product_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.pur_doc_product_id_seq OWNED BY public.pur_doc_product.id;


--
-- Name: pur_doc_table; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.pur_doc_table (
    id bigint NOT NULL,
    owner_id bigint NOT NULL,
    product_table_id integer NOT NULL,
    amount numeric(24,8) NOT NULL,
    vat_rate_id smallint,
    vat_amount numeric(24,8) NOT NULL,
    total_amount numeric(24,8) NOT NULL
);


ALTER TABLE public.pur_doc_table OWNER TO postgres;

--
-- Name: pur_doc_table_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.pur_doc_table_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.pur_doc_table_id_seq OWNER TO postgres;

--
-- Name: pur_doc_table_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.pur_doc_table_id_seq OWNED BY public.pur_doc_table.id;


--
-- Name: sale_condition; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.sale_condition (
    id bigint NOT NULL,
    organization_id integer NOT NULL,
    costing_method_id smallint NOT NULL,
    vat_rate_id smallint NOT NULL,
    start_date timestamp without time zone DEFAULT now() NOT NULL,
    end_date timestamp without time zone,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL,
    CONSTRAINT ck_sale_condition_dates CHECK (((end_date IS NULL) OR (end_date >= start_date)))
);


ALTER TABLE public.sale_condition OWNER TO postgres;

--
-- Name: sale_condition_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.sale_condition_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.sale_condition_id_seq OWNER TO postgres;

--
-- Name: sale_condition_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.sale_condition_id_seq OWNED BY public.sale_condition.id;


--
-- Name: sale_doc; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.sale_doc (
    id bigint NOT NULL,
    organization_id integer NOT NULL,
    doc_number character varying(100) NOT NULL,
    doc_date timestamp without time zone NOT NULL,
    counterparty_id integer NOT NULL,
    warehouse_id integer NOT NULL,
    currency_id smallint NOT NULL,
    total_amount numeric(24,8) DEFAULT 0 NOT NULL,
    vat_amount numeric(24,8) DEFAULT 0 NOT NULL,
    final_amount numeric(24,8) DEFAULT 0 NOT NULL,
    status_id smallint NOT NULL,
    comment character varying(1000),
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL,
    contract_id bigint,
    exchange_rate numeric(18,6) DEFAULT 1 NOT NULL,
    posted_at timestamp without time zone,
    posted_by_user_id integer,
    cancelled_at timestamp without time zone,
    cancelled_by_user_id integer
);


ALTER TABLE public.sale_doc OWNER TO postgres;

--
-- Name: sale_doc_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.sale_doc_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.sale_doc_id_seq OWNER TO postgres;

--
-- Name: sale_doc_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.sale_doc_id_seq OWNED BY public.sale_doc.id;


--
-- Name: sale_doc_product; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.sale_doc_product (
    id bigint NOT NULL,
    owner_id bigint NOT NULL,
    product_id integer NOT NULL,
    quantity numeric(19,6) NOT NULL,
    unit_price numeric(24,8) NOT NULL,
    cost_price numeric(24,8) DEFAULT 0 NOT NULL,
    amount numeric(24,8) NOT NULL,
    vat_rate_id smallint,
    vat_amount numeric(24,8) DEFAULT 0 NOT NULL,
    total_amount numeric(24,8) NOT NULL,
    unit_id smallint DEFAULT 1 NOT NULL
);


ALTER TABLE public.sale_doc_product OWNER TO postgres;

--
-- Name: sale_doc_product_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.sale_doc_product_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.sale_doc_product_id_seq OWNER TO postgres;

--
-- Name: sale_doc_product_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.sale_doc_product_id_seq OWNED BY public.sale_doc_product.id;


--
-- Name: sale_doc_table; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.sale_doc_table (
    id bigint NOT NULL,
    product_table_id integer NOT NULL,
    amount numeric(24,8) NOT NULL,
    vat_rate_id smallint,
    vat_amount numeric(24,8) DEFAULT 0 NOT NULL,
    total_amount numeric(24,8) NOT NULL,
    cost_price numeric(24,8) DEFAULT 0 NOT NULL,
    owner_id bigint NOT NULL
);


ALTER TABLE public.sale_doc_table OWNER TO postgres;

--
-- Name: sale_doc_table_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.sale_doc_table_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.sale_doc_table_id_seq OWNER TO postgres;

--
-- Name: sale_doc_table_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.sale_doc_table_id_seq OWNED BY public.sale_doc_table.id;


--
-- Name: sys_audit_log; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.sys_audit_log (
    id bigint NOT NULL,
    organization_id integer,
    schema_name character varying(100) NOT NULL,
    table_name character varying(100) NOT NULL,
    record_id character varying(100),
    action character varying(10) NOT NULL,
    old_data jsonb,
    new_data jsonb,
    changed_user_id integer,
    request_id character varying(100),
    client_addr inet,
    application_name character varying(200),
    changed_date timestamp without time zone DEFAULT now() NOT NULL,
    CONSTRAINT chk_sys_audit_log_action CHECK (((action)::text = ANY ((ARRAY['INSERT'::character varying, 'UPDATE'::character varying, 'DELETE'::character varying])::text[])))
);


ALTER TABLE public.sys_audit_log OWNER TO postgres;

--
-- Name: sys_audit_log_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.sys_audit_log_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.sys_audit_log_id_seq OWNER TO postgres;

--
-- Name: sys_audit_log_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.sys_audit_log_id_seq OWNED BY public.sys_audit_log.id;


--
-- Name: sys_email_verification_token; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.sys_email_verification_token (
    id bigint NOT NULL,
    user_id integer NOT NULL,
    token_hash character varying(512) NOT NULL,
    email character varying(200) NOT NULL,
    expires_at timestamp without time zone NOT NULL,
    verified_at timestamp without time zone,
    created_date timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.sys_email_verification_token OWNER TO postgres;

--
-- Name: sys_email_verification_token_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.sys_email_verification_token ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.sys_email_verification_token_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: sys_module; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.sys_module (
    id integer NOT NULL,
    code character varying(100) NOT NULL,
    short_name character varying(250) NOT NULL,
    full_name character varying(300) NOT NULL,
    sub_group_id integer NOT NULL,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL,
    parent_id integer,
    route character varying(250),
    icon character varying(100),
    sort_order integer DEFAULT 0 NOT NULL,
    is_visible boolean DEFAULT true NOT NULL
);


ALTER TABLE public.sys_module OWNER TO postgres;

--
-- Name: sys_module_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.sys_module_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.sys_module_id_seq OWNER TO postgres;

--
-- Name: sys_module_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.sys_module_id_seq OWNED BY public.sys_module.id;


--
-- Name: sys_module_sub_group; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.sys_module_sub_group (
    id integer NOT NULL,
    code character varying(100) NOT NULL,
    short_name character varying(250) NOT NULL,
    full_name character varying(300) NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.sys_module_sub_group OWNER TO postgres;

--
-- Name: sys_module_sub_group_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.sys_module_sub_group_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.sys_module_sub_group_id_seq OWNER TO postgres;

--
-- Name: sys_module_sub_group_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.sys_module_sub_group_id_seq OWNED BY public.sys_module_sub_group.id;


--
-- Name: sys_password_reset_token; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.sys_password_reset_token (
    id bigint NOT NULL,
    user_id integer NOT NULL,
    token_hash character varying(512) NOT NULL,
    expires_at timestamp without time zone NOT NULL,
    used_at timestamp without time zone,
    created_date timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.sys_password_reset_token OWNER TO postgres;

--
-- Name: sys_password_reset_token_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.sys_password_reset_token ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.sys_password_reset_token_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: sys_refresh_token; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.sys_refresh_token (
    id bigint NOT NULL,
    user_id integer NOT NULL,
    token_hash character varying(512) NOT NULL,
    jwt_id character varying(128),
    device_name character varying(250),
    ip_address character varying(64),
    user_agent character varying(500),
    expires_at timestamp without time zone NOT NULL,
    revoked_at timestamp without time zone,
    replaced_by_token_hash character varying(512),
    created_date timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.sys_refresh_token OWNER TO postgres;

--
-- Name: sys_refresh_token_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.sys_refresh_token ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.sys_refresh_token_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: sys_role; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.sys_role (
    id integer NOT NULL,
    short_name character varying(100) NOT NULL,
    full_name character varying(255) NOT NULL,
    state_id smallint NOT NULL,
    created_date timestamp without time zone DEFAULT now() NOT NULL,
    organization_id integer,
    has_global_access boolean DEFAULT false NOT NULL,
    code character varying(100),
    description character varying(500),
    is_system boolean DEFAULT false NOT NULL,
    is_owner_role boolean DEFAULT false NOT NULL,
    sort_order integer DEFAULT 0 NOT NULL
);


ALTER TABLE public.sys_role OWNER TO postgres;

--
-- Name: sys_role_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.sys_role_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.sys_role_id_seq OWNER TO postgres;

--
-- Name: sys_role_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.sys_role_id_seq OWNED BY public.sys_role.id;


--
-- Name: sys_role_module; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.sys_role_module (
    role_id integer NOT NULL,
    module_id integer NOT NULL,
    created_date timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.sys_role_module OWNER TO postgres;

--
-- Name: sys_user; Type: TABLE; Schema: public; Owner: postgres
--

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
    organization_id integer,
    email_verified boolean DEFAULT false NOT NULL,
    email_verified_at timestamp without time zone,
    last_login_ip character varying(64),
    is_platform_admin boolean DEFAULT false NOT NULL,
    timezone character varying(100)
);


ALTER TABLE public.sys_user OWNER TO postgres;

--
-- Name: sys_user_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.sys_user_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.sys_user_id_seq OWNER TO postgres;

--
-- Name: sys_user_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.sys_user_id_seq OWNED BY public.sys_user.id;


--
-- Name: sys_user_organization; Type: TABLE; Schema: public; Owner: postgres
--

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
    blocked_at timestamp without time zone
);


ALTER TABLE public.sys_user_organization OWNER TO postgres;

--
-- Name: acc_account_resolve_rule id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_account_resolve_rule ALTER COLUMN id SET DEFAULT nextval('public.acc_account_resolve_rule_id_seq'::regclass);


--
-- Name: acc_account_type id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_account_type ALTER COLUMN id SET DEFAULT nextval('public.acc_account_type_id_seq'::regclass);


--
-- Name: acc_accounting_policy id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_accounting_policy ALTER COLUMN id SET DEFAULT nextval('public.acc_accounting_policy_id_seq'::regclass);


--
-- Name: acc_chart_account id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_chart_account ALTER COLUMN id SET DEFAULT nextval('public.acc_chart_account_id_seq'::regclass);


--
-- Name: acc_chart_account_subkonto id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_chart_account_subkonto ALTER COLUMN id SET DEFAULT nextval('public.acc_chart_account_subkonto_id_seq'::regclass);


--
-- Name: acc_payment_purpose id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_payment_purpose ALTER COLUMN id SET DEFAULT nextval('public.acc_payment_purpose_id_seq'::regclass);


--
-- Name: acc_posting_alias id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_posting_alias ALTER COLUMN id SET DEFAULT nextval('public.acc_posting_alias_id_seq'::regclass);


--
-- Name: acc_posting_rule id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_posting_rule ALTER COLUMN id SET DEFAULT nextval('public.acc_posting_rule_id_seq'::regclass);


--
-- Name: acc_posting_rule_line id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_posting_rule_line ALTER COLUMN id SET DEFAULT nextval('public.acc_posting_rule_line_id_seq'::regclass);


--
-- Name: acc_reg_entry id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_reg_entry ALTER COLUMN id SET DEFAULT nextval('public.acc_reg_entry_id_seq'::regclass);


--
-- Name: acc_reg_entry_subkonto id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_reg_entry_subkonto ALTER COLUMN id SET DEFAULT nextval('public.acc_reg_entry_subkonto_id_seq'::regclass);


--
-- Name: acc_subkonto_type id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_subkonto_type ALTER COLUMN id SET DEFAULT nextval('public.acc_subkonto_type_id_seq'::regclass);


--
-- Name: bank_operation id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.bank_operation ALTER COLUMN id SET DEFAULT nextval('public.bank_operation_id_seq'::regclass);


--
-- Name: bank_operation_line id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.bank_operation_line ALTER COLUMN id SET DEFAULT nextval('public.bank_operation_line_id_seq'::regclass);


--
-- Name: cash_box id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cash_box ALTER COLUMN id SET DEFAULT nextval('public.cash_box_id_seq'::regclass);


--
-- Name: cash_operation id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cash_operation ALTER COLUMN id SET DEFAULT nextval('public.cash_operation_id_seq'::regclass);


--
-- Name: cmn_bank id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_bank ALTER COLUMN id SET DEFAULT nextval('public.cmn_bank_id_seq'::regclass);


--
-- Name: cmn_contract id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_contract ALTER COLUMN id SET DEFAULT nextval('public.cmn_contract_id_seq'::regclass);


--
-- Name: cmn_contract_type id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_contract_type ALTER COLUMN id SET DEFAULT nextval('public.cmn_contract_type_id_seq'::regclass);


--
-- Name: cmn_costing_method id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_costing_method ALTER COLUMN id SET DEFAULT nextval('public.cmn_costing_method_id_seq'::regclass);


--
-- Name: cmn_counterparty_type id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_counterparty_type ALTER COLUMN id SET DEFAULT nextval('public.cmn_counterparty_type_id_seq'::regclass);


--
-- Name: cmn_currency id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_currency ALTER COLUMN id SET DEFAULT nextval('public.cmn_currency_id_seq'::regclass);


--
-- Name: cmn_district id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_district ALTER COLUMN id SET DEFAULT nextval('public.cmn_district_id_seq'::regclass);


--
-- Name: cmn_document_status id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_document_status ALTER COLUMN id SET DEFAULT nextval('public.cmn_document_status_id_seq'::regclass);


--
-- Name: cmn_document_type id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_document_type ALTER COLUMN id SET DEFAULT nextval('public.cmn_document_type_id_seq'::regclass);


--
-- Name: cmn_language id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_language ALTER COLUMN id SET DEFAULT nextval('public.cmn_language_id_seq'::regclass);


--
-- Name: cmn_operation_type id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_operation_type ALTER COLUMN id SET DEFAULT nextval('public.cmn_operation_type_id_seq'::regclass);


--
-- Name: cmn_payment_type id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_payment_type ALTER COLUMN id SET DEFAULT nextval('public.cmn_payment_type_id_seq'::regclass);


--
-- Name: cmn_price_rounding_method id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_price_rounding_method ALTER COLUMN id SET DEFAULT nextval('public.cmn_price_rounding_method_id_seq'::regclass);


--
-- Name: cmn_pricing_condition id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_pricing_condition ALTER COLUMN id SET DEFAULT nextval('public.cmn_pricing_condition_id_seq'::regclass);


--
-- Name: cmn_product_price_type id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_product_price_type ALTER COLUMN id SET DEFAULT nextval('public.cmn_product_price_type_id_seq'::regclass);


--
-- Name: cmn_product_table_status id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_product_table_status ALTER COLUMN id SET DEFAULT nextval('public.cmn_product_table_status_id_seq'::regclass);


--
-- Name: cmn_region id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_region ALTER COLUMN id SET DEFAULT nextval('public.cmn_region_id_seq'::regclass);


--
-- Name: cmn_tax_type id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_tax_type ALTER COLUMN id SET DEFAULT nextval('public.cmn_tax_type_id_seq'::regclass);


--
-- Name: cmn_translation id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_translation ALTER COLUMN id SET DEFAULT nextval('public.cmn_translation_id_seq'::regclass);


--
-- Name: cmn_unit id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_unit ALTER COLUMN id SET DEFAULT nextval('public.cmn_unit_id_seq'::regclass);


--
-- Name: cmn_vat_rate id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_vat_rate ALTER COLUMN id SET DEFAULT nextval('public.cmn_vat_rate_id_seq'::regclass);


--
-- Name: counterparty_bank_account id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.counterparty_bank_account ALTER COLUMN id SET DEFAULT nextval('public.counterparty_bank_account_id_seq'::regclass);


--
-- Name: counterparty_card id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.counterparty_card ALTER COLUMN id SET DEFAULT nextval('public.counterparty_card_id_seq'::regclass);


--
-- Name: counterparty_contact id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.counterparty_contact ALTER COLUMN id SET DEFAULT nextval('public.counterparty_contact_id_seq'::regclass);


--
-- Name: counterparty_reg_balance id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.counterparty_reg_balance ALTER COLUMN id SET DEFAULT nextval('public.counterparty_reg_balance_id_seq'::regclass);


--
-- Name: inv_product id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_product ALTER COLUMN id SET DEFAULT nextval('public.inv_product_id_seq'::regclass);


--
-- Name: inv_product_group id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_product_group ALTER COLUMN id SET DEFAULT nextval('public.inv_product_group_id_seq'::regclass);


--
-- Name: inv_product_price id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_product_price ALTER COLUMN id SET DEFAULT nextval('public.inv_product_price_id_seq'::regclass);


--
-- Name: inv_product_table id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_product_table ALTER COLUMN id SET DEFAULT nextval('public.inv_product_table_id_seq'::regclass);


--
-- Name: inv_reg_balance id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_reg_balance ALTER COLUMN id SET DEFAULT nextval('public.inv_reg_balance_id_seq'::regclass);


--
-- Name: inv_warehouse id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_warehouse ALTER COLUMN id SET DEFAULT nextval('public.inv_warehouse_id_seq'::regclass);


--
-- Name: money_reg_balance id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.money_reg_balance ALTER COLUMN id SET DEFAULT nextval('public.money_reg_balance_id_seq'::regclass);


--
-- Name: org_bank_account id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_bank_account ALTER COLUMN id SET DEFAULT nextval('public.org_bank_account_id_seq'::regclass);


--
-- Name: org_branch id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_branch ALTER COLUMN id SET DEFAULT nextval('public.org_branch_id_seq'::regclass);


--
-- Name: org_department id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_department ALTER COLUMN id SET DEFAULT nextval('public.org_department_id_seq'::regclass);


--
-- Name: org_organization id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_organization ALTER COLUMN id SET DEFAULT nextval('public.org_organization_id_seq'::regclass);


--
-- Name: org_position id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_position ALTER COLUMN id SET DEFAULT nextval('public.org_position_id_seq'::regclass);


--
-- Name: pur_doc id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.pur_doc ALTER COLUMN id SET DEFAULT nextval('public.pur_doc_id_seq'::regclass);


--
-- Name: pur_doc_product id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.pur_doc_product ALTER COLUMN id SET DEFAULT nextval('public.pur_doc_product_id_seq'::regclass);


--
-- Name: pur_doc_table id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.pur_doc_table ALTER COLUMN id SET DEFAULT nextval('public.pur_doc_table_id_seq'::regclass);


--
-- Name: sale_condition id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sale_condition ALTER COLUMN id SET DEFAULT nextval('public.sale_condition_id_seq'::regclass);


--
-- Name: sale_doc id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sale_doc ALTER COLUMN id SET DEFAULT nextval('public.sale_doc_id_seq'::regclass);


--
-- Name: sale_doc_product id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sale_doc_product ALTER COLUMN id SET DEFAULT nextval('public.sale_doc_product_id_seq'::regclass);


--
-- Name: sale_doc_table id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sale_doc_table ALTER COLUMN id SET DEFAULT nextval('public.sale_doc_table_id_seq'::regclass);


--
-- Name: sys_audit_log id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sys_audit_log ALTER COLUMN id SET DEFAULT nextval('public.sys_audit_log_id_seq'::regclass);


--
-- Name: sys_module id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sys_module ALTER COLUMN id SET DEFAULT nextval('public.sys_module_id_seq'::regclass);


--
-- Name: sys_module_sub_group id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sys_module_sub_group ALTER COLUMN id SET DEFAULT nextval('public.sys_module_sub_group_id_seq'::regclass);


--
-- Name: sys_role id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sys_role ALTER COLUMN id SET DEFAULT nextval('public.sys_role_id_seq'::regclass);


--
-- Name: sys_user id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sys_user ALTER COLUMN id SET DEFAULT nextval('public.sys_user_id_seq'::regclass);


--
-- Data for Name: acc_account_resolve_rule; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.acc_account_resolve_rule (id, policy_id, alias, dimension_key, dimension_value, account_id, priority) FROM stdin;
34	1	Inventory	_none	_default	1014	100
35	1	Expense	serviceType	production	1013	10
36	1	Expense	serviceType	admin	1034	10
37	1	Expense	serviceType	_default	1035	100
38	1	Supplier	_none	_default	1036	100
39	1	SupplierAdvance	_none	_default	1016	100
40	1	Customer	_none	_default	1015	100
41	1	CustomerAdvance	_none	_default	1037	100
42	1	PaymentAccount	paymentMethod	bank	1027	10
43	1	PaymentAccount	paymentMethod	cash	1048	10
44	1	PaymentAccount	paymentMethod	_default	1027	100
51	1	AssetWriteOff	assetType	inventory	1014	10
52	1	Employee	_none	_default	1067	100
53	1	EmployeeAdvance	_none	_default	1071	100
54	1	Founder	_none	_default	1066	100
55	1	LoanGiven	_none	_default	1047	100
56	1	LoanReceived	_none	_default	1069	100
59	1	TaxProfit	_none	_default	1055	100
60	1	TaxExcise	_none	_default	1056	100
61	1	TaxProperty	_none	_default	1057	100
62	1	TaxLand	_none	_default	1058	100
63	1	TaxOther	_none	_default	1059	100
66	1	BankFee	_none	_default	1035	100
68	1	CashInTransit	_none	_default	1073	100
46	1	VATOut	_none	_default	1039	100
47	1	SalesRevenue	_none	_default	1042	100
48	1	ServiceRevenue	_none	_default	1045	100
49	1	CostOfGoods	_none	_default	1029	100
50	1	CostOfService	_none	_default	1032	100
57	1	TaxVAT	_none	_default	1039	100
58	1	TaxNDFL	_none	_default	1051	100
64	1	SocialInsurance	_none	_default	1061	100
65	1	PensionFund	_none	_default	1064	100
69	1	VATIn	vatKind	goods	1020	10
70	1	VATIn	vatKind	services	1021	10
71	1	VATIn	vatKind	_default	1020	100
\.


--
-- Data for Name: acc_account_type; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.acc_account_type (id, code, name, state_id, created_date) FROM stdin;
1	active	Aktiv	1	2026-06-09 15:28:20.198084
2	passive	Passiv	1	2026-06-09 15:28:20.198084
3	active_passive	Aktiv-passiv	1	2026-06-09 15:28:20.198084
4	off_balance	Balansdan tashqari	1	2026-06-09 15:28:20.198084
\.


--
-- Data for Name: acc_accounting_period; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.acc_accounting_period (id, organization_id, year, month, start_date, end_date, is_closed, closed_at, closed_by_user_id, created_date) FROM stdin;
\.


--
-- Data for Name: acc_accounting_policy; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.acc_accounting_policy (id, code, name, state_id) FROM stdin;
1	STANDARD	Стандартная политика РУз	1
\.


--
-- Data for Name: acc_chart_account; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.acc_chart_account (id, parent_id, code, name, is_group, state_id, created_date, account_type_id, is_quantity, is_currency) FROM stdin;
1047	\N	4720	Расчеты по предоставленным займам	f	1	2026-06-25 16:48:03.401278	1	f	f
1048	\N	5010	Основная касса организации	f	1	2026-06-25 16:48:03.401278	1	f	f
1049	\N	5020	Основная касса организации (в валюте)	f	1	2026-06-25 16:48:03.401278	1	f	t
1050	\N	6420	Налог на доходы физических лиц	t	1	2026-06-25 16:48:03.401278	2	f	f
1051	\N	6420.1	НДФЛ (оплате труда)	f	1	2026-06-25 16:48:03.401278	2	f	f
1052	\N	6420.2	НДФЛ (дивиденды и проценты)	f	1	2026-06-25 16:48:03.401278	2	f	f
1053	\N	6420.3	НДФЛ (прочее)	f	1	2026-06-25 16:48:03.401278	2	f	f
1054	\N	6420.4	НДФЛ (индивидуального предпринимателя)	f	1	2026-06-25 16:48:03.401278	2	f	f
1055	\N	6430	Налог на прибыль (ЕНП при УСН)	f	1	2026-06-25 16:48:03.401278	2	f	f
1056	\N	6440	Акцизы	f	1	2026-06-25 16:48:03.401278	2	f	f
1057	\N	6450	Налог на имущество	f	1	2026-06-25 16:48:03.401278	2	f	f
1058	\N	6460	Земельный налог	f	1	2026-06-25 16:48:03.401278	2	f	f
1059	\N	6490	Прочие налоги и сборы	f	1	2026-06-25 16:48:03.401278	2	f	f
1060	\N	6510	Расчеты по социальному страхованию	t	1	2026-06-25 16:48:03.401278	2	f	f
1061	\N	6510.1	Расчеты по единому социальному платежу	f	1	2026-06-25 16:48:03.401278	2	f	f
1062	\N	6510.2	Расчеты по взносам граждан в ГПФ	f	1	2026-06-25 16:48:03.401278	2	f	f
1063	\N	6530	Расчеты по пенсионному обеспечению	t	1	2026-06-25 16:48:03.401278	2	f	f
1064	\N	6530.1	ИНПС (обязательная часть)	f	1	2026-06-25 16:48:03.401278	2	f	f
1065	\N	6530.2	ИНПС (добровольная часть)	f	1	2026-06-25 16:48:03.401278	2	f	f
1066	\N	6610	Расчеты по выплате доходов (дивидендов)	f	1	2026-06-25 16:48:03.401278	2	f	f
1068	\N	6715	Расчеты с персоналом по оплате труда (в валюте)	f	1	2026-06-25 16:48:03.401278	2	f	t
1070	\N	6815	Краткосрочные банковские кредиты (в валюте)	f	1	2026-06-25 16:48:03.401278	2	f	t
1072	\N	6975	Расчеты с подотчетными лицами (в валюте)	f	1	2026-06-25 16:48:03.401278	1	f	t
1067	\N	6710	Расчеты с персоналом по оплате труда	f	1	2026-06-25 16:48:03.401278	2	f	f
1069	\N	6810	Краткосрочные банковские кредиты	f	1	2026-06-25 16:48:03.401278	2	f	f
1071	\N	6970	Расчеты с подотчетными лицами	f	1	2026-06-25 16:48:03.401278	1	f	f
1073	\N	5710	Переводы в пути	f	1	2026-07-04 09:53:20.29198	1	f	f
1029	1028	9120.1	Себестоимость реализованных товаров по деятельности с основной системой налогообложения	f	1	2026-06-25 10:24:59.251712	1	f	f
1030	1028	9120.2	Себестоимость реализованных товаров по деятельности с особой системой налогообложения	f	1	2026-06-25 10:24:59.251712	1	f	f
1018	1017	4410.1	НДС при приобретении основных средств	f	1	2026-06-25 10:24:59.251712	1	f	f
1019	1017	4410.2	НДС по приобретенным нематериальным активам	f	1	2026-06-25 10:24:59.251712	1	f	f
1020	1017	4410.3	НДС по приобретенным материально-производственным запасам	f	1	2026-06-25 10:24:59.251712	1	f	f
1021	1017	4410.4	НДС по приобретенным услугам	f	1	2026-06-25 10:24:59.251712	1	f	f
1022	1017	4410.5	НДС, уплаченный при ввозе товаров на территорию Республики Узбекистан	f	1	2026-06-25 10:24:59.251712	1	f	f
1023	1017	4410.7	НДС по товарам, реализованным по ставке 0% (экспорт)	f	1	2026-06-25 10:24:59.251712	1	f	f
1024	1017	4410.8	НДС при строительстве основных средств	f	1	2026-06-25 10:24:59.251712	1	f	f
1025	1017	4410.9	НДС по уменьшению стоимости реализации	f	1	2026-06-25 10:24:59.251712	1	f	f
1026	1017	4410.10	Авансовые платежи по НДС	f	1	2026-06-25 10:24:59.251712	1	f	f
1004	\N	1010	Сырье и материалы	f	1	2026-06-25 10:24:59.251712	1	t	f
1005	\N	1020	Покупные полуфабрикаты и комплектующие изделия, конструкции и детали	f	1	2026-06-25 10:24:59.251712	1	t	f
1006	\N	1030	Топливо	f	1	2026-06-25 10:24:59.251712	1	t	f
1007	\N	1040	Запасные части	f	1	2026-06-25 10:24:59.251712	1	t	f
1008	\N	1050	Строительные материалы	f	1	2026-06-25 10:24:59.251712	1	t	f
1009	\N	1060	Тара и тарные материалы	f	1	2026-06-25 10:24:59.251712	1	t	f
1010	\N	1070	Материалы, переданные в переработку на сторону	f	1	2026-06-25 10:24:59.251712	1	t	f
1011	\N	1080	Инвентарь и хозяйственные принадлежности	f	1	2026-06-25 10:24:59.251712	1	t	f
1012	\N	1090	Прочие материалы	f	1	2026-06-25 10:24:59.251712	1	t	f
1013	\N	2010	Основное производство	f	1	2026-06-25 10:24:59.251712	1	f	f
1014	\N	2910	Товары на складах	f	1	2026-06-25 10:24:59.251712	1	t	f
1015	\N	4010	Расчеты с покупателями и заказчиками	f	1	2026-06-25 10:24:59.251712	1	f	f
1016	\N	4310	Расчеты по авансам выданным	f	1	2026-06-25 10:24:59.251712	1	f	f
1017	\N	4410	НДС по приобретенным ценностям	t	1	2026-06-25 10:24:59.251712	1	f	f
1027	\N	5110	Расчетные счета	f	1	2026-06-25 10:24:59.251712	1	f	f
1028	\N	9120	Себестоимость реализованных товаров	t	1	2026-06-25 10:24:59.251712	1	f	f
1031	\N	9130	Себестоимость выполненных работ, оказанных услуг	t	1	2026-06-25 10:24:59.251712	1	f	f
1034	\N	9420	Административные расходы	f	1	2026-06-25 10:24:59.251712	1	f	f
1035	\N	9430	Прочие операционные расходы	f	1	2026-06-25 10:24:59.251712	1	f	f
1036	\N	6010	Расчеты с поставщиками и подрядчиками	f	1	2026-06-25 10:24:59.251712	2	f	f
1037	\N	6310	Расчеты по авансам полученным	f	1	2026-06-25 10:24:59.251712	2	f	f
1038	\N	6410	Налог на добавленную стоимость	t	1	2026-06-25 10:24:59.251712	2	f	f
1041	\N	9020	Доходы от реализации товаров	t	1	2026-06-25 10:24:59.251712	3	f	f
1044	\N	9030	Доходы от выполнения работ, оказания услуг	t	1	2026-06-25 10:24:59.251712	3	f	f
1039	1038	6410.1	Налог на добавленную стоимость начисленный при реализации	f	1	2026-06-25 10:24:59.251712	2	f	f
1040	1038	6410.2	НДС при исполнении обязанностей налогового агента	f	1	2026-06-25 10:24:59.251712	2	f	f
1042	1041	9020.1	Доходы от реализации товаров по деятельности с основной системой налогообложения	f	1	2026-06-25 10:24:59.251712	3	f	f
1043	1041	9020.2	Доходы от реализации товаров по деятельности с особой системой налогообложения	f	1	2026-06-25 10:24:59.251712	3	f	f
1045	1044	9030.1	Доходы от выполнение работ, оказания услуг по деятельности с основной системой налогообложения	f	1	2026-06-25 10:24:59.251712	3	f	f
1046	1044	9030.2	Доходы от выполнения работ, оказания услуг по деятельности с особой системой налогообложения	f	1	2026-06-25 10:24:59.251712	3	f	f
1032	1031	9130.1	Себестоимость выполненных работ, оказанных услуг по деятельности с основной системой налогообложения	f	1	2026-06-25 10:24:59.251712	1	f	f
1033	1031	9130.2	Себестоимость выполненных работ, оказанных услуг по деятельности с особой системой налогообложения	f	1	2026-06-25 10:24:59.251712	1	f	f
\.


--
-- Data for Name: acc_chart_account_subkonto; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.acc_chart_account_subkonto (id, organization_id, account_id, subkonto_type_id, sort_order, is_required, state_id, created_date) FROM stdin;
\.


--
-- Data for Name: acc_payment_purpose; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.acc_payment_purpose (id, code, alias_id, name, requires_counterparty, operation_type_id) FROM stdin;
3	CUSTOMER_RECEIPT	5	Payment from customer	t	1
4	CUSTOMER_ADVANCE	6	Advance from customer	t	1
9	LOAN_REPAYMENT	19	Loan repayment	f	1
1	SUPPLIER_PAYMENT	3	Supplier payment	t	2
2	SUPPLIER_ADVANCE	4	Advance to supplier	t	2
5	SALARY	15	Salary	t	2
6	ACCOUNTABLE_ADVANCE	16	Accountable amounts	t	2
7	DIVIDENDS	17	Dividend payment	t	2
8	LOAN_GIVEN	18	Loan given	t	2
10	LOAN_RECEIVED	19	Loan received	f	2
11	TAX_VAT	20	VAT	f	2
12	TAX_NDFL	21	Personal income tax	f	2
13	TAX_PROFIT	22	Profit tax	f	2
14	TAX_PROPERTY	24	Property tax	f	2
15	TAX_LAND	25	Land tax	f	2
16	SOCIAL_INSURANCE	27	Social insurance tax	f	2
17	PENSION_FUND	28	Pension fund	f	2
18	BANK_FEE	29	Bank fee	f	2
21	UNSPECIFIED_IN	5	Назначение не указано (приход)	f	1
22	UNSPECIFIED_OUT	3	Назначение не указано (расход)	f	2
23	CASH_COLLECTION_SENT	34	Cash collection to transit	f	2
24	CASH_COLLECTION_RECEIVED	34	Cash collection from transit	f	1
\.


--
-- Data for Name: acc_payment_purpose_translation; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) FROM stdin;
3	1	Xaridordan to'lov
4	1	Xaridordan avans
9	1	Kreditni qaytarish
1	1	Yetkazib beruvchiga to'lov
2	1	Yetkazib beruvchiga avans
5	1	Mehnat haqi
6	1	Hisobdor summalar
7	1	Dividendlar to'lovi
8	1	Qarz berish
10	1	Kredit olish
11	1	QQS
12	1	JShDS
13	1	Foyda solig'i
14	1	Mol-mulk solig'i
15	1	Yer solig'i
16	1	Ijtimoiy soliq (YaIJ)
17	1	INPS
18	1	Bank komissiyasi
3	2	Оплата от клиента
4	2	Аванс от клиента
9	2	Погашение кредита
1	2	Оплата поставщику
2	2	Аванс поставщику
5	2	Заработная плата
6	2	Подотчётные суммы
7	2	Выплата дивидендов
8	2	Выдача займа
10	2	Получение кредита
11	2	НДС
12	2	НДФЛ
13	2	Налог на прибыль
14	2	Налог на имущество
15	2	Земельный налог
16	2	Социальный налог (ЕСП)
17	2	ИНПС
18	2	Банковская комиссия
3	3	Payment from customer
4	3	Advance from customer
9	3	Loan repayment
1	3	Supplier payment
2	3	Advance to supplier
5	3	Salary
6	3	Accountable amounts
7	3	Dividend payment
8	3	Loan given
10	3	Loan received
11	3	VAT
12	3	Personal income tax
13	3	Profit tax
14	3	Property tax
15	3	Land tax
16	3	Social insurance tax
17	3	Pension fund
18	3	Bank fee
\.


--
-- Data for Name: acc_posting_alias; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.acc_posting_alias (id, code, name) FROM stdin;
1	Inventory	Tovar ombor qoldig'i
2	Expense	Xarajat (xizmat)
3	Supplier	Yetkazib beruvchi — qarzni yopish
4	SupplierAdvance	Yetkazib beruvchiga avans
5	Customer	Xaridor — qarzni yopish
6	CustomerAdvance	Xaridordan avans
7	SalesRevenue	Tovarlar realizatsiyasidan daromad
8	ServiceRevenue	Xizmatlardan daromad
9	VATIn	Hisobga olinadigan QQS
10	VATOut	To'lanadigan QQS
11	CostOfGoods	Sotilgan tovarlar tannarxi
12	CostOfService	Ko'rsatilgan xizmatlar tannarxi
13	AssetWriteOff	Aktivni hisobdan chiqarish (tovar/OS)
14	PaymentAccount	Pul hisobvarag'i (bank/kassa)
15	Employee	Xodim — mehnat haqi
16	EmployeeAdvance	Xodim — hisobdor summalar
17	Founder	Asoschi — dividendlar
18	LoanGiven	Berilgan qarz
19	LoanReceived	Olingan kredit/qarz
20	TaxVAT	QQS (byudjet)
21	TaxNDFL	JShDS
22	TaxProfit	Foyda solig'i
23	TaxExcise	Aktsiz solig'i
24	TaxProperty	Mol-mulk solig'i
25	TaxLand	Yer solig'i
26	TaxOther	Boshqa soliq va yig'imlar
27	SocialInsurance	Ijtimoiy soliq (YaIJ)
28	PensionFund	INPS
29	BankFee	Bank komissiyasi
30	CashBoxSource	Naqd pulni hisobvaraqlari (keluvchi)
31	CashBoxDestination	Naqd pulni hisobvaraqlari (chiquvchi)
32	TaxAuthority	Soliq organi / byudjet bilan hisob-kitob
34	CashInTransit	Денежные средства в пути
\.


--
-- Data for Name: acc_posting_alias_translation; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.acc_posting_alias_translation (posting_alias_id, language_id, name) FROM stdin;
1	1	Tovar ombor qoldig'i
2	1	Xarajat (xizmat)
3	1	Yetkazib beruvchi — qarzni yopish
4	1	Yetkazib beruvchiga avans
5	1	Xaridor — qarzni yopish
6	1	Xaridordan avans
7	1	Tovarlar realizatsiyasidan daromad
8	1	Xizmatlardan daromad
9	1	Hisobga olinadigan QQS
10	1	To'lanadigan QQS
11	1	Sotilgan tovarlar tannarxi
12	1	Ko'rsatilgan xizmatlar tannarxi
13	1	Aktivni hisobdan chiqarish (tovar/OS)
14	1	Pul hisobvarag'i (bank/kassa)
15	1	Xodim — mehnat haqi
16	1	Xodim — hisobdor summalar
17	1	Asoschi — dividendlar
18	1	Berilgan qarz
19	1	Olingan kredit/qarz
20	1	QQS (byudjet)
21	1	JShDS
22	1	Foyda solig'i
23	1	Aktsiz solig'i
24	1	Mol-mulk solig'i
25	1	Yer solig'i
26	1	Boshqa soliq va yig'imlar
27	1	Ijtimoiy soliq (YaIJ)
28	1	INPS
29	1	Bank komissiyasi
1	2	Товар на складе
2	2	Расход (услуга)
3	2	Поставщик — погашение долга
4	2	Поставщик — аванс
5	2	Покупатель — погашение долга
6	2	Покупатель — аванс
7	2	Доход от реализации товаров
8	2	Доход от услуг
9	2	НДС к зачёту
10	2	НДС к уплате
11	2	Себестоимость реализованных товаров
12	2	Себестоимость услуг
13	2	Списание актива (товар/ОС)
14	2	Денежный счёт (банк/касса)
15	2	Сотрудник — оплата труда
16	2	Сотрудник — подотчётные суммы
17	2	Учредитель — дивиденды
18	2	Заём выданный
19	2	Кредит/заём полученный
20	2	НДС (бюджет)
21	2	НДФЛ
22	2	Налог на прибыль
23	2	Акцизы
24	2	Налог на имущество
25	2	Земельный налог
26	2	Прочие налоги и сборы
27	2	Социальный налог (ЕСП)
28	2	ИНПС
29	2	Банковская комиссия
1	3	Inventory on stock
2	3	Expense (service)
3	3	Supplier — debt settlement
4	3	Advance to supplier
5	3	Customer — debt settlement
6	3	Advance from customer
7	3	Revenue from goods sales
8	3	Revenue from services
9	3	Input VAT
10	3	Output VAT
11	3	Cost of goods sold
12	3	Cost of services rendered
13	3	Asset write-off (inventory/fixed asset)
14	3	Cash account (bank/cash)
15	3	Employee — payroll
16	3	Employee — accountable amounts
17	3	Founder — dividends
18	3	Loan given
19	3	Loan/credit received
20	3	VAT (budget)
21	3	Personal income tax
22	3	Profit tax
23	3	Excise tax
24	3	Property tax
25	3	Land tax
26	3	Other taxes and fees
27	3	Social insurance tax
28	3	Pension fund
29	3	Bank fee
30	1	Naqd pulni hisobvaraqlari (keluvchi)
31	1	Naqd pulni hisobvaraqlari (chiquvchi)
30	2	Источник кассового счета
31	2	Счет кассы назначения
30	3	Cash box source account
31	3	Cash box destination account
32	3	Tax authority / budget settlement
32	1	Soliq organi / byudjet bilan hisob-kitob
32	2	Налоговый орган / расчёты с бюджетом
34	1	Yo'ldagi pul mablag'lari
34	2	Денежные средства в пути
34	3	Cash in transit
\.


--
-- Data for Name: acc_posting_batch; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.acc_posting_batch (id, organization_id, document_type_id, document_id, status, posted_by_user_id, posted_at, reversed_by_user_id, reversed_at, comment) FROM stdin;
4	8	4	4	REVERSAL	4	2026-07-03 11:24:39.808074	\N	\N	Cash operation cancelled
3	8	4	4	REVERSED	4	2026-07-02 19:45:36.122916	4	2026-07-03 11:24:40.404157	Cash operation confirmed
6	8	4	5	REVERSAL	4	2026-07-03 11:42:48.955476	\N	\N	Cash operation cancelled
5	8	4	5	REVERSED	4	2026-07-03 11:42:39.413371	4	2026-07-03 11:42:49.002278	Cash operation confirmed
7	8	4	6	POSTED	4	2026-07-03 11:55:54.036363	\N	\N	Cash operation confirmed
9	8	4	7	REVERSAL	4	2026-07-03 12:04:51.374787	\N	\N	Cash operation cancelled
8	8	4	7	REVERSED	4	2026-07-03 12:04:18.170951	4	2026-07-03 12:04:51.393346	Cash operation confirmed
14	8	3	41	POSTED	4	2026-07-04 08:30:34.323424	\N	\N	Bank operation confirmed
18	8	4	8	POSTED	4	2026-07-04 11:03:33.889251	\N	\N	Cash operation confirmed
19	8	4	10	POSTED	4	2026-07-04 12:05:26.406604	\N	\N	Cash operation confirmed
20	8	4	11	POSTED	4	2026-07-04 12:08:18.603622	\N	\N	Cash operation confirmed
21	8	4	12	POSTED	4	2026-07-04 12:19:36.110062	\N	\N	Cash operation confirmed
22	8	4	15	POSTED	4	2026-07-04 12:22:53.884931	\N	\N	Cash operation confirmed
23	8	4	14	POSTED	4	2026-07-04 12:23:18.028871	\N	\N	Cash operation confirmed
24	8	4	13	POSTED	4	2026-07-04 12:24:02.043734	\N	\N	Cash operation confirmed
25	8	4	16	POSTED	4	2026-07-04 12:24:35.2951	\N	\N	Cash operation confirmed
26	8	4	9	POSTED	4	2026-07-04 12:27:31.956262	\N	\N	Cash operation confirmed
\.


--
-- Data for Name: acc_posting_rule; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.acc_posting_rule (id, code, name) FROM stdin;
1	PURCHASE_GOODS	Поступление товара
2	PURCHASE_SERVICE	Получение услуги
3	SALE_GOODS	Реализация товара
4	SALE_SERVICE	Оказанная услуга
5	DEBIT_OPERATION	Банковская/кассовая операция — приход
6	CREDIT_OPERATION	Банковская/кассовая операция — расход
\.


--
-- Data for Name: acc_posting_rule_line; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.acc_posting_rule_line (id, template_id, order_number, amount_source, is_optional, debit_alias_id, credit_alias_id) FROM stdin;
7	3	3	Cost	f	11	1
4	2	2	VAT	f	9	3
3	2	1	Base	f	2	3
2	1	2	VAT	f	9	3
1	1	1	Base	f	1	3
11	5	1	Total	t	14	5
12	5	1	Total	t	14	6
5	3	1	Base	f	5	7
8	4	1	Base	f	5	8
9	4	2	VAT	f	5	10
6	3	2	VAT	f	5	10
10	4	3	Cost	t	12	13
21	6	1	Total	t	18	14
20	6	1	Total	t	32	14
19	6	1	Total	t	17	14
18	6	1	Total	t	16	14
17	6	1	Total	t	15	14
16	6	1	Total	t	4	14
15	6	1	Total	t	3	14
14	5	1	Total	t	14	16
13	5	1	Total	t	14	19
22	5	1	Total	t	14	34
23	6	1	Total	t	34	14
\.


--
-- Data for Name: acc_reg_entry; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.acc_reg_entry (id, organization_id, document_type_id, document_id, debit_account_id, credit_account_id, currency_id, amount, doc_date, created_date, operation_type_id, debit_quantity, credit_quantity, content, journal_number, posting_batch_id, source_line_id, reversal_entry_id) FROM stdin;
234	8	1	92	1014	1036	1	40000.00	2026-06-27 18:05:43	2026-06-27 13:07:18.414255	\N	4.000	\N	Поступление товара	PUR-2026-000001	\N	\N	\N
235	8	1	92	1017	1036	1	4800.00	2026-06-27 18:05:43	2026-06-27 13:07:18.419823	\N	4.000	\N	Поступление товара	PUR-2026-000001	\N	\N	\N
236	8	1	93	1035	1036	1	10000.00	2026-06-27 18:07:17	2026-06-27 13:07:50.821646	\N	\N	\N	Получение услуги	PUR-2026-000001	\N	\N	\N
237	8	1	93	1017	1036	1	1200.00	2026-06-27 18:07:17	2026-06-27 13:07:50.821707	\N	\N	\N	Получение услуги	PUR-2026-000001	\N	\N	\N
238	8	1	94	1035	1036	1	10000.00	2026-06-29 10:06:32	2026-06-29 05:13:54.422249	\N	\N	\N	Получение услуги	PUR-2026-000001	\N	\N	\N
239	8	1	94	1017	1036	1	1200.00	2026-06-29 10:06:32	2026-06-29 05:13:54.422277	\N	\N	\N	Получение услуги	PUR-2026-000001	\N	\N	\N
240	8	1	95	1014	1036	1	33000.00	2026-06-15 14:58:00	2026-06-29 09:59:15.866571	\N	3.000	\N	Поступление товара	PUR-2026-000001	\N	\N	\N
241	8	1	95	1017	1036	1	3960.00	2026-06-15 14:58:00	2026-06-29 09:59:15.87216	\N	3.000	\N	Поступление товара	PUR-2026-000001	\N	\N	\N
242	8	1	96	1014	1036	1	30000.00	2026-06-29 14:59:15	2026-06-29 10:01:54.065351	\N	2.000	\N	Поступление товара	PUR-2026-000001	\N	\N	\N
243	8	1	96	1017	1036	1	4500.00	2026-06-29 14:59:15	2026-06-29 10:01:54.065385	\N	2.000	\N	Поступление товара	PUR-2026-000001	\N	\N	\N
244	8	1	97	1035	1036	1	20000.00	2026-06-29 17:55:00	2026-06-29 12:57:52.692291	\N	\N	\N	Получение услуги	PUR-2026-000001	\N	\N	\N
245	8	1	97	1017	1036	1	2400.00	2026-06-29 17:55:00	2026-06-29 12:57:52.698531	\N	\N	\N	Получение услуги	PUR-2026-000001	\N	\N	\N
246	8	2	80	1028	1014	1	178080.00	2026-06-29 17:36:29.055026	2026-06-30 07:28:29.912929	\N	\N	3.000	Реализация товара	100000078	\N	\N	\N
247	8	2	80	1028	1014	1	118720.00	2026-06-29 17:36:29.055026	2026-06-30 07:28:29.915641	\N	\N	2.000	Реализация товара	100000078	\N	\N	\N
248	8	2	80	1015	1041	1	326480.00	2026-06-29 17:36:29.055026	2026-06-30 07:28:29.915652	\N	\N	\N	Реализация товара	100000078	\N	\N	\N
249	8	2	80	1015	1038	1	39177.60	2026-06-29 17:36:29.055026	2026-06-30 07:28:29.915764	\N	\N	\N	Реализация товара	100000078	\N	\N	\N
250	8	3	37	1027	1037	1	2550000.00	2026-07-02 08:31:56	2026-07-02 10:05:02.980548	\N	\N	\N	Банковская/кассовая операция — приход		\N	2	\N
251	8	4	4	1027	1015	1	2550000.00	2026-07-02 17:28:22	2026-07-02 14:45:36.52786	\N	\N	\N	Банковская/кассовая операция — приход	100000002	3	\N	\N
252	8	4	4	1027	1037	1	2550000.00	2026-07-02 17:28:22	2026-07-02 14:45:36.530947	\N	\N	\N	Банковская/кассовая операция — приход	100000002	3	\N	\N
253	8	4	4	1027	1069	1	2550000.00	2026-07-02 17:28:22	2026-07-02 14:45:36.530954	\N	\N	\N	Банковская/кассовая операция — приход	100000002	3	\N	\N
254	8	4	4	1027	1071	1	2550000.00	2026-07-02 17:28:22	2026-07-02 14:45:36.530957	\N	\N	\N	Банковская/кассовая операция — приход	100000002	3	\N	\N
255	8	4	4	1015	1027	1	2550000.00	2026-07-03 11:24:39.883602	2026-07-03 11:24:39.883602	\N	\N	\N	Reversal: Банковская/кассовая операция — приход	100000002	4	\N	251
256	8	4	4	1037	1027	1	2550000.00	2026-07-03 11:24:39.883602	2026-07-03 11:24:39.883602	\N	\N	\N	Reversal: Банковская/кассовая операция — приход	100000002	4	\N	252
257	8	4	4	1069	1027	1	2550000.00	2026-07-03 11:24:39.883602	2026-07-03 11:24:39.883602	\N	\N	\N	Reversal: Банковская/кассовая операция — приход	100000002	4	\N	253
258	8	4	4	1071	1027	1	2550000.00	2026-07-03 11:24:39.883602	2026-07-03 11:24:39.883602	\N	\N	\N	Reversal: Банковская/кассовая операция — приход	100000002	4	\N	254
259	8	4	5	1027	1015	1	10000000.00	2026-07-03 11:37:07	2026-07-03 06:42:39.588563	\N	\N	\N	Банковская/кассовая операция — приход	100000003	5	\N	\N
260	8	4	5	1027	1037	1	10000000.00	2026-07-03 11:37:07	2026-07-03 06:42:39.593266	\N	\N	\N	Банковская/кассовая операция — приход	100000003	5	\N	\N
261	8	4	5	1027	1069	1	10000000.00	2026-07-03 11:37:07	2026-07-03 06:42:39.593304	\N	\N	\N	Банковская/кассовая операция — приход	100000003	5	\N	\N
262	8	4	5	1027	1071	1	10000000.00	2026-07-03 11:37:07	2026-07-03 06:42:39.593314	\N	\N	\N	Банковская/кассовая операция — приход	100000003	5	\N	\N
263	8	4	5	1015	1027	1	10000000.00	2026-07-03 11:42:48.967906	2026-07-03 11:42:48.967906	\N	\N	\N	Reversal: Банковская/кассовая операция — приход	100000003	6	\N	259
264	8	4	5	1037	1027	1	10000000.00	2026-07-03 11:42:48.967906	2026-07-03 11:42:48.967906	\N	\N	\N	Reversal: Банковская/кассовая операция — приход	100000003	6	\N	260
265	8	4	5	1069	1027	1	10000000.00	2026-07-03 11:42:48.967906	2026-07-03 11:42:48.967906	\N	\N	\N	Reversal: Банковская/кассовая операция — приход	100000003	6	\N	261
266	8	4	5	1071	1027	1	10000000.00	2026-07-03 11:42:48.967906	2026-07-03 11:42:48.967906	\N	\N	\N	Reversal: Банковская/кассовая операция — приход	100000003	6	\N	262
267	8	4	6	1027	1015	1	2550000.00	2026-07-03 11:55:01	2026-07-03 06:55:54.044259	\N	\N	\N	Банковская/кассовая операция — приход	100000004	7	\N	\N
268	8	4	6	1027	1037	1	2550000.00	2026-07-03 11:55:01	2026-07-03 06:55:54.044278	\N	\N	\N	Банковская/кассовая операция — приход	100000004	7	\N	\N
269	8	4	6	1027	1069	1	2550000.00	2026-07-03 11:55:01	2026-07-03 06:55:54.044289	\N	\N	\N	Банковская/кассовая операция — приход	100000004	7	\N	\N
270	8	4	6	1027	1071	1	2550000.00	2026-07-03 11:55:01	2026-07-03 06:55:54.044307	\N	\N	\N	Банковская/кассовая операция — приход	100000004	7	\N	\N
271	8	4	7	1027	1015	1	1000.00	2026-07-03 12:03:25	2026-07-03 07:04:18.178728	\N	\N	\N	Банковская/кассовая операция — приход	100000005	8	\N	\N
272	8	4	7	1027	1037	1	1000.00	2026-07-03 12:03:25	2026-07-03 07:04:18.17875	\N	\N	\N	Банковская/кассовая операция — приход	100000005	8	\N	\N
273	8	4	7	1027	1069	1	1000.00	2026-07-03 12:03:25	2026-07-03 07:04:18.178762	\N	\N	\N	Банковская/кассовая операция — приход	100000005	8	\N	\N
274	8	4	7	1027	1071	1	1000.00	2026-07-03 12:03:25	2026-07-03 07:04:18.178769	\N	\N	\N	Банковская/кассовая операция — приход	100000005	8	\N	\N
275	8	4	7	1015	1027	1	1000.00	2026-07-03 12:04:51.379721	2026-07-03 12:04:51.379721	\N	\N	\N	Reversal: Банковская/кассовая операция — приход	100000005	9	\N	271
276	8	4	7	1037	1027	1	1000.00	2026-07-03 12:04:51.379721	2026-07-03 12:04:51.379721	\N	\N	\N	Reversal: Банковская/кассовая операция — приход	100000005	9	\N	272
277	8	4	7	1069	1027	1	1000.00	2026-07-03 12:04:51.379721	2026-07-03 12:04:51.379721	\N	\N	\N	Reversal: Банковская/кассовая операция — приход	100000005	9	\N	273
278	8	4	7	1071	1027	1	1000.00	2026-07-03 12:04:51.379721	2026-07-03 12:04:51.379721	\N	\N	\N	Reversal: Банковская/кассовая операция — приход	100000005	9	\N	274
279	8	3	41	1027	1037	1	3550000.00	2026-07-04 03:27:53	2026-07-04 03:30:34.64211	\N	\N	\N	Банковская/кассовая операция — приход	100000040	14	7	\N
280	8	4	8	1073	1027	1	1550000.00	2026-07-04 10:53:00	2026-07-04 11:03:34.043319	\N	\N	\N	Банковская/кассовая операция — расход	100000006	18	\N	\N
281	8	4	10	1027	1015	1	100000000.00	2026-07-04 10:53:00	2026-07-04 12:05:26.4335	\N	\N	\N	Банковская/кассовая операция — приход	100000008	19	\N	\N
282	8	4	11	1036	1048	1	40000000.00	2026-07-04 11:09:31	2026-07-04 12:08:18.626581	\N	\N	\N	Банковская/кассовая операция — расход	100000009	20	\N	\N
283	8	4	12	1048	1073	1	10000000.00	2026-07-04 12:10:12	2026-07-04 12:19:36.135048	\N	\N	\N	Банковская/кассовая операция — приход	100000010	21	\N	\N
284	8	4	15	1048	1073	1	2550000.00	2026-07-04 12:21:58	2026-07-04 12:22:53.907191	\N	\N	\N	Банковская/кассовая операция — приход	100000013	22	\N	\N
285	8	4	14	1048	1073	1	10000000.00	2026-07-04 12:21:58	2026-07-04 12:23:18.047998	\N	\N	\N	Банковская/кассовая операция — приход	100000012	23	\N	\N
286	8	4	13	1048	1073	1	3550000.00	2026-07-04 12:19:59	2026-07-04 12:24:02.054261	\N	\N	\N	Банковская/кассовая операция — приход	100000011	24	\N	\N
287	8	4	16	1048	1073	1	1000.00	2026-07-04 12:24:15	2026-07-04 12:24:35.314042	\N	\N	\N	Банковская/кассовая операция — приход	100000014	25	\N	\N
288	8	4	9	1036	1027	1	1000.00	2026-07-04 10:53:00	2026-07-04 12:27:31.965826	\N	\N	\N	Банковская/кассовая операция — расход	100000007	26	\N	\N
\.


--
-- Data for Name: acc_reg_entry_subkonto; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.acc_reg_entry_subkonto (id, entry_id, side, subkonto_type_id, sort_order, entity_id, display_value, created_date) FROM stdin;
763	234	CR	3	4	18	Artel	2026-06-27 18:07:18.419193
764	234	CR	9	5	9	{"number":"100000009","date":"2026-05-20T12:58:18.282"}	2026-06-27 18:07:18.419326
765	234	DR	1	1	23	ARTEL, икки камерали HD 316 FND ECO FROST қора-жилосиз ранг	2026-06-27 18:07:18.419534
766	234	DR	2	2	7	amonov	2026-06-27 18:07:18.419535
767	234	DR	10	3	92	{"number":"PUR-2026-000001","date":"2026-06-27T18:05:43"}	2026-06-27 18:07:18.419536
768	235	CR	3	4	18	Artel	2026-06-27 18:07:18.419828
769	235	CR	9	5	9	{"number":"100000009","date":"2026-05-20T12:58:18.282"}	2026-06-27 18:07:18.419829
770	235	DR	10	3	92	{"number":"PUR-2026-000001","date":"2026-06-27T18:05:43"}	2026-06-27 18:07:18.420028
771	235	DR	3	4	18	Artel	2026-06-27 18:07:18.420029
772	236	CR	3	2	18	Artel	2026-06-27 18:07:50.821664
773	236	CR	9	3	10	{"number":"100000010","date":"2026-06-23T05:58:06"}	2026-06-27 18:07:50.821666
774	237	CR	3	2	18	Artel	2026-06-27 18:07:50.82171
775	237	CR	9	3	10	{"number":"100000010","date":"2026-06-23T05:58:06"}	2026-06-27 18:07:50.821711
776	237	DR	10	1	93	{"number":"PUR-2026-000001","date":"2026-06-27T18:07:17"}	2026-06-27 18:07:50.821713
777	237	DR	3	2	18	Artel	2026-06-27 18:07:50.821714
778	238	CR	3	2	18	Artel	2026-06-29 10:13:54.422262
779	238	CR	9	3	9	{"number":"100000009","date":"2026-05-20T12:58:18.282"}	2026-06-29 10:13:54.422265
780	239	CR	3	2	18	Artel	2026-06-29 10:13:54.42228
781	239	CR	9	3	9	{"number":"100000009","date":"2026-05-20T12:58:18.282"}	2026-06-29 10:13:54.422282
782	239	DR	10	1	94	{"number":"PUR-2026-000001","date":"2026-06-29T10:06:32"}	2026-06-29 10:13:54.422286
783	239	DR	3	2	18	Artel	2026-06-29 10:13:54.422288
784	240	CR	3	4	18	Artel	2026-06-29 14:59:15.871554
785	240	CR	9	5	9	{"number":"100000009","date":"2026-05-20T12:58:18.282"}	2026-06-29 14:59:15.87172
786	240	DR	1	1	23	ARTEL, икки камерали HD 316 FND ECO FROST қора-жилосиз ранг	2026-06-29 14:59:15.871928
787	240	DR	2	2	7	amonov	2026-06-29 14:59:15.87193
788	240	DR	10	3	95	{"number":"PUR-2026-000001","date":"2026-06-15T14:58:00"}	2026-06-29 14:59:15.871933
789	241	CR	3	4	18	Artel	2026-06-29 14:59:15.872167
790	241	CR	9	5	9	{"number":"100000009","date":"2026-05-20T12:58:18.282"}	2026-06-29 14:59:15.87217
791	241	DR	10	3	95	{"number":"PUR-2026-000001","date":"2026-06-15T14:58:00"}	2026-06-29 14:59:15.872368
792	241	DR	3	4	18	Artel	2026-06-29 14:59:15.87237
793	242	CR	3	4	18	Artel	2026-06-29 15:01:54.065364
794	242	CR	9	5	10	{"number":"100000010","date":"2026-06-23T05:58:06"}	2026-06-29 15:01:54.065366
795	242	DR	1	1	24	ARTEL, икки камерали HD 341 FND ECO FROST ёмғирли-асфалт ранг	2026-06-29 15:01:54.065371
796	242	DR	2	2	7	amonov	2026-06-29 15:01:54.065373
797	242	DR	10	3	96	{"number":"PUR-2026-000001","date":"2026-06-29T14:59:15"}	2026-06-29 15:01:54.065375
798	243	CR	3	4	18	Artel	2026-06-29 15:01:54.065401
799	243	CR	9	5	10	{"number":"100000010","date":"2026-06-23T05:58:06"}	2026-06-29 15:01:54.065403
800	243	DR	10	3	96	{"number":"PUR-2026-000001","date":"2026-06-29T14:59:15"}	2026-06-29 15:01:54.065407
801	243	DR	3	4	18	Artel	2026-06-29 15:01:54.065409
802	244	CR	3	2	18	Artel	2026-06-29 17:57:52.698151
803	244	CR	9	3	9	{"number":"100000009","date":"2026-05-20T12:58:18.282"}	2026-06-29 17:57:52.698316
804	245	CR	3	2	18	Artel	2026-06-29 17:57:52.69854
805	245	CR	9	3	9	{"number":"100000009","date":"2026-05-20T12:58:18.282"}	2026-06-29 17:57:52.698543
806	245	DR	10	1	97	{"number":"PUR-2026-000001","date":"2026-06-29T17:55:00"}	2026-06-29 17:57:52.698858
807	245	DR	3	2	18	Artel	2026-06-29 17:57:52.698862
808	246	CR	1	1	23	ARTEL, икки камерали HD 316 FND ECO FROST қора-жилосиз ранг	2026-06-30 12:28:29.915457
809	246	CR	2	2	7	amonov	2026-06-30 12:28:29.915524
810	246	CR	10	3	95	{"number":"100000077","date":"2026-06-15T14:58:00"}	2026-06-30 12:28:29.915525
811	246	CR	11	4	80	{"number":"100000078","date":"2026-06-29T17:36:29.055026"}	2026-06-30 12:28:29.915525
812	247	CR	1	1	23	ARTEL, икки камерали HD 316 FND ECO FROST қора-жилосиз ранг	2026-06-30 12:28:29.915646
813	247	CR	2	2	7	amonov	2026-06-30 12:28:29.915647
814	247	CR	10	3	92	{"number":"100000074","date":"2026-06-27T18:05:43"}	2026-06-30 12:28:29.915647
815	247	CR	11	4	80	{"number":"100000078","date":"2026-06-29T17:36:29.055026"}	2026-06-30 12:28:29.915648
816	248	DR	3	1	18	Artel	2026-06-30 12:28:29.915756
817	249	CR	3	1	18	Artel	2026-06-30 12:28:29.915862
818	249	CR	11	2	80	{"number":"100000078","date":"2026-06-29T17:36:29.055026","vatRateId":2}	2026-06-30 12:28:29.915862
819	249	DR	3	1	18	Artel	2026-06-30 12:28:29.915865
820	250	CR	3	3	18	Artel	2026-07-02 15:05:02.987025
821	250	CR	9	4	9	{"number":"100000009","date":"2026-05-20T12:58:18.282"}	2026-07-02 15:05:02.987187
822	250	DR	4	1	12	20208000005157348001	2026-07-02 15:05:02.987415
823	251	CR	3	1	4	{"type":"CASH_OPERATION","id":4}	2026-07-02 19:45:36.530665
824	251	CR	3	2	18	Artel	2026-07-02 19:45:36.53074
825	252	CR	3	1	4	{"type":"CASH_OPERATION","id":4}	2026-07-02 19:45:36.530949
826	252	CR	3	2	18	Artel	2026-07-02 19:45:36.530949
827	255	DR	3	1	4	{"type":"CASH_OPERATION","id":4}	2026-07-03 11:24:39.883602
828	255	DR	3	2	18	Artel	2026-07-03 11:24:39.883602
829	256	DR	3	1	4	{"type":"CASH_OPERATION","id":4}	2026-07-03 11:24:39.883602
830	256	DR	3	2	18	Artel	2026-07-03 11:24:39.883602
831	259	CR	3	1	5	{"type":"CASH_OPERATION","id":5}	2026-07-03 11:42:39.593011
832	259	CR	3	2	18	Artel	2026-07-03 11:42:39.593017
833	260	CR	3	1	5	{"type":"CASH_OPERATION","id":5}	2026-07-03 11:42:39.593288
834	260	CR	3	2	18	Artel	2026-07-03 11:42:39.593292
835	263	DR	3	1	5	{"type":"CASH_OPERATION","id":5}	2026-07-03 11:42:48.967906
836	263	DR	3	2	18	Artel	2026-07-03 11:42:48.967906
837	264	DR	3	1	5	{"type":"CASH_OPERATION","id":5}	2026-07-03 11:42:48.967906
838	264	DR	3	2	18	Artel	2026-07-03 11:42:48.967906
839	267	CR	3	1	6	{"type":"CASH_OPERATION","id":6}	2026-07-03 11:55:54.044269
840	267	CR	3	2	18	Artel	2026-07-03 11:55:54.044271
841	268	CR	3	1	6	{"type":"CASH_OPERATION","id":6}	2026-07-03 11:55:54.044281
842	268	CR	3	2	18	Artel	2026-07-03 11:55:54.044282
843	271	CR	3	1	7	{"type":"CASH_OPERATION","id":7}	2026-07-03 12:04:18.17874
844	271	CR	3	2	18	Artel	2026-07-03 12:04:18.178742
845	272	CR	3	1	7	{"type":"CASH_OPERATION","id":7}	2026-07-03 12:04:18.178752
846	272	CR	3	2	18	Artel	2026-07-03 12:04:18.178753
847	275	DR	3	1	7	{"type":"CASH_OPERATION","id":7}	2026-07-03 12:04:51.379721
848	275	DR	3	2	18	Artel	2026-07-03 12:04:51.379721
849	276	DR	3	1	7	{"type":"CASH_OPERATION","id":7}	2026-07-03 12:04:51.379721
850	276	DR	3	2	18	Artel	2026-07-03 12:04:51.379721
851	279	CR	3	3	19	Farrux Tech	2026-07-04 08:30:34.642132
852	279	CR	9	4	11	{"number":"100000011","date":"2026-07-03T16:19:50"}	2026-07-04 08:30:34.642137
853	279	DR	4	1	12	20208000005157348001	2026-07-04 08:30:34.642339
854	281	CR	3	1	10	{"type":"CASH_OPERATION","id":10}	2026-07-04 12:05:26.433522
855	281	CR	3	2	19	Farrux Tech	2026-07-04 12:05:26.433525
856	282	DR	3	1	11	{"type":"CASH_OPERATION","id":11}	2026-07-04 12:08:18.626597
857	282	DR	3	2	19	Farrux Tech	2026-07-04 12:08:18.626599
858	288	DR	3	1	9	{"type":"CASH_OPERATION","id":9}	2026-07-04 12:27:31.96584
859	288	DR	3	2	18	Artel	2026-07-04 12:27:31.965841
\.


--
-- Data for Name: acc_subkonto_type; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.acc_subkonto_type (id, code, name, source_table, state_id, created_date) FROM stdin;
1	product	Tovar / xizmat	inv_product	1	2026-06-25 10:43:24.6827
2	warehouse	Ombor	inv_warehouse	1	2026-06-25 10:43:24.6827
3	counterparty	Kontragent	counterparty_card	1	2026-06-25 10:43:24.6827
4	bank_account	Bank hisobi	org_bank_account	1	2026-06-25 10:43:24.6827
5	cash_box	Kassa	cash_box	1	2026-06-25 10:43:24.6827
6	employee	Xodim	sys_user	1	2026-06-25 10:43:24.6827
7	tax	Soliq	cmn_tax_type	1	2026-06-25 10:43:24.6827
8	bank_operation	Bank operatsiyasi	bank_operation	1	2026-06-25 10:43:24.6827
9	contract	Shartnoma	cmn_contract	1	2026-06-25 10:43:24.6827
10	puchase	Xarid	pur_doc	1	2026-06-25 10:43:24.6827
11	sale	Sotuv	sale_doc	1	2026-06-25 10:43:24.6827
\.


--
-- Data for Name: bank_operation; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.bank_operation (id, organization_id, bank_account_id, operation_type_id, payment_type_id, counterparty_id, doc_number, doc_date, currency_id, amount, comment, status_id, state_id, created_date, exchange_rate, posted_at, posted_by_user_id, cancelled_at, cancelled_by_user_id, counterparty_bank_account_id, contract_id, payment_purpose_id) FROM stdin;
36	8	12	1	2	19	100000035	2026-07-01 12:51:28	1	2550000.00	summmm	2	1	2026-07-01 17:52:12.621999	12000.000000	2026-07-01 17:52:12.621583	4	\N	\N	1	10	4
37	8	12	1	2	18	100000036	2026-07-02 08:31:56	1	2550000.00	summmm	2	1	2026-07-02 15:05:01.903034	12000.000000	2026-07-02 15:05:01.90264	4	\N	\N	1	9	4
38	8	12	1	\N	18	100000037	2026-07-03 07:03:25	1	3550000.00	create test	1	1	2026-07-03 12:19:26.631772	12000.000000	\N	\N	\N	\N	1	10	9
39	8	9	1	\N	19	100000038	2026-07-03 06:19:50	1	10000000.00	summmm	3	1	2026-07-03 16:20:09.88283	12000.000000	\N	\N	2026-07-04 08:31:49.192506	4	1	9	9
40	8	12	1	\N	19	100000039	2026-07-03 11:33:03	1	3550000.00	\N	3	1	2026-07-03 16:38:12.389208	1.000000	\N	\N	2026-07-04 08:31:28.978349	4	1	14	9
41	8	12	1	2	19	100000040	2026-07-04 03:27:53	1	3550000.00	create test	2	1	2026-07-04 08:30:10.372839	12000.000000	2026-07-04 08:30:34.909001	4	\N	\N	1	11	4
35	8	12	1	2	18	100000034	2026-06-04 12:25:21	1	1408000.00	00634 2026 йил май ишчи ходим иш хакки хисобидан реестрга асосан пл картага кучирилди.	2	1	2026-06-25 12:49:53.328932	1.000000	\N	\N	\N	\N	\N	\N	21
34	8	12	1	2	18	100000033	2026-06-04 12:24:52	1	5016000.00	00634 00634 2026 йил май ишчи ходимлар иш хакки хисобидан реестрга асосан пл картага кучирилди.	2	1	2026-06-25 12:49:53.32893	1.000000	\N	\N	\N	\N	\N	\N	21
33	8	12	1	2	18	100000032	2026-05-21 09:42:11	1	4400000.00	00634 2026 йил май ойи ишчи ходимлар иш хакки хисобидан реестрга асосан пл картага кучирилди.	2	1	2026-06-25 12:49:53.328928	1.000000	\N	\N	\N	\N	\N	\N	21
32	8	12	1	2	18	100000031	2026-05-04 10:09:31	1	7392000.00	00634 2026 йил апрель йи ишчи ходимлар иш хакки хисобидан реестрга асосан пл картага кучирилди.	2	1	2026-06-25 12:49:53.328927	1.000000	\N	\N	\N	\N	\N	\N	21
31	8	12	1	2	18	100000030	2026-04-03 11:33:28	1	7392000.00	00634 2026 йил март ойи ишчи ходимлар иш хакки хисобидан реестрга асосан пл картага кучирилди.	2	1	2026-06-25 12:49:53.328919	1.000000	\N	\N	\N	\N	\N	\N	21
30	8	12	1	2	18	100000029	2026-03-03 10:47:26	1	9520000.00	00634 2026 йил февраль ойи ишчи ходимлар иш хакки хисобидан реестрга асосан пл картага кучирилди.	2	1	2026-06-25 12:49:53.328917	1.000000	\N	\N	\N	\N	\N	\N	21
28	8	12	1	2	18	100000027	2026-03-03 10:39:54	1	9520000.00	00634 2024 йил июнь ойи ишчи ходимлар иш хакки хисобидан реестрга асосан пл картага кучирилди.	2	1	2026-06-25 12:49:53.328914	1.000000	\N	\N	\N	\N	\N	\N	21
27	8	12	1	2	18	100000026	2026-02-04 07:20:40	1	5720000.00	00634 2026 йил январь ойи ишчи ходимлар иш хакки хисобидан реестрга асосан пл картага кучирилди.	2	1	2026-06-25 12:49:53.328912	1.000000	\N	\N	\N	\N	\N	\N	21
26	8	12	1	2	18	100000025	2026-01-13 05:24:28	1	5000000.00	00634 2026 йил январь-феврал ойилари хисобидан ишчи ходим иш хакки хисобидан реестрга асосан пл картага кучирилди.	2	1	2026-06-25 12:49:53.328911	1.000000	\N	\N	\N	\N	\N	\N	21
25	8	12	1	2	18	100000024	2026-01-05 10:21:34	1	10120000.00	00634 2025 йил декабрь ойи ишчи ходимлар иш хакки хисобидан реестрга асосан пл картага кучирилди.	2	1	2026-06-25 12:49:53.328908	1.000000	\N	\N	\N	\N	\N	\N	21
24	8	12	1	2	18	100000023	2026-06-25 06:58:33	1	2550000.00	\N	2	2	2026-06-25 12:00:56.882823	1.000000	\N	\N	\N	\N	\N	\N	21
9	8	11	1	2	20	100000008	2026-01-05 10:24:55	1	4400000.00	00634Зачисление на счет пласт. карты 8600300486606289 КИБ: 2025 йил декабрь ойи ишчи ходимлар иш хакки хисобидан реестрга асосан пл картага кучирилди.	2	2	2026-06-25 11:46:02.911371	1.000000	\N	\N	\N	\N	\N	\N	21
22	8	11	1	2	20	100000021	2026-05-21 09:42:46	1	4400000.00	00634Зачисление на счет пласт. карты 8600300486606289 КИБ: 2026 йил май ойи ишчи ходимлар иш хакки хисобидан реестрга асосан пл картага кучирилди.	2	1	2026-06-25 11:46:02.911391	1.000000	\N	\N	\N	\N	\N	\N	21
20	8	11	1	2	20	100000019	2026-05-04 10:10:55	1	4400000.00	00634Зачисление на счет пласт. карты 8600300486606289 КИБ: 2026 йил апрель ойи ишчи ходимлар иш хакки хисобидан реестрга асосан пл картага кучирилди.	2	1	2026-06-25 11:46:02.911388	1.000000	\N	\N	\N	\N	\N	\N	21
18	8	11	1	2	20	100000017	2026-04-03 11:34:49	1	4400000.00	00634Зачисление на счет пласт. карты 8600300486606289 КИБ: 2026 йил март ойи ишчи ходимлар иш хакки хисобидан реестрга асосан пл картага кучирилди.	2	1	2026-06-25 11:46:02.911386	1.000000	\N	\N	\N	\N	\N	\N	21
16	8	11	1	2	20	100000015	2026-03-03 10:49:19	1	3800000.00	00634Зачисление на счет пласт. карты 8600300486606289 КИБ: 2026 йил февраль ойи ишчи ходимлар иш хакки хисобидан реестрга асосан пл картага кучирилди.	2	1	2026-06-25 11:46:02.911383	1.000000	\N	\N	\N	\N	\N	\N	21
14	8	11	1	2	21	100000013	2026-03-03 10:52:07	1	9520000.00	00634 2024 йил июнь ойи ишчи ходимлар иш хакки хисобидан реестрга асосан пл картага кучирилди. (Тулов максадига аниклик киритинг)	2	1	2026-06-25 11:46:02.911377	1.000000	\N	\N	\N	\N	\N	\N	21
11	8	11	1	2	20	100000010	2026-01-13 05:30:01	1	5000000.00	00634Зачисление на счет пласт. карты 8600300486606289 КИБ: 2026 йил январь-феврал ойилари хисобидан ишчи ходим иш хакки хисобидан реестрга асосан пл картага кучирилди.	2	1	2026-06-25 11:46:02.911373	1.000000	\N	\N	\N	\N	\N	\N	21
29	8	12	2	2	18	100000028	2026-03-03 10:52:07	1	9520000.00	00634 2024 йил июнь ойи ишчи ходимлар иш хакки хисобидан реестрга асосан пл картага кучирилди. (Тулов максадига аниклик киритинг)	2	1	2026-06-25 12:49:53.328916	1.000000	\N	\N	\N	\N	\N	\N	22
8	8	11	2	2	21	100000007	2026-01-05 10:21:34	1	10120000.00	00634 2025 йил декабрь ойи ишчи ходимлар иш хакки хисобидан реестрга асосан пл картага кучирилди.	2	2	2026-06-25 11:46:02.911305	1.000000	\N	\N	\N	\N	\N	\N	22
23	8	11	2	2	21	100000022	2026-06-04 12:24:52	1	5016000.00	00634 00634 2026 йил май ишчи ходимлар иш хакки хисобидан реестрга асосан пл картага кучирилди.	2	1	2026-06-25 11:46:02.911392	1.000000	\N	\N	\N	\N	\N	\N	22
21	8	11	2	2	21	100000020	2026-05-21 09:42:11	1	4400000.00	00634 2026 йил май ойи ишчи ходимлар иш хакки хисобидан реестрга асосан пл картага кучирилди.	2	1	2026-06-25 11:46:02.91139	1.000000	\N	\N	\N	\N	\N	\N	22
19	8	11	2	2	21	100000018	2026-05-04 10:09:31	1	7392000.00	00634 2026 йил апрель йи ишчи ходимлар иш хакки хисобидан реестрга асосан пл картага кучирилди.	2	1	2026-06-25 11:46:02.911387	1.000000	\N	\N	\N	\N	\N	\N	22
17	8	11	2	2	21	100000016	2026-04-03 11:33:28	1	7392000.00	00634 2026 йил март ойи ишчи ходимлар иш хакки хисобидан реестрга асосан пл картага кучирилди.	2	1	2026-06-25 11:46:02.911385	1.000000	\N	\N	\N	\N	\N	\N	22
15	8	11	2	2	21	100000014	2026-03-03 10:47:26	1	9520000.00	00634 2026 йил февраль ойи ишчи ходимлар иш хакки хисобидан реестрга асосан пл картага кучирилди.	2	1	2026-06-25 11:46:02.911378	1.000000	\N	\N	\N	\N	\N	\N	22
13	8	11	2	2	21	100000012	2026-03-03 10:39:54	1	9520000.00	00634 2024 йил июнь ойи ишчи ходимлар иш хакки хисобидан реестрга асосан пл картага кучирилди.	2	1	2026-06-25 11:46:02.911376	1.000000	\N	\N	\N	\N	\N	\N	22
12	8	11	2	2	21	100000011	2026-02-04 07:20:40	1	5720000.00	00634 2026 йил январь ойи ишчи ходимлар иш хакки хисобидан реестрга асосан пл картага кучирилди.	2	1	2026-06-25 11:46:02.911375	1.000000	\N	\N	\N	\N	\N	\N	22
10	8	11	2	2	21	100000009	2026-01-13 05:24:28	1	5000000.00	00634 2026 йил январь-феврал ойилари хисобидан ишчи ходим иш хакки хисобидан реестрга асосан пл картага кучирилди.	2	1	2026-06-25 11:46:02.911372	1.000000	\N	\N	\N	\N	\N	\N	22
7	2	1	2	2	\N	100000006	2026-06-24 15:34:53	4	1102.00	Codex paymentType many test 2	2	2	2026-06-24 15:34:53.638815	1.000000	\N	\N	\N	\N	\N	\N	22
6	2	1	2	2	\N	100000005	2026-06-24 15:34:53	4	1101.00	Codex paymentType many test 1	2	2	2026-06-24 15:34:53.638813	1.000000	\N	\N	\N	\N	\N	\N	22
5	2	1	2	2	\N	100000004	2026-06-24 15:34:52	4	1100.00	Codex paymentType single test	2	2	2026-06-24 15:34:52.995844	1.000000	\N	\N	\N	\N	\N	\N	22
4	2	1	2	\N	\N	100000003	2026-06-24 15:03:00	4	1002.00	Codex many create test 2	2	2	2026-06-24 15:03:00.548021	1.000000	\N	\N	\N	\N	\N	\N	22
3	2	1	2	\N	\N	100000002	2026-06-24 15:03:00	4	1001.00	Codex many create test 1	2	2	2026-06-24 15:03:00.548019	1.000000	\N	\N	\N	\N	\N	\N	22
2	2	1	2	\N	\N	100000001	2026-06-24 15:02:59	4	1000.00	Codex single create test	2	2	2026-06-24 15:02:59.985932	1.000000	\N	\N	\N	\N	\N	\N	22
\.


--
-- Data for Name: bank_operation_line; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.bank_operation_line (id, bank_operation_id, order_number, payment_purpose_id, counterparty_id, amount, comment) FROM stdin;
1	36	1	4	19	2550000.00	summmm
2	37	1	4	18	2550000.00	summmm
3	38	1	9	18	3550000.00	create test
5	39	1	9	19	10000000.00	summmm
6	40	1	9	19	3550000.00	\N
7	41	1	4	19	3550000.00	create test
\.


--
-- Data for Name: cash_box; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.cash_box (id, organization_id, branch_id, code, name, currency_id, state_id, created_date, is_main, responsible_user_id, opening_balance, opening_balance_date) FROM stdin;
4	8	6	6839	kassa	1	1	2026-07-02 16:36:54.004443	f	\N	0.00	\N
\.


--
-- Data for Name: cash_operation; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.cash_operation (id, organization_id, cash_box_id, operation_type_id, payment_type_id, counterparty_id, doc_number, doc_date, currency_id, amount, comment, status_id, state_id, created_date, exchange_rate, posted_at, posted_by_user_id, cancelled_at, cancelled_by_user_id, destination_cash_box_id, payment_purpose_id) FROM stdin;
17	8	4	1	2	17	100000015	2026-07-04 12:24:54	1	1000.00		3	1	2026-07-04 12:25:18.545664	1.000000	\N	\N	2026-07-04 12:26:20.004595	4	\N	24
9	8	4	2	2	18	100000007	2026-07-04 10:53:00	1	1000.00		2	1	2026-07-04 11:04:41.396372	1.000000	2026-07-04 12:27:31.974301	4	\N	\N	\N	1
7	8	4	1	\N	18	100000005	2026-07-03 12:03:25	1	1000.00	create test	3	1	2026-07-03 12:04:08.984397	1.000000	2026-07-03 12:04:18.190119	4	2026-07-03 12:04:51.394741	4	\N	21
6	8	4	1	\N	18	100000004	2026-07-03 11:55:01	1	2550000.00	summmm	2	1	2026-07-03 11:55:32.793748	1.000000	2026-07-03 11:55:54.065379	4	\N	\N	\N	21
5	8	4	1	\N	18	100000003	2026-07-03 11:37:07	1	10000000.00	create test	3	1	2026-07-03 11:37:32.607436	1.000000	2026-07-03 11:42:39.642946	4	2026-07-03 11:42:49.006074	4	\N	21
4	8	4	1	\N	18	100000002	2026-07-02 17:28:22	1	2550000.00	summmm	3	1	2026-07-02 17:32:42.464594	1.000000	2026-07-02 19:45:36.88631	4	2026-07-03 11:24:40.416112	4	\N	21
3	8	4	2	\N	18	7878	2026-07-02 17:07:58	1	2550000.00	summmm	1	2	2026-07-02 17:08:12.108429	1.000000	\N	\N	\N	\N	\N	22
8	8	4	2	\N	17	100000006	2026-07-04 10:53:00	1	1550000.00	summmm	2	1	2026-07-04 11:02:35.793746	1.000000	2026-07-04 11:03:34.212499	4	\N	\N	\N	23
10	8	4	1	\N	19	100000008	2026-07-04 10:53:00	1	100000000.00	summmm	2	1	2026-07-04 12:05:20.893702	1.000000	2026-07-04 12:05:26.580645	4	\N	\N	\N	3
11	8	4	2	1	19	100000009	2026-07-04 11:09:31	1	40000000.00	create test	2	1	2026-07-04 12:06:21.540743	12000.000000	2026-07-04 12:08:18.646542	4	\N	\N	\N	1
12	8	4	1	1	18	100000010	2026-07-04 12:10:12	1	10000000.00		2	1	2026-07-04 12:11:59.254284	12000.000000	2026-07-04 12:19:36.147352	4	\N	\N	\N	24
15	8	4	1	1	18	100000013	2026-07-04 12:21:58	1	2550000.00		2	1	2026-07-04 12:22:34.200745	12000.000000	2026-07-04 12:22:53.918155	4	\N	\N	\N	24
14	8	4	1	1	18	100000012	2026-07-04 12:21:58	1	10000000.00		2	1	2026-07-04 12:22:16.880025	12000.000000	2026-07-04 12:23:18.054785	4	\N	\N	\N	24
13	8	4	1	1	18	100000011	2026-07-04 12:19:59	1	3550000.00		2	1	2026-07-04 12:20:14.818983	1.000000	2026-07-04 12:24:02.058818	4	\N	\N	\N	24
16	8	4	1	1	18	100000014	2026-07-04 12:24:15	1	1000.00		2	1	2026-07-04 12:24:30.863325	1.000000	2026-07-04 12:24:35.324369	4	\N	\N	\N	24
18	8	4	1	1	20	100000016	2026-07-04 12:25:48	1	1000.00		1	1	2026-07-04 12:26:04.642746	1.000000	\N	\N	\N	\N	\N	24
\.


--
-- Data for Name: cmn_bank; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.cmn_bank (id, code, name, mfo, state_id, created_date, inn) FROM stdin;
3	KAPITAL	Kapitalbank	\N	1	2026-06-06 16:39:53.224198	\N
4	HAMKOR	Hamkorbank	\N	1	2026-06-06 16:39:53.224198	\N
5	CODEX191315	Codex Test Bank Updated	99998	1	2026-06-24 19:13:15.478874	\N
2	IPOTEKA	Ipoteka bank	\N	1	2026-06-06 16:39:53.224198	\N
1	NBU	Milliy bank	\N	1	2026-06-06 16:39:53.224198	\N
6	Agro	AgroBank	1234	1	2026-06-25 13:53:41.945698	\N
\.


--
-- Data for Name: cmn_contract; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.cmn_contract (id, organization_id, counterparty_id, contract_number, contract_date, start_date, end_date, comment, state_id, created_date, contract_type_id) FROM stdin;
7	8	16	100000007	2026-06-20 15:35:42	2026-06-20 00:00:00	2027-08-26 23:59:59.999999		1	2026-06-20 15:39:04.060566	1
9	8	18	100000009	2026-05-20 12:58:18.282	2026-05-20 00:00:00	2026-09-20 23:59:59.999999	string	1	2026-06-20 18:00:25.144259	1
10	8	18	100000010	2026-06-23 05:58:06	2026-06-23 05:58:06	2026-07-31 06:02:00		1	2026-06-23 06:02:11.011106	1
11	8	19	100000011	2026-07-03 16:19:50	2026-07-03 16:19:50	2026-07-31 16:31:00	summmm	1	2026-07-03 16:31:52.213041	2
12	8	19	100000012	2026-07-03 16:32:02	2026-07-03 16:32:02	2026-07-31 16:32:00	summmm	2	2026-07-03 16:32:17.918529	1
13	8	19	100000013	2026-07-03 16:33:03	2026-07-03 16:33:03	2026-07-31 16:35:00		2	2026-07-03 16:36:41.514788	1
14	8	19	100000014	2026-07-03 16:33:03	2026-07-01 16:37:00	2026-07-31 16:37:00		1	2026-07-03 16:37:24.464865	1
\.


--
-- Data for Name: cmn_contract_type; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.cmn_contract_type (id, code, name, state_id, created_date) FROM stdin;
1	supplier	Yetkazib beruvchi bilan shartnoma	1	2026-06-17 17:35:47.346557
2	customer	Xaridor bilan shartnoma	1	2026-06-17 17:35:47.346557
\.


--
-- Data for Name: cmn_costing_method; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.cmn_costing_method (id, code, name) FROM stdin;
1	FIFO	FIFO - birinchi kirgan birinchi chiqadi
2	LIFO	LIFO - oxirgi kirgan birinchi chiqadi
3	AVERAGE	O'rtacha tannarx
\.


--
-- Data for Name: cmn_counterparty_type; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.cmn_counterparty_type (id, code, name, state_id) FROM stdin;
1	client	Mijoz	1
2	supplier	Yetkazib beruvchi	1
3	client_supplier	Mijoz va yetkazib beruvchi	1
\.


--
-- Data for Name: cmn_currency; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.cmn_currency (id, code, name, symbol, state_id) FROM stdin;
1	UZS	Uzbek so'm	so'm	1
2	USD	US Dollar	$	1
3	RUB	Russian Ruble	₽	1
4	EUR	Euro	€	1
\.


--
-- Data for Name: cmn_district; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.cmn_district (id, short_name, full_name, region_id, state_id, created_date) FROM stdin;
1	Bektemir	Bektemir	1	1	2026-06-05 16:31:33.88899
2	Mirzo Ulug‘bek	Mirzo Ulug‘bek	1	1	2026-06-05 16:31:33.88899
3	Mirobod	Mirobod	1	1	2026-06-05 16:31:33.88899
4	Olmazor	Olmazor	1	1	2026-06-05 16:31:33.88899
5	Sergeli	Sergeli	1	1	2026-06-05 16:31:33.88899
6	Uchtepa	Uchtepa	1	1	2026-06-05 16:31:33.88899
7	Yashnobod	Yashnobod	1	1	2026-06-05 16:31:33.88899
8	Chilonzor	Chilonzor	1	1	2026-06-05 16:31:33.88899
9	Shayxontohur	Shayxontohur	1	1	2026-06-05 16:31:33.88899
10	Yunusobod	Yunusobod	1	1	2026-06-05 16:31:33.88899
11	Yakkasaroy	Yakkasaroy	1	1	2026-06-05 16:31:33.88899
12	Amudaryo	Amudaryo	6	1	2026-06-05 16:31:33.88899
13	Beruniy	Beruniy	6	1	2026-06-05 16:31:33.88899
14	Qanliko‘l	Qanliko‘l	6	1	2026-06-05 16:31:33.88899
15	Qorao‘zak	Qorao‘zak	6	1	2026-06-05 16:31:33.88899
16	Kegeyli	Kegeyli	6	1	2026-06-05 16:31:33.88899
17	Qo‘ng‘irot	Qo‘ng‘irot	6	1	2026-06-05 16:31:33.88899
18	Mo‘ynoq	Mo‘ynoq	6	1	2026-06-05 16:31:33.88899
19	Nuqus	Nuqus	6	1	2026-06-05 16:31:33.88899
20	Taxtako‘pir	Taxtako‘pir	6	1	2026-06-05 16:31:33.88899
21	To‘rtko‘l	To‘rtko‘l	6	1	2026-06-05 16:31:33.88899
22	Xo‘jayli	Xo‘jayli	6	1	2026-06-05 16:31:33.88899
23	Chimboy	Chimboy	6	1	2026-06-05 16:31:33.88899
24	Shumanay	Shumanay	6	1	2026-06-05 16:31:33.88899
25	Ellikqal’a	Ellikqal’a	6	1	2026-06-05 16:31:33.88899
26	Taxiatosh	Taxiatosh	6	1	2026-06-05 16:31:33.88899
27	Bo‘zatov	Bo‘zatov	6	1	2026-06-05 16:31:33.88899
28	Bektemir	Bektemir	1	1	2026-06-05 16:36:13.009757
29	Mirzo Ulug‘bek	Mirzo Ulug‘bek	1	1	2026-06-05 16:36:13.009757
30	Mirobod	Mirobod	1	1	2026-06-05 16:36:13.009757
31	Olmazor	Olmazor	1	1	2026-06-05 16:36:13.009757
32	Sergeli	Sergeli	1	1	2026-06-05 16:36:13.009757
33	Uchtepa	Uchtepa	1	1	2026-06-05 16:36:13.009757
34	Yashnobod	Yashnobod	1	1	2026-06-05 16:36:13.009757
35	Chilonzor	Chilonzor	1	1	2026-06-05 16:36:13.009757
36	Shayxontohur	Shayxontohur	1	1	2026-06-05 16:36:13.009757
37	Yunusobod	Yunusobod	1	1	2026-06-05 16:36:13.009757
38	Yakkasaroy	Yakkasaroy	1	1	2026-06-05 16:36:13.009757
39	Bo‘stonliq	Bo‘stonliq	2	1	2026-06-05 16:36:13.009757
40	Oqqo‘rg‘on	Oqqo‘rg‘on	2	1	2026-06-05 16:36:13.009757
41	Ohangaron	Ohangaron	2	1	2026-06-05 16:36:13.009757
42	Bekobod	Bekobod	2	1	2026-06-05 16:36:13.009757
43	Bo‘ka	Bo‘ka	2	1	2026-06-05 16:36:13.009757
44	Zangiota	Zangiota	2	1	2026-06-05 16:36:13.009757
45	Qibray	Qibray	2	1	2026-06-05 16:36:13.009757
46	Quyichirchiq	Quyichirchiq	2	1	2026-06-05 16:36:13.009757
47	Parkent	Parkent	2	1	2026-06-05 16:36:13.009757
48	Piskent	Piskent	2	1	2026-06-05 16:36:13.009757
49	Toshkent	Toshkent	2	1	2026-06-05 16:36:13.009757
50	O‘rtachirchiq	O‘rtachirchiq	2	1	2026-06-05 16:36:13.009757
51	Chinoz	Chinoz	2	1	2026-06-05 16:36:13.009757
52	Yuqorichirchiq	Yuqorichirchiq	2	1	2026-06-05 16:36:13.009757
53	Yangiyo‘l	Yangiyo‘l	2	1	2026-06-05 16:36:13.009757
54	Olmaliq	Olmaliq	2	1	2026-06-05 16:36:13.009757
55	Angren	Angren	2	1	2026-06-05 16:36:13.009757
56	Chirchiq	Chirchiq	2	1	2026-06-05 16:36:13.009757
57	Nurafshon	Nurafshon	2	2	2026-06-05 16:36:13.009757
58	Andijon	Andijon	3	1	2026-06-05 16:36:13.009757
59	Asaka	Asaka	3	1	2026-06-05 16:36:13.009757
60	Oltinko‘l	Oltinko‘l	3	1	2026-06-05 16:36:13.009757
61	Baliqchi	Baliqchi	3	1	2026-06-05 16:36:13.009757
62	Bo‘z	Bo‘z	3	1	2026-06-05 16:36:13.009757
63	Buloqboshi	Buloqboshi	3	1	2026-06-05 16:36:13.009757
64	Jalolquduq	Jalolquduq	3	1	2026-06-05 16:36:13.009757
65	Izboskan	Izboskan	3	1	2026-06-05 16:36:13.009757
66	Qo‘rg‘ontepa	Qo‘rg‘ontepa	3	1	2026-06-05 16:36:13.009757
67	Marhamat	Marhamat	3	1	2026-06-05 16:36:13.009757
68	Paxtaobod	Paxtaobod	3	1	2026-06-05 16:36:13.009757
69	Ulug‘nor	Ulug‘nor	3	1	2026-06-05 16:36:13.009757
70	Xo‘jaobod	Xo‘jaobod	3	1	2026-06-05 16:36:13.009757
71	Xonobod	Xonobod	3	1	2026-06-05 16:36:13.009757
72	Shahrixon	Shahrixon	3	1	2026-06-05 16:36:13.009757
73	Buxoro	Buxoro	4	1	2026-06-05 16:36:13.009757
74	Kogon	Kogon	4	1	2026-06-05 16:36:13.009757
75	Olot	Olot	4	1	2026-06-05 16:36:13.009757
76	Vobkent	Vobkent	4	1	2026-06-05 16:36:13.009757
77	G‘ijduvon	G‘ijduvon	4	1	2026-06-05 16:36:13.009757
78	Jondor	Jondor	4	1	2026-06-05 16:36:13.009757
79	Qorako‘l	Qorako‘l	4	1	2026-06-05 16:36:13.009757
80	Peshku	Peshku	4	1	2026-06-05 16:36:13.009757
81	Romitan	Romitan	4	1	2026-06-05 16:36:13.009757
82	Qorovulbozor	Qorovulbozor	4	1	2026-06-05 16:36:13.009757
83	Shofirkon	Shofirkon	4	1	2026-06-05 16:36:13.009757
84	Arnasoy	Arnasoy	5	1	2026-06-05 16:36:13.009757
85	Yangiobod	Yangiobod	5	1	2026-06-05 16:36:13.009757
86	Baxmal	Baxmal	5	1	2026-06-05 16:36:13.009757
87	G‘allaorol	G‘allaorol	5	1	2026-06-05 16:36:13.009757
88	Do‘stlik	Do‘stlik	5	1	2026-06-05 16:36:13.009757
89	Paxtakor	Paxtakor	5	1	2026-06-05 16:36:13.009757
90	Zomin	Zomin	5	1	2026-06-05 16:36:13.009757
91	Jizzax	Jizzax	5	1	2026-06-05 16:36:13.009757
92	Zarbdor	Zarbdor	5	1	2026-06-05 16:36:13.009757
93	Zafarobod	Zafarobod	5	1	2026-06-05 16:36:13.009757
94	Mirzacho‘l	Mirzacho‘l	5	1	2026-06-05 16:36:13.009757
95	Forish	Forish	5	1	2026-06-05 16:36:13.009757
96	Amudaryo	Amudaryo	6	1	2026-06-05 16:36:13.009757
97	Beruniy	Beruniy	6	1	2026-06-05 16:36:13.009757
98	Qanliko‘l	Qanliko‘l	6	1	2026-06-05 16:36:13.009757
99	Qorao‘zak	Qorao‘zak	6	1	2026-06-05 16:36:13.009757
100	Kegeyli	Kegeyli	6	1	2026-06-05 16:36:13.009757
101	Qo‘ng‘irot	Qo‘ng‘irot	6	1	2026-06-05 16:36:13.009757
102	Mo‘ynoq	Mo‘ynoq	6	1	2026-06-05 16:36:13.009757
103	Nuqus	Nuqus	6	1	2026-06-05 16:36:13.009757
104	Taxtako‘pir	Taxtako‘pir	6	1	2026-06-05 16:36:13.009757
105	To‘rtko‘l	To‘rtko‘l	6	1	2026-06-05 16:36:13.009757
106	Xo‘jayli	Xo‘jayli	6	1	2026-06-05 16:36:13.009757
107	Chimboy	Chimboy	6	1	2026-06-05 16:36:13.009757
108	Shumanay	Shumanay	6	1	2026-06-05 16:36:13.009757
109	Ellikqal’a	Ellikqal’a	6	1	2026-06-05 16:36:13.009757
110	Taxiatosh	Taxiatosh	6	1	2026-06-05 16:36:13.009757
111	Bo‘zatov	Bo‘zatov	6	1	2026-06-05 16:36:13.009757
112	G‘uzor	G‘uzor	7	1	2026-06-05 16:36:13.009757
113	Dehqonobod	Dehqonobod	7	1	2026-06-05 16:36:13.009757
114	Qamashi	Qamashi	7	1	2026-06-05 16:36:13.009757
115	Qarshi	Qarshi	7	1	2026-06-05 16:36:13.009757
116	Koson	Koson	7	1	2026-06-05 16:36:13.009757
117	Kasbi	Kasbi	7	1	2026-06-05 16:36:13.009757
118	Kitob	Kitob	7	1	2026-06-05 16:36:13.009757
119	Mirishkor	Mirishkor	7	1	2026-06-05 16:36:13.009757
120	Muborak	Muborak	7	1	2026-06-05 16:36:13.009757
121	Nishon	Nishon	7	1	2026-06-05 16:36:13.009757
122	Chiroqchi	Chiroqchi	7	1	2026-06-05 16:36:13.009757
123	Shahrisabz	Shahrisabz	7	1	2026-06-05 16:36:13.009757
124	Yakkabog‘	Yakkabog‘	7	1	2026-06-05 16:36:13.009757
125	Konimex	Konimex	8	1	2026-06-05 16:36:13.009757
126	Qiziltepa	Qiziltepa	8	1	2026-06-05 16:36:13.009757
127	Navbahor	Navbahor	8	1	2026-06-05 16:36:13.009757
128	Karmana	Karmana	8	1	2026-06-05 16:36:13.009757
129	Nurata	Nurata	8	1	2026-06-05 16:36:13.009757
130	Tomdi	Tomdi	8	1	2026-06-05 16:36:13.009757
131	Uchquduq	Uchquduq	8	1	2026-06-05 16:36:13.009757
132	Xatirchi	Xatirchi	8	1	2026-06-05 16:36:13.009757
133	Zarafshon	Zarafshon	8	1	2026-06-05 16:36:13.009757
134	Navoiy	Navoiy	8	1	2026-06-05 16:36:13.009757
135	Kosonsoy	Kosonsoy	9	1	2026-06-05 16:36:13.009757
136	Mingbuloq	Mingbuloq	9	1	2026-06-05 16:36:13.009757
137	Namangan	Namangan	9	1	2026-06-05 16:36:13.009757
138	Naryn	Naryn	9	1	2026-06-05 16:36:13.009757
139	Pop	Pop	9	1	2026-06-05 16:36:13.009757
140	Turakurgan	Turakurgan	9	1	2026-06-05 16:36:13.009757
141	Uychi	Uychi	9	1	2026-06-05 16:36:13.009757
142	Uchkurgan	Uchkurgan	9	1	2026-06-05 16:36:13.009757
143	Chartak	Chartak	9	1	2026-06-05 16:36:13.009757
144	Chust	Chust	9	1	2026-06-05 16:36:13.009757
145	Yangiqo‘rg‘on	Yangiqo‘rg‘on	9	1	2026-06-05 16:36:13.009757
146	Ishtixon	Ishtixon	10	1	2026-06-05 16:36:13.009757
147	Bulung‘ur	Bulung‘ur	10	1	2026-06-05 16:36:13.009757
148	Jomboy	Jomboy	10	1	2026-06-05 16:36:13.009757
149	Kattaqo‘rg‘on	Kattaqo‘rg‘on	10	1	2026-06-05 16:36:13.009757
150	Qo‘shrabot	Qo‘shrabot	10	1	2026-06-05 16:36:13.009757
151	Narpay	Narpay	10	1	2026-06-05 16:36:13.009757
152	Nurobod	Nurobod	10	1	2026-06-05 16:36:13.009757
153	Payariq	Payariq	10	1	2026-06-05 16:36:13.009757
154	Pastdarg‘om	Pastdarg‘om	10	1	2026-06-05 16:36:13.009757
155	Paxtachi	Paxtachi	10	1	2026-06-05 16:36:13.009757
156	Samarqand	Samarqand	10	1	2026-06-05 16:36:13.009757
157	Toyloq	Toyloq	10	1	2026-06-05 16:36:13.009757
158	Urgut	Urgut	10	1	2026-06-05 16:36:13.009757
159	Oltinsoy	Oltinsoy	11	1	2026-06-05 16:36:13.009757
160	Angor	Angor	11	1	2026-06-05 16:36:13.009757
161	Boysun	Boysun	11	1	2026-06-05 16:36:13.009757
162	Bandixon	Bandixon	11	1	2026-06-05 16:36:13.009757
163	Denov	Denov	11	1	2026-06-05 16:36:13.009757
164	Jarqo‘rg‘on	Jarqo‘rg‘on	11	1	2026-06-05 16:36:13.009757
165	Qumqo‘rg‘on	Qumqo‘rg‘on	11	1	2026-06-05 16:36:13.009757
166	Qiziriq	Qiziriq	11	1	2026-06-05 16:36:13.009757
167	Muzrabot	Muzrabot	11	1	2026-06-05 16:36:13.009757
168	Sariosiyo	Sariosiyo	11	1	2026-06-05 16:36:13.009757
169	Termiz	Termiz	11	1	2026-06-05 16:36:13.009757
170	Sherobod	Sherobod	11	1	2026-06-05 16:36:13.009757
171	Sho‘rchi	Sho‘rchi	11	1	2026-06-05 16:36:13.009757
172	Oqoltin	Oqoltin	12	1	2026-06-05 16:36:13.009757
173	Bayaut	Bayaut	12	1	2026-06-05 16:36:13.009757
174	Guliston	Guliston	12	1	2026-06-05 16:36:13.009757
175	Mirzaobod	Mirzaobod	12	1	2026-06-05 16:36:13.009757
176	Sardoba	Sardoba	12	1	2026-06-05 16:36:13.009757
177	Sayxunobod	Sayxunobod	12	1	2026-06-05 16:36:13.009757
178	Sirdaryo	Sirdaryo	12	1	2026-06-05 16:36:13.009757
179	Xovos	Xovos	12	1	2026-06-05 16:36:13.009757
180	Shirin	Shirin	12	1	2026-06-05 16:36:13.009757
181	Yangiyer	Yangiyer	12	1	2026-06-05 16:36:13.009757
182	Oltariq	Oltariq	13	1	2026-06-05 16:36:13.009757
183	Qo‘shtepa	Qo‘shtepa	13	1	2026-06-05 16:36:13.009757
184	Bag‘dod	Bag‘dod	13	1	2026-06-05 16:36:13.009757
185	Beshariq	Beshariq	13	1	2026-06-05 16:36:13.009757
186	Buvayda	Buvayda	13	1	2026-06-05 16:36:13.009757
187	Dang‘ara	Dang‘ara	13	1	2026-06-05 16:36:13.009757
188	Quva	Quva	13	1	2026-06-05 16:36:13.009757
189	Rishton	Rishton	13	1	2026-06-05 16:36:13.009757
190	Soh	Soh	13	1	2026-06-05 16:36:13.009757
191	Toshloq	Toshloq	13	1	2026-06-05 16:36:13.009757
192	O‘zbekiston	O‘zbekiston	13	1	2026-06-05 16:36:13.009757
193	Uchko‘prik	Uchko‘prik	13	1	2026-06-05 16:36:13.009757
194	Farg‘ona	Farg‘ona	13	1	2026-06-05 16:36:13.009757
195	Furqat	Furqat	13	1	2026-06-05 16:36:13.009757
196	Yozyovon	Yozyovon	13	1	2026-06-05 16:36:13.009757
197	Qo‘qon	Qo‘qon	13	1	2026-06-05 16:36:13.009757
198	Quvasoy	Quvasoy	13	1	2026-06-05 16:36:13.009757
199	Marg‘ilon	Marg‘ilon	13	1	2026-06-05 16:36:13.009757
200	Bog‘ot	Bog‘ot	14	1	2026-06-05 16:36:13.009757
201	Gurlan	Gurlan	14	1	2026-06-05 16:36:13.009757
202	Qo‘shko‘pir	Qo‘shko‘pir	14	1	2026-06-05 16:36:13.009757
203	Urgench	Urgench	14	1	2026-06-05 16:36:13.009757
204	Xazorasp	Xazorasp	14	1	2026-06-05 16:36:13.009757
205	Xonqa	Xonqa	14	1	2026-06-05 16:36:13.009757
206	Xiva	Xiva	14	1	2026-06-05 16:36:13.009757
207	Shovot	Shovot	14	1	2026-06-05 16:36:13.009757
208	Yangiariq	Yangiariq	14	1	2026-06-05 16:36:13.009757
209	Yangibozor	Yangibozor	14	1	2026-06-05 16:36:13.009757
\.


--
-- Data for Name: cmn_document_sequence; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.cmn_document_sequence (id, organization_id, document_type_id, prefix, suffix, current_number, padding, year, month, reset_period, state_id, created_date) FROM stdin;
\.


--
-- Data for Name: cmn_document_status; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.cmn_document_status (id, code, name, state_id) FROM stdin;
1	draft	Qoralama	1
2	posted	O'tkazilgan	1
3	cancelled	Bekor qilingan	1
4	pending	Kutilmoqda	1
\.


--
-- Data for Name: cmn_document_type; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.cmn_document_type (id, code, name, state_id, created_date) FROM stdin;
1	purchase	Xarid / Kirim	1	2026-06-06 16:40:07.223429
2	sale	Sotuv	1	2026-06-06 16:40:07.223429
3	bank_operation	Bank operatsiyasi	1	2026-06-06 16:40:07.223429
4	cash_operation	Kassa operatsiyasi	1	2026-06-06 16:40:07.223429
5	salary	Ish haqi	1	2026-06-06 16:40:07.223429
6	expense	Xarajat	1	2026-06-06 16:40:07.223429
\.


--
-- Data for Name: cmn_language; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.cmn_language (id, code, name, native_name, is_default, sort_order, state_id, created_date) FROM stdin;
1	uz	Uzbek	O'zbekcha	t	1	1	2026-06-06 10:44:19.082498
2	ru	Russian	Русский	f	2	1	2026-06-06 10:44:19.082498
3	en	English	English	f	3	1	2026-06-06 10:44:19.082498
\.


--
-- Data for Name: cmn_operation_type; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.cmn_operation_type (id, code, name, state_id, created_date) FROM stdin;
1	in	Kirim	1	2026-06-06 16:40:19.40134
2	out	Chiqim	1	2026-06-06 16:40:19.40134
3	transfer	O'tkazma	1	2026-06-06 16:40:19.40134
4	debt_increase	Qarz oshishi	1	2026-06-06 16:40:19.40134
5	debt_decrease	Qarz kamayishi	1	2026-06-06 16:40:19.40134
\.


--
-- Data for Name: cmn_payment_type; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.cmn_payment_type (id, code, name, state_id) FROM stdin;
1	cash	Naqd	1
2	bank	Bank	1
3	card	Karta	1
4	transfer	O'tkazma	1
\.


--
-- Data for Name: cmn_price_rounding_method; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.cmn_price_rounding_method (id, code, name) FROM stdin;
1	NONE	O'zgartirmasdan
2	UP	Yuqoriga yaxlitlash
3	DOWN	Pastga yaxlitlash
4	NEAREST	Eng yaqin qiymatga yaxlitlash
\.


--
-- Data for Name: cmn_pricing_condition; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.cmn_pricing_condition (id, organization_id, pricing_method_id, pricing_value, rounding_method_id, rounding_precision, start_date, end_date, state_id, created_date) FROM stdin;
1	8	1	10.00	4	100000.00	2026-06-27 16:26:44	\N	2	2026-06-27 16:29:09.970103
8	8	1	10.00	4	1.00	2026-06-01 00:00:00	\N	1	2026-06-29 15:47:45.47747
\.


--
-- Data for Name: cmn_pricing_method; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.cmn_pricing_method (id, code, name) FROM stdin;
1	COST_PLUS_PERCENT	Tannarx + foizli marja
2	COST_PLUS_AMOUNT	Tannarx + belgilangan summa
3	FIXED_PRICE	Belgilangan narx
\.


--
-- Data for Name: cmn_product_price_type; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.cmn_product_price_type (id, code, name) FROM stdin;
1	AVERAGE_COST_PRICE	Hisoblangan o'rtacha tannarx
2	FIXED_SALE_PRICE	Belgilangan sotuv narxi
\.


--
-- Data for Name: cmn_product_table_status; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.cmn_product_table_status (id, code, name, state_id) FROM stdin;
1	IN_STOCK	На складе	1
2	RESERVED	Зарезервирован	1
3	SOLD	Продан	1
4	RETURNED_TO_SUPPLIER	Возвращен поставщику	1
5	RETURNED_FROM_CUSTOMER	Возвращен покупателем	1
6	WRITTEN_OFF	Списан	1
7	LOST	Утерян	1
8	BLOCKED	Заблокирован	1
\.


--
-- Data for Name: cmn_product_type; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.cmn_product_type (id, code, name, is_service, description) FROM stdin;
1	good	Tovar	f	Qayta sotish uchun sotib olingan tovar
2	good_retail	Chakana savdo tovari	f	Chakana savdo tarmog'i orqali sotiladigan tovar
3	good_other	Boshqa tovar	f	Ko'rgazmadagi, yo'ldagi, komissiyaga yuborilgan tovar va h.k.
4	material_raw	Xomashyo va materiallar	f	Ishlab chiqarishda ishlatiladigan asosiy xomashyo
5	material_component	Sotib olingan yarim tayyor mahsulot va komplektlovchilar	f	Sotib olingan yarim tayyor buyumlar, konstruksiyalar va detallar
6	material_spare_part	Ehtiyot qismlar	f	Uskunalarni ta'mirlash va texnik xizmat ko'rsatish uchun ehtiyot qismlar
7	material_construction	Qurilish materiallari	f	Qurilish va ta'mirlash ishlari uchun materiallar
8	material_packaging	Idish va idishbop materiallar	f	Qadoqlash materiallari va idishlar
9	material_other	Boshqa materiallar	f	Boshqa guruhlarga kiritilmagan materiallar
10	semi_finished	O'z ishlab chiqarishi yarim tayyor mahsuloti	f	Tashkilot tomonidan mustaqil ishlab chiqarilgan yarim tayyor mahsulot
11	finished_goods	Tayyor mahsulot	f	Tashkilot ishlab chiqargan va sotishga tayyor mahsulot
12	service_main	Xizmat (asosiy faoliyat)	t	Tashkilotning asosiy faoliyati doirasida ko'rsatiladigan xizmat
13	service_toll	Tolling xizmati	t	Mijoz xomashyosini qayta ishlash xizmati
14	service_auxiliary	Yordamchi xizmat	t	Yordamchi ishlab chiqarish xizmati
15	service_maintenance	Xizmat ko'rsatuvchi xo'jalik	t	Xizmat ko'rsatuvchi bo'linmalar xizmatlari (oshxona, ijtimoiy ob'ektlar va h.k.)
16	service_rental	Ijaraga berish xizmati	t	Buyumlarni ijaraga berish
\.


--
-- Data for Name: cmn_product_type_translation; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.cmn_product_type_translation (product_type_id, language_id, name, description) FROM stdin;
1	1	Tovar	Qayta sotish uchun sotib olingan tovar
2	1	Chakana savdo tovari	Chakana savdo tarmog'i orqali sotiladigan tovar
3	1	Boshqa tovar	Ko'rgazmadagi, yo'ldagi, komissiyaga yuborilgan tovar va h.k.
4	1	Xomashyo va materiallar	Ishlab chiqarishda ishlatiladigan asosiy xomashyo
5	1	Sotib olingan yarim tayyor mahsulot va komplektlovchilar	Sotib olingan yarim tayyor buyumlar, konstruksiyalar va detallar
6	1	Ehtiyot qismlar	Uskunalarni ta'mirlash va texnik xizmat ko'rsatish uchun ehtiyot qismlar
7	1	Qurilish materiallari	Qurilish va ta'mirlash ishlari uchun materiallar
8	1	Idish va idishbop materiallar	Qadoqlash materiallari va idishlar
9	1	Boshqa materiallar	Boshqa guruhlarga kiritilmagan materiallar
10	1	O'z ishlab chiqarishi yarim tayyor mahsuloti	Tashkilot tomonidan mustaqil ishlab chiqarilgan yarim tayyor mahsulot
11	1	Tayyor mahsulot	Tashkilot ishlab chiqargan va sotishga tayyor mahsulot
12	1	Xizmat (asosiy faoliyat)	Tashkilotning asosiy faoliyati doirasida ko'rsatiladigan xizmat
13	1	Tolling xizmati	Mijoz xomashyosini qayta ishlash xizmati
14	1	Yordamchi xizmat	Yordamchi ishlab chiqarish xizmati
15	1	Xizmat ko'rsatuvchi xo'jalik	Xizmat ko'rsatuvchi bo'linmalar xizmatlari (oshxona, ijtimoiy ob'ektlar va h.k.)
16	1	Ijaraga berish xizmati	Buyumlarni ijaraga berish
1	2	Товар	Товар, приобретённый для перепродажи
2	2	Товар в розничной торговле	Товар, реализуемый через розничную сеть
3	2	Прочий товар	Товар на выставке, в пути, отгруженный на комиссию и пр.
4	2	Сырьё и материалы	Основное сырьё, используемое в производстве
5	2	Покупные полуфабрикаты и комплектующие	Комплектующие изделия, конструкции и детали
6	2	Запасные части	Запчасти для ремонта и обслуживания оборудования
7	2	Строительные материалы	Материалы для строительных и ремонтных работ
8	2	Тара и тарные материалы	Упаковочные материалы и тара
9	2	Прочие материалы	Прочие материалы, не отнесённые к другим группам
10	2	Полуфабрикат собственного производства	Полуфабрикаты, произведённые организацией самостоятельно
11	2	Готовая продукция	Продукция, выпущенная организацией и готовая к реализации
12	2	Услуга (основная деятельность)	Услуга, оказываемая в рамках основной деятельности организации
13	2	Переработка давальческого сырья	Услуга по переработке сырья, принадлежащего заказчику
14	2	Вспомогательная услуга	Услуга вспомогательного производства
15	2	Обслуживающее производство/хозяйство	Услуги обслуживающих подразделений (столовая, соцобъекты и т.п.)
16	2	Услуга проката	Сдача предметов в прокат
1	3	Good	Item purchased for resale
2	3	Retail good	Good sold through a retail outlet
3	3	Other good	Good on display, in transit, shipped on consignment, etc.
4	3	Raw material	Primary material used in production
5	3	Purchased component	Purchased semi-finished parts, assemblies and details
6	3	Spare part	Spare part for equipment repair and maintenance
7	3	Construction material	Material used for construction and repair works
8	3	Packaging material	Packaging materials and containers
9	3	Other material	Materials not classified into other groups
10	3	Semi-finished product	Semi-finished product manufactured in-house
11	3	Finished goods	Product manufactured by the organization and ready for sale
12	3	Service (core activity)	Service provided as part of the core business activity
13	3	Toll processing service	Service of processing customer-supplied raw materials
14	3	Auxiliary service	Service of an auxiliary production unit
15	3	Servicing facility	Services of servicing units (canteen, social facilities, etc.)
16	3	Rental service	Renting out items
\.


--
-- Data for Name: cmn_region; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.cmn_region (id, short_name, full_name, state_id, created_date) FROM stdin;
1	Toshkent shahri	Toshkent shahri	1	2026-06-05 16:36:13.009757
2	Toshkent	Toshkent	1	2026-06-05 16:36:13.009757
3	Andijon	Andijon	1	2026-06-05 16:36:13.009757
4	Buxoro	Buxoro	1	2026-06-05 16:36:13.009757
5	Jizzax	Jizzax	1	2026-06-05 16:36:13.009757
6	Qoraqalpog‘iston Respublikasi	Qoraqalpog‘iston Respublikasi	1	2026-06-05 16:36:13.009757
7	Qashqadaryo	Qashqadaryo	1	2026-06-05 16:36:13.009757
8	Navoiy	Navoiy	1	2026-06-05 16:36:13.009757
9	Namangan	Namangan	1	2026-06-05 16:36:13.009757
10	Samarqand	Samarqand	1	2026-06-05 16:36:13.009757
11	Surxondaryo	Surxondaryo	1	2026-06-05 16:36:13.009757
12	Sirdaryo	Sirdaryo	1	2026-06-05 16:36:13.009757
13	Farg‘ona	Farg‘ona	1	2026-06-05 16:36:13.009757
14	Xorazm	Xorazm	1	2026-06-05 16:36:13.009757
\.


--
-- Data for Name: cmn_state; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.cmn_state (id, short_name, full_name, created_date) FROM stdin;
1	A	Aktiv	2026-06-05 16:34:55.941031
2	P	Passiv	2026-06-05 16:34:55.941031
\.


--
-- Data for Name: cmn_tax_type; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.cmn_tax_type (id, code, name, state_id, created_date) FROM stdin;
1	none	Soliqsiz	1	2026-06-06 16:40:32.193179
2	vat	QQS	1	2026-06-06 16:40:32.193179
\.


--
-- Data for Name: cmn_translation; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.cmn_translation (id, language_id, table_name, record_id, column_name, value, created_date) FROM stdin;
1	3	cmn_currency	1	name	Uzbek sum	2026-06-06 10:44:54.451195
2	2	cmn_currency	1	name	Узбекский сум	2026-06-06 10:44:54.451195
3	1	cmn_currency	1	name	So'm	2026-06-06 10:44:54.451195
4	3	cmn_currency	2	name	US Dollar	2026-06-06 10:44:54.451195
5	2	cmn_currency	2	name	Доллар США	2026-06-06 10:44:54.451195
6	1	cmn_currency	2	name	AQSH dollari	2026-06-06 10:44:54.451195
7	3	cmn_currency	3	name	Russian Ruble	2026-06-06 10:44:54.451195
8	2	cmn_currency	3	name	Российский рубль	2026-06-06 10:44:54.451195
9	1	cmn_currency	3	name	Rossiya rubli	2026-06-06 10:44:54.451195
10	3	cmn_currency	4	name	Euro	2026-06-06 10:44:54.451195
11	2	cmn_currency	4	name	Евро	2026-06-06 10:44:54.451195
12	1	cmn_currency	4	name	Yevro	2026-06-06 10:44:54.451195
13	3	cmn_unit	1	name	Piece	2026-06-06 10:44:54.451195
14	2	cmn_unit	1	name	Штука	2026-06-06 10:44:54.451195
15	1	cmn_unit	1	name	Dona	2026-06-06 10:44:54.451195
16	3	cmn_unit	2	name	Kilogram	2026-06-06 10:44:54.451195
17	2	cmn_unit	2	name	Килограмм	2026-06-06 10:44:54.451195
18	1	cmn_unit	2	name	Kilogram	2026-06-06 10:44:54.451195
19	3	cmn_unit	3	name	Liter	2026-06-06 10:44:54.451195
20	2	cmn_unit	3	name	Литр	2026-06-06 10:44:54.451195
21	1	cmn_unit	3	name	Litr	2026-06-06 10:44:54.451195
22	3	cmn_unit	4	name	Meter	2026-06-06 10:44:54.451195
23	2	cmn_unit	4	name	Метр	2026-06-06 10:44:54.451195
24	1	cmn_unit	4	name	Metr	2026-06-06 10:44:54.451195
25	3	cmn_unit	5	name	Service	2026-06-06 10:44:54.451195
26	2	cmn_unit	5	name	Услуга	2026-06-06 10:44:54.451195
27	1	cmn_unit	5	name	Xizmat	2026-06-06 10:44:54.451195
28	3	cmn_document_status	1	name	Draft	2026-06-06 10:44:54.451195
29	2	cmn_document_status	1	name	Черновик	2026-06-06 10:44:54.451195
30	1	cmn_document_status	1	name	Qoralama	2026-06-06 10:44:54.451195
31	3	cmn_document_status	2	name	Posted	2026-06-06 10:44:54.451195
32	2	cmn_document_status	2	name	Проведён	2026-06-06 10:44:54.451195
33	1	cmn_document_status	2	name	O'tkazilgan	2026-06-06 10:44:54.451195
34	3	cmn_document_status	3	name	Cancelled	2026-06-06 10:44:54.451195
35	2	cmn_document_status	3	name	Отменён	2026-06-06 10:44:54.451195
36	1	cmn_document_status	3	name	Bekor qilingan	2026-06-06 10:44:54.451195
37	3	cmn_counterparty_type	1	name	Client	2026-06-06 10:44:54.451195
38	2	cmn_counterparty_type	1	name	Клиент	2026-06-06 10:44:54.451195
39	1	cmn_counterparty_type	1	name	Mijoz	2026-06-06 10:44:54.451195
40	3	cmn_counterparty_type	2	name	Supplier	2026-06-06 10:44:54.451195
41	2	cmn_counterparty_type	2	name	Поставщик	2026-06-06 10:44:54.451195
42	1	cmn_counterparty_type	2	name	Yetkazib beruvchi	2026-06-06 10:44:54.451195
43	3	cmn_counterparty_type	3	name	Client and supplier	2026-06-06 10:44:54.451195
44	2	cmn_counterparty_type	3	name	Клиент и поставщик	2026-06-06 10:44:54.451195
45	1	cmn_counterparty_type	3	name	Mijoz va yetkazib beruvchi	2026-06-06 10:44:54.451195
46	3	cmn_payment_type	1	name	Cash	2026-06-06 10:44:54.451195
47	2	cmn_payment_type	1	name	Наличные	2026-06-06 10:44:54.451195
48	1	cmn_payment_type	1	name	Naqd	2026-06-06 10:44:54.451195
49	3	cmn_payment_type	2	name	Bank	2026-06-06 10:44:54.451195
50	2	cmn_payment_type	2	name	Банк	2026-06-06 10:44:54.451195
51	1	cmn_payment_type	2	name	Bank	2026-06-06 10:44:54.451195
52	3	cmn_payment_type	3	name	Card	2026-06-06 10:44:54.451195
53	2	cmn_payment_type	3	name	Карта	2026-06-06 10:44:54.451195
54	1	cmn_payment_type	3	name	Karta	2026-06-06 10:44:54.451195
55	3	cmn_payment_type	4	name	Transfer	2026-06-06 10:44:54.451195
56	2	cmn_payment_type	4	name	Перевод	2026-06-06 10:44:54.451195
57	1	cmn_payment_type	4	name	O'tkazma	2026-06-06 10:44:54.451195
\.


--
-- Data for Name: cmn_unit; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.cmn_unit (id, code, name, state_id) FROM stdin;
1	dona	Dona	1
2	kg	Kilogram	1
3	litr	Litr	1
4	metr	Metr	1
5	xizmat	Xizmat	1
\.


--
-- Data for Name: cmn_vat_rate; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.cmn_vat_rate (id, code, name, rate, state_id, created_date, effective_from, effective_to) FROM stdin;
1	vat_0	QQS 0%	0.00	1	2026-06-06 16:40:45.727928	\N	\N
2	vat_12	QQS 12%	12.00	1	2026-06-06 16:40:45.727928	\N	\N
3	vat_15	QQS 15%	15.00	1	2026-06-06 16:40:45.727928	\N	\N
4	vat_6	QQS 6%	6.00	1	2026-06-27 13:53:02.619573	\N	\N
\.


--
-- Data for Name: counterparty_account_payment_purpose_hint; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.counterparty_account_payment_purpose_hint (counterparty_bank_account_id, payment_purpose_id, usage_count, last_used_date) FROM stdin;
\.


--
-- Data for Name: counterparty_bank_account; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.counterparty_bank_account (id, organization_id, counterparty_id, bank_id, account_number, currency_id, is_main, state_id, created_date) FROM stdin;
1	8	19	4	a5s831ascxpic13mx90vnaq	1	t	1	2026-07-01 17:51:23.081089
\.


--
-- Data for Name: counterparty_card; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.counterparty_card (id, organization_id, counterparty_type_id, short_name, full_name, inn, phone_number, email, region_id, district_id, address, state_id, created_date, code, is_customer, is_supplier, is_vat_payer, oked, external_id) FROM stdin;
16	8	1	as	Asta	12132145631	+998 99 890-08-58	\N	4	77	Navoiy, Uzbekistan	1	2026-06-20 15:37:24.758641	\N	t	t	f	\N	\N
17	8	2	aaaa	Shaxriddinbek	12132145631	+998 99 890-08-58	\N	3	58	Navoiy, Uzbekistan	1	2026-06-20 16:03:26.590196	\N	t	t	f	\N	\N
18	8	2	Artel	Artel	222222222	+998 00 222-00-22	\N	2	51		1	2026-06-20 17:54:14.649893	\N	t	t	f	\N	\N
19	8	1	Farrux Tech	Farrux Tech	999888777	+998 00 111-44-11	\N	8	129		1	2026-06-20 18:20:49.024257	\N	t	t	f	\N	\N
20	8	3	Ava	Avalon	22618000562088110001	+998 99 556-56-88	\N	4	78	Navoiy, Uzbekistan	1	2026-06-24 18:15:21.1843	\N	t	t	f	\N	\N
21	8	1	Aval	Avaloncha	20208000005157348001	+998 99 890-08-58	\N	3	59	Navoiy, Uzbekistan	1	2026-06-25 11:14:04.759077	\N	t	t	f	\N	\N
\.


--
-- Data for Name: counterparty_contact; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.counterparty_contact (id, organization_id, counterparty_id, full_name, phone_number, email, "position", comment, state_id, created_date) FROM stdin;
\.


--
-- Data for Name: counterparty_reg_balance; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.counterparty_reg_balance (id, organization_id, document_type_id, document_id, counterparty_id, operation_type_id, currency_id, amount, doc_date, created_date, posting_batch_id, source_line_id, reversal_entry_id) FROM stdin;
2	8	4	4	18	5	1	2550000.00	2026-07-02 17:28:22	2026-07-02 19:45:36.807768	3	\N	\N
3	8	4	4	18	4	1	2550000.00	2026-07-03 11:24:40.304295	2026-07-03 11:24:40.304295	4	2	2
4	8	4	5	18	5	1	10000000.00	2026-07-03 11:37:07	2026-07-03 11:42:39.634148	5	\N	\N
5	8	4	5	18	4	1	10000000.00	2026-07-03 11:42:48.997151	2026-07-03 11:42:48.997151	6	4	4
6	8	4	6	18	5	1	2550000.00	2026-07-03 11:55:01	2026-07-03 11:55:54.061324	7	\N	\N
7	8	4	7	18	5	1	1000.00	2026-07-03 12:03:25	2026-07-03 12:04:18.188125	8	\N	\N
8	8	4	7	18	4	1	1000.00	2026-07-03 12:04:51.391238	2026-07-03 12:04:51.391238	9	7	7
9	8	3	41	19	5	1	3550000.00	2026-07-04 03:27:53	2026-07-04 08:30:34.853802	14	\N	\N
10	8	4	10	19	5	1	100000000.00	2026-07-04 10:53:00	2026-07-04 12:05:26.514701	19	\N	\N
11	8	4	11	19	5	1	40000000.00	2026-07-04 11:09:31	2026-07-04 12:08:18.641325	20	\N	\N
12	8	4	9	18	5	1	1000.00	2026-07-04 10:53:00	2026-07-04 12:27:31.972371	26	\N	\N
\.


--
-- Data for Name: inv_inventory_adjustment_doc; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.inv_inventory_adjustment_doc (id, organization_id, doc_number, doc_date, warehouse_id, adjustment_type, status_id, comment, state_id, created_date, posted_at, posted_by_user_id, cancelled_at, cancelled_by_user_id) FROM stdin;
\.


--
-- Data for Name: inv_inventory_adjustment_doc_table; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.inv_inventory_adjustment_doc_table (id, owner_id, product_table_id, cost_price, original_status_id, original_state_id, original_warehouse_id, was_created) FROM stdin;
\.


--
-- Data for Name: inv_inventory_adjustment_line; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.inv_inventory_adjustment_line (id, owner_id, product_id, unit_id, quantity, comment) FROM stdin;
\.


--
-- Data for Name: inv_inventory_count_doc; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.inv_inventory_count_doc (id, organization_id, doc_number, doc_date, warehouse_id, status_id, comment, state_id, created_date, count_completed_at, count_completed_by_user_id, positive_adjustment_doc_id, negative_adjustment_doc_id, posted_at, posted_by_user_id, cancelled_at, cancelled_by_user_id) FROM stdin;
\.


--
-- Data for Name: inv_inventory_count_doc_table; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.inv_inventory_count_doc_table (id, owner_id, product_table_id, barcode, serial_number, marking_number, cost_price) FROM stdin;
\.


--
-- Data for Name: inv_inventory_count_line; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.inv_inventory_count_line (id, owner_id, product_id, unit_id, counted_quantity, default_cost_price, comment) FROM stdin;
\.


--
-- Data for Name: inv_product; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.inv_product (id, organization_id, product_group_id, unit_id, barcode, name, description, is_service, state_id, created_date, mxik, is_piece_tracked, code, sku, article, default_vat_rate_id, min_stock, product_type_id, is_sold, is_purchased) FROM stdin;
23	8	13	1	08418001001005219	ARTEL, икки камерали HD 316 FND ECO FROST қора-жилосиз ранг		f	1	2026-06-27 15:18:21.488187	08418001001005219	t	\N	\N	\N	\N	\N	1	t	t
24	8	13	1	08418001001005223	ARTEL, икки камерали HD 341 FND ECO FROST ёмғирли-асфалт ранг		f	1	2026-06-27 15:18:21.488261	08418001001005223	t	\N	\N	\N	\N	\N	1	t	t
25	8	14	5	09903001001000000	Газ таъминоти хизматлари		t	1	2026-06-27 15:21:30.132046	09903001001000000	f	\N	\N	\N	\N	\N	1	t	t
26	8	14	5	09905001001000000	Электр энергия хизматлари		t	1	2026-06-27 15:21:30.132198	09905001001000000	f	\N	\N	\N	\N	\N	1	t	t
\.


--
-- Data for Name: inv_product_group; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.inv_product_group (id, organization_id, name, state_id, created_date, code, parent_id, sort_order) FROM stdin;
13	8	Muzlatgichlar	1	2026-06-27 15:18:21.48753	\N	\N	0
14	8	Komunnal xizmatlar	1	2026-06-27 15:21:30.13086	\N	\N	0
\.


--
-- Data for Name: inv_product_price; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.inv_product_price (id, organization_id, product_id, currency_id, price_type_id, unit_id, price, start_date, end_date, state_id, created_date) FROM stdin;
1	8	23	1	1	1	11680.00000000	2026-06-01 00:00:00	\N	1	2026-06-29 14:56:50.128533
2	8	24	1	1	1	17250.00000000	2026-06-29 15:01:54.112203	\N	1	2026-06-29 15:01:54.112203
\.


--
-- Data for Name: inv_product_table; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.inv_product_table (id, product_id, organization_id, state_id, created_date, serial_number, marking_number, status_id, current_warehouse_id) FROM stdin;
457	23	8	1	2026-06-27 18:07:17.497799	\N	0104780074203717210t*wRjFf>m9eK-'dqeRV91UZF092b0xBdzcw+3GSPfZP95mjWrYDKhom4PWWecsUayvv2sQ=	1	\N
458	23	8	1	2026-06-27 18:07:17.497799	\N	010478010915122821)nUfDN1p=8G.G2nk8_sJ91UZF092d3lJcOvPHgtC3DxncIcpDZVxRY9BDkfyC1+gYcgxFyQ=	1	\N
463	24	8	1	2026-06-29 15:01:54.008417	\N	aohjfoiwhio	1	\N
477	24	8	1	2026-07-04 08:57:17.31933	\N	/api/manuals/counterparties	2	\N
462	24	8	1	2026-06-29 15:01:54.008414	\N	fwliejfoiwjfopwjp'ef856	1	\N
455	23	8	1	2026-06-27 18:07:17.497344	\N	0104780074206893217UkCJ6Gu*gG_j.wf6WnX91XUWh92JkB/xWNpbjVhcmtkWVdsT3lhVTVhYjdUQm8yQgOW3lE=	3	\N
456	23	8	1	2026-06-27 18:07:17.497737	\N	010478007420371721NZVF0x+-fnd<lcpI11Wc91UZF092SDB2UZccAGRfCmSPNYJYumzGxmcnTaumTSS1HOIBuOE=	3	\N
459	23	8	1	2026-06-29 14:59:14.793003	\N	qwdqdqwdqwdqwdqdqd	3	\N
460	23	8	1	2026-06-29 14:59:14.793431	\N	qdqdqdqdqdqdqdqdqdq2855	3	\N
461	23	8	1	2026-06-29 14:59:14.793497	\N	qdqdqdqdqdq98d4q8d789qwd7q	3	\N
\.


--
-- Data for Name: inv_reg_balance; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.inv_reg_balance (id, organization_id, document_type_id, document_id, warehouse_id, product_id, operation_type_id, quantity, amount, doc_date, created_date, posting_batch_id, source_line_id, reversal_entry_id, product_table_id) FROM stdin;
203	8	1	92	7	23	1	1.000	11200.00	2026-06-27 18:05:43	2026-06-27 18:07:18.670817	\N	\N	\N	\N
204	8	1	92	7	23	1	1.000	11200.00	2026-06-27 18:05:43	2026-06-27 18:07:18.670939	\N	\N	\N	\N
205	8	1	92	7	23	1	1.000	11200.00	2026-06-27 18:05:43	2026-06-27 18:07:18.67094	\N	\N	\N	\N
206	8	1	92	7	23	1	1.000	11200.00	2026-06-27 18:05:43	2026-06-27 18:07:18.67094	\N	\N	\N	\N
207	8	1	95	7	23	1	1.000	12320.00	2026-06-15 14:58:00	2026-06-29 14:59:16.180774	\N	\N	\N	\N
208	8	1	95	7	23	1	1.000	12320.00	2026-06-15 14:58:00	2026-06-29 14:59:16.180929	\N	\N	\N	\N
209	8	1	95	7	23	1	1.000	12320.00	2026-06-15 14:58:00	2026-06-29 14:59:16.180931	\N	\N	\N	\N
210	8	1	96	7	24	1	1.000	17250.00	2026-06-29 14:59:15	2026-06-29 15:01:54.095025	\N	\N	\N	\N
211	8	1	96	7	24	1	1.000	17250.00	2026-06-29 14:59:15	2026-06-29 15:01:54.095028	\N	\N	\N	\N
212	8	2	80	7	23	2	1.000	59360.00	2026-06-29 17:36:29.055026	2026-06-30 12:28:30.247402	\N	\N	\N	\N
213	8	2	80	7	23	2	1.000	59360.00	2026-06-29 17:36:29.055026	2026-06-30 12:28:30.247467	\N	\N	\N	\N
214	8	2	80	7	23	2	1.000	59360.00	2026-06-29 17:36:29.055026	2026-06-30 12:28:30.247468	\N	\N	\N	\N
215	8	2	80	7	23	2	1.000	59360.00	2026-06-29 17:36:29.055026	2026-06-30 12:28:30.247468	\N	\N	\N	\N
216	8	2	80	7	23	2	1.000	59360.00	2026-06-29 17:36:29.055026	2026-06-30 12:28:30.247469	\N	\N	\N	\N
\.


--
-- Data for Name: inv_transfer_doc; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.inv_transfer_doc (id, organization_id, doc_number, doc_date, source_warehouse_id, destination_warehouse_id, status_id, comment, state_id, created_date, posted_at, posted_by_user_id, cancelled_at, cancelled_by_user_id) FROM stdin;
\.


--
-- Data for Name: inv_transfer_doc_table; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.inv_transfer_doc_table (id, owner_id, product_table_id, source_warehouse_id, destination_warehouse_id, cost_price) FROM stdin;
\.


--
-- Data for Name: inv_transfer_line; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.inv_transfer_line (id, owner_id, product_id, unit_id, quantity, comment) FROM stdin;
\.


--
-- Data for Name: inv_warehouse; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.inv_warehouse (id, organization_id, branch_id, name, responsible_user_id, state_id, created_date, code, address, is_main) FROM stdin;
7	8	6	amonov	15	1	2026-06-20 15:38:43.144691	\N	\N	f
8	8	6	Artel	12	1	2026-07-04 09:31:59.46417	4927	\N	f
\.


--
-- Data for Name: money_reg_balance; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.money_reg_balance (id, organization_id, document_type_id, document_id, source_type, source_id, operation_type_id, currency_id, amount, doc_date, created_date, posting_batch_id, source_line_id, reversal_entry_id) FROM stdin;
1	2	1	1	KassaUzb	1	1	1	800000.00	2026-06-08 05:56:31.616	2026-06-08 10:59:21.788241	\N	\N	\N
2	8	4	4	CASH_OPERATION	4	1	1	2550000.00	2026-07-02 17:28:22	2026-07-02 19:45:36.736296	3	\N	\N
3	8	4	4	CASH_OPERATION	4	2	1	2550000.00	2026-07-03 11:24:40.198842	2026-07-03 11:24:40.198842	4	\N	2
4	8	4	5	CASH_OPERATION	4	1	1	10000000.00	2026-07-03 11:37:07	2026-07-03 11:42:39.621975	5	\N	\N
5	8	4	5	CASH_OPERATION	4	2	1	10000000.00	2026-07-03 11:42:48.989607	2026-07-03 11:42:48.989607	6	\N	4
6	8	4	6	CASH_OPERATION	4	1	1	2550000.00	2026-07-03 11:55:01	2026-07-03 11:55:54.057553	7	\N	\N
7	8	4	7	CASH_OPERATION	4	1	1	1000.00	2026-07-03 12:03:25	2026-07-03 12:04:18.186244	8	\N	\N
8	8	4	7	CASH_OPERATION	4	2	1	1000.00	2026-07-03 12:04:51.388258	2026-07-03 12:04:51.388258	9	\N	7
9	8	3	41	BANK_OPERATION	12	1	1	3550000.00	2026-07-04 03:27:53	2026-07-04 08:30:34.796786	14	\N	\N
10	8	4	8	CASH_OPERATION	4	2	1	1550000.00	2026-07-04 10:53:00	2026-07-04 11:03:34.157137	18	\N	\N
11	8	4	10	CASH_OPERATION	4	1	1	100000000.00	2026-07-04 10:53:00	2026-07-04 12:05:26.509243	19	\N	\N
12	8	4	11	CASH_OPERATION	4	2	1	40000000.00	2026-07-04 11:09:31	2026-07-04 12:08:18.637003	20	\N	\N
13	8	4	12	CASH_OPERATION	4	1	1	10000000.00	2026-07-04 12:10:12	2026-07-04 12:19:36.142532	21	\N	\N
14	8	4	15	CASH_OPERATION	4	1	1	2550000.00	2026-07-04 12:21:58	2026-07-04 12:22:53.913759	22	\N	\N
15	8	4	14	CASH_OPERATION	4	1	1	10000000.00	2026-07-04 12:21:58	2026-07-04 12:23:18.052166	23	\N	\N
16	8	4	13	CASH_OPERATION	4	1	1	3550000.00	2026-07-04 12:19:59	2026-07-04 12:24:02.057073	24	\N	\N
17	8	4	16	CASH_OPERATION	4	1	1	1000.00	2026-07-04 12:24:15	2026-07-04 12:24:35.320169	25	\N	\N
18	8	4	9	CASH_OPERATION	4	2	1	1000.00	2026-07-04 10:53:00	2026-07-04 12:27:31.970559	26	\N	\N
\.


--
-- Data for Name: org_bank_account; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.org_bank_account (id, organization_id, bank_id, account_number, currency_id, is_main, state_id, created_date, code, name, opening_balance, opening_balance_date) FROM stdin;
1	2	1	064232	4	t	1	2026-06-08 10:34:07.891936	\N	\N	0.00	\N
8	8	4	0099855144112	3	t	1	2026-06-24 11:59:17.881525	\N	\N	0.00	\N
10	8	3	064232347878	3	t	1	2026-06-24 15:34:56.767807	\N	\N	0.00	\N
11	8	3	23106000105157348001	1	t	1	2026-06-24 17:01:04.482009	\N	\N	0.00	\N
12	8	2	20208000005157348001	1	t	1	2026-06-24 17:57:51.101968	\N	\N	0.00	\N
9	8	4	0099855144117	1	t	1	2026-06-24 15:34:17.666512	\N	\N	0.00	\N
\.


--
-- Data for Name: org_branch; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.org_branch (id, organization_id, code, name, region_id, district_id, address, phone_number, state_id, created_date) FROM stdin;
6	8	Malibu9876	Atalik	3	58	\N	+998 99 890-08-58	1	2026-06-20 15:38:25.066056
\.


--
-- Data for Name: org_claim_request; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.org_claim_request (id, organization_id, requested_by_user_id, inn, organization_name, status, review_comment, reviewed_by_user_id, reviewed_at, created_date) FROM stdin;
\.


--
-- Data for Name: org_defaults; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.org_defaults (id, organization_id, branch_id, warehouse_id, cash_box_id, bank_account_id, receivable_account_id, payable_account_id, inventory_account_id, cash_account_id, bank_accounting_account_id, revenue_account_id, expense_account_id, cogs_account_id, created_date) FROM stdin;
\.


--
-- Data for Name: org_department; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.org_department (id, organization_id, branch_id, code, name, state_id, created_date) FROM stdin;
\.


--
-- Data for Name: org_organization; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.org_organization (id, short_name, full_name, inn, phone_number, region_id, district_id, address, director, is_parent, state_id, created_date, default_language_id, tenant_id, setup_status, setup_completed_at, email, website, oked) FROM stdin;
8	baraka_market	Baraka market	1599789	+998 99 897-06-42	8	127	Alisher Navoiy 17	Hafizov Sardorbek	f	1	2026-06-12 11:24:38.218896	1	\N	not_started	\N	\N	\N	\N
2	Najot Ta'lim	Najot Ta'lim Xususiy	310540000	+998 99 871-23-12	8	125	Toshkent shahar, Mirzo Ulug'bek tumani, Amir Temur ko'chasi 1-uy test	Karimov Jasur test	t	1	2026-06-05 16:49:46.331959	2	\N	archived	\N	\N	\N	\N
\.


--
-- Data for Name: org_organization_config; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.org_organization_config (organization_id, inventory_valuation_method, accounting_policy_id, base_currency_id, accounting_start_date, fiscal_year_start_month) FROM stdin;
\.


--
-- Data for Name: org_position; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.org_position (id, organization_id, code, name, state_id, created_date) FROM stdin;
\.


--
-- Data for Name: org_setup_state; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.org_setup_state (id, organization_id, current_step, organization_completed, tax_completed, accounting_completed, defaults_completed, users_completed, is_completed, completed_at, updated_date, created_date) FROM stdin;
\.


--
-- Data for Name: org_tax_settings; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.org_tax_settings (id, organization_id, tax_type_id, is_vat_payer, vat_registration_number, effective_from, effective_to, state_id, created_date) FROM stdin;
\.


--
-- Data for Name: org_user_invitation; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.org_user_invitation (id, organization_id, email, role_id, invited_by_user_id, token_hash, expires_at, accepted_at, accepted_by_user_id, state_id, created_date) FROM stdin;
\.


--
-- Data for Name: platform_tenant; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.platform_tenant (id, name, slug, owner_user_id, state_id, created_date, updated_date) FROM stdin;
2	user	us	3	1	2026-07-01 12:28:04.261515	\N
1	amonov	mah	1	1	2026-07-01 11:50:59.92317	2026-07-01 12:29:12.582604
\.


--
-- Data for Name: pur_doc; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.pur_doc (id, organization_id, doc_number, doc_date, counterparty_id, warehouse_id, currency_id, total_amount, vat_amount, final_amount, status_id, comment, state_id, created_date, contract_id, exchange_rate, posted_at, posted_by_user_id, cancelled_at, cancelled_by_user_id) FROM stdin;
92	8	100000074	2026-06-27 18:05:43	18	7	1	40000.00000000	4800.00000000	44800.00000000	2	\N	1	2026-06-27 18:07:17.501272	9	1.000000	\N	\N	\N	\N
93	8	100000075	2026-06-27 18:07:17	18	7	1	10000.00000000	1200.00000000	11200.00000000	2	\N	1	2026-06-27 18:07:50.767417	10	1.000000	\N	\N	\N	\N
94	8	100000076	2026-06-29 10:06:32	18	7	1	10000.00000000	1200.00000000	11200.00000000	2	\N	1	2026-06-29 10:13:54.392765	9	1.000000	\N	\N	\N	\N
95	8	100000077	2026-06-15 14:58:00	18	7	1	33000.00000000	3960.00000000	36960.00000000	2	\N	1	2026-06-29 14:59:14.796206	9	1.000000	\N	\N	\N	\N
96	8	100000078	2026-06-29 14:59:15	18	7	1	30000.00000000	4500.00000000	34500.00000000	2	\N	1	2026-06-29 15:01:54.008454	10	1.000000	\N	\N	\N	\N
97	8	100000079	2026-06-29 17:55:00	18	7	1	20000.00000000	2400.00000000	22400.00000000	2	\N	1	2026-06-29 17:57:51.597146	9	1.000000	\N	\N	\N	\N
98	8	100000080	2026-07-03 15:47:49	18	7	1	10000.00000000	1200.00000000	11200.00000000	3	\N	1	2026-07-03 15:48:13.121681	9	1.000000	\N	\N	2026-07-03 15:48:35.667066	4
99	8	100000081	2026-07-03 15:59:16	18	7	1	10000.00000000	1200.00000000	11200.00000000	3	\N	1	2026-07-03 16:15:55.260463	9	1.000000	\N	\N	2026-07-03 16:18:01.551278	4
100	8	100000082	2026-07-03 15:59:16	18	7	1	10000.00000000	1200.00000000	11200.00000000	3	\N	1	2026-07-03 16:25:23.358861	9	1.000000	\N	\N	2026-07-03 17:35:35.247432	4
101	8	100000083	2026-07-03 17:21:43	18	7	1	10000.00000000	1200.00000000	11200.00000000	3	\N	1	2026-07-03 17:37:48.377285	9	1.000000	\N	\N	2026-07-04 08:32:38.653805	4
102	8	100000084	2026-07-04 08:32:26	18	7	1	10000.00000000	1200.00000000	11200.00000000	3	\N	1	2026-07-04 08:33:22.100738	9	1.000000	\N	\N	2026-07-04 08:47:52.115002	4
103	8	100000085	2026-07-04 08:47:40	18	7	1	10000.00000000	1200.00000000	11200.00000000	1	\N	1	2026-07-04 08:57:17.324885	10	1.000000	\N	\N	\N	\N
\.


--
-- Data for Name: pur_doc_product; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.pur_doc_product (id, owner_id, product_id, quantity, unit_id, amount, vat_rate_id, vat_amount, total_amount, unit_price) FROM stdin;
14	92	23	4.000000	1	40000.00000000	2	4800.00000000	44800.00000000	10000.00000000
15	93	25	2.000000	5	10000.00000000	2	1200.00000000	11200.00000000	5000.00000000
16	94	25	1.000000	5	10000.00000000	2	1200.00000000	11200.00000000	10000.00000000
17	95	23	3.000000	1	33000.00000000	2	3960.00000000	36960.00000000	11000.00000000
18	96	24	2.000000	1	30000.00000000	3	4500.00000000	34500.00000000	15000.00000000
19	97	25	2.000000	5	20000.00000000	2	2400.00000000	22400.00000000	10000.00000000
20	98	24	1.000000	1	10000.00000000	2	1200.00000000	11200.00000000	10000.00000000
21	99	24	1.000000	1	10000.00000000	2	1200.00000000	11200.00000000	10000.00000000
23	100	24	1.000000	1	10000.00000000	2	1200.00000000	11200.00000000	10000.00000000
31	101	24	1.000000	1	10000.00000000	2	1200.00000000	11200.00000000	10000.00000000
32	102	24	1.000000	1	10000.00000000	2	1200.00000000	11200.00000000	10000.00000000
33	103	24	1.000000	1	10000.00000000	2	1200.00000000	11200.00000000	10000.00000000
\.


--
-- Data for Name: pur_doc_table; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.pur_doc_table (id, owner_id, product_table_id, amount, vat_rate_id, vat_amount, total_amount) FROM stdin;
15	14	455	10000.00000000	2	1200.00000000	11200.00000000
16	14	456	10000.00000000	2	1200.00000000	11200.00000000
17	14	457	10000.00000000	2	1200.00000000	11200.00000000
18	14	458	10000.00000000	2	1200.00000000	11200.00000000
19	17	459	11000.00000000	2	1320.00000000	12320.00000000
20	17	460	11000.00000000	2	1320.00000000	12320.00000000
21	17	461	11000.00000000	2	1320.00000000	12320.00000000
22	18	462	15000.00000000	3	2250.00000000	17250.00000000
23	18	463	15000.00000000	3	2250.00000000	17250.00000000
37	33	477	10000.00000000	2	1200.00000000	11200.00000000
\.


--
-- Data for Name: sale_condition; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.sale_condition (id, organization_id, costing_method_id, vat_rate_id, start_date, end_date, state_id, created_date) FROM stdin;
1	8	1	2	2026-04-01 00:00:00	2026-12-31 23:59:59.999999	1	2026-06-27 16:03:42.262514
2	8	3	4	2026-06-15 17:02:00	\N	1	2026-06-27 17:04:40.566346
3	8	1	2	2026-06-29 14:50:40	\N	2	2026-06-29 16:09:34.054396
4	8	1	2	2026-06-25 16:09:00	\N	1	2026-06-29 16:10:04.471712
\.


--
-- Data for Name: sale_doc; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.sale_doc (id, organization_id, doc_number, doc_date, counterparty_id, warehouse_id, currency_id, total_amount, vat_amount, final_amount, status_id, comment, state_id, created_date, contract_id, exchange_rate, posted_at, posted_by_user_id, cancelled_at, cancelled_by_user_id) FROM stdin;
79	8	100000077	2026-06-29 17:21:54.01643	18	7	1	65296.00000000	7835.52000000	73131.52000000	1	FIFO bo'yicha sotilyapti	2	2026-06-29 17:21:54.01643	9	1.000000	\N	\N	\N	\N
78	8	100000076	2026-06-29 16:42:30.843443	18	7	1	18975.00000000	2277.00000000	21252.00000000	3	\N	1	2026-06-29 16:42:30.843443	9	1.000000	\N	\N	\N	\N
80	8	100000078	2026-06-29 17:36:29.055026	18	7	1	326480.00000000	39177.60000000	365657.60000000	2	\N	1	2026-06-29 17:36:29.055026	10	1.000000	\N	\N	\N	\N
\.


--
-- Data for Name: sale_doc_product; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.sale_doc_product (id, owner_id, product_id, quantity, unit_price, cost_price, amount, vat_rate_id, vat_amount, total_amount, unit_id) FROM stdin;
28	80	23	5.000000	65296.00000000	59360.00000000	326480.00000000	2	39177.60000000	365657.60000000	1
26	78	24	1.000000	18975.00000000	17250.00000000	18975.00000000	2	2277.00000000	21252.00000000	1
\.


--
-- Data for Name: sale_doc_table; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.sale_doc_table (id, product_table_id, amount, vat_rate_id, vat_amount, total_amount, cost_price, owner_id) FROM stdin;
98	462	18975.00000000	2	2277.00000000	21252.00000000	17250.00000000	26
99	459	65296.00000000	2	7835.52000000	73131.52000000	59360.00000000	28
100	460	65296.00000000	2	7835.52000000	73131.52000000	59360.00000000	28
101	461	65296.00000000	2	7835.52000000	73131.52000000	59360.00000000	28
102	455	65296.00000000	2	7835.52000000	73131.52000000	59360.00000000	28
103	456	65296.00000000	2	7835.52000000	73131.52000000	59360.00000000	28
\.


--
-- Data for Name: sys_audit_log; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.sys_audit_log (id, organization_id, schema_name, table_name, record_id, action, old_data, new_data, changed_user_id, request_id, client_addr, application_name, changed_date) FROM stdin;
90	8	public	pur_doc	39	INSERT	\N	{"id": 39, "lines": [{"id": 211, "price": 122.00, "amount": 122.00, "ownerId": 39, "quantity": 1.000, "vatAmount": 0.00, "vatRateId": 1, "productName": "ArtelTestTovar", "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "103CC8ABN800189", "markingNumber": "010478011969800321Z6'WbR_4SBKQScZRZTjZ91UZF092NHdPZLJm0YcNie/mIPsicNrxcSHARvSblBOB1lrwuk0=", "productTableId": 327}, {"id": 212, "price": 122.00, "amount": 122.00, "ownerId": 39, "quantity": 1.000, "vatAmount": 0.00, "vatRateId": 1, "productName": "ArtelTestTovar", "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "131GNDABM800359", "markingNumber": "0104780074206893217UkCJ6Gu*gG_j.wf6WnX91XUWh92JkB/xWNpbjVhcmtkWVdsT3lhVTVhYjdUQm8yQgOW3lE=", "productTableId": 328}, {"id": 213, "price": 122.00, "amount": 122.00, "ownerId": 39, "quantity": 1.000, "vatAmount": 0.00, "vatRateId": 1, "productName": "ArtelTestTovar", "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "131GHFBUMA00234", "markingNumber": "010478011969586621B0kWxHgU+uvCk4JBnQCl91UZF092d0R1ZXXvRqo4NzIVvnorLIIDBXmBIpFADOELOkYy1nw=", "productTableId": 329}, {"id": 214, "price": 122.00, "amount": 122.00, "ownerId": 39, "quantity": 1.000, "vatAmount": 0.00, "vatRateId": 1, "productName": "Product 2", "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "103CC8ABMA00031", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huWyHl5kILiUnqU1=", "productTableId": 330}, {"id": 215, "price": 122.00, "amount": 122.00, "ownerId": 39, "quantity": 1.000, "vatAmount": 0.00, "vatRateId": 1, "productName": "Product 2", "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "901ONUCCM100183", "markingNumber": "010478011969586621UhrC?(yoyw*FKRN*XD!I91pZNF92aRpYJTRlRjJaTFVUQkJ2eXZXcStVYSpnc3ZKSVl8wCc=", "productTableId": 331}, {"id": 216, "price": 122.00, "amount": 122.00, "ownerId": 39, "quantity": 1.000, "vatAmount": 0.00, "vatRateId": 1, "productName": "Product 2", "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "321MHCACNC01023", "markingNumber": "010478011969586621UiDWeTQo6DHYfk-.7bS(91bxJH92p0ndulNPQlBvZHlNS1ItRElPNmFzbmpLUTlaaoDeL+Q=", "productTableId": 332}, {"id": 217, "price": 122.00, "amount": 122.00, "ownerId": 39, "quantity": 1.000, "vatAmount": 0.00, "vatRateId": 1, "productName": "Product 3", "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "131GS3ABMC00569", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huWyHl5kILiUnqU=", "productTableId": 333}, {"id": 218, "price": 122.00, "amount": 122.00, "ownerId": 39, "quantity": 1.000, "vatAmount": 0.00, "vatRateId": 1, "productName": "Product 3", "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "901ONUCCM100437", "markingNumber": "010478007420371721Hh=eSjFi6bZ8GHuur\\"gi91UZF092TzFFdfOAfdPwV/p68X3IFoiYBL9085NyxvvSEU5QGlY=", "productTableId": 334}], "comment": "", "docDate": "2026-06-22T16:15:13", "stateId": 1, "statusId": 1, "docNumber": "100000021", "stateName": "Aktiv", "vatAmount": 0.00, "contractId": 9, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-22T16:19:53.701859", "finalAmount": 976.00, "totalAmount": 976.00, "warehouseId": 7, "currencyName": "Uzbek so'm", "warehouseName": "amonov", "contractNumber": "100000009", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-22 16:19:53.83953
91	8	public	sale_doc	63	INSERT	\N	{"id": 63, "lines": [{"id": 47, "price": 122.00, "amount": 122.00, "ownerId": 63, "quantity": 1.000, "costPrice": 122.00, "productId": 4, "vatAmount": 0.00, "vatRateId": null, "productName": "ArtelTestTovar", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "103CC8ABN800189", "markingNumber": "010478011969800321Z6'WbR_4SBKQScZRZTjZ91UZF092NHdPZLJm0YcNie/mIPsicNrxcSHARvSblBOB1lrwuk0=", "productTableId": 327}, {"id": 48, "price": 122.00, "amount": 122.00, "ownerId": 63, "quantity": 1.000, "costPrice": 122.00, "productId": 4, "vatAmount": 0.00, "vatRateId": null, "productName": "ArtelTestTovar", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "131GNDABM800359", "markingNumber": "0104780074206893217UkCJ6Gu*gG_j.wf6WnX91XUWh92JkB/xWNpbjVhcmtkWVdsT3lhVTVhYjdUQm8yQgOW3lE=", "productTableId": 328}, {"id": 49, "price": 122.00, "amount": 122.00, "ownerId": 63, "quantity": 1.000, "costPrice": 122.00, "productId": 4, "vatAmount": 0.00, "vatRateId": null, "productName": "ArtelTestTovar", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "131GHFBUMA00234", "markingNumber": "010478011969586621B0kWxHgU+uvCk4JBnQCl91UZF092d0R1ZXXvRqo4NzIVvnorLIIDBXmBIpFADOELOkYy1nw=", "productTableId": 329}, {"id": 50, "price": 122.00, "amount": 122.00, "ownerId": 63, "quantity": 1.000, "costPrice": 122.00, "productId": 6, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 2", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "103CC8ABMA00031", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huWyHl5kILiUnqU1=", "productTableId": 330}, {"id": 51, "price": 122.00, "amount": 122.00, "ownerId": 63, "quantity": 1.000, "costPrice": 122.00, "productId": 6, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 2", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "901ONUCCM100183", "markingNumber": "010478011969586621UhrC?(yoyw*FKRN*XD!I91pZNF92aRpYJTRlRjJaTFVUQkJ2eXZXcStVYSpnc3ZKSVl8wCc=", "productTableId": 331}, {"id": 52, "price": 122.00, "amount": 122.00, "ownerId": 63, "quantity": 1.000, "costPrice": 122.00, "productId": 6, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 2", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "321MHCACNC01023", "markingNumber": "010478011969586621UiDWeTQo6DHYfk-.7bS(91bxJH92p0ndulNPQlBvZHlNS1ItRElPNmFzbmpLUTlaaoDeL+Q=", "productTableId": 332}, {"id": 53, "price": 122.00, "amount": 122.00, "ownerId": 63, "quantity": 1.000, "costPrice": 122.00, "productId": 7, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 3", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "131GS3ABMC00569", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huWyHl5kILiUnqU=", "productTableId": 333}, {"id": 54, "price": 122.00, "amount": 122.00, "ownerId": 63, "quantity": 1.000, "costPrice": 122.00, "productId": 7, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 3", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "901ONUCCM100437", "markingNumber": "010478007420371721Hh=eSjFi6bZ8GHuur\\"gi91UZF092TzFFdfOAfdPwV/p68X3IFoiYBL9085NyxvvSEU5QGlY=", "productTableId": 334}], "comment": "", "docDate": "2026-06-22T16:58:50.411098", "stateId": 1, "statusId": 1, "docNumber": "100000061", "stateName": "Aktiv", "vatAmount": 0.00, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-22T16:58:50.411098", "finalAmount": 976.00, "totalAmount": 976.00, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-22 16:58:50.493422
92	8	public	sale_doc	63	UPDATE	{"id": 63, "lines": [{"id": 47, "price": 122.00, "amount": 122.00, "ownerId": 63, "quantity": 1.000, "costPrice": 122.00, "productId": 4, "vatAmount": 0.00, "vatRateId": null, "productName": "ArtelTestTovar", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "103CC8ABN800189", "markingNumber": "010478011969800321Z6'WbR_4SBKQScZRZTjZ91UZF092NHdPZLJm0YcNie/mIPsicNrxcSHARvSblBOB1lrwuk0=", "productTableId": 327}, {"id": 48, "price": 122.00, "amount": 122.00, "ownerId": 63, "quantity": 1.000, "costPrice": 122.00, "productId": 4, "vatAmount": 0.00, "vatRateId": null, "productName": "ArtelTestTovar", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "131GNDABM800359", "markingNumber": "0104780074206893217UkCJ6Gu*gG_j.wf6WnX91XUWh92JkB/xWNpbjVhcmtkWVdsT3lhVTVhYjdUQm8yQgOW3lE=", "productTableId": 328}, {"id": 49, "price": 122.00, "amount": 122.00, "ownerId": 63, "quantity": 1.000, "costPrice": 122.00, "productId": 4, "vatAmount": 0.00, "vatRateId": null, "productName": "ArtelTestTovar", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "131GHFBUMA00234", "markingNumber": "010478011969586621B0kWxHgU+uvCk4JBnQCl91UZF092d0R1ZXXvRqo4NzIVvnorLIIDBXmBIpFADOELOkYy1nw=", "productTableId": 329}, {"id": 50, "price": 122.00, "amount": 122.00, "ownerId": 63, "quantity": 1.000, "costPrice": 122.00, "productId": 6, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 2", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "103CC8ABMA00031", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huWyHl5kILiUnqU1=", "productTableId": 330}, {"id": 51, "price": 122.00, "amount": 122.00, "ownerId": 63, "quantity": 1.000, "costPrice": 122.00, "productId": 6, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 2", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "901ONUCCM100183", "markingNumber": "010478011969586621UhrC?(yoyw*FKRN*XD!I91pZNF92aRpYJTRlRjJaTFVUQkJ2eXZXcStVYSpnc3ZKSVl8wCc=", "productTableId": 331}, {"id": 52, "price": 122.00, "amount": 122.00, "ownerId": 63, "quantity": 1.000, "costPrice": 122.00, "productId": 6, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 2", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "321MHCACNC01023", "markingNumber": "010478011969586621UiDWeTQo6DHYfk-.7bS(91bxJH92p0ndulNPQlBvZHlNS1ItRElPNmFzbmpLUTlaaoDeL+Q=", "productTableId": 332}, {"id": 53, "price": 122.00, "amount": 122.00, "ownerId": 63, "quantity": 1.000, "costPrice": 122.00, "productId": 7, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 3", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "131GS3ABMC00569", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huWyHl5kILiUnqU=", "productTableId": 333}, {"id": 54, "price": 122.00, "amount": 122.00, "ownerId": 63, "quantity": 1.000, "costPrice": 122.00, "productId": 7, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 3", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "901ONUCCM100437", "markingNumber": "010478007420371721Hh=eSjFi6bZ8GHuur\\"gi91UZF092TzFFdfOAfdPwV/p68X3IFoiYBL9085NyxvvSEU5QGlY=", "productTableId": 334}], "comment": "", "docDate": "2026-06-22T16:58:50.411098", "stateId": 1, "statusId": 1, "docNumber": "100000061", "stateName": "Aktiv", "vatAmount": 0.00, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-22T16:58:50.411098", "finalAmount": 976.00, "totalAmount": 976.00, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	{"id": 63, "lines": [{"id": 47, "price": 122.00, "amount": 122.00, "ownerId": 63, "quantity": 1.000, "costPrice": 122.00, "productId": 4, "vatAmount": 0.00, "vatRateId": null, "productName": "ArtelTestTovar", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "103CC8ABN800189", "markingNumber": "010478011969800321Z6'WbR_4SBKQScZRZTjZ91UZF092NHdPZLJm0YcNie/mIPsicNrxcSHARvSblBOB1lrwuk0=", "productTableId": 327}, {"id": 48, "price": 122.00, "amount": 122.00, "ownerId": 63, "quantity": 1.000, "costPrice": 122.00, "productId": 4, "vatAmount": 0.00, "vatRateId": null, "productName": "ArtelTestTovar", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "131GNDABM800359", "markingNumber": "0104780074206893217UkCJ6Gu*gG_j.wf6WnX91XUWh92JkB/xWNpbjVhcmtkWVdsT3lhVTVhYjdUQm8yQgOW3lE=", "productTableId": 328}, {"id": 49, "price": 122.00, "amount": 122.00, "ownerId": 63, "quantity": 1.000, "costPrice": 122.00, "productId": 4, "vatAmount": 0.00, "vatRateId": null, "productName": "ArtelTestTovar", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "131GHFBUMA00234", "markingNumber": "010478011969586621B0kWxHgU+uvCk4JBnQCl91UZF092d0R1ZXXvRqo4NzIVvnorLIIDBXmBIpFADOELOkYy1nw=", "productTableId": 329}, {"id": 50, "price": 122.00, "amount": 122.00, "ownerId": 63, "quantity": 1.000, "costPrice": 122.00, "productId": 6, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 2", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "103CC8ABMA00031", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huWyHl5kILiUnqU1=", "productTableId": 330}, {"id": 51, "price": 122.00, "amount": 122.00, "ownerId": 63, "quantity": 1.000, "costPrice": 122.00, "productId": 6, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 2", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "901ONUCCM100183", "markingNumber": "010478011969586621UhrC?(yoyw*FKRN*XD!I91pZNF92aRpYJTRlRjJaTFVUQkJ2eXZXcStVYSpnc3ZKSVl8wCc=", "productTableId": 331}, {"id": 52, "price": 122.00, "amount": 122.00, "ownerId": 63, "quantity": 1.000, "costPrice": 122.00, "productId": 6, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 2", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "321MHCACNC01023", "markingNumber": "010478011969586621UiDWeTQo6DHYfk-.7bS(91bxJH92p0ndulNPQlBvZHlNS1ItRElPNmFzbmpLUTlaaoDeL+Q=", "productTableId": 332}, {"id": 53, "price": 122.00, "amount": 122.00, "ownerId": 63, "quantity": 1.000, "costPrice": 122.00, "productId": 7, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 3", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "131GS3ABMC00569", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huWyHl5kILiUnqU=", "productTableId": 333}, {"id": 54, "price": 122.00, "amount": 122.00, "ownerId": 63, "quantity": 1.000, "costPrice": 122.00, "productId": 7, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 3", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "901ONUCCM100437", "markingNumber": "010478007420371721Hh=eSjFi6bZ8GHuur\\"gi91UZF092TzFFdfOAfdPwV/p68X3IFoiYBL9085NyxvvSEU5QGlY=", "productTableId": 334}], "comment": null, "docDate": "2026-06-22T16:58:50.411098", "stateId": 1, "statusId": 1, "docNumber": "100000061", "stateName": "Aktiv", "vatAmount": 0.00, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-22T16:58:50.411098", "finalAmount": 976.00, "totalAmount": 976.00, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "counterpartyId": 17, "organizationId": 8, "counterpartyName": "aaaa", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-22 16:59:06.475049
93	8	public	sale_doc	63	UPDATE	{"id": 63, "lines": [{"id": 47, "price": 122.00, "amount": 122.00, "ownerId": 63, "quantity": 1.000, "costPrice": 122.00, "productId": 4, "vatAmount": 0.00, "vatRateId": null, "productName": "ArtelTestTovar", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "103CC8ABN800189", "markingNumber": "010478011969800321Z6'WbR_4SBKQScZRZTjZ91UZF092NHdPZLJm0YcNie/mIPsicNrxcSHARvSblBOB1lrwuk0=", "productTableId": 327}, {"id": 48, "price": 122.00, "amount": 122.00, "ownerId": 63, "quantity": 1.000, "costPrice": 122.00, "productId": 4, "vatAmount": 0.00, "vatRateId": null, "productName": "ArtelTestTovar", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "131GNDABM800359", "markingNumber": "0104780074206893217UkCJ6Gu*gG_j.wf6WnX91XUWh92JkB/xWNpbjVhcmtkWVdsT3lhVTVhYjdUQm8yQgOW3lE=", "productTableId": 328}, {"id": 49, "price": 122.00, "amount": 122.00, "ownerId": 63, "quantity": 1.000, "costPrice": 122.00, "productId": 4, "vatAmount": 0.00, "vatRateId": null, "productName": "ArtelTestTovar", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "131GHFBUMA00234", "markingNumber": "010478011969586621B0kWxHgU+uvCk4JBnQCl91UZF092d0R1ZXXvRqo4NzIVvnorLIIDBXmBIpFADOELOkYy1nw=", "productTableId": 329}, {"id": 50, "price": 122.00, "amount": 122.00, "ownerId": 63, "quantity": 1.000, "costPrice": 122.00, "productId": 6, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 2", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "103CC8ABMA00031", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huWyHl5kILiUnqU1=", "productTableId": 330}, {"id": 51, "price": 122.00, "amount": 122.00, "ownerId": 63, "quantity": 1.000, "costPrice": 122.00, "productId": 6, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 2", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "901ONUCCM100183", "markingNumber": "010478011969586621UhrC?(yoyw*FKRN*XD!I91pZNF92aRpYJTRlRjJaTFVUQkJ2eXZXcStVYSpnc3ZKSVl8wCc=", "productTableId": 331}, {"id": 52, "price": 122.00, "amount": 122.00, "ownerId": 63, "quantity": 1.000, "costPrice": 122.00, "productId": 6, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 2", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "321MHCACNC01023", "markingNumber": "010478011969586621UiDWeTQo6DHYfk-.7bS(91bxJH92p0ndulNPQlBvZHlNS1ItRElPNmFzbmpLUTlaaoDeL+Q=", "productTableId": 332}, {"id": 53, "price": 122.00, "amount": 122.00, "ownerId": 63, "quantity": 1.000, "costPrice": 122.00, "productId": 7, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 3", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "131GS3ABMC00569", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huWyHl5kILiUnqU=", "productTableId": 333}, {"id": 54, "price": 122.00, "amount": 122.00, "ownerId": 63, "quantity": 1.000, "costPrice": 122.00, "productId": 7, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 3", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "901ONUCCM100437", "markingNumber": "010478007420371721Hh=eSjFi6bZ8GHuur\\"gi91UZF092TzFFdfOAfdPwV/p68X3IFoiYBL9085NyxvvSEU5QGlY=", "productTableId": 334}], "comment": null, "docDate": "2026-06-22T16:58:50.411098", "stateId": 1, "statusId": 1, "docNumber": "100000061", "stateName": "Aktiv", "vatAmount": 0.00, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-22T16:58:50.411098", "finalAmount": 976.00, "totalAmount": 976.00, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "counterpartyId": 17, "organizationId": 8, "counterpartyName": "aaaa", "organizationName": "baraka_market"}	{"id": 63, "lines": [{"id": 47, "price": 134.20, "amount": 134.20, "ownerId": 63, "quantity": 1.000, "costPrice": 122.00, "productId": 4, "vatAmount": 16.10, "vatRateId": 2, "productName": "ArtelTestTovar", "totalAmount": 150.30, "vatRateName": "QQS 12%", "serialNumber": "103CC8ABN800189", "markingNumber": "010478011969800321Z6'WbR_4SBKQScZRZTjZ91UZF092NHdPZLJm0YcNie/mIPsicNrxcSHARvSblBOB1lrwuk0=", "productTableId": 327}, {"id": 48, "price": 134.20, "amount": 134.20, "ownerId": 63, "quantity": 1.000, "costPrice": 122.00, "productId": 4, "vatAmount": 16.10, "vatRateId": 2, "productName": "ArtelTestTovar", "totalAmount": 150.30, "vatRateName": "QQS 12%", "serialNumber": "131GNDABM800359", "markingNumber": "0104780074206893217UkCJ6Gu*gG_j.wf6WnX91XUWh92JkB/xWNpbjVhcmtkWVdsT3lhVTVhYjdUQm8yQgOW3lE=", "productTableId": 328}, {"id": 49, "price": 134.20, "amount": 134.20, "ownerId": 63, "quantity": 1.000, "costPrice": 122.00, "productId": 4, "vatAmount": 16.10, "vatRateId": 2, "productName": "ArtelTestTovar", "totalAmount": 150.30, "vatRateName": "QQS 12%", "serialNumber": "131GHFBUMA00234", "markingNumber": "010478011969586621B0kWxHgU+uvCk4JBnQCl91UZF092d0R1ZXXvRqo4NzIVvnorLIIDBXmBIpFADOELOkYy1nw=", "productTableId": 329}, {"id": 50, "price": 134.20, "amount": 134.20, "ownerId": 63, "quantity": 1.000, "costPrice": 122.00, "productId": 6, "vatAmount": 16.10, "vatRateId": 2, "productName": "Product 2", "totalAmount": 150.30, "vatRateName": "QQS 12%", "serialNumber": "103CC8ABMA00031", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huWyHl5kILiUnqU1=", "productTableId": 330}, {"id": 51, "price": 134.20, "amount": 134.20, "ownerId": 63, "quantity": 1.000, "costPrice": 122.00, "productId": 6, "vatAmount": 16.10, "vatRateId": 2, "productName": "Product 2", "totalAmount": 150.30, "vatRateName": "QQS 12%", "serialNumber": "901ONUCCM100183", "markingNumber": "010478011969586621UhrC?(yoyw*FKRN*XD!I91pZNF92aRpYJTRlRjJaTFVUQkJ2eXZXcStVYSpnc3ZKSVl8wCc=", "productTableId": 331}, {"id": 52, "price": 134.20, "amount": 134.20, "ownerId": 63, "quantity": 1.000, "costPrice": 122.00, "productId": 6, "vatAmount": 16.10, "vatRateId": 2, "productName": "Product 2", "totalAmount": 150.30, "vatRateName": "QQS 12%", "serialNumber": "321MHCACNC01023", "markingNumber": "010478011969586621UiDWeTQo6DHYfk-.7bS(91bxJH92p0ndulNPQlBvZHlNS1ItRElPNmFzbmpLUTlaaoDeL+Q=", "productTableId": 332}, {"id": 53, "price": 134.20, "amount": 134.20, "ownerId": 63, "quantity": 1.000, "costPrice": 122.00, "productId": 7, "vatAmount": 20.13, "vatRateId": 3, "productName": "Product 3", "totalAmount": 154.33, "vatRateName": "QQS 15%", "serialNumber": "131GS3ABMC00569", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huWyHl5kILiUnqU=", "productTableId": 333}, {"id": 54, "price": 134.20, "amount": 134.20, "ownerId": 63, "quantity": 1.000, "costPrice": 122.00, "productId": 7, "vatAmount": 20.13, "vatRateId": 3, "productName": "Product 3", "totalAmount": 154.33, "vatRateName": "QQS 15%", "serialNumber": "901ONUCCM100437", "markingNumber": "010478007420371721Hh=eSjFi6bZ8GHuur\\"gi91UZF092TzFFdfOAfdPwV/p68X3IFoiYBL9085NyxvvSEU5QGlY=", "productTableId": 334}], "comment": null, "docDate": "2026-06-22T16:58:50.411098", "stateId": 1, "statusId": 2, "docNumber": "100000061", "stateName": "Aktiv", "vatAmount": 136.86, "currencyId": 1, "statusName": "O'tkazilgan", "createdDate": "2026-06-22T16:58:50.411098", "finalAmount": 1210.46, "totalAmount": 1073.60, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "counterpartyId": 17, "organizationId": 8, "counterpartyName": "aaaa", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-22 17:08:33.829632
94	8	public	pur_doc	40	INSERT	\N	{"id": 40, "lines": [{"id": 219, "price": 122.00, "amount": 122.00, "ownerId": 40, "quantity": 1.000, "vatAmount": 0.00, "vatRateId": 1, "productName": "ArtelTestTovar", "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "103CC8ABN8001895", "markingNumber": "010478011969800321Z6'WbR_4SBKQScZRZTjZ91UZF092NHdPZLJm0YcNie/mIPsicNrxcSHARvSblBOB1lrwuk07=", "productTableId": 351}, {"id": 220, "price": 122.00, "amount": 122.00, "ownerId": 40, "quantity": 1.000, "vatAmount": 0.00, "vatRateId": 1, "productName": "ArtelTestTovar", "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "901ONUCCM1004375", "markingNumber": "010478007420371721Hh=eSjFi6bZ8GHuur\\"gi91UZF092TzFFdfOAfdPwV/p68X3IFoiYBL9085NyxvvSEU5QGlY7=", "productTableId": 352}, {"id": 221, "price": 122.00, "amount": 122.00, "ownerId": 40, "quantity": 1.000, "vatAmount": 0.00, "vatRateId": 1, "productName": "ArtelTestTovar", "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "901ONUCCM1001835", "markingNumber": "010478011969586621UhrC?(yoyw*FKRN*XD!I91pZNF92aRpYJTRlRjJaTFVUQkJ2eXZXcStVYSpnc3ZKSVl8wCc7=", "productTableId": 353}, {"id": 222, "price": 122.00, "amount": 122.00, "ownerId": 40, "quantity": 1.000, "vatAmount": 0.00, "vatRateId": 1, "productName": "Product 2", "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "103CC8ABMA000315", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huWyHl5kILiUnqU7=", "productTableId": 354}, {"id": 223, "price": 122.00, "amount": 122.00, "ownerId": 40, "quantity": 1.000, "vatAmount": 0.00, "vatRateId": 1, "productName": "Product 2", "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "321MHCACNC010235", "markingNumber": "010478011969586621UiDWeTQo6DHYfk-.7bS(91bxJH92p0ndulNPQlBvZHlNS1ItRElPNmFzbmpLUTlaaoDeL+Q7=", "productTableId": 355}, {"id": 224, "price": 122.00, "amount": 122.00, "ownerId": 40, "quantity": 1.000, "vatAmount": 0.00, "vatRateId": 1, "productName": "Product 2", "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "131GNDABM8003595", "markingNumber": "0104780074206893217UkCJ6Gu*gG_j.wf6WnX91XUWh92JkB/xWNpbjVhcmtkWVdsT3lhVTVhYjdUQm8yQgOW3lE7=", "productTableId": 356}, {"id": 225, "price": 122.00, "amount": 122.00, "ownerId": 40, "quantity": 1.000, "vatAmount": 0.00, "vatRateId": 1, "productName": "Product 3", "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "131GS3ABMC005695", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huWyHl5kILiUnqU77=", "productTableId": 357}, {"id": 226, "price": 122.00, "amount": 122.00, "ownerId": 40, "quantity": 1.000, "vatAmount": 0.00, "vatRateId": 1, "productName": "Product 3", "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "131GHFBUMA002345", "markingNumber": "010478011969586621B0kWxHgU+uvCk4JBnQCl91UZF092d0R1ZXXvRqo4NzIVvnorLIIDBXmBIpFADOELOkYy1nw7=", "productTableId": 358}], "comment": "", "docDate": "2026-06-22T17:12:06", "stateId": 1, "statusId": 1, "docNumber": "100000022", "stateName": "Aktiv", "vatAmount": 0.00, "contractId": 9, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-22T17:13:47.004791", "finalAmount": 976.00, "totalAmount": 976.00, "warehouseId": 7, "currencyName": "Uzbek so'm", "warehouseName": "amonov", "contractNumber": "100000009", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-22 17:13:47.494746
95	8	public	pur_doc	41	INSERT	\N	{"id": 41, "lines": [{"id": 227, "price": 122.00, "amount": 122.00, "ownerId": 41, "quantity": 1.000, "vatAmount": 0.00, "vatRateId": 1, "productName": "ArtelTestTovar", "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "103CC8ABN8001892V", "markingNumber": "010478011969800321Z6'WbR_4SBKQScZRZTjZ91UZF092NHdPZLJm0YcNie/mIPsicNrxcSHARvSblBXXOB1lrwuk01=", "productTableId": 371}, {"id": 228, "price": 122.00, "amount": 122.00, "ownerId": 41, "quantity": 1.000, "vatAmount": 0.00, "vatRateId": 1, "productName": "ArtelTestTovar", "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "901ONUCCM1004372V", "markingNumber": "010478007420371721Hh=eSjFi6bZ8GHuur\\"gi91UZF092TzFFdfOAfdPwV/p68X3IFoiYBL9085NyxvvSXXEU5QGlY1=", "productTableId": 372}, {"id": 229, "price": 122.00, "amount": 122.00, "ownerId": 41, "quantity": 1.000, "vatAmount": 0.00, "vatRateId": 1, "productName": "Product 2", "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "103CC8ABMA000312V", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huWyHXl5kILiUnqU12=", "productTableId": 373}, {"id": 230, "price": 122.00, "amount": 122.00, "ownerId": 41, "quantity": 1.000, "vatAmount": 0.00, "vatRateId": 1, "productName": "Product 3", "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "131GS3ABMC005692V", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huWyHl5kILiUnqU1=X", "productTableId": 374}], "comment": "", "docDate": "2026-06-22T18:47:37", "stateId": 1, "statusId": 1, "docNumber": "100000023", "stateName": "Aktiv", "vatAmount": 0.00, "contractId": 9, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-22T18:50:38.978396", "finalAmount": 488.00, "totalAmount": 488.00, "warehouseId": 7, "currencyName": "Uzbek so'm", "warehouseName": "amonov", "contractNumber": "100000009", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-22 18:50:39.083495
96	8	public	sale_doc	64	INSERT	\N	{"id": 64, "lines": [{"id": 55, "price": 122.00, "amount": 122.00, "ownerId": 64, "quantity": 1.000, "costPrice": 122.00, "productId": 4, "vatAmount": 0.00, "vatRateId": null, "productName": "ArtelTestTovar", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "103CC8ABN8001892V", "markingNumber": "010478011969800321Z6'WbR_4SBKQScZRZTjZ91UZF092NHdPZLJm0YcNie/mIPsicNrxcSHARvSblBXXOB1lrwuk01=", "productTableId": 371}, {"id": 56, "price": 122.00, "amount": 122.00, "ownerId": 64, "quantity": 1.000, "costPrice": 122.00, "productId": 4, "vatAmount": 0.00, "vatRateId": null, "productName": "ArtelTestTovar", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "901ONUCCM1004372V", "markingNumber": "010478007420371721Hh=eSjFi6bZ8GHuur\\"gi91UZF092TzFFdfOAfdPwV/p68X3IFoiYBL9085NyxvvSXXEU5QGlY1=", "productTableId": 372}, {"id": 57, "price": 122.00, "amount": 122.00, "ownerId": 64, "quantity": 1.000, "costPrice": 122.00, "productId": 7, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 3", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "131GS3ABMC005692V", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huWyHl5kILiUnqU1=X", "productTableId": 374}], "comment": "", "docDate": "2026-06-22T18:53:20.091602", "stateId": 1, "statusId": 1, "docNumber": "100000062", "stateName": "Aktiv", "vatAmount": 0.00, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-22T18:53:20.091602", "finalAmount": 366.00, "totalAmount": 366.00, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "counterpartyId": 19, "organizationId": 8, "counterpartyName": "Farrux Tech", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-22 18:53:20.235437
97	8	public	sale_doc	64	DELETE	{"id": 64, "lines": [{"id": 55, "price": 122.00, "amount": 122.00, "ownerId": 64, "quantity": 1.000, "costPrice": 122.00, "productId": 4, "vatAmount": 0.00, "vatRateId": null, "productName": "ArtelTestTovar", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "103CC8ABN8001892V", "markingNumber": "010478011969800321Z6'WbR_4SBKQScZRZTjZ91UZF092NHdPZLJm0YcNie/mIPsicNrxcSHARvSblBXXOB1lrwuk01=", "productTableId": 371}, {"id": 56, "price": 122.00, "amount": 122.00, "ownerId": 64, "quantity": 1.000, "costPrice": 122.00, "productId": 4, "vatAmount": 0.00, "vatRateId": null, "productName": "ArtelTestTovar", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "901ONUCCM1004372V", "markingNumber": "010478007420371721Hh=eSjFi6bZ8GHuur\\"gi91UZF092TzFFdfOAfdPwV/p68X3IFoiYBL9085NyxvvSXXEU5QGlY1=", "productTableId": 372}, {"id": 57, "price": 122.00, "amount": 122.00, "ownerId": 64, "quantity": 1.000, "costPrice": 122.00, "productId": 7, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 3", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "131GS3ABMC005692V", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huWyHl5kILiUnqU1=X", "productTableId": 374}], "comment": "", "docDate": "2026-06-22T18:53:20.091602", "stateId": 1, "statusId": 1, "docNumber": "100000062", "stateName": "Aktiv", "vatAmount": 0.00, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-22T18:53:20.091602", "finalAmount": 366.00, "totalAmount": 366.00, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "counterpartyId": 19, "organizationId": 8, "counterpartyName": "Farrux Tech", "organizationName": "baraka_market"}	{"id": 64, "lines": [], "comment": "", "docDate": "2026-06-22T18:53:20.091602", "stateId": 2, "statusId": 1, "docNumber": "100000062", "stateName": "Passiv", "vatAmount": 0.00, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-22T18:53:20.091602", "finalAmount": 366.00, "totalAmount": 366.00, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "counterpartyId": 19, "organizationId": 8, "counterpartyName": "Farrux Tech", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-22 23:27:42.662967
98	8	public	sale_doc	64	UPDATE	{"id": 64, "lines": [], "comment": "", "docDate": "2026-06-22T18:53:20.091602", "stateId": 2, "statusId": 1, "docNumber": "100000062", "stateName": "Passiv", "vatAmount": 0.00, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-22T18:53:20.091602", "finalAmount": 366.00, "totalAmount": 366.00, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "counterpartyId": 19, "organizationId": 8, "counterpartyName": "Farrux Tech", "organizationName": "baraka_market"}	{"id": 64, "lines": [], "comment": "", "docDate": "2026-06-22T18:53:20.091602", "stateId": 1, "statusId": 1, "docNumber": "100000062", "stateName": "Aktiv", "vatAmount": 0.00, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-22T18:53:20.091602", "finalAmount": 366.00, "totalAmount": 366.00, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "counterpartyId": 19, "organizationId": 8, "counterpartyName": "Farrux Tech", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-23 00:22:37.506655
99	8	public	sale_doc	64	UPDATE	{"id": 64, "lines": [], "comment": "", "docDate": "2026-06-22T18:53:20.091602", "stateId": 1, "statusId": 1, "docNumber": "100000062", "stateName": "Aktiv", "vatAmount": 0.00, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-22T18:53:20.091602", "finalAmount": 366.00, "totalAmount": 366.00, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "counterpartyId": 19, "organizationId": 8, "counterpartyName": "Farrux Tech", "organizationName": "baraka_market"}	{"id": 64, "lines": [], "comment": "", "docDate": "2026-06-22T18:53:20.091602", "stateId": 2, "statusId": 1, "docNumber": "100000062", "stateName": "Passiv", "vatAmount": 0.00, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-22T18:53:20.091602", "finalAmount": 366.00, "totalAmount": 366.00, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "counterpartyId": 19, "organizationId": 8, "counterpartyName": "Farrux Tech", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-23 00:23:11.10492
100	8	public	sale_doc	64	UPDATE	{"id": 64, "lines": [], "comment": "", "docDate": "2026-06-22T18:53:20.091602", "stateId": 2, "statusId": 1, "docNumber": "100000062", "stateName": "Passiv", "vatAmount": 0.00, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-22T18:53:20.091602", "finalAmount": 366.00, "totalAmount": 366.00, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "counterpartyId": 19, "organizationId": 8, "counterpartyName": "Farrux Tech", "organizationName": "baraka_market"}	{"id": 64, "lines": [], "comment": "", "docDate": "2026-06-22T18:53:20.091602", "stateId": 1, "statusId": 1, "docNumber": "100000062", "stateName": "Aktiv", "vatAmount": 0.00, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-22T18:53:20.091602", "finalAmount": 366.00, "totalAmount": 366.00, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "counterpartyId": 19, "organizationId": 8, "counterpartyName": "Farrux Tech", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-23 00:23:24.884806
101	8	public	sale_doc	64	UPDATE	{"id": 64, "lines": [], "comment": "", "docDate": "2026-06-22T18:53:20.091602", "stateId": 1, "statusId": 1, "docNumber": "100000062", "stateName": "Aktiv", "vatAmount": 0.00, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-22T18:53:20.091602", "finalAmount": 366.00, "totalAmount": 366.00, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "counterpartyId": 19, "organizationId": 8, "counterpartyName": "Farrux Tech", "organizationName": "baraka_market"}	{"id": 64, "lines": [], "comment": "", "docDate": "2026-06-22T18:53:20.091602", "stateId": 2, "statusId": 1, "docNumber": "100000062", "stateName": "Passiv", "vatAmount": 0.00, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-22T18:53:20.091602", "finalAmount": 366.00, "totalAmount": 366.00, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-23 00:25:59.753114
102	8	public	sale_doc	64	UPDATE	{"id": 64, "lines": [], "comment": "", "docDate": "2026-06-22T18:53:20.091602", "stateId": 2, "statusId": 1, "docNumber": "100000062", "stateName": "Passiv", "vatAmount": 0.00, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-22T18:53:20.091602", "finalAmount": 366.00, "totalAmount": 366.00, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	{"id": 64, "lines": [], "comment": "", "docDate": "2026-06-22T18:53:20.091602", "stateId": 1, "statusId": 1, "docNumber": "100000062", "stateName": "Aktiv", "vatAmount": 0.00, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-22T18:53:20.091602", "finalAmount": 366.00, "totalAmount": 366.00, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-23 00:27:03.218149
103	8	public	sale_doc	64	UPDATE	{"id": 64, "lines": [], "comment": "", "docDate": "2026-06-22T18:53:20.091602", "stateId": 1, "statusId": 1, "docNumber": "100000062", "stateName": "Aktiv", "vatAmount": 0.00, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-22T18:53:20.091602", "finalAmount": 366.00, "totalAmount": 366.00, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	{"id": 64, "lines": [], "comment": "", "docDate": "2026-06-22T18:53:20.091602", "stateId": 1, "statusId": 1, "docNumber": "100000062", "stateName": "Aktiv", "vatAmount": 0.00, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-22T18:53:20.091602", "finalAmount": 366.00, "totalAmount": 366.00, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "counterpartyId": 16, "organizationId": 8, "counterpartyName": "as", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-23 00:30:54.869001
104	8	public	sale_doc	64	UPDATE	{"id": 64, "lines": [], "comment": "", "docDate": "2026-06-22T18:53:20.091602", "stateId": 1, "statusId": 1, "docNumber": "100000062", "stateName": "Aktiv", "vatAmount": 0.00, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-22T18:53:20.091602", "finalAmount": 366.00, "totalAmount": 366.00, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "counterpartyId": 16, "organizationId": 8, "counterpartyName": "as", "organizationName": "baraka_market"}	{"id": 64, "lines": [], "comment": "", "docDate": "2026-06-22T18:53:20.091602", "stateId": 1, "statusId": 1, "docNumber": "100000062", "stateName": "Aktiv", "vatAmount": 0.00, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-22T18:53:20.091602", "finalAmount": 366.00, "totalAmount": 366.00, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "counterpartyId": 19, "organizationId": 8, "counterpartyName": "Farrux Tech", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-23 00:34:42.572355
105	8	public	sale_doc	64	UPDATE	{"id": 64, "lines": [], "comment": "", "docDate": "2026-06-22T18:53:20.091602", "stateId": 1, "statusId": 1, "docNumber": "100000062", "stateName": "Aktiv", "vatAmount": 0.00, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-22T18:53:20.091602", "finalAmount": 366.00, "totalAmount": 366.00, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "counterpartyId": 19, "organizationId": 8, "counterpartyName": "Farrux Tech", "organizationName": "baraka_market"}	{"id": 64, "lines": [], "comment": "", "docDate": "2026-06-22T18:53:20.091602", "stateId": 1, "statusId": 1, "docNumber": "100000062", "stateName": "Aktiv", "vatAmount": 0.00, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-22T18:53:20.091602", "finalAmount": 366.00, "totalAmount": 366.00, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-23 00:35:31.841435
111	8	public	sale_doc	66	INSERT	\N	{"id": 66, "lines": [{"id": 65, "price": 122.00, "amount": 122.00, "ownerId": 66, "quantity": 1.000, "costPrice": 122.00, "productId": 4, "vatAmount": 0.00, "vatRateId": null, "productName": "ArtelTestTovar", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "103CC8ABN1800189A", "markingNumber": "010478011969800321Z6'WbR_4SBKQScZRZTjZ91UZF092NHdPZLJm0YcNie/mIPsicNrxcSHARq2wvSblBOB1lrwuk0=", "productTableId": 381}, {"id": 66, "price": 122.00, "amount": 122.00, "ownerId": 66, "quantity": 1.000, "costPrice": 122.00, "productId": 6, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 2", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "103CC8ABMA100031A", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huW2qwyHl5kILiUnqU=", "productTableId": 382}, {"id": 67, "price": 122.00, "amount": 122.00, "ownerId": 66, "quantity": 1.000, "costPrice": 122.00, "productId": 7, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 3", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "131GS3ABMC100569A", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huW22qwyHl5kILiUnqU=", "productTableId": 383}, {"id": 68, "price": 122.00, "amount": 122.00, "ownerId": 66, "quantity": 1.000, "costPrice": 122.00, "productId": 7, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 3", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "901ONUCCM1100437A", "markingNumber": "010478007420371721Hh=eSjFi6bZ8GHuur\\"gi91UZF092TzFFdfOAfdPwV/qw=", "productTableId": 384}, {"id": 69, "price": 122.00, "amount": 122.00, "ownerId": 66, "quantity": 1.000, "costPrice": 122.00, "productId": 7, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 3", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "901ONUCCM100183A1", "markingNumber": "010478011969586621UhrC?(yoyw*FKRN*XD!I91pZNF92aRpYJTRlRjJaTFVUQkJ2eXZXcStVYSpncqw3ZKSVl8wCc=", "productTableId": 385}, {"id": 70, "price": 122.00, "amount": 122.00, "ownerId": 66, "quantity": 1.000, "costPrice": 122.00, "productId": 7, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 3", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "321MHCACN1C01023A", "markingNumber": "010478011969586621UiDWeTQo6DHYfk-.7bS(91bxJH92p0ndulNPQlBvZHlNS1ItRElPNmFzbmpLUqwTlaaoDeL+Q=", "productTableId": 386}, {"id": 71, "price": 122.00, "amount": 122.00, "ownerId": 66, "quantity": 1.000, "costPrice": 122.00, "productId": 7, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 3", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "131GNDABM1800359A", "markingNumber": "0104780074206893217UkCJ6Gu*gG_j.wf6WnX91XUWh92JkB/xWNpbjVhcmtkWVdsT3lhVTVhYjdUqwQm8yQgOW3lE=", "productTableId": 387}, {"id": 72, "price": 122.00, "amount": 122.00, "ownerId": 66, "quantity": 1.000, "costPrice": 122.00, "productId": 7, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 3", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "131GHFBU1MA00234A", "markingNumber": "010478011969586621B0kWxHgU+uvCk4JBnQCl91UZF092d0R1ZXXvRqo4NzIVvnorLIIDBXmBIpFADqwOELOkYy1nw=", "productTableId": 388}], "comment": "", "docDate": "2026-06-23T08:32:30.007057", "stateId": 1, "statusId": 1, "docNumber": "100000064", "stateName": "Aktiv", "vatAmount": 0.00, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-23T08:32:30.007057", "finalAmount": 976.00, "totalAmount": 976.00, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "counterpartyId": 16, "organizationId": 8, "counterpartyName": "as", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-23 08:32:30.054215
106	8	public	pur_doc	41	DELETE	{"id": 41, "lines": [{"id": 227, "price": 122.00, "amount": 122.00, "ownerId": 41, "quantity": 1.000, "vatAmount": 0.00, "vatRateId": 1, "productName": "ArtelTestTovar", "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "103CC8ABN8001892V", "markingNumber": "010478011969800321Z6'WbR_4SBKQScZRZTjZ91UZF092NHdPZLJm0YcNie/mIPsicNrxcSHARvSblBXXOB1lrwuk01=", "productTableId": 371}, {"id": 228, "price": 122.00, "amount": 122.00, "ownerId": 41, "quantity": 1.000, "vatAmount": 0.00, "vatRateId": 1, "productName": "ArtelTestTovar", "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "901ONUCCM1004372V", "markingNumber": "010478007420371721Hh=eSjFi6bZ8GHuur\\"gi91UZF092TzFFdfOAfdPwV/p68X3IFoiYBL9085NyxvvSXXEU5QGlY1=", "productTableId": 372}, {"id": 229, "price": 122.00, "amount": 122.00, "ownerId": 41, "quantity": 1.000, "vatAmount": 0.00, "vatRateId": 1, "productName": "Product 2", "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "103CC8ABMA000312V", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huWyHXl5kILiUnqU12=", "productTableId": 373}, {"id": 230, "price": 122.00, "amount": 122.00, "ownerId": 41, "quantity": 1.000, "vatAmount": 0.00, "vatRateId": 1, "productName": "Product 3", "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "131GS3ABMC005692V", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huWyHl5kILiUnqU1=X", "productTableId": 374}], "comment": "", "docDate": "2026-06-22T18:47:37", "stateId": 1, "statusId": 1, "docNumber": "100000023", "stateName": "Aktiv", "vatAmount": 0.00, "contractId": 9, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-22T18:50:38.978396", "finalAmount": 488.00, "totalAmount": 488.00, "warehouseId": 7, "currencyName": "Uzbek so'm", "warehouseName": "amonov", "contractNumber": "100000009", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	{"id": 41, "lines": [], "comment": "", "docDate": "2026-06-22T18:47:37", "stateId": 2, "statusId": 1, "docNumber": "100000023", "stateName": "Passiv", "vatAmount": 0.00, "contractId": 9, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-22T18:50:38.978396", "finalAmount": 488.00, "totalAmount": 488.00, "warehouseId": 7, "currencyName": "Uzbek so'm", "warehouseName": "amonov", "contractNumber": "100000009", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-23 00:36:10.593159
107	8	public	pur_doc	42	INSERT	\N	{"id": 42, "lines": [{"id": 231, "price": 122.00, "amount": 122.00, "ownerId": 42, "quantity": 1.000, "vatAmount": 0.00, "vatRateId": 1, "productName": "ArtelTestTovar", "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "103CC8ABN1800189A", "markingNumber": "010478011969800321Z6'WbR_4SBKQScZRZTjZ91UZF092NHdPZLJm0YcNie/mIPsicNrxcSHARq2wvSblBOB1lrwuk0=", "productTableId": 381}, {"id": 232, "price": 122.00, "amount": 122.00, "ownerId": 42, "quantity": 1.000, "vatAmount": 0.00, "vatRateId": 1, "productName": "Product 2", "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "103CC8ABMA100031A", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huW2qwyHl5kILiUnqU=", "productTableId": 382}, {"id": 233, "price": 122.00, "amount": 122.00, "ownerId": 42, "quantity": 1.000, "vatAmount": 0.00, "vatRateId": 1, "productName": "Product 3", "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "131GS3ABMC100569A", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huW22qwyHl5kILiUnqU=", "productTableId": 383}, {"id": 234, "price": 122.00, "amount": 122.00, "ownerId": 42, "quantity": 1.000, "vatAmount": 0.00, "vatRateId": 1, "productName": "Product 3", "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "901ONUCCM1100437A", "markingNumber": "010478007420371721Hh=eSjFi6bZ8GHuur\\"gi91UZF092TzFFdfOAfdPwV/qw=", "productTableId": 384}, {"id": 235, "price": 122.00, "amount": 122.00, "ownerId": 42, "quantity": 1.000, "vatAmount": 0.00, "vatRateId": 1, "productName": "Product 3", "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "901ONUCCM100183A1", "markingNumber": "010478011969586621UhrC?(yoyw*FKRN*XD!I91pZNF92aRpYJTRlRjJaTFVUQkJ2eXZXcStVYSpncqw3ZKSVl8wCc=", "productTableId": 385}, {"id": 236, "price": 122.00, "amount": 122.00, "ownerId": 42, "quantity": 1.000, "vatAmount": 0.00, "vatRateId": 1, "productName": "Product 3", "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "321MHCACN1C01023A", "markingNumber": "010478011969586621UiDWeTQo6DHYfk-.7bS(91bxJH92p0ndulNPQlBvZHlNS1ItRElPNmFzbmpLUqwTlaaoDeL+Q=", "productTableId": 386}, {"id": 237, "price": 122.00, "amount": 122.00, "ownerId": 42, "quantity": 1.000, "vatAmount": 0.00, "vatRateId": 1, "productName": "Product 3", "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "131GNDABM1800359A", "markingNumber": "0104780074206893217UkCJ6Gu*gG_j.wf6WnX91XUWh92JkB/xWNpbjVhcmtkWVdsT3lhVTVhYjdUqwQm8yQgOW3lE=", "productTableId": 387}, {"id": 238, "price": 122.00, "amount": 122.00, "ownerId": 42, "quantity": 1.000, "vatAmount": 0.00, "vatRateId": 1, "productName": "Product 3", "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "131GHFBU1MA00234A", "markingNumber": "010478011969586621B0kWxHgU+uvCk4JBnQCl91UZF092d0R1ZXXvRqo4NzIVvnorLIIDBXmBIpFADqwOELOkYy1nw=", "productTableId": 388}], "comment": "", "docDate": "2026-06-23T00:36:13", "stateId": 1, "statusId": 1, "docNumber": "100000024", "stateName": "Aktiv", "vatAmount": 0.00, "contractId": 9, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-23T00:38:09.219781", "finalAmount": 976.00, "totalAmount": 976.00, "warehouseId": 7, "currencyName": "Uzbek so'm", "warehouseName": "amonov", "contractNumber": "100000009", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-23 00:38:09.292429
108	8	public	pur_doc	43	INSERT	\N	{"id": 43, "lines": [{"id": 239, "price": 122.00, "amount": 122.00, "ownerId": 43, "quantity": 1.000, "vatAmount": 0.00, "vatRateId": 1, "productName": "ArtelTestTovar", "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "103CAC8ABN800189", "markingNumber": "010478011969800321Z6'WbR_4SBKQScZRZTjZ91UZF092NHdPZLJm0YcNie/mIPsicNrxcSHARvASblBOB1lrwuk0=", "productTableId": 389}, {"id": 240, "price": 122.00, "amount": 122.00, "ownerId": 43, "quantity": 1.000, "vatAmount": 0.00, "vatRateId": 1, "productName": "ArtelTestTovar", "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "131GHFBAUMA00234", "markingNumber": "010478011969586621B0kWxHgU+uvCk4JBnQCl91UZF092d0R1ZXXvRqo4NzIVvnorLIIDBXmBIpFAADOELOkYy1nw=", "productTableId": 390}, {"id": 241, "price": 122.00, "amount": 122.00, "ownerId": 43, "quantity": 1.000, "vatAmount": 0.00, "vatRateId": 1, "productName": "Product 2", "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "103CCA8ABMA00031", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huWAWyHl5kILiUnqU=", "productTableId": 391}, {"id": 242, "price": 122.00, "amount": 122.00, "ownerId": 43, "quantity": 1.000, "vatAmount": 0.00, "vatRateId": 1, "productName": "Product 2", "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "321MHCAACNC01023", "markingNumber": "010478011969586621UiDWeTQo6DHYfk-.7bS(91bxJH92p0ndulNPQlBvZHlNS1ItRElPNmFzbmpALUTlaaoDeL+Q=", "productTableId": 392}, {"id": 243, "price": 122.00, "amount": 122.00, "ownerId": 43, "quantity": 1.000, "vatAmount": 0.00, "vatRateId": 1, "productName": "Product 2", "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "131GNDAABM800359", "markingNumber": "0104780074206893217UkCJ6Gu*gG_j.wf6WnX91XUWh92JkB/xWNpbjVhcmtkWVdsT3lhVTVhYjdUQAm8yQgOW3lE=", "productTableId": 393}, {"id": 244, "price": 122.00, "amount": 122.00, "ownerId": 43, "quantity": 1.000, "vatAmount": 0.00, "vatRateId": 1, "productName": "Product 3", "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "131GS3AABMC00569", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huAWyHl5kILiUnqU=", "productTableId": 394}, {"id": 245, "price": 122.00, "amount": 122.00, "ownerId": 43, "quantity": 1.000, "vatAmount": 0.00, "vatRateId": 1, "productName": "Product 3", "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "901ONUCACM100437", "markingNumber": "010478007420371721Hh=eSjFi6bZ8GHuur\\"gi91UZF092TzFFdfOAfdPwV/p68X3IFoiYBL9085NAyxvvSEU5QGlY=", "productTableId": 395}, {"id": 246, "price": 122.00, "amount": 122.00, "ownerId": 43, "quantity": 1.000, "vatAmount": 0.00, "vatRateId": 1, "productName": "Product 3", "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "901ONUACCM100183", "markingNumber": "010478011969586621UhrC?(yoyw*FKRN*XD!I91pZNF92aRpYJTRlRjJaTFVUQkJ2eXZXcStVYSpnAc3ZKSVl8wCc=", "productTableId": 396}], "comment": "", "docDate": "2026-06-23T06:03:18", "stateId": 1, "statusId": 1, "docNumber": "100000025", "stateName": "Aktiv", "vatAmount": 0.00, "contractId": 9, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-23T06:05:20.252059", "finalAmount": 976.00, "totalAmount": 976.00, "warehouseId": 7, "currencyName": "Uzbek so'm", "warehouseName": "amonov", "contractNumber": "100000009", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-23 06:05:20.333237
109	8	public	sale_doc	65	INSERT	\N	{"id": 65, "lines": [{"id": 58, "price": 122.00, "amount": 122.00, "ownerId": 65, "quantity": 1.000, "costPrice": 122.00, "productId": 4, "vatAmount": 0.00, "vatRateId": null, "productName": "ArtelTestTovar", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "103CAC8ABN800189", "markingNumber": "010478011969800321Z6'WbR_4SBKQScZRZTjZ91UZF092NHdPZLJm0YcNie/mIPsicNrxcSHARvASblBOB1lrwuk0=", "productTableId": 389}, {"id": 59, "price": 122.00, "amount": 122.00, "ownerId": 65, "quantity": 1.000, "costPrice": 122.00, "productId": 7, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 3", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "901ONUACCM100183", "markingNumber": "010478011969586621UhrC?(yoyw*FKRN*XD!I91pZNF92aRpYJTRlRjJaTFVUQkJ2eXZXcStVYSpnAc3ZKSVl8wCc=", "productTableId": 396}, {"id": 60, "price": 122.00, "amount": 122.00, "ownerId": 65, "quantity": 1.000, "costPrice": 122.00, "productId": 7, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 3", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "901ONUCACM100437", "markingNumber": "010478007420371721Hh=eSjFi6bZ8GHuur\\"gi91UZF092TzFFdfOAfdPwV/p68X3IFoiYBL9085NAyxvvSEU5QGlY=", "productTableId": 395}, {"id": 61, "price": 122.00, "amount": 122.00, "ownerId": 65, "quantity": 1.000, "costPrice": 122.00, "productId": 7, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 3", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "131GS3AABMC00569", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huAWyHl5kILiUnqU=", "productTableId": 394}, {"id": 62, "price": 122.00, "amount": 122.00, "ownerId": 65, "quantity": 1.000, "costPrice": 122.00, "productId": 6, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 2", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "321MHCAACNC01023", "markingNumber": "010478011969586621UiDWeTQo6DHYfk-.7bS(91bxJH92p0ndulNPQlBvZHlNS1ItRElPNmFzbmpALUTlaaoDeL+Q=", "productTableId": 392}, {"id": 63, "price": 122.00, "amount": 122.00, "ownerId": 65, "quantity": 1.000, "costPrice": 122.00, "productId": 4, "vatAmount": 0.00, "vatRateId": null, "productName": "ArtelTestTovar", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "131GHFBAUMA00234", "markingNumber": "010478011969586621B0kWxHgU+uvCk4JBnQCl91UZF092d0R1ZXXvRqo4NzIVvnorLIIDBXmBIpFAADOELOkYy1nw=", "productTableId": 390}, {"id": 64, "price": 122.00, "amount": 122.00, "ownerId": 65, "quantity": 1.000, "costPrice": 122.00, "productId": 6, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 2", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "103CCA8ABMA00031", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huWAWyHl5kILiUnqU=", "productTableId": 391}], "comment": "", "docDate": "2026-06-23T07:46:01.352582", "stateId": 1, "statusId": 1, "docNumber": "100000063", "stateName": "Aktiv", "vatAmount": 0.00, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-23T07:46:01.352582", "finalAmount": 854.00, "totalAmount": 854.00, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-23 07:46:01.402623
110	8	public	sale_doc	65	UPDATE	{"id": 65, "lines": [{"id": 58, "price": 122.00, "amount": 122.00, "ownerId": 65, "quantity": 1.000, "costPrice": 122.00, "productId": 4, "vatAmount": 0.00, "vatRateId": null, "productName": "ArtelTestTovar", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "103CAC8ABN800189", "markingNumber": "010478011969800321Z6'WbR_4SBKQScZRZTjZ91UZF092NHdPZLJm0YcNie/mIPsicNrxcSHARvASblBOB1lrwuk0=", "productTableId": 389}, {"id": 59, "price": 122.00, "amount": 122.00, "ownerId": 65, "quantity": 1.000, "costPrice": 122.00, "productId": 7, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 3", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "901ONUACCM100183", "markingNumber": "010478011969586621UhrC?(yoyw*FKRN*XD!I91pZNF92aRpYJTRlRjJaTFVUQkJ2eXZXcStVYSpnAc3ZKSVl8wCc=", "productTableId": 396}, {"id": 60, "price": 122.00, "amount": 122.00, "ownerId": 65, "quantity": 1.000, "costPrice": 122.00, "productId": 7, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 3", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "901ONUCACM100437", "markingNumber": "010478007420371721Hh=eSjFi6bZ8GHuur\\"gi91UZF092TzFFdfOAfdPwV/p68X3IFoiYBL9085NAyxvvSEU5QGlY=", "productTableId": 395}, {"id": 61, "price": 122.00, "amount": 122.00, "ownerId": 65, "quantity": 1.000, "costPrice": 122.00, "productId": 7, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 3", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "131GS3AABMC00569", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huAWyHl5kILiUnqU=", "productTableId": 394}, {"id": 62, "price": 122.00, "amount": 122.00, "ownerId": 65, "quantity": 1.000, "costPrice": 122.00, "productId": 6, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 2", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "321MHCAACNC01023", "markingNumber": "010478011969586621UiDWeTQo6DHYfk-.7bS(91bxJH92p0ndulNPQlBvZHlNS1ItRElPNmFzbmpALUTlaaoDeL+Q=", "productTableId": 392}, {"id": 63, "price": 122.00, "amount": 122.00, "ownerId": 65, "quantity": 1.000, "costPrice": 122.00, "productId": 4, "vatAmount": 0.00, "vatRateId": null, "productName": "ArtelTestTovar", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "131GHFBAUMA00234", "markingNumber": "010478011969586621B0kWxHgU+uvCk4JBnQCl91UZF092d0R1ZXXvRqo4NzIVvnorLIIDBXmBIpFAADOELOkYy1nw=", "productTableId": 390}, {"id": 64, "price": 122.00, "amount": 122.00, "ownerId": 65, "quantity": 1.000, "costPrice": 122.00, "productId": 6, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 2", "totalAmount": 122.00, "vatRateName": null, "serialNumber": "103CCA8ABMA00031", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huWAWyHl5kILiUnqU=", "productTableId": 391}], "comment": "", "docDate": "2026-06-23T07:46:01.352582", "stateId": 1, "statusId": 1, "docNumber": "100000063", "stateName": "Aktiv", "vatAmount": 0.00, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-23T07:46:01.352582", "finalAmount": 854.00, "totalAmount": 854.00, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	{"id": 65, "lines": [{"id": 58, "price": 145.00, "amount": 145.00, "ownerId": 65, "quantity": 1.000, "costPrice": 122.00, "productId": 4, "vatAmount": 17.40, "vatRateId": 2, "productName": "ArtelTestTovar", "totalAmount": 162.40, "vatRateName": "QQS 12%", "serialNumber": "103CAC8ABN800189", "markingNumber": "010478011969800321Z6'WbR_4SBKQScZRZTjZ91UZF092NHdPZLJm0YcNie/mIPsicNrxcSHARvASblBOB1lrwuk0=", "productTableId": 389}, {"id": 59, "price": 134.20, "amount": 134.20, "ownerId": 65, "quantity": 1.000, "costPrice": 122.00, "productId": 7, "vatAmount": 16.10, "vatRateId": 2, "productName": "Product 3", "totalAmount": 150.30, "vatRateName": "QQS 12%", "serialNumber": "901ONUACCM100183", "markingNumber": "010478011969586621UhrC?(yoyw*FKRN*XD!I91pZNF92aRpYJTRlRjJaTFVUQkJ2eXZXcStVYSpnAc3ZKSVl8wCc=", "productTableId": 396}, {"id": 60, "price": 134.20, "amount": 134.20, "ownerId": 65, "quantity": 1.000, "costPrice": 122.00, "productId": 7, "vatAmount": 16.10, "vatRateId": 2, "productName": "Product 3", "totalAmount": 150.30, "vatRateName": "QQS 12%", "serialNumber": "901ONUCACM100437", "markingNumber": "010478007420371721Hh=eSjFi6bZ8GHuur\\"gi91UZF092TzFFdfOAfdPwV/p68X3IFoiYBL9085NAyxvvSEU5QGlY=", "productTableId": 395}, {"id": 61, "price": 134.20, "amount": 134.20, "ownerId": 65, "quantity": 1.000, "costPrice": 122.00, "productId": 7, "vatAmount": 16.10, "vatRateId": 2, "productName": "Product 3", "totalAmount": 150.30, "vatRateName": "QQS 12%", "serialNumber": "131GS3AABMC00569", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huAWyHl5kILiUnqU=", "productTableId": 394}, {"id": 62, "price": 136.64, "amount": 136.64, "ownerId": 65, "quantity": 1.000, "costPrice": 122.00, "productId": 6, "vatAmount": 16.40, "vatRateId": 2, "productName": "Product 2", "totalAmount": 153.04, "vatRateName": "QQS 12%", "serialNumber": "321MHCAACNC01023", "markingNumber": "010478011969586621UiDWeTQo6DHYfk-.7bS(91bxJH92p0ndulNPQlBvZHlNS1ItRElPNmFzbmpALUTlaaoDeL+Q=", "productTableId": 392}, {"id": 63, "price": 145.00, "amount": 145.00, "ownerId": 65, "quantity": 1.000, "costPrice": 122.00, "productId": 4, "vatAmount": 17.40, "vatRateId": 2, "productName": "ArtelTestTovar", "totalAmount": 162.40, "vatRateName": "QQS 12%", "serialNumber": "131GHFBAUMA00234", "markingNumber": "010478011969586621B0kWxHgU+uvCk4JBnQCl91UZF092d0R1ZXXvRqo4NzIVvnorLIIDBXmBIpFAADOELOkYy1nw=", "productTableId": 390}, {"id": 64, "price": 136.64, "amount": 136.64, "ownerId": 65, "quantity": 1.000, "costPrice": 122.00, "productId": 6, "vatAmount": 16.40, "vatRateId": 2, "productName": "Product 2", "totalAmount": 153.04, "vatRateName": "QQS 12%", "serialNumber": "103CCA8ABMA00031", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huWAWyHl5kILiUnqU=", "productTableId": 391}], "comment": "", "docDate": "2026-06-23T07:46:01.352582", "stateId": 1, "statusId": 2, "docNumber": "100000063", "stateName": "Aktiv", "vatAmount": 115.90, "currencyId": 1, "statusName": "O'tkazilgan", "createdDate": "2026-06-23T07:46:01.352582", "finalAmount": 1081.78, "totalAmount": 965.88, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-23 08:30:43.590201
112	8	public	sale_doc	70	INSERT	\N	{"id": 70, "comment": null, "docDate": "2026-06-23T17:30:20.364066", "stateId": 1, "products": [{"id": 10, "amount": 0.00, "tables": [], "quantity": 3.000, "costPrice": 0.00, "productId": 6, "unitPrice": 0.00, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 2", "totalAmount": 0.00, "vatRateName": null}, {"id": 11, "amount": 0.00, "tables": [], "quantity": 2.000, "costPrice": 0.00, "productId": 7, "unitPrice": 0.00, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 3", "totalAmount": 0.00, "vatRateName": null}], "statusId": 1, "docNumber": "100000068", "stateName": "Aktiv", "vatAmount": 0.00, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-23T17:30:20.364066", "finalAmount": 0.00, "totalAmount": 0.00, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-23 17:30:21.270816
113	8	public	sale_doc	70	UPDATE	\N	{"id": 70, "comment": null, "docDate": "2026-06-23T17:30:20.364066", "stateId": 1, "products": [{"id": 10, "amount": 0.00, "tables": [{"id": 73, "amount": 0.00, "costPrice": 122.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 0.00, "serialNumber": "103CC8ABMA000315", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huWyHl5kILiUnqU7=", "productTableId": 354}, {"id": 74, "amount": 0.00, "costPrice": 0.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 0.00, "serialNumber": "103CC8ABMA000312V", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huWyHXl5kILiUnqU12=", "productTableId": 373}, {"id": 75, "amount": 0.00, "costPrice": 122.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 0.00, "serialNumber": "131GNDAABM800359", "markingNumber": "0104780074206893217UkCJ6Gu*gG_j.wf6WnX91XUWh92JkB/xWNpbjVhcmtkWVdsT3lhVTVhYjdUQAm8yQgOW3lE=", "productTableId": 393}], "quantity": 3.000, "costPrice": 244.00, "productId": 6, "unitPrice": 0.00, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 2", "totalAmount": 0.00, "vatRateName": null}, {"id": 11, "amount": 0.00, "tables": [{"id": 76, "amount": 0.00, "costPrice": 122.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 0.00, "serialNumber": "131GHFBUMA002345", "markingNumber": "010478011969586621B0kWxHgU+uvCk4JBnQCl91UZF092d0R1ZXXvRqo4NzIVvnorLIIDBXmBIpFADOELOkYy1nw7=", "productTableId": 358}, {"id": 77, "amount": 0.00, "costPrice": 0.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 0.00, "serialNumber": "131GS3ABMC005692V", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huWyHl5kILiUnqU1=X", "productTableId": 374}], "quantity": 2.000, "costPrice": 122.00, "productId": 7, "unitPrice": 0.00, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 3", "totalAmount": 0.00, "vatRateName": null}], "statusId": 4, "docNumber": "100000068", "stateName": "Aktiv", "vatAmount": 0.00, "currencyId": 1, "statusName": "Kutilmoqda", "createdDate": "2026-06-23T17:30:20.364066", "finalAmount": 0.00, "totalAmount": 0.00, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-23 18:21:41.702808
114	\N	public	sale_doc	70	UPDATE	{"id": 70, "comment": null, "docDate": "2026-06-23T17:30:20.364066", "stateId": 1, "products": [{"id": 10, "amount": 0.00, "tables": [{"id": 73, "amount": 0.00, "costPrice": 122.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 0.00, "serialNumber": "103CC8ABMA000315", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huWyHl5kILiUnqU7=", "productTableId": 354}, {"id": 74, "amount": 0.00, "costPrice": 0.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 0.00, "serialNumber": "103CC8ABMA000312V", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huWyHXl5kILiUnqU12=", "productTableId": 373}, {"id": 75, "amount": 0.00, "costPrice": 122.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 0.00, "serialNumber": "131GNDAABM800359", "markingNumber": "0104780074206893217UkCJ6Gu*gG_j.wf6WnX91XUWh92JkB/xWNpbjVhcmtkWVdsT3lhVTVhYjdUQAm8yQgOW3lE=", "productTableId": 393}], "quantity": 3.000, "costPrice": 244.00, "productId": 6, "unitPrice": 0.00, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 2", "totalAmount": 0.00, "vatRateName": null}, {"id": 11, "amount": 0.00, "tables": [{"id": 76, "amount": 0.00, "costPrice": 122.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 0.00, "serialNumber": "131GHFBUMA002345", "markingNumber": "010478011969586621B0kWxHgU+uvCk4JBnQCl91UZF092d0R1ZXXvRqo4NzIVvnorLIIDBXmBIpFADOELOkYy1nw7=", "productTableId": 358}, {"id": 77, "amount": 0.00, "costPrice": 0.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 0.00, "serialNumber": "131GS3ABMC005692V", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huWyHl5kILiUnqU1=X", "productTableId": 374}], "quantity": 2.000, "costPrice": 122.00, "productId": 7, "unitPrice": 0.00, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 3", "totalAmount": 0.00, "vatRateName": null}], "statusId": 4, "docNumber": "100000068", "stateName": "Aktiv", "vatAmount": 0.00, "currencyId": 1, "statusName": "Kutilmoqda", "createdDate": "2026-06-23T17:30:20.364066", "finalAmount": 0.00, "totalAmount": 0.00, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	{"id": 70, "comment": null, "docDate": "2026-06-23T17:30:20.364066", "stateId": 1, "products": [{"id": 10, "amount": 0.00, "tables": [{"id": 73, "amount": 0.00, "costPrice": 122.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 0.00, "serialNumber": "103CC8ABMA000315", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huWyHl5kILiUnqU7=", "productTableId": 354}, {"id": 74, "amount": 0.00, "costPrice": 0.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 0.00, "serialNumber": "103CC8ABMA000312V", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huWyHXl5kILiUnqU12=", "productTableId": 373}, {"id": 75, "amount": 0.00, "costPrice": 122.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 0.00, "serialNumber": "131GNDAABM800359", "markingNumber": "0104780074206893217UkCJ6Gu*gG_j.wf6WnX91XUWh92JkB/xWNpbjVhcmtkWVdsT3lhVTVhYjdUQAm8yQgOW3lE=", "productTableId": 393}], "quantity": 3.000, "costPrice": 244.00, "productId": 6, "unitPrice": 0.00, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 2", "totalAmount": 0.00, "vatRateName": null}, {"id": 11, "amount": 0.00, "tables": [{"id": 76, "amount": 0.00, "costPrice": 122.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 0.00, "serialNumber": "131GHFBUMA002345", "markingNumber": "010478011969586621B0kWxHgU+uvCk4JBnQCl91UZF092d0R1ZXXvRqo4NzIVvnorLIIDBXmBIpFADOELOkYy1nw7=", "productTableId": 358}, {"id": 77, "amount": 0.00, "costPrice": 0.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 0.00, "serialNumber": "131GS3ABMC005692V", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huWyHl5kILiUnqU1=X", "productTableId": 374}], "quantity": 2.000, "costPrice": 122.00, "productId": 7, "unitPrice": 0.00, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 3", "totalAmount": 0.00, "vatRateName": null}], "statusId": 3, "docNumber": "100000068", "stateName": "Aktiv", "vatAmount": 0.00, "currencyId": 1, "statusName": "Bekor qilingan", "createdDate": "2026-06-23T17:30:20.364066", "finalAmount": 0.00, "totalAmount": 0.00, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-23 18:56:05.732704
115	8	public	pur_doc	44	INSERT	\N	{"id": 44, "lines": [], "comment": null, "docDate": "2026-06-24T06:49:02", "stateId": 1, "statusId": 1, "docNumber": "100000026", "stateName": "Aktiv", "vatAmount": 0.00, "contractId": null, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-24T06:50:46.533547", "finalAmount": 132500.00, "totalAmount": 132500.00, "warehouseId": 7, "currencyName": "Uzbek so'm", "serviceLines": [{"id": 247, "price": 132500.00, "amount": 132500.00, "ownerId": 44, "quantity": 1.000, "serviceId": null, "vatAmount": 0.00, "vatRateId": null, "itemTypeId": 2, "productName": null, "serviceName": "royal", "totalAmount": 132500.00, "vatRateName": null, "serialNumber": null, "markingNumber": null, "productTableId": null, "expenseAccountId": 644, "expenseAccountName": "Оборудование к установке"}], "warehouseName": "amonov", "contractNumber": null, "counterpartyId": 17, "organizationId": 8, "counterpartyName": "aaaa", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-24 06:50:47.017784
116	8	public	pur_doc	45	INSERT	\N	{"id": 45, "lines": [{"id": 248, "price": 122.00, "amount": 122.00, "ownerId": 45, "quantity": 1.000, "serviceId": null, "vatAmount": 0.00, "vatRateId": 1, "itemTypeId": 1, "productName": "ArtelTestTovar", "serviceName": null, "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "103CC8ABNq800189", "markingNumber": "010478011969800321Z6'WbR_4SBKQScZRZTjZ91UZF092NHdPZLJm0YcNie/mIPsicNrxcSHARvS12blBOB1lrwuk0=", "productTableId": 398, "expenseAccountId": null, "expenseAccountName": null}, {"id": 249, "price": 122.00, "amount": 122.00, "ownerId": 45, "quantity": 1.000, "serviceId": null, "vatAmount": 0.00, "vatRateId": 1, "itemTypeId": 1, "productName": "ArtelTestTovar", "serviceName": null, "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "901ONUCCqM100437", "markingNumber": "010478007420371721Hh=eSjFi6bZ8GHuur\\"gi91UZF092TzFFdfOAfdPwV/p68X3IFoiYBL9085NyxvvSEU5QGlY=12", "productTableId": 399, "expenseAccountId": null, "expenseAccountName": null}, {"id": 250, "price": 122.00, "amount": 122.00, "ownerId": 45, "quantity": 1.000, "serviceId": null, "vatAmount": 0.00, "vatRateId": 1, "itemTypeId": 1, "productName": "ArtelTestTovar", "serviceName": null, "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "901ONUCCqM100183", "markingNumber": "010478011969586621UhrC?(yoyw*FKRN*XD!I91pZNF92aRpYJTRlRjJaTFVUQkJ2eXZXcStVYSp12nc3ZKSVl8wCc=", "productTableId": 400, "expenseAccountId": null, "expenseAccountName": null}, {"id": 251, "price": 122.00, "amount": 122.00, "ownerId": 45, "quantity": 1.000, "serviceId": null, "vatAmount": 0.00, "vatRateId": 1, "itemTypeId": 1, "productName": "ArtelTestTovar", "serviceName": null, "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "321MHCACqNC01023", "markingNumber": "010478011969586621UiDWeTQo6DHYfk-.7bS(91bxJH92p0ndulNPQlBvZHlNS1ItRElPNmFzbmp12LUTlaaoDeL+Q=", "productTableId": 401, "expenseAccountId": null, "expenseAccountName": null}, {"id": 252, "price": 122.00, "amount": 122.00, "ownerId": 45, "quantity": 1.000, "serviceId": null, "vatAmount": 0.00, "vatRateId": 1, "itemTypeId": 1, "productName": "ArtelTestTovar", "serviceName": null, "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "131GNDABMq800359", "markingNumber": "0104780074206893217UkCJ6Gu*gG_j.wf6WnX91XUWh92JkB/xWNpbjVhcmtkWVdsT3lhVTVhYjdUQ12m8yQgOW3lE=", "productTableId": 402, "expenseAccountId": null, "expenseAccountName": null}, {"id": 253, "price": 122.00, "amount": 122.00, "ownerId": 45, "quantity": 1.000, "serviceId": null, "vatAmount": 0.00, "vatRateId": 1, "itemTypeId": 1, "productName": "ArtelTestTovar", "serviceName": null, "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "131GHFBUqMA00234", "markingNumber": "010478011969586621B0kWxHgU+uvCk4JBnQCl91UZF092d0R1ZXXvRqo4NzIVvnorLIIDBXmBIpFAD12OELOkYy1nw=", "productTableId": 403, "expenseAccountId": null, "expenseAccountName": null}, {"id": 254, "price": 122.00, "amount": 122.00, "ownerId": 45, "quantity": 1.000, "serviceId": null, "vatAmount": 0.00, "vatRateId": 1, "itemTypeId": 1, "productName": "ArtelTestTovar", "serviceName": null, "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "131GHFBUq2MA00234", "markingNumber": "010478011969586621B0kWxHgU+uvCk4JBnQCl91UZF092d0R1ZXXvRqo4NzIVvnorLIIDBXmBIpFAD12OEeLOkYy1nw=", "productTableId": 404, "expenseAccountId": null, "expenseAccountName": null}, {"id": 255, "price": 122.00, "amount": 122.00, "ownerId": 45, "quantity": 1.000, "serviceId": null, "vatAmount": 0.00, "vatRateId": 1, "itemTypeId": 1, "productName": "Product 2", "serviceName": null, "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "103CC8ABMqA00031", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86hu12WyHl5kILiUnqU=", "productTableId": 405, "expenseAccountId": null, "expenseAccountName": null}, {"id": 256, "price": 122.00, "amount": 122.00, "ownerId": 45, "quantity": 1.000, "serviceId": null, "vatAmount": 0.00, "vatRateId": 1, "itemTypeId": 1, "productName": "Product 3", "serviceName": null, "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "131GS3ABMqC00569", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86hu2WyHl5kILiUnqU=", "productTableId": 406, "expenseAccountId": null, "expenseAccountName": null}], "comment": null, "docDate": "2026-06-24T06:50:46", "stateId": 1, "statusId": 1, "docNumber": "100000027", "stateName": "Aktiv", "vatAmount": 0.00, "contractId": null, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-24T07:12:17.553992", "finalAmount": 133598.00, "totalAmount": 133598.00, "warehouseId": 7, "currencyName": "Uzbek so'm", "serviceLines": [{"id": 257, "price": 132500.00, "amount": 132500.00, "ownerId": 45, "quantity": 1.000, "serviceId": null, "vatAmount": 0.00, "vatRateId": null, "itemTypeId": 2, "productName": null, "serviceName": "elektr", "totalAmount": 132500.00, "vatRateName": null, "serialNumber": null, "markingNumber": null, "productTableId": null, "expenseAccountId": 687, "expenseAccountName": "Отклонение в стоимости товаров"}], "warehouseName": "amonov", "contractNumber": null, "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-24 07:12:17.949364
117	8	public	pur_doc	46	INSERT	\N	{"id": 46, "lines": [{"id": 258, "price": 122.00, "amount": 122.00, "ownerId": 46, "quantity": 1.000, "serviceId": null, "vatAmount": 0.00, "vatRateId": 1, "itemTypeId": 1, "productName": "ArtelTestTovar", "serviceName": null, "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "103CC8ABN8600189", "markingNumber": "010478011969800321Z6'WbR_4SBKQScZRZTjZ91UZF092NHdPZLJm0YcNie/mIPsicNrxcSHARvSb4lBOB1lrwuk0=", "productTableId": 408, "expenseAccountId": null, "expenseAccountName": null}, {"id": 259, "price": 122.00, "amount": 122.00, "ownerId": 46, "quantity": 1.000, "serviceId": null, "vatAmount": 0.00, "vatRateId": 1, "itemTypeId": 1, "productName": "ArtelTestTovar", "serviceName": null, "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "131GS3ABMC007569", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huWy44Hl5kILiUnqU=", "productTableId": 409, "expenseAccountId": null, "expenseAccountName": null}, {"id": 260, "price": 122.00, "amount": 122.00, "ownerId": 46, "quantity": 1.000, "serviceId": null, "vatAmount": 0.00, "vatRateId": 1, "itemTypeId": 1, "productName": "Product 2", "serviceName": null, "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "103CC8ABMA500031", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huW4yHl5kILiUnqU=", "productTableId": 410, "expenseAccountId": null, "expenseAccountName": null}, {"id": 261, "price": 122.00, "amount": 122.00, "ownerId": 46, "quantity": 1.000, "serviceId": null, "vatAmount": 0.00, "vatRateId": 1, "itemTypeId": 1, "productName": "Product 3", "serviceName": null, "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "131GS3ABMC008569", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huWy4Hl5kILiUnqU=", "productTableId": 411, "expenseAccountId": null, "expenseAccountName": null}], "comment": null, "docDate": "2026-06-24T07:12:17", "stateId": 1, "statusId": 1, "docNumber": "100000028", "stateName": "Aktiv", "vatAmount": 0.00, "contractId": null, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-24T07:15:37.016209", "finalAmount": 134488.00, "totalAmount": 134488.00, "warehouseId": 7, "currencyName": "Uzbek so'm", "serviceLines": [{"id": 262, "price": 12000.00, "amount": 12000.00, "ownerId": 46, "quantity": 1.000, "serviceId": null, "vatAmount": 0.00, "vatRateId": null, "itemTypeId": 2, "productName": null, "serviceName": "gaz", "totalAmount": 12000.00, "vatRateName": null, "serialNumber": null, "markingNumber": null, "productTableId": null, "expenseAccountId": 607, "expenseAccountName": "Основные средства"}, {"id": 263, "price": 122000.00, "amount": 122000.00, "ownerId": 46, "quantity": 1.000, "serviceId": null, "vatAmount": 0.00, "vatRateId": null, "itemTypeId": 2, "productName": null, "serviceName": "elektr", "totalAmount": 122000.00, "vatRateName": null, "serialNumber": null, "markingNumber": null, "productTableId": null, "expenseAccountId": 609, "expenseAccountName": "Благоустройство земли"}], "warehouseName": "amonov", "contractNumber": null, "counterpartyId": 17, "organizationId": 8, "counterpartyName": "aaaa", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-24 07:15:37.107319
118	8	public	sale_doc	71	INSERT	\N	{"id": 71, "comment": null, "docDate": "2026-06-24T07:51:12.541226", "stateId": 1, "products": [{"id": 12, "amount": 0.00, "tables": [], "quantity": 1.000, "costPrice": 0.00, "productId": 4, "unitPrice": 0.00, "vatAmount": 0.00, "vatRateId": null, "productName": "ArtelTestTovar", "totalAmount": 0.00, "vatRateName": null}, {"id": 13, "amount": 0.00, "tables": [], "quantity": 5.000, "costPrice": 0.00, "productId": 6, "unitPrice": 0.00, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 2", "totalAmount": 0.00, "vatRateName": null}, {"id": 14, "amount": 0.00, "tables": [], "quantity": 4.000, "costPrice": 0.00, "productId": 7, "unitPrice": 0.00, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 3", "totalAmount": 0.00, "vatRateName": null}], "statusId": 1, "docNumber": "100000069", "stateName": "Aktiv", "vatAmount": 0.00, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-24T07:51:12.541226", "finalAmount": 0.00, "totalAmount": 0.00, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-24 07:51:12.578612
119	8	public	sale_doc	71	UPDATE	\N	{"id": 71, "comment": null, "docDate": "2026-06-24T07:51:12.541226", "stateId": 1, "products": [{"id": 12, "amount": 0.00, "tables": [{"id": 78, "amount": 0.00, "costPrice": 122.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 0.00, "serialNumber": "131GS3ABMC007569", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huWy44Hl5kILiUnqU=", "productTableId": 409}], "quantity": 1.000, "costPrice": 122.00, "productId": 4, "unitPrice": 0.00, "vatAmount": 0.00, "vatRateId": null, "productName": "ArtelTestTovar", "totalAmount": 0.00, "vatRateName": null}, {"id": 13, "amount": 0.00, "tables": [{"id": 79, "amount": 0.00, "costPrice": 122.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 0.00, "serialNumber": "103CC8ABMqA00031", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86hu12WyHl5kILiUnqU=", "productTableId": 405}, {"id": 80, "amount": 0.00, "costPrice": 122.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 0.00, "serialNumber": "103CC8ABMA000315", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huWyHl5kILiUnqU7=", "productTableId": 354}, {"id": 81, "amount": 0.00, "costPrice": 0.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 0.00, "serialNumber": "103CC8ABMA000312V", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huWyHXl5kILiUnqU12=", "productTableId": 373}, {"id": 82, "amount": 0.00, "costPrice": 122.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 0.00, "serialNumber": "131GNDAABM800359", "markingNumber": "0104780074206893217UkCJ6Gu*gG_j.wf6WnX91XUWh92JkB/xWNpbjVhcmtkWVdsT3lhVTVhYjdUQAm8yQgOW3lE=", "productTableId": 393}, {"id": 83, "amount": 0.00, "costPrice": 122.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 0.00, "serialNumber": "103CC8ABMA500031", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huW4yHl5kILiUnqU=", "productTableId": 410}], "quantity": 5.000, "costPrice": 488.00, "productId": 6, "unitPrice": 0.00, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 2", "totalAmount": 0.00, "vatRateName": null}, {"id": 14, "amount": 0.00, "tables": [{"id": 84, "amount": 0.00, "costPrice": 122.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 0.00, "serialNumber": "131GS3ABMC005695", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huWyHl5kILiUnqU77=", "productTableId": 357}, {"id": 85, "amount": 0.00, "costPrice": 122.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 0.00, "serialNumber": "131GHFBUMA002345", "markingNumber": "010478011969586621B0kWxHgU+uvCk4JBnQCl91UZF092d0R1ZXXvRqo4NzIVvnorLIIDBXmBIpFADOELOkYy1nw7=", "productTableId": 358}, {"id": 86, "amount": 0.00, "costPrice": 0.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 0.00, "serialNumber": "131GS3ABMC005692V", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huWyHl5kILiUnqU1=X", "productTableId": 374}, {"id": 87, "amount": 0.00, "costPrice": 122.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 0.00, "serialNumber": "131GS3ABMC008569", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86huWy4Hl5kILiUnqU=", "productTableId": 411}], "quantity": 4.000, "costPrice": 366.00, "productId": 7, "unitPrice": 0.00, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 3", "totalAmount": 0.00, "vatRateName": null}], "statusId": 4, "docNumber": "100000069", "stateName": "Aktiv", "vatAmount": 0.00, "currencyId": 1, "statusName": "Kutilmoqda", "createdDate": "2026-06-24T07:51:12.541226", "finalAmount": 0.00, "totalAmount": 0.00, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-24 08:01:49.730798
120	8	public	pur_doc	47	INSERT	\N	{"id": 47, "lines": [{"id": 264, "price": 122.00, "amount": 122.00, "ownerId": 47, "quantity": 1.000, "serviceId": null, "vatAmount": 0.00, "vatRateId": 1, "itemTypeId": 1, "productName": "Product 3", "serviceName": null, "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "131GHFBUQMA00234", "markingNumber": "010478011969586621B0kWxHgU+uvCk4JBnQCl91UZF092d0R1ZXXvRqo4NzIVvnorLIIDBXmBIpFAwDOELOkYy1nw=", "productTableId": 413, "expenseAccountId": null, "expenseAccountName": null}], "comment": null, "docDate": "2026-06-24T07:15:36", "stateId": 1, "statusId": 1, "docNumber": "100000029", "stateName": "Aktiv", "vatAmount": 0.00, "contractId": 9, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-24T09:43:30.99489", "finalAmount": 130122.00, "totalAmount": 130122.00, "warehouseId": 7, "currencyName": "Uzbek so'm", "serviceLines": [{"id": 265, "price": 130000.00, "amount": 130000.00, "ownerId": 47, "quantity": 1.000, "serviceId": null, "vatAmount": 0.00, "vatRateId": null, "itemTypeId": 2, "productName": null, "serviceName": "svet", "totalAmount": 130000.00, "vatRateName": null, "serialNumber": null, "markingNumber": null, "productTableId": null, "expenseAccountId": 609, "expenseAccountName": "Благоустройство земли"}], "warehouseName": "amonov", "contractNumber": "100000009", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-24 09:43:31.205299
121	8	public	pur_doc	48	INSERT	\N	{"id": 48, "lines": [], "comment": null, "docDate": "2026-06-24T09:43:30", "stateId": 1, "statusId": 1, "docNumber": "100000030", "stateName": "Aktiv", "vatAmount": 0.00, "contractId": 9, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-24T09:48:01.578881", "finalAmount": 144000.00, "totalAmount": 144000.00, "warehouseId": 7, "currencyName": "Uzbek so'm", "serviceLines": [{"id": 266, "price": 144000.00, "amount": 144000.00, "ownerId": 48, "quantity": 1.000, "serviceId": null, "vatAmount": 0.00, "vatRateId": null, "itemTypeId": 2, "productName": null, "serviceName": "xarajat", "totalAmount": 144000.00, "vatRateName": null, "serialNumber": null, "markingNumber": null, "productTableId": null, "expenseAccountId": 787, "expenseAccountName": "Авансы по ИНПС (обязательная часть)"}], "warehouseName": "amonov", "contractNumber": "100000009", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-24 09:48:01.645856
122	8	public	pur_doc	49	INSERT	\N	{"id": 49, "lines": [{"id": 267, "price": 122.00, "amount": 122.00, "ownerId": 49, "quantity": 1.000, "serviceId": null, "vatAmount": 0.00, "vatRateId": 1, "itemTypeId": 1, "productName": "ArtelTestTovar", "serviceName": null, "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "qedsdsdewd", "markingNumber": "kjqjwhlieihwqelorfef;gewf745", "productTableId": 414, "expenseAccountId": null, "expenseAccountName": null}, {"id": 268, "price": 122.00, "amount": 122.00, "ownerId": 49, "quantity": 1.000, "serviceId": null, "vatAmount": 0.00, "vatRateId": 1, "itemTypeId": 1, "productName": "Product 2", "serviceName": null, "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "2er32fw", "markingNumber": "qweroi;3247trgfq3rf", "productTableId": 415, "expenseAccountId": null, "expenseAccountName": null}], "comment": null, "docDate": "2026-06-24T09:48:01", "stateId": 1, "statusId": 1, "docNumber": "100000031", "stateName": "Aktiv", "vatAmount": 0.00, "contractId": 9, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-24T09:58:47.566919", "finalAmount": 244.00, "totalAmount": 244.00, "warehouseId": 7, "currencyName": "Uzbek so'm", "serviceLines": [], "warehouseName": "amonov", "contractNumber": "100000009", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-24 09:58:47.651531
123	8	public	sale_doc	72	INSERT	\N	{"id": 72, "comment": null, "docDate": "2026-06-24T10:03:00.066911", "stateId": 1, "products": [{"id": 15, "amount": 0.00, "tables": [], "quantity": 2.000, "costPrice": 0.00, "productId": 4, "unitPrice": 0.00, "vatAmount": 0.00, "vatRateId": null, "productName": "ArtelTestTovar", "totalAmount": 0.00, "vatRateName": null}, {"id": 16, "amount": 0.00, "tables": [], "quantity": 2.000, "costPrice": 0.00, "productId": 6, "unitPrice": 0.00, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 2", "totalAmount": 0.00, "vatRateName": null}, {"id": 17, "amount": 0.00, "tables": [], "quantity": 1.000, "costPrice": 0.00, "productId": 7, "unitPrice": 0.00, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 3", "totalAmount": 0.00, "vatRateName": null}], "statusId": 1, "docNumber": "100000070", "stateName": "Aktiv", "vatAmount": 0.00, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-24T10:03:00.066911", "finalAmount": 0.00, "totalAmount": 0.00, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-24 10:03:00.098593
124	8	public	sale_doc	72	UPDATE	\N	{"id": 72, "comment": null, "docDate": "2026-06-24T10:03:00.066911", "stateId": 1, "products": [{"id": 15, "amount": 0.00, "tables": [{"id": 88, "amount": 0.00, "costPrice": 122.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 0.00, "serialNumber": "103CC8ABN8001895", "markingNumber": "010478011969800321Z6'WbR_4SBKQScZRZTjZ91UZF092NHdPZLJm0YcNie/mIPsicNrxcSHARvSblBOB1lrwuk07=", "productTableId": 351}, {"id": 89, "amount": 0.00, "costPrice": 0.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 0.00, "serialNumber": "103CC8ABN8001892V", "markingNumber": "010478011969800321Z6'WbR_4SBKQScZRZTjZ91UZF092NHdPZLJm0YcNie/mIPsicNrxcSHARvSblBXXOB1lrwuk01=", "productTableId": 371}], "quantity": 2.000, "costPrice": 122.00, "productId": 4, "unitPrice": 0.00, "vatAmount": 0.00, "vatRateId": null, "productName": "ArtelTestTovar", "totalAmount": 0.00, "vatRateName": null}, {"id": 16, "amount": 0.00, "tables": [{"id": 90, "amount": 0.00, "costPrice": 122.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 0.00, "serialNumber": "321MHCACNC010235", "markingNumber": "010478011969586621UiDWeTQo6DHYfk-.7bS(91bxJH92p0ndulNPQlBvZHlNS1ItRElPNmFzbmpLUTlaaoDeL+Q7=", "productTableId": 355}, {"id": 91, "amount": 0.00, "costPrice": 122.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 0.00, "serialNumber": "131GNDABM8003595", "markingNumber": "0104780074206893217UkCJ6Gu*gG_j.wf6WnX91XUWh92JkB/xWNpbjVhcmtkWVdsT3lhVTVhYjdUQm8yQgOW3lE7=", "productTableId": 356}], "quantity": 2.000, "costPrice": 244.00, "productId": 6, "unitPrice": 0.00, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 2", "totalAmount": 0.00, "vatRateName": null}, {"id": 17, "amount": 0.00, "tables": [{"id": 92, "amount": 0.00, "costPrice": 122.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 0.00, "serialNumber": "131GS3ABMqC00569", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86hu2WyHl5kILiUnqU=", "productTableId": 406}], "quantity": 1.000, "costPrice": 122.00, "productId": 7, "unitPrice": 0.00, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 3", "totalAmount": 0.00, "vatRateName": null}], "statusId": 4, "docNumber": "100000070", "stateName": "Aktiv", "vatAmount": 0.00, "currencyId": 1, "statusName": "Kutilmoqda", "createdDate": "2026-06-24T10:03:00.066911", "finalAmount": 0.00, "totalAmount": 0.00, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-24 10:06:37.749408
125	8	public	sale_doc	72	UPDATE	{"id": 72, "comment": null, "docDate": "2026-06-24T10:03:00.066911", "stateId": 1, "products": [{"id": 15, "amount": 0.00, "tables": [{"id": 88, "amount": 0.00, "costPrice": 122.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 0.00, "serialNumber": "103CC8ABN8001895", "markingNumber": "010478011969800321Z6'WbR_4SBKQScZRZTjZ91UZF092NHdPZLJm0YcNie/mIPsicNrxcSHARvSblBOB1lrwuk07=", "productTableId": 351}, {"id": 89, "amount": 0.00, "costPrice": 0.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 0.00, "serialNumber": "103CC8ABN8001892V", "markingNumber": "010478011969800321Z6'WbR_4SBKQScZRZTjZ91UZF092NHdPZLJm0YcNie/mIPsicNrxcSHARvSblBXXOB1lrwuk01=", "productTableId": 371}], "quantity": 2.000, "costPrice": 122.00, "productId": 4, "unitPrice": 0.00, "vatAmount": 0.00, "vatRateId": null, "productName": "ArtelTestTovar", "totalAmount": 0.00, "vatRateName": null}, {"id": 16, "amount": 0.00, "tables": [{"id": 90, "amount": 0.00, "costPrice": 122.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 0.00, "serialNumber": "321MHCACNC010235", "markingNumber": "010478011969586621UiDWeTQo6DHYfk-.7bS(91bxJH92p0ndulNPQlBvZHlNS1ItRElPNmFzbmpLUTlaaoDeL+Q7=", "productTableId": 355}, {"id": 91, "amount": 0.00, "costPrice": 122.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 0.00, "serialNumber": "131GNDABM8003595", "markingNumber": "0104780074206893217UkCJ6Gu*gG_j.wf6WnX91XUWh92JkB/xWNpbjVhcmtkWVdsT3lhVTVhYjdUQm8yQgOW3lE7=", "productTableId": 356}], "quantity": 2.000, "costPrice": 244.00, "productId": 6, "unitPrice": 0.00, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 2", "totalAmount": 0.00, "vatRateName": null}, {"id": 17, "amount": 0.00, "tables": [{"id": 92, "amount": 0.00, "costPrice": 122.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 0.00, "serialNumber": "131GS3ABMqC00569", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86hu2WyHl5kILiUnqU=", "productTableId": 406}], "quantity": 1.000, "costPrice": 122.00, "productId": 7, "unitPrice": 0.00, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 3", "totalAmount": 0.00, "vatRateName": null}], "statusId": 4, "docNumber": "100000070", "stateName": "Aktiv", "vatAmount": 0.00, "currencyId": 1, "statusName": "Kutilmoqda", "createdDate": "2026-06-24T10:03:00.066911", "finalAmount": 0.00, "totalAmount": 0.00, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	{"id": 72, "comment": null, "docDate": "2026-06-24T10:03:00.066911", "stateId": 1, "products": [{"id": 15, "amount": 268.40, "tables": [{"id": 88, "amount": 134.20, "costPrice": 122.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 134.20, "serialNumber": "103CC8ABN8001895", "markingNumber": "010478011969800321Z6'WbR_4SBKQScZRZTjZ91UZF092NHdPZLJm0YcNie/mIPsicNrxcSHARvSblBOB1lrwuk07=", "productTableId": 351}, {"id": 89, "amount": 134.20, "costPrice": 0.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 134.20, "serialNumber": "103CC8ABN8001892V", "markingNumber": "010478011969800321Z6'WbR_4SBKQScZRZTjZ91UZF092NHdPZLJm0YcNie/mIPsicNrxcSHARvSblBXXOB1lrwuk01=", "productTableId": 371}], "quantity": 2.000, "costPrice": 122.00, "productId": 4, "unitPrice": 134.20, "vatAmount": 0.00, "vatRateId": null, "productName": "ArtelTestTovar", "totalAmount": 268.40, "vatRateName": null}, {"id": 16, "amount": 280.60, "tables": [{"id": 90, "amount": 140.30, "costPrice": 122.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 140.30, "serialNumber": "321MHCACNC010235", "markingNumber": "010478011969586621UiDWeTQo6DHYfk-.7bS(91bxJH92p0ndulNPQlBvZHlNS1ItRElPNmFzbmpLUTlaaoDeL+Q7=", "productTableId": 355}, {"id": 91, "amount": 140.30, "costPrice": 122.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 140.30, "serialNumber": "131GNDABM8003595", "markingNumber": "0104780074206893217UkCJ6Gu*gG_j.wf6WnX91XUWh92JkB/xWNpbjVhcmtkWVdsT3lhVTVhYjdUQm8yQgOW3lE7=", "productTableId": 356}], "quantity": 2.000, "costPrice": 244.00, "productId": 6, "unitPrice": 140.30, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 2", "totalAmount": 280.60, "vatRateName": null}, {"id": 17, "amount": 140.30, "tables": [{"id": 92, "amount": 140.30, "costPrice": 122.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 140.30, "serialNumber": "131GS3ABMqC00569", "markingNumber": "010478007420371721u+YEnHW_>m3<4SujfeaC91UZF092dlByNQ6SBj/MtyKzfFentAKF8o86hu2WyHl5kILiUnqU=", "productTableId": 406}], "quantity": 1.000, "costPrice": 122.00, "productId": 7, "unitPrice": 140.30, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 3", "totalAmount": 140.30, "vatRateName": null}], "statusId": 2, "docNumber": "100000070", "stateName": "Aktiv", "vatAmount": 0.00, "currencyId": 1, "statusName": "O'tkazilgan", "createdDate": "2026-06-24T10:03:00.066911", "finalAmount": 689.30, "totalAmount": 689.30, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-24 11:15:24.605035
126	8	public	sale_doc	73	INSERT	\N	{"id": 73, "comment": null, "docDate": "2026-06-24T11:32:29.984233", "stateId": 1, "products": [{"id": 18, "amount": 0.00, "tables": [], "quantity": 5.000, "costPrice": 0.00, "productId": 4, "unitPrice": 0.00, "vatAmount": 0.00, "vatRateId": null, "productName": "ArtelTestTovar", "totalAmount": 0.00, "vatRateName": null}], "statusId": 1, "docNumber": "100000071", "stateName": "Aktiv", "vatAmount": 0.00, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-24T11:32:29.984233", "finalAmount": 0.00, "totalAmount": 0.00, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-24 11:32:30.014901
127	8	public	sale_doc	73	UPDATE	\N	{"id": 73, "comment": null, "docDate": "2026-06-24T11:32:29.984233", "stateId": 1, "products": [{"id": 18, "amount": 0.00, "tables": [{"id": 93, "amount": 0.00, "costPrice": 122.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 0.00, "serialNumber": "103CC8ABNq800189", "markingNumber": "010478011969800321Z6'WbR_4SBKQScZRZTjZ91UZF092NHdPZLJm0YcNie/mIPsicNrxcSHARvS12blBOB1lrwuk0=", "productTableId": 398}, {"id": 94, "amount": 0.00, "costPrice": 122.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 0.00, "serialNumber": "131GNDABMq800359", "markingNumber": "0104780074206893217UkCJ6Gu*gG_j.wf6WnX91XUWh92JkB/xWNpbjVhcmtkWVdsT3lhVTVhYjdUQ12m8yQgOW3lE=", "productTableId": 402}, {"id": 95, "amount": 0.00, "costPrice": 122.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 0.00, "serialNumber": "131GHFBUqMA00234", "markingNumber": "010478011969586621B0kWxHgU+uvCk4JBnQCl91UZF092d0R1ZXXvRqo4NzIVvnorLIIDBXmBIpFAD12OELOkYy1nw=", "productTableId": 403}, {"id": 96, "amount": 0.00, "costPrice": 122.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 0.00, "serialNumber": "131GHFBUq2MA00234", "markingNumber": "010478011969586621B0kWxHgU+uvCk4JBnQCl91UZF092d0R1ZXXvRqo4NzIVvnorLIIDBXmBIpFAD12OEeLOkYy1nw=", "productTableId": 404}, {"id": 97, "amount": 0.00, "costPrice": 122.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 0.00, "serialNumber": "103CC8ABN8600189", "markingNumber": "010478011969800321Z6'WbR_4SBKQScZRZTjZ91UZF092NHdPZLJm0YcNie/mIPsicNrxcSHARvSb4lBOB1lrwuk0=", "productTableId": 408}], "quantity": 5.000, "costPrice": 610.00, "productId": 4, "unitPrice": 0.00, "vatAmount": 0.00, "vatRateId": null, "productName": "ArtelTestTovar", "totalAmount": 0.00, "vatRateName": null}], "statusId": 4, "docNumber": "100000071", "stateName": "Aktiv", "vatAmount": 0.00, "currencyId": 1, "statusName": "Kutilmoqda", "createdDate": "2026-06-24T11:32:29.984233", "finalAmount": 0.00, "totalAmount": 0.00, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-24 11:33:24.130188
128	8	public	sale_doc	73	UPDATE	{"id": 73, "comment": null, "docDate": "2026-06-24T11:32:29.984233", "stateId": 1, "products": [{"id": 18, "amount": 0.00, "tables": [{"id": 93, "amount": 0.00, "costPrice": 122.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 0.00, "serialNumber": "103CC8ABNq800189", "markingNumber": "010478011969800321Z6'WbR_4SBKQScZRZTjZ91UZF092NHdPZLJm0YcNie/mIPsicNrxcSHARvS12blBOB1lrwuk0=", "productTableId": 398}, {"id": 94, "amount": 0.00, "costPrice": 122.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 0.00, "serialNumber": "131GNDABMq800359", "markingNumber": "0104780074206893217UkCJ6Gu*gG_j.wf6WnX91XUWh92JkB/xWNpbjVhcmtkWVdsT3lhVTVhYjdUQ12m8yQgOW3lE=", "productTableId": 402}, {"id": 95, "amount": 0.00, "costPrice": 122.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 0.00, "serialNumber": "131GHFBUqMA00234", "markingNumber": "010478011969586621B0kWxHgU+uvCk4JBnQCl91UZF092d0R1ZXXvRqo4NzIVvnorLIIDBXmBIpFAD12OELOkYy1nw=", "productTableId": 403}, {"id": 96, "amount": 0.00, "costPrice": 122.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 0.00, "serialNumber": "131GHFBUq2MA00234", "markingNumber": "010478011969586621B0kWxHgU+uvCk4JBnQCl91UZF092d0R1ZXXvRqo4NzIVvnorLIIDBXmBIpFAD12OEeLOkYy1nw=", "productTableId": 404}, {"id": 97, "amount": 0.00, "costPrice": 122.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 0.00, "serialNumber": "103CC8ABN8600189", "markingNumber": "010478011969800321Z6'WbR_4SBKQScZRZTjZ91UZF092NHdPZLJm0YcNie/mIPsicNrxcSHARvSb4lBOB1lrwuk0=", "productTableId": 408}], "quantity": 5.000, "costPrice": 610.00, "productId": 4, "unitPrice": 0.00, "vatAmount": 0.00, "vatRateId": null, "productName": "ArtelTestTovar", "totalAmount": 0.00, "vatRateName": null}], "statusId": 4, "docNumber": "100000071", "stateName": "Aktiv", "vatAmount": 0.00, "currencyId": 1, "statusName": "Kutilmoqda", "createdDate": "2026-06-24T11:32:29.984233", "finalAmount": 0.00, "totalAmount": 0.00, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	{"id": 73, "comment": null, "docDate": "2026-06-24T11:32:29.984233", "stateId": 1, "products": [{"id": 18, "amount": 660.00, "tables": [{"id": 93, "amount": 132.00, "costPrice": 122.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 132.00, "serialNumber": "103CC8ABNq800189", "markingNumber": "010478011969800321Z6'WbR_4SBKQScZRZTjZ91UZF092NHdPZLJm0YcNie/mIPsicNrxcSHARvS12blBOB1lrwuk0=", "productTableId": 398}, {"id": 94, "amount": 132.00, "costPrice": 122.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 132.00, "serialNumber": "131GNDABMq800359", "markingNumber": "0104780074206893217UkCJ6Gu*gG_j.wf6WnX91XUWh92JkB/xWNpbjVhcmtkWVdsT3lhVTVhYjdUQ12m8yQgOW3lE=", "productTableId": 402}, {"id": 95, "amount": 132.00, "costPrice": 122.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 132.00, "serialNumber": "131GHFBUqMA00234", "markingNumber": "010478011969586621B0kWxHgU+uvCk4JBnQCl91UZF092d0R1ZXXvRqo4NzIVvnorLIIDBXmBIpFAD12OELOkYy1nw=", "productTableId": 403}, {"id": 96, "amount": 132.00, "costPrice": 122.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 132.00, "serialNumber": "131GHFBUq2MA00234", "markingNumber": "010478011969586621B0kWxHgU+uvCk4JBnQCl91UZF092d0R1ZXXvRqo4NzIVvnorLIIDBXmBIpFAD12OEeLOkYy1nw=", "productTableId": 404}, {"id": 97, "amount": 132.00, "costPrice": 122.00, "vatAmount": 0.00, "vatRateId": null, "totalAmount": 132.00, "serialNumber": "103CC8ABN8600189", "markingNumber": "010478011969800321Z6'WbR_4SBKQScZRZTjZ91UZF092NHdPZLJm0YcNie/mIPsicNrxcSHARvSb4lBOB1lrwuk0=", "productTableId": 408}], "quantity": 5.000, "costPrice": 610.00, "productId": 4, "unitPrice": 132.00, "vatAmount": 0.00, "vatRateId": null, "productName": "ArtelTestTovar", "totalAmount": 660.00, "vatRateName": null}], "statusId": 2, "docNumber": "100000071", "stateName": "Aktiv", "vatAmount": 0.00, "currencyId": 1, "statusName": "O'tkazilgan", "createdDate": "2026-06-24T11:32:29.984233", "finalAmount": 660.00, "totalAmount": 660.00, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-24 11:35:42.180481
153	8	public	pur_doc	78	INSERT	\N	{"id": 78, "lines": [], "comment": null, "docDate": "2026-06-26T20:15:22", "stateId": 1, "statusId": 1, "docNumber": "100000060", "stateName": "Aktiv", "vatAmount": 0.00000000, "contractId": null, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-26T20:16:43.73427", "finalAmount": 0.00000000, "totalAmount": 0.00000000, "warehouseId": 7, "currencyName": "Uzbek so'm", "warehouseName": "amonov", "contractNumber": null, "counterpartyId": 17, "organizationId": 8, "counterpartyName": "aaaa", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-26 20:16:43.794371
129	8	public	pur_doc	50	INSERT	\N	{"id": 50, "lines": [{"id": 269, "price": 122.00, "amount": 122.00, "ownerId": 50, "quantity": 1.000, "serviceId": null, "vatAmount": 0.00, "vatRateId": 1, "itemTypeId": 1, "productName": "ArtelTestTovar", "serviceName": null, "totalAmount": 122.00, "vatRateName": "QQS 0%", "serialNumber": "wdqqqqw", "markingNumber": "34t34tgergfergfqewrfewf", "productTableId": 416, "expenseAccountId": null, "expenseAccountName": null}], "comment": null, "docDate": "2026-06-24T09:58:47", "stateId": 1, "statusId": 1, "docNumber": "100000032", "stateName": "Aktiv", "vatAmount": 0.00, "contractId": null, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-24T13:01:55.751753", "finalAmount": 12122.00, "totalAmount": 12122.00, "warehouseId": 7, "currencyName": "Uzbek so'm", "serviceLines": [{"id": 270, "price": 12000.00, "amount": 12000.00, "ownerId": 50, "quantity": 1.000, "serviceId": 2, "vatAmount": 0.00, "vatRateId": null, "itemTypeId": 2, "productName": null, "serviceName": "Internet va aloqa xizmatlari", "totalAmount": 12000.00, "vatRateName": null, "serialNumber": null, "markingNumber": null, "productTableId": null, "expenseAccountId": 1002, "expenseAccountName": "Ma'muriy xarajatlar"}], "warehouseName": "amonov", "contractNumber": null, "counterpartyId": 17, "organizationId": 8, "counterpartyName": "aaaa", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-24 13:01:56.890329
130	2	public	bank_operation	2	INSERT	\N	{"id": 2, "amount": 1000.00, "comment": "Codex single create test", "docDate": "2026-06-24T15:02:59", "stateId": 1, "statusId": 2, "docNumber": "100000001", "stateName": "Aktiv", "currencyId": 4, "statusName": "O'tkazilgan", "createdDate": "2026-06-24T15:02:59.985932", "currencyName": "Euro", "bankAccountId": 1, "paymentTypeId": null, "counterpartyId": null, "organizationId": 2, "operationTypeId": 2, "paymentTypeName": null, "counterpartyName": null, "organizationName": "Najot Ta'lim", "bankAccountNumber": "064232", "operationTypeName": "Chiqim"}	12	\N	\N	\N	2026-06-24 15:03:00.333742
131	2	public	bank_operation	2	DELETE	{"id": 2, "amount": 1000.00, "comment": "Codex single create test", "docDate": "2026-06-24T15:02:59", "stateId": 1, "statusId": 2, "docNumber": "100000001", "stateName": "Aktiv", "currencyId": 4, "statusName": "O'tkazilgan", "createdDate": "2026-06-24T15:02:59.985932", "currencyName": "Euro", "bankAccountId": 1, "paymentTypeId": null, "counterpartyId": null, "organizationId": 2, "operationTypeId": 2, "paymentTypeName": null, "counterpartyName": null, "organizationName": "Najot Ta'lim", "bankAccountNumber": "064232", "operationTypeName": "Chiqim"}	{"id": 2, "amount": 1000.00, "comment": "Codex single create test", "docDate": "2026-06-24T15:02:59", "stateId": 2, "statusId": 2, "docNumber": "100000001", "stateName": "Passiv", "currencyId": 4, "statusName": "O'tkazilgan", "createdDate": "2026-06-24T15:02:59.985932", "currencyName": "Euro", "bankAccountId": 1, "paymentTypeId": null, "counterpartyId": null, "organizationId": 2, "operationTypeId": 2, "paymentTypeName": null, "counterpartyName": null, "organizationName": "Najot Ta'lim", "bankAccountNumber": "064232", "operationTypeName": "Chiqim"}	12	\N	\N	\N	2026-06-24 15:03:00.938416
132	2	public	bank_operation	3	DELETE	{"id": 3, "amount": 1001.00, "comment": "Codex many create test 1", "docDate": "2026-06-24T15:03:00", "stateId": 1, "statusId": 2, "docNumber": "100000002", "stateName": "Aktiv", "currencyId": 4, "statusName": "O'tkazilgan", "createdDate": "2026-06-24T15:03:00.548019", "currencyName": "Euro", "bankAccountId": 1, "paymentTypeId": null, "counterpartyId": null, "organizationId": 2, "operationTypeId": 2, "paymentTypeName": null, "counterpartyName": null, "organizationName": "Najot Ta'lim", "bankAccountNumber": "064232", "operationTypeName": "Chiqim"}	{"id": 3, "amount": 1001.00, "comment": "Codex many create test 1", "docDate": "2026-06-24T15:03:00", "stateId": 2, "statusId": 2, "docNumber": "100000002", "stateName": "Passiv", "currencyId": 4, "statusName": "O'tkazilgan", "createdDate": "2026-06-24T15:03:00.548019", "currencyName": "Euro", "bankAccountId": 1, "paymentTypeId": null, "counterpartyId": null, "organizationId": 2, "operationTypeId": 2, "paymentTypeName": null, "counterpartyName": null, "organizationName": "Najot Ta'lim", "bankAccountNumber": "064232", "operationTypeName": "Chiqim"}	12	\N	\N	\N	2026-06-24 15:03:01.066375
133	2	public	bank_operation	4	DELETE	{"id": 4, "amount": 1002.00, "comment": "Codex many create test 2", "docDate": "2026-06-24T15:03:00", "stateId": 1, "statusId": 2, "docNumber": "100000003", "stateName": "Aktiv", "currencyId": 4, "statusName": "O'tkazilgan", "createdDate": "2026-06-24T15:03:00.548021", "currencyName": "Euro", "bankAccountId": 1, "paymentTypeId": null, "counterpartyId": null, "organizationId": 2, "operationTypeId": 2, "paymentTypeName": null, "counterpartyName": null, "organizationName": "Najot Ta'lim", "bankAccountNumber": "064232", "operationTypeName": "Chiqim"}	{"id": 4, "amount": 1002.00, "comment": "Codex many create test 2", "docDate": "2026-06-24T15:03:00", "stateId": 2, "statusId": 2, "docNumber": "100000003", "stateName": "Passiv", "currencyId": 4, "statusName": "O'tkazilgan", "createdDate": "2026-06-24T15:03:00.548021", "currencyName": "Euro", "bankAccountId": 1, "paymentTypeId": null, "counterpartyId": null, "organizationId": 2, "operationTypeId": 2, "paymentTypeName": null, "counterpartyName": null, "organizationName": "Najot Ta'lim", "bankAccountNumber": "064232", "operationTypeName": "Chiqim"}	12	\N	\N	\N	2026-06-24 15:03:01.209231
134	2	public	bank_operation	5	INSERT	\N	{"id": 5, "amount": 1100.00, "comment": "Codex paymentType single test", "docDate": "2026-06-24T15:34:52", "stateId": 1, "statusId": 2, "docNumber": "100000004", "stateName": "Aktiv", "currencyId": 4, "statusName": "O'tkazilgan", "createdDate": "2026-06-24T15:34:52.995844", "currencyName": "Euro", "bankAccountId": 1, "paymentTypeId": 2, "counterpartyId": null, "organizationId": 2, "operationTypeId": 2, "paymentTypeName": "Bank", "counterpartyName": null, "organizationName": "Najot Ta'lim", "bankAccountNumber": "064232", "operationTypeName": "Chiqim"}	12	\N	\N	\N	2026-06-24 15:34:53.347153
140	8	public	bank_operation	24	INSERT	\N	{"id": 24, "amount": 2550000.00, "comment": null, "docDate": "2026-06-25T06:58:33", "stateId": 1, "statusId": 2, "docNumber": "100000023", "stateName": "Aktiv", "currencyId": 1, "statusName": "O'tkazilgan", "createdDate": "2026-06-25T12:00:56.882823", "currencyName": "Uzbek so'm", "bankAccountId": 12, "paymentTypeId": 2, "counterpartyId": 18, "organizationId": 8, "operationTypeId": 1, "paymentTypeName": "Bank", "counterpartyName": "Artel", "organizationName": "baraka_market", "bankAccountNumber": "20208000005157348001", "operationTypeName": "Kirim"}	4	\N	\N	\N	2026-06-25 12:00:56.945275
135	2	public	bank_operation	5	DELETE	{"id": 5, "amount": 1100.00, "comment": "Codex paymentType single test", "docDate": "2026-06-24T15:34:52", "stateId": 1, "statusId": 2, "docNumber": "100000004", "stateName": "Aktiv", "currencyId": 4, "statusName": "O'tkazilgan", "createdDate": "2026-06-24T15:34:52.995844", "currencyName": "Euro", "bankAccountId": 1, "paymentTypeId": 2, "counterpartyId": null, "organizationId": 2, "operationTypeId": 2, "paymentTypeName": "Bank", "counterpartyName": null, "organizationName": "Najot Ta'lim", "bankAccountNumber": "064232", "operationTypeName": "Chiqim"}	{"id": 5, "amount": 1100.00, "comment": "Codex paymentType single test", "docDate": "2026-06-24T15:34:52", "stateId": 2, "statusId": 2, "docNumber": "100000004", "stateName": "Passiv", "currencyId": 4, "statusName": "O'tkazilgan", "createdDate": "2026-06-24T15:34:52.995844", "currencyName": "Euro", "bankAccountId": 1, "paymentTypeId": 2, "counterpartyId": null, "organizationId": 2, "operationTypeId": 2, "paymentTypeName": "Bank", "counterpartyName": null, "organizationName": "Najot Ta'lim", "bankAccountNumber": "064232", "operationTypeName": "Chiqim"}	12	\N	\N	\N	2026-06-24 15:34:54.006186
136	2	public	bank_operation	6	DELETE	{"id": 6, "amount": 1101.00, "comment": "Codex paymentType many test 1", "docDate": "2026-06-24T15:34:53", "stateId": 1, "statusId": 2, "docNumber": "100000005", "stateName": "Aktiv", "currencyId": 4, "statusName": "O'tkazilgan", "createdDate": "2026-06-24T15:34:53.638813", "currencyName": "Euro", "bankAccountId": 1, "paymentTypeId": 2, "counterpartyId": null, "organizationId": 2, "operationTypeId": 2, "paymentTypeName": "Bank", "counterpartyName": null, "organizationName": "Najot Ta'lim", "bankAccountNumber": "064232", "operationTypeName": "Chiqim"}	{"id": 6, "amount": 1101.00, "comment": "Codex paymentType many test 1", "docDate": "2026-06-24T15:34:53", "stateId": 2, "statusId": 2, "docNumber": "100000005", "stateName": "Passiv", "currencyId": 4, "statusName": "O'tkazilgan", "createdDate": "2026-06-24T15:34:53.638813", "currencyName": "Euro", "bankAccountId": 1, "paymentTypeId": 2, "counterpartyId": null, "organizationId": 2, "operationTypeId": 2, "paymentTypeName": "Bank", "counterpartyName": null, "organizationName": "Najot Ta'lim", "bankAccountNumber": "064232", "operationTypeName": "Chiqim"}	12	\N	\N	\N	2026-06-24 15:34:54.140805
137	2	public	bank_operation	7	DELETE	{"id": 7, "amount": 1102.00, "comment": "Codex paymentType many test 2", "docDate": "2026-06-24T15:34:53", "stateId": 1, "statusId": 2, "docNumber": "100000006", "stateName": "Aktiv", "currencyId": 4, "statusName": "O'tkazilgan", "createdDate": "2026-06-24T15:34:53.638815", "currencyName": "Euro", "bankAccountId": 1, "paymentTypeId": 2, "counterpartyId": null, "organizationId": 2, "operationTypeId": 2, "paymentTypeName": "Bank", "counterpartyName": null, "organizationName": "Najot Ta'lim", "bankAccountNumber": "064232", "operationTypeName": "Chiqim"}	{"id": 7, "amount": 1102.00, "comment": "Codex paymentType many test 2", "docDate": "2026-06-24T15:34:53", "stateId": 2, "statusId": 2, "docNumber": "100000006", "stateName": "Passiv", "currencyId": 4, "statusName": "O'tkazilgan", "createdDate": "2026-06-24T15:34:53.638815", "currencyName": "Euro", "bankAccountId": 1, "paymentTypeId": 2, "counterpartyId": null, "organizationId": 2, "operationTypeId": 2, "paymentTypeName": "Bank", "counterpartyName": null, "organizationName": "Najot Ta'lim", "bankAccountNumber": "064232", "operationTypeName": "Chiqim"}	12	\N	\N	\N	2026-06-24 15:34:54.341888
138	8	public	bank_operation	9	DELETE	{"id": 9, "amount": 4400000.00, "comment": "00634Зачисление на счет пласт. карты 8600300486606289 КИБ: 2025 йил декабрь ойи ишчи ходимлар иш хакки хисобидан реестрга асосан пл картага кучирилди.", "docDate": "2026-01-05T10:24:55", "stateId": 1, "statusId": 2, "docNumber": "100000008", "stateName": "Aktiv", "currencyId": 1, "statusName": "O'tkazilgan", "createdDate": "2026-06-25T11:46:02.911371", "currencyName": "Uzbek so'm", "bankAccountId": 11, "paymentTypeId": 2, "counterpartyId": 20, "organizationId": 8, "operationTypeId": 1, "paymentTypeName": "Bank", "counterpartyName": "Ava", "organizationName": "baraka_market", "bankAccountNumber": "23106000105157348001", "operationTypeName": "Kirim"}	{"id": 9, "amount": 4400000.00, "comment": "00634Зачисление на счет пласт. карты 8600300486606289 КИБ: 2025 йил декабрь ойи ишчи ходимлар иш хакки хисобидан реестрга асосан пл картага кучирилди.", "docDate": "2026-01-05T10:24:55", "stateId": 2, "statusId": 2, "docNumber": "100000008", "stateName": "Passiv", "currencyId": 1, "statusName": "O'tkazilgan", "createdDate": "2026-06-25T11:46:02.911371", "currencyName": "Uzbek so'm", "bankAccountId": 11, "paymentTypeId": 2, "counterpartyId": 20, "organizationId": 8, "operationTypeId": 1, "paymentTypeName": "Bank", "counterpartyName": "Ava", "organizationName": "baraka_market", "bankAccountNumber": "23106000105157348001", "operationTypeName": "Kirim"}	4	\N	\N	\N	2026-06-25 11:54:24.876612
139	8	public	bank_operation	8	DELETE	{"id": 8, "amount": 10120000.00, "comment": "00634 2025 йил декабрь ойи ишчи ходимлар иш хакки хисобидан реестрга асосан пл картага кучирилди.", "docDate": "2026-01-05T10:21:34", "stateId": 1, "statusId": 2, "docNumber": "100000007", "stateName": "Aktiv", "currencyId": 1, "statusName": "O'tkazilgan", "createdDate": "2026-06-25T11:46:02.911305", "currencyName": "Uzbek so'm", "bankAccountId": 11, "paymentTypeId": 2, "counterpartyId": 21, "organizationId": 8, "operationTypeId": 2, "paymentTypeName": "Bank", "counterpartyName": "Aval", "organizationName": "baraka_market", "bankAccountNumber": "23106000105157348001", "operationTypeName": "Chiqim"}	{"id": 8, "amount": 10120000.00, "comment": "00634 2025 йил декабрь ойи ишчи ходимлар иш хакки хисобидан реестрга асосан пл картага кучирилди.", "docDate": "2026-01-05T10:21:34", "stateId": 2, "statusId": 2, "docNumber": "100000007", "stateName": "Passiv", "currencyId": 1, "statusName": "O'tkazilgan", "createdDate": "2026-06-25T11:46:02.911305", "currencyName": "Uzbek so'm", "bankAccountId": 11, "paymentTypeId": 2, "counterpartyId": 21, "organizationId": 8, "operationTypeId": 2, "paymentTypeName": "Bank", "counterpartyName": "Aval", "organizationName": "baraka_market", "bankAccountNumber": "23106000105157348001", "operationTypeName": "Chiqim"}	4	\N	\N	\N	2026-06-25 11:55:03.275659
141	8	public	bank_operation	24	DELETE	{"id": 24, "amount": 2550000.00, "comment": null, "docDate": "2026-06-25T06:58:33", "stateId": 1, "statusId": 2, "docNumber": "100000023", "stateName": "Aktiv", "currencyId": 1, "statusName": "O'tkazilgan", "createdDate": "2026-06-25T12:00:56.882823", "currencyName": "Uzbek so'm", "bankAccountId": 12, "paymentTypeId": 2, "counterpartyId": 18, "organizationId": 8, "operationTypeId": 1, "paymentTypeName": "Bank", "counterpartyName": "Artel", "organizationName": "baraka_market", "bankAccountNumber": "20208000005157348001", "operationTypeName": "Kirim"}	{"id": 24, "amount": 2550000.00, "comment": null, "docDate": "2026-06-25T06:58:33", "stateId": 2, "statusId": 2, "docNumber": "100000023", "stateName": "Passiv", "currencyId": 1, "statusName": "O'tkazilgan", "createdDate": "2026-06-25T12:00:56.882823", "currencyName": "Uzbek so'm", "bankAccountId": 12, "paymentTypeId": 2, "counterpartyId": 18, "organizationId": 8, "operationTypeId": 1, "paymentTypeName": "Bank", "counterpartyName": "Artel", "organizationName": "baraka_market", "bankAccountNumber": "20208000005157348001", "operationTypeName": "Kirim"}	4	\N	\N	\N	2026-06-25 12:28:42.609262
142	8	public	pur_doc	70	INSERT	\N	{"id": 70, "lines": [], "comment": null, "docDate": "2026-06-25T16:04:07", "stateId": 1, "statusId": 1, "docNumber": "100000052", "stateName": "Aktiv", "vatAmount": 0.00, "contractId": 7, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-25T17:32:48.691929", "finalAmount": 22.00, "totalAmount": 22.00, "warehouseId": 7, "currencyName": "Uzbek so'm", "serviceLines": [{"id": 291, "price": 22.00, "amount": 22.00, "ownerId": 70, "quantity": 1.000, "serviceId": 4, "vatAmount": 0.00, "vatRateId": null, "itemTypeId": 2, "productName": null, "serviceName": "Xizmat", "totalAmount": 22.00, "vatRateName": null, "serialNumber": null, "markingNumber": null, "productTableId": null, "expenseAccountId": 1013, "expenseAccountName": "Основное производство"}], "warehouseName": "amonov", "contractNumber": "100000007", "counterpartyId": 16, "organizationId": 8, "counterpartyName": "as", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-25 17:32:50.602119
143	8	public	pur_doc	73	INSERT	\N	{"id": 73, "lines": [], "comment": null, "docDate": "2026-06-25T16:04:07", "stateId": 1, "statusId": 1, "docNumber": "100000055", "stateName": "Aktiv", "vatAmount": 0.00, "contractId": 7, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-25T17:35:30.481056", "finalAmount": 12000.00, "totalAmount": 12000.00, "warehouseId": 7, "currencyName": "Uzbek so'm", "serviceLines": [{"id": 294, "price": 12000.00, "amount": 12000.00, "ownerId": 73, "quantity": 1.000, "serviceId": 4, "vatAmount": 0.00, "vatRateId": null, "itemTypeId": 2, "productName": null, "serviceName": "Xizmat", "totalAmount": 12000.00, "vatRateName": null, "serialNumber": null, "markingNumber": null, "productTableId": null, "expenseAccountId": 1013, "expenseAccountName": "Основное производство"}], "warehouseName": "amonov", "contractNumber": "100000007", "counterpartyId": 16, "organizationId": 8, "counterpartyName": "as", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-25 17:35:32.120788
144	8	public	pur_doc	70	DELETE	{"id": 70, "lines": [], "comment": null, "docDate": "2026-06-25T16:04:07", "stateId": 1, "statusId": 1, "docNumber": "100000052", "stateName": "Aktiv", "vatAmount": 0.00, "contractId": 7, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-25T17:32:48.691929", "finalAmount": 22.00, "totalAmount": 22.00, "warehouseId": 7, "currencyName": "Uzbek so'm", "serviceLines": [{"id": 291, "price": 22.00, "amount": 22.00, "ownerId": 70, "quantity": 1.000, "serviceId": 4, "vatAmount": 0.00, "vatRateId": null, "itemTypeId": 2, "productName": null, "serviceName": "Xizmat", "totalAmount": 22.00, "vatRateName": null, "serialNumber": null, "markingNumber": null, "productTableId": null, "expenseAccountId": 1013, "expenseAccountName": "Основное производство"}], "warehouseName": "amonov", "contractNumber": "100000007", "counterpartyId": 16, "organizationId": 8, "counterpartyName": "as", "organizationName": "baraka_market"}	{"id": 70, "lines": [], "comment": null, "docDate": "2026-06-25T16:04:07", "stateId": 2, "statusId": 1, "docNumber": "100000052", "stateName": "Passiv", "vatAmount": 0.00, "contractId": 7, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-25T17:32:48.691929", "finalAmount": 22.00, "totalAmount": 22.00, "warehouseId": 7, "currencyName": "Uzbek so'm", "serviceLines": [], "warehouseName": "amonov", "contractNumber": "100000007", "counterpartyId": 16, "organizationId": 8, "counterpartyName": "as", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-25 17:35:42.73865
145	8	public	pur_doc	73	DELETE	{"id": 73, "lines": [], "comment": null, "docDate": "2026-06-25T16:04:07", "stateId": 1, "statusId": 1, "docNumber": "100000055", "stateName": "Aktiv", "vatAmount": 0.00, "contractId": 7, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-25T17:35:30.481056", "finalAmount": 12000.00, "totalAmount": 12000.00, "warehouseId": 7, "currencyName": "Uzbek so'm", "serviceLines": [{"id": 294, "price": 12000.00, "amount": 12000.00, "ownerId": 73, "quantity": 1.000, "serviceId": 4, "vatAmount": 0.00, "vatRateId": null, "itemTypeId": 2, "productName": null, "serviceName": "Xizmat", "totalAmount": 12000.00, "vatRateName": null, "serialNumber": null, "markingNumber": null, "productTableId": null, "expenseAccountId": 1013, "expenseAccountName": "Основное производство"}], "warehouseName": "amonov", "contractNumber": "100000007", "counterpartyId": 16, "organizationId": 8, "counterpartyName": "as", "organizationName": "baraka_market"}	{"id": 73, "lines": [], "comment": null, "docDate": "2026-06-25T16:04:07", "stateId": 2, "statusId": 1, "docNumber": "100000055", "stateName": "Passiv", "vatAmount": 0.00, "contractId": 7, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-25T17:35:30.481056", "finalAmount": 12000.00, "totalAmount": 12000.00, "warehouseId": 7, "currencyName": "Uzbek so'm", "serviceLines": [], "warehouseName": "amonov", "contractNumber": "100000007", "counterpartyId": 16, "organizationId": 8, "counterpartyName": "as", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-25 17:35:45.946733
146	8	public	pur_doc	74	INSERT	\N	{"id": 74, "lines": [], "comment": null, "docDate": "2026-06-25T17:35:32", "stateId": 1, "statusId": 1, "docNumber": "100000056", "stateName": "Aktiv", "vatAmount": 0.00, "contractId": null, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-25T17:36:31.46088", "finalAmount": 132500.00, "totalAmount": 132500.00, "warehouseId": 7, "currencyName": "Uzbek so'm", "serviceLines": [{"id": 295, "price": 132500.00, "amount": 132500.00, "ownerId": 74, "quantity": 1.000, "serviceId": 4, "vatAmount": 0.00, "vatRateId": null, "itemTypeId": 2, "productName": null, "serviceName": "Xizmat", "totalAmount": 132500.00, "vatRateName": null, "serialNumber": null, "markingNumber": null, "productTableId": null, "expenseAccountId": 1013, "expenseAccountName": "Основное производство"}], "warehouseName": "amonov", "contractNumber": null, "counterpartyId": 17, "organizationId": 8, "counterpartyName": "aaaa", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-25 17:36:31.532917
147	8	public	sale_doc	74	INSERT	\N	{"id": 74, "comment": null, "docDate": "2026-06-25T17:58:32.685299", "stateId": 1, "products": [{"id": 19, "amount": 750000.00, "tables": [], "quantity": 5.000, "costPrice": 0.00, "productId": 4, "unitPrice": 150000.00, "vatAmount": 0.00, "vatRateId": null, "productName": "ArtelTestTovar", "totalAmount": 750000.00, "vatRateName": null}, {"id": 20, "amount": 160000.00, "tables": [], "quantity": 1.000, "costPrice": 0.00, "productId": 6, "unitPrice": 160000.00, "vatAmount": 0.00, "vatRateId": null, "productName": "Product 2", "totalAmount": 160000.00, "vatRateName": null}], "statusId": 1, "docNumber": "100000072", "stateName": "Aktiv", "vatAmount": 0.00, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-25T17:58:32.685299", "finalAmount": 910000.00, "totalAmount": 910000.00, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-25 17:58:32.962497
148	8	public	sale_doc	75	INSERT	\N	{"id": 75, "comment": null, "docDate": "2026-06-26T14:48:52.74468", "stateId": 1, "products": [{"id": 21, "amount": 150000.00000000, "tables": [], "quantity": 1.000000, "costPrice": 0.00000000, "productId": 4, "unitPrice": 150000.00000000, "vatAmount": 0.00000000, "vatRateId": null, "productName": "ArtelTestTovar", "totalAmount": 150000.00000000, "vatRateName": null}, {"id": 22, "amount": 160000.00000000, "tables": [], "quantity": 1.000000, "costPrice": 0.00000000, "productId": 6, "unitPrice": 160000.00000000, "vatAmount": 0.00000000, "vatRateId": null, "productName": "Product 2", "totalAmount": 160000.00000000, "vatRateName": null}, {"id": 23, "amount": 170000.00000000, "tables": [], "quantity": 1.000000, "costPrice": 0.00000000, "productId": 7, "unitPrice": 170000.00000000, "vatAmount": 0.00000000, "vatRateId": null, "productName": "Product 3", "totalAmount": 170000.00000000, "vatRateName": null}], "statusId": 1, "docNumber": "100000073", "stateName": "Aktiv", "vatAmount": 0.00000000, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-26T14:48:52.74468", "finalAmount": 480000.00000000, "totalAmount": 480000.00000000, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-26 14:48:53.435085
149	8	public	sale_doc	76	INSERT	\N	{"id": 76, "comment": null, "docDate": "2026-06-26T14:51:00.936498", "stateId": 1, "products": [{"id": 24, "amount": 450000.00000000, "tables": [], "quantity": 3.000000, "costPrice": 0.00000000, "productId": 4, "unitPrice": 150000.00000000, "vatAmount": 0.00000000, "vatRateId": null, "productName": "ArtelTestTovar", "totalAmount": 450000.00000000, "vatRateName": null}], "statusId": 1, "docNumber": "100000074", "stateName": "Aktiv", "vatAmount": 0.00000000, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-26T14:51:00.936498", "finalAmount": 450000.00000000, "totalAmount": 450000.00000000, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-26 14:51:00.972295
150	8	public	pur_doc	75	INSERT	\N	{"id": 75, "lines": [{"id": 1, "items": [{"id": 1, "amount": 100000.00000000, "vatAmount": 12000.00000000, "vatRateId": 2, "totalAmount": 112000.00000000, "vatRateName": "QQS 12%", "serialNumber": null, "markingNumber": "i48vN8mkTXO6RAf0C4XTUsQt5aUBHw30", "productTableId": 438}], "amount": 100000.00000000, "unitId": 1, "ownerId": 75, "quantity": 1.000000, "unitName": "Dona", "productId": 4, "unitPrice": 100000.00000000, "vatAmount": 12000.00000000, "vatRateId": 2, "itemTypeId": 1, "productName": "ArtelTestTovar", "totalAmount": 112000.00000000, "vatRateName": "QQS 12%"}], "comment": null, "docDate": "2026-06-26T17:46:30", "stateId": 1, "statusId": 1, "docNumber": "100000057", "stateName": "Aktiv", "vatAmount": 12000.00000000, "contractId": 9, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-26T19:42:43.669111", "finalAmount": 112000.00000000, "totalAmount": 100000.00000000, "warehouseId": 7, "currencyName": "Uzbek so'm", "warehouseName": "amonov", "contractNumber": "100000009", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-26 19:42:45.075934
151	8	public	pur_doc	76	INSERT	\N	{"id": 76, "lines": [], "comment": null, "docDate": "2026-06-26T19:42:44", "stateId": 1, "statusId": 1, "docNumber": "100000058", "stateName": "Aktiv", "vatAmount": 0.00000000, "contractId": 7, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-26T19:50:07.068935", "finalAmount": 0.00000000, "totalAmount": 0.00000000, "warehouseId": 7, "currencyName": "Uzbek so'm", "warehouseName": "amonov", "contractNumber": "100000007", "counterpartyId": 16, "organizationId": 8, "counterpartyName": "as", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-26 19:50:08.431569
152	8	public	pur_doc	77	INSERT	\N	{"id": 77, "lines": [], "comment": null, "docDate": "2026-06-26T20:05:08", "stateId": 1, "statusId": 1, "docNumber": "100000059", "stateName": "Aktiv", "vatAmount": 0.00000000, "contractId": null, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-26T20:15:21.243121", "finalAmount": 0.00000000, "totalAmount": 0.00000000, "warehouseId": 7, "currencyName": "Uzbek so'm", "warehouseName": "amonov", "contractNumber": null, "counterpartyId": 17, "organizationId": 8, "counterpartyName": "aaaa", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-26 20:15:21.336939
154	8	public	pur_doc	79	INSERT	\N	{"id": 79, "lines": [], "comment": null, "docDate": "2026-06-27T10:09:37", "stateId": 1, "statusId": 1, "docNumber": "100000061", "stateName": "Aktiv", "vatAmount": 0.00000000, "contractId": null, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-27T10:09:57.973753", "finalAmount": 0.00000000, "totalAmount": 0.00000000, "warehouseId": 7, "currencyName": "Uzbek so'm", "warehouseName": "amonov", "contractNumber": null, "counterpartyId": 17, "organizationId": 8, "counterpartyName": "aaaa", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-27 10:09:59.066456
155	8	public	pur_doc	80	INSERT	\N	{"id": 80, "lines": [{"id": 2, "items": [{"id": 2, "amount": 100000.00000000, "vatAmount": 15000.00000000, "vatRateId": 3, "totalAmount": 115000.00000000, "vatRateName": "QQS 15%", "serialNumber": null, "markingNumber": "cfgvhbjkkoiuytr67890", "productTableId": 439}, {"id": 3, "amount": 100000.00000000, "vatAmount": 15000.00000000, "vatRateId": 3, "totalAmount": 115000.00000000, "vatRateName": "QQS 15%", "serialNumber": null, "markingNumber": "hgfc45gd54wsrxd56edc", "productTableId": 440}], "amount": 200000.00000000, "unitId": 2, "ownerId": 80, "quantity": 2.000000, "unitName": "Kilogram", "productId": 13, "unitPrice": 100000.00000000, "vatAmount": 30000.00000000, "vatRateId": 3, "itemTypeId": 1, "productName": "Product 5", "totalAmount": 230000.00000000, "vatRateName": "QQS 15%"}], "comment": null, "docDate": "2026-06-27T10:09:58", "stateId": 1, "statusId": 1, "docNumber": "100000062", "stateName": "Aktiv", "vatAmount": 30000.00000000, "contractId": null, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-27T10:12:39.349314", "finalAmount": 230000.00000000, "totalAmount": 200000.00000000, "warehouseId": 7, "currencyName": "Uzbek so'm", "warehouseName": "amonov", "contractNumber": null, "counterpartyId": 17, "organizationId": 8, "counterpartyName": "aaaa", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-27 10:12:40.281056
156	8	public	pur_doc	81	INSERT	\N	{"id": 81, "lines": [], "comment": null, "docDate": "2026-06-27T10:38:46", "stateId": 1, "statusId": 1, "docNumber": "100000063", "stateName": "Aktiv", "vatAmount": 0.00000000, "contractId": null, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-27T10:39:10.296098", "finalAmount": 0.00000000, "totalAmount": 0.00000000, "warehouseId": 7, "currencyName": "Uzbek so'm", "warehouseName": "amonov", "contractNumber": null, "counterpartyId": 17, "organizationId": 8, "counterpartyName": "aaaa", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-27 10:39:11.342589
157	8	public	pur_doc	82	INSERT	\N	{"id": 82, "lines": [], "comment": null, "docDate": "2026-06-27T10:44:57", "stateId": 1, "statusId": 1, "docNumber": "100000064", "stateName": "Aktiv", "vatAmount": 0.00000000, "contractId": 10, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-27T10:45:44.175788", "finalAmount": 0.00000000, "totalAmount": 0.00000000, "warehouseId": 7, "currencyName": "Uzbek so'm", "warehouseName": "amonov", "contractNumber": "100000010", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-27 10:45:44.352001
158	8	public	pur_doc	83	INSERT	\N	{"id": 83, "lines": [], "comment": null, "docDate": "2026-06-27T10:44:57", "stateId": 1, "statusId": 1, "docNumber": "100000065", "stateName": "Aktiv", "vatAmount": 0.00000000, "contractId": 10, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-27T10:46:53.037448", "finalAmount": 0.00000000, "totalAmount": 0.00000000, "warehouseId": 7, "currencyName": "Uzbek so'm", "warehouseName": "amonov", "contractNumber": "100000010", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-27 10:46:53.103659
159	8	public	pur_doc	84	INSERT	\N	{"id": 84, "lines": [{"id": 3, "items": [], "amount": 3400000.00000000, "unitId": 5, "ownerId": 84, "quantity": 1700.000000, "unitName": "Xizmat", "productId": 18, "unitPrice": 2000.00000000, "vatAmount": 408000.00000000, "vatRateId": 2, "productName": "Elektr energiya xizmatlari", "totalAmount": 3808000.00000000, "vatRateName": "QQS 12%"}], "comment": null, "docDate": "2026-06-27T10:44:57", "stateId": 1, "statusId": 1, "docNumber": "100000066", "stateName": "Aktiv", "vatAmount": 408000.00000000, "contractId": 10, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-27T11:14:59.360757", "finalAmount": 3808000.00000000, "totalAmount": 3400000.00000000, "warehouseId": 7, "currencyName": "Uzbek so'm", "warehouseName": "amonov", "contractNumber": "100000010", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-27 11:15:01.305875
160	8	public	pur_doc	85	INSERT	\N	{"id": 85, "lines": [{"id": 4, "items": [], "amount": 10000000.00000000, "unitId": 1, "ownerId": 85, "quantity": 1000.000000, "unitName": "Dona", "productId": 6, "unitPrice": 10000.00000000, "vatAmount": 1200000.00000000, "vatRateId": 2, "productName": "Product 2", "totalAmount": 11200000.00000000, "vatRateName": "QQS 12%"}], "comment": null, "docDate": "2026-06-27T10:45:39", "stateId": 1, "statusId": 1, "docNumber": "100000067", "stateName": "Aktiv", "vatAmount": 1200000.00000000, "contractId": 9, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-27T11:18:30.099871", "finalAmount": 11200000.00000000, "totalAmount": 10000000.00000000, "warehouseId": 7, "currencyName": "Uzbek so'm", "warehouseName": "amonov", "contractNumber": "100000009", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-27 11:18:30.20445
161	8	public	pur_doc	86	INSERT	\N	{"id": 86, "lines": [{"id": 5, "items": [], "amount": 10000000.00000000, "unitId": 1, "ownerId": 86, "quantity": 10000.000000, "unitName": "Dona", "productId": 4, "unitPrice": 1000.00000000, "vatAmount": 1200000.00000000, "vatRateId": 2, "productName": "ArtelTestTovar", "totalAmount": 11200000.00000000, "vatRateName": "QQS 12%"}, {"id": 6, "items": [], "amount": 10000000.00000000, "unitId": 2, "ownerId": 86, "quantity": 1000.000000, "unitName": "Kilogram", "productId": 5, "unitPrice": 10000.00000000, "vatAmount": 1200000.00000000, "vatRateId": 2, "productName": "qoshiqcha", "totalAmount": 11200000.00000000, "vatRateName": "QQS 12%"}], "comment": null, "docDate": "2026-06-27T11:18:28", "stateId": 1, "statusId": 1, "docNumber": "100000068", "stateName": "Aktiv", "vatAmount": 2400000.00000000, "contractId": 9, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-27T12:16:01.123909", "finalAmount": 22400000.00000000, "totalAmount": 20000000.00000000, "warehouseId": 7, "currencyName": "Uzbek so'm", "warehouseName": "amonov", "contractNumber": "100000009", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-27 12:16:01.257224
162	8	public	pur_doc	87	INSERT	\N	{"id": 87, "lines": [{"id": 7, "items": [], "amount": 10000.00000000, "unitId": 4, "ownerId": 87, "quantity": 1.000000, "unitName": "Metr", "productId": 3, "unitPrice": 10000.00000000, "vatAmount": 1200.00000000, "vatRateId": 2, "productName": "kil", "totalAmount": 11200.00000000, "vatRateName": "QQS 12%"}, {"id": 8, "items": [], "amount": 40000.00000000, "unitId": 1, "ownerId": 87, "quantity": 2.000000, "unitName": "Dona", "productId": 4, "unitPrice": 20000.00000000, "vatAmount": 4800.00000000, "vatRateId": 2, "productName": "ArtelTestTovar", "totalAmount": 44800.00000000, "vatRateName": "QQS 12%"}], "comment": null, "docDate": "2026-06-27T12:21:47", "stateId": 1, "statusId": 1, "docNumber": "100000069", "stateName": "Aktiv", "vatAmount": 6000.00000000, "contractId": 10, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-27T12:25:05.607073", "finalAmount": 56000.00000000, "totalAmount": 50000.00000000, "warehouseId": 7, "currencyName": "Uzbek so'm", "warehouseName": "amonov", "contractNumber": "100000010", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-27 12:25:05.663027
163	8	public	pur_doc	88	INSERT	\N	{"id": 88, "lines": [{"id": 9, "items": [{"id": 4, "amount": 10000.00000000, "vatAmount": 1200.00000000, "vatRateId": 2, "totalAmount": 11200.00000000, "vatRateName": "QQS 12%", "serialNumber": null, "markingNumber": "0104780126311421217mGiFdVNlMDCVqobU&jk", "productTableId": 444}, {"id": 5, "amount": 10000.00000000, "vatAmount": 1200.00000000, "vatRateId": 2, "totalAmount": 11200.00000000, "vatRateName": "QQS 12%", "serialNumber": null, "markingNumber": "010478012631142121E.FuBJE!HscaUpKlA.H/", "productTableId": 445}], "amount": 20000.00000000, "unitId": 1, "ownerId": 88, "quantity": 2.000000, "unitName": "Dona", "productId": 20, "unitPrice": 10000.00000000, "vatAmount": 2400.00000000, "vatRateId": 2, "productName": "Milliy bank", "totalAmount": 22400.00000000, "vatRateName": "QQS 12%"}, {"id": 10, "items": [], "amount": 40000.00000000, "unitId": 1, "ownerId": 88, "quantity": 2.000000, "unitName": "Dona", "productId": 4, "unitPrice": 20000.00000000, "vatAmount": 6000.00000000, "vatRateId": 3, "productName": "ArtelTestTovar", "totalAmount": 46000.00000000, "vatRateName": "QQS 15%"}], "comment": null, "docDate": "2026-06-27T12:25:04", "stateId": 1, "statusId": 1, "docNumber": "100000070", "stateName": "Aktiv", "vatAmount": 8400.00000000, "contractId": 9, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-27T12:29:47.339191", "finalAmount": 68400.00000000, "totalAmount": 60000.00000000, "warehouseId": 7, "currencyName": "Uzbek so'm", "warehouseName": "amonov", "contractNumber": "100000009", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-27 12:29:47.5613
164	8	public	pur_doc	89	INSERT	\N	{"id": 89, "lines": [{"id": 11, "items": [{"id": 6, "amount": 25000.00000000, "vatAmount": 3000.00000000, "vatRateId": 2, "totalAmount": 28000.00000000, "vatRateName": "QQS 12%", "serialNumber": null, "markingNumber": "qwww0104780126311421217mGiFdVNlMDCVqobU&jk", "productTableId": 446}, {"id": 7, "amount": 25000.00000000, "vatAmount": 3000.00000000, "vatRateId": 2, "totalAmount": 28000.00000000, "vatRateName": "QQS 12%", "serialNumber": null, "markingNumber": "lkjj0104780126311421217mGiFdVNlMDCVqobU&jk", "productTableId": 447}], "amount": 50000.00000000, "unitId": 1, "ownerId": 89, "quantity": 2.000000, "unitName": "Dona", "productId": 21, "unitPrice": 25000.00000000, "vatAmount": 6000.00000000, "vatRateId": 2, "productMxik": null, "productName": "Milliy bank", "totalAmount": 56000.00000000, "vatRateName": "QQS 12%"}], "comment": null, "docDate": "2026-05-15T12:55:00", "stateId": 1, "statusId": 1, "docNumber": "100000071", "stateName": "Aktiv", "vatAmount": 6000.00000000, "contractId": 9, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-27T12:57:13.311281", "finalAmount": 56000.00000000, "totalAmount": 50000.00000000, "warehouseId": 7, "currencyName": "Uzbek so'm", "warehouseName": "amonov", "contractNumber": "100000009", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-27 12:57:15.228864
165	8	public	pur_doc	90	INSERT	\N	{"id": 90, "lines": [{"id": 12, "items": [{"id": 8, "amount": 30000.00000000, "vatAmount": 4500.00000000, "vatRateId": 3, "totalAmount": 34500.00000000, "vatRateName": "QQS 15%", "serialNumber": null, "markingNumber": "qwqwq0104780126311421217mGiFdVNlMDCVqobU&jk", "productTableId": 448}, {"id": 9, "amount": 30000.00000000, "vatAmount": 4500.00000000, "vatRateId": 3, "totalAmount": 34500.00000000, "vatRateName": "QQS 15%", "serialNumber": null, "markingNumber": "qwsadwd0104780126311421217mGiFdVNlMDCVqobU&jk", "productTableId": 449}, {"id": 10, "amount": 30000.00000000, "vatAmount": 4500.00000000, "vatRateId": 3, "totalAmount": 34500.00000000, "vatRateName": "QQS 15%", "serialNumber": null, "markingNumber": "qewfddfsd0104780126311421217mGiFdVNlMDCVqobU&jk", "productTableId": 450}], "amount": 90000.00000000, "unitId": 1, "ownerId": 90, "quantity": 3.000000, "unitName": "Dona", "productId": 20, "unitPrice": 30000.00000000, "vatAmount": 13500.00000000, "vatRateId": 3, "productMxik": null, "productName": "Milliy bank", "totalAmount": 103500.00000000, "vatRateName": "QQS 15%"}], "comment": null, "docDate": "2026-06-01T12:57:00", "stateId": 1, "statusId": 1, "docNumber": "100000072", "stateName": "Aktiv", "vatAmount": 13500.00000000, "contractId": 9, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-27T12:58:32.876757", "finalAmount": 103500.00000000, "totalAmount": 90000.00000000, "warehouseId": 7, "currencyName": "Uzbek so'm", "warehouseName": "amonov", "contractNumber": "100000009", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-27 12:58:33.032486
166	8	public	pur_doc	91	INSERT	\N	{"id": 91, "lines": [{"id": 13, "items": [{"id": 11, "amount": 32000.00000000, "vatAmount": 3840.00000000, "vatRateId": 2, "totalAmount": 35840.00000000, "vatRateName": "QQS 12%", "serialNumber": null, "markingNumber": "7412gdgd0104780126311421217mGiFdVNlMDCVqobU&jk", "productTableId": 451}, {"id": 12, "amount": 32000.00000000, "vatAmount": 3840.00000000, "vatRateId": 2, "totalAmount": 35840.00000000, "vatRateName": "QQS 12%", "serialNumber": null, "markingNumber": "defwfwgerger0104780126311421217mGiFdVNlMDCVqobU&jk", "productTableId": 452}, {"id": 13, "amount": 32000.00000000, "vatAmount": 3840.00000000, "vatRateId": 2, "totalAmount": 35840.00000000, "vatRateName": "QQS 12%", "serialNumber": null, "markingNumber": "wdfrwefeff0104780126311421217mGiFdVNlMDCVqobU&jk", "productTableId": 453}, {"id": 14, "amount": 32000.00000000, "vatAmount": 3840.00000000, "vatRateId": 2, "totalAmount": 35840.00000000, "vatRateName": "QQS 12%", "serialNumber": null, "markingNumber": "74qwdDQEWdqwDW0104780126311421217mGiFdVNlMDCVqobU&jk", "productTableId": 454}], "amount": 128000.00000000, "unitId": 1, "ownerId": 91, "quantity": 4.000000, "unitName": "Dona", "productId": 20, "unitPrice": 32000.00000000, "vatAmount": 15360.00000000, "vatRateId": 2, "productMxik": null, "productName": "Milliy bank", "totalAmount": 143360.00000000, "vatRateName": "QQS 12%"}], "comment": null, "docDate": "2026-06-20T12:58:00", "stateId": 1, "statusId": 1, "docNumber": "100000073", "stateName": "Aktiv", "vatAmount": 15360.00000000, "contractId": 9, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-27T12:59:38.390107", "finalAmount": 143360.00000000, "totalAmount": 128000.00000000, "warehouseId": 7, "currencyName": "Uzbek so'm", "warehouseName": "amonov", "contractNumber": "100000009", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-27 12:59:38.456583
167	8	public	pur_doc	92	INSERT	\N	{"id": 92, "lines": [{"id": 14, "items": [{"id": 15, "amount": 10000.00000000, "vatAmount": 1200.00000000, "vatRateId": 2, "totalAmount": 11200.00000000, "vatRateName": "QQS 12%", "serialNumber": null, "markingNumber": "0104780074206893217UkCJ6Gu*gG_j.wf6WnX91XUWh92JkB/xWNpbjVhcmtkWVdsT3lhVTVhYjdUQm8yQgOW3lE=", "productTableId": 455}, {"id": 16, "amount": 10000.00000000, "vatAmount": 1200.00000000, "vatRateId": 2, "totalAmount": 11200.00000000, "vatRateName": "QQS 12%", "serialNumber": null, "markingNumber": "010478007420371721NZVF0x+-fnd<lcpI11Wc91UZF092SDB2UZccAGRfCmSPNYJYumzGxmcnTaumTSS1HOIBuOE=", "productTableId": 456}, {"id": 17, "amount": 10000.00000000, "vatAmount": 1200.00000000, "vatRateId": 2, "totalAmount": 11200.00000000, "vatRateName": "QQS 12%", "serialNumber": null, "markingNumber": "0104780074203717210t*wRjFf>m9eK-'dqeRV91UZF092b0xBdzcw+3GSPfZP95mjWrYDKhom4PWWecsUayvv2sQ=", "productTableId": 457}, {"id": 18, "amount": 10000.00000000, "vatAmount": 1200.00000000, "vatRateId": 2, "totalAmount": 11200.00000000, "vatRateName": "QQS 12%", "serialNumber": null, "markingNumber": "010478010915122821)nUfDN1p=8G.G2nk8_sJ91UZF092d3lJcOvPHgtC3DxncIcpDZVxRY9BDkfyC1+gYcgxFyQ=", "productTableId": 458}], "amount": 40000.00000000, "unitId": 1, "ownerId": 92, "quantity": 4.000000, "unitName": "Dona", "productId": 23, "unitPrice": 10000.00000000, "vatAmount": 4800.00000000, "vatRateId": 2, "productMxik": "08418001001005219", "productName": "ARTEL, икки камерали HD 316 FND ECO FROST қора-жилосиз ранг", "totalAmount": 44800.00000000, "vatRateName": "QQS 12%"}], "comment": null, "docDate": "2026-06-27T18:05:43", "stateId": 1, "statusId": 1, "docNumber": "100000074", "stateName": "Aktiv", "vatAmount": 4800.00000000, "contractId": 9, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-27T18:07:17.501272", "finalAmount": 44800.00000000, "totalAmount": 40000.00000000, "warehouseId": 7, "currencyName": "Uzbek so'm", "warehouseName": "amonov", "contractNumber": "100000009", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-27 18:07:18.873712
168	8	public	pur_doc	93	INSERT	\N	{"id": 93, "lines": [{"id": 15, "items": [], "amount": 10000.00000000, "unitId": 5, "ownerId": 93, "quantity": 2.000000, "unitName": "Xizmat", "productId": 25, "unitPrice": 5000.00000000, "vatAmount": 1200.00000000, "vatRateId": 2, "productMxik": "09903001001000000", "productName": "Газ таъминоти хизматлари", "totalAmount": 11200.00000000, "vatRateName": "QQS 12%"}], "comment": null, "docDate": "2026-06-27T18:07:17", "stateId": 1, "statusId": 1, "docNumber": "100000075", "stateName": "Aktiv", "vatAmount": 1200.00000000, "contractId": 10, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-27T18:07:50.767417", "finalAmount": 11200.00000000, "totalAmount": 10000.00000000, "warehouseId": 7, "currencyName": "Uzbek so'm", "warehouseName": "amonov", "contractNumber": "100000010", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-27 18:07:50.878324
169	8	public	pur_doc	94	INSERT	\N	{"id": 94, "lines": [{"id": 16, "items": [], "amount": 10000.00000000, "unitId": 5, "ownerId": 94, "quantity": 1.000000, "unitName": "Xizmat", "productId": 25, "unitPrice": 10000.00000000, "vatAmount": 1200.00000000, "vatRateId": 2, "productMxik": "09903001001000000", "productName": "Газ таъминоти хизматлари", "totalAmount": 11200.00000000, "vatRateName": "QQS 12%"}], "comment": null, "docDate": "2026-06-29T10:06:32", "stateId": 1, "statusId": 1, "docNumber": "100000076", "stateName": "Aktiv", "vatAmount": 1200.00000000, "contractId": 9, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-29T10:13:54.392765", "finalAmount": 11200.00000000, "totalAmount": 10000.00000000, "warehouseId": 7, "currencyName": "Uzbek so'm", "warehouseName": "amonov", "contractNumber": "100000009", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-29 10:13:54.464506
170	8	public	pur_doc	95	INSERT	\N	{"id": 95, "lines": [{"id": 17, "items": [{"id": 19, "amount": 11000.00000000, "vatAmount": 1320.00000000, "vatRateId": 2, "totalAmount": 12320.00000000, "vatRateName": "QQS 12%", "serialNumber": null, "markingNumber": "qwdqdqwdqwdqwdqdqd", "productTableId": 459}, {"id": 20, "amount": 11000.00000000, "vatAmount": 1320.00000000, "vatRateId": 2, "totalAmount": 12320.00000000, "vatRateName": "QQS 12%", "serialNumber": null, "markingNumber": "qdqdqdqdqdqdqdqdqdq2855", "productTableId": 460}, {"id": 21, "amount": 11000.00000000, "vatAmount": 1320.00000000, "vatRateId": 2, "totalAmount": 12320.00000000, "vatRateName": "QQS 12%", "serialNumber": null, "markingNumber": "qdqdqdqdqdq98d4q8d789qwd7q", "productTableId": 461}], "amount": 33000.00000000, "unitId": 1, "ownerId": 95, "quantity": 3.000000, "unitName": "Dona", "productId": 23, "unitPrice": 11000.00000000, "vatAmount": 3960.00000000, "vatRateId": 2, "productMxik": "08418001001005219", "productName": "ARTEL, икки камерали HD 316 FND ECO FROST қора-жилосиз ранг", "totalAmount": 36960.00000000, "vatRateName": "QQS 12%"}], "comment": null, "docDate": "2026-06-15T14:58:00", "stateId": 1, "statusId": 1, "docNumber": "100000077", "stateName": "Aktiv", "vatAmount": 3960.00000000, "contractId": 9, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-29T14:59:14.796206", "finalAmount": 36960.00000000, "totalAmount": 33000.00000000, "warehouseId": 7, "currencyName": "Uzbek so'm", "warehouseName": "amonov", "contractNumber": "100000009", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-29 14:59:16.470506
171	8	public	pur_doc	96	INSERT	\N	{"id": 96, "lines": [{"id": 18, "items": [{"id": 22, "amount": 15000.00000000, "vatAmount": 2250.00000000, "vatRateId": 3, "totalAmount": 17250.00000000, "vatRateName": "QQS 15%", "serialNumber": null, "markingNumber": "fwliejfoiwjfopwjp'ef856", "productTableId": 462}, {"id": 23, "amount": 15000.00000000, "vatAmount": 2250.00000000, "vatRateId": 3, "totalAmount": 17250.00000000, "vatRateName": "QQS 15%", "serialNumber": null, "markingNumber": "aohjfoiwhio", "productTableId": 463}], "amount": 30000.00000000, "unitId": 1, "ownerId": 96, "quantity": 2.000000, "unitName": "Dona", "productId": 24, "unitPrice": 15000.00000000, "vatAmount": 4500.00000000, "vatRateId": 3, "productMxik": "08418001001005223", "productName": "ARTEL, икки камерали HD 341 FND ECO FROST ёмғирли-асфалт ранг", "totalAmount": 34500.00000000, "vatRateName": "QQS 15%"}], "comment": null, "docDate": "2026-06-29T14:59:15", "stateId": 1, "statusId": 1, "docNumber": "100000078", "stateName": "Aktiv", "vatAmount": 4500.00000000, "contractId": 10, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-29T15:01:54.008454", "finalAmount": 34500.00000000, "totalAmount": 30000.00000000, "warehouseId": 7, "currencyName": "Uzbek so'm", "warehouseName": "amonov", "contractNumber": "100000010", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-29 15:01:54.179718
183	8	public	cash_operation	3	INSERT	\N	{"id": 3, "amount": 2550000.00, "comment": "summmm", "docDate": "2026-07-02T17:07:58", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000001", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-02T17:08:12.108429", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": null, "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "operationTypeId": 2, "paymentTypeName": null, "counterpartyName": "Artel", "organizationName": "baraka_market", "cancelledByUserId": null, "operationTypeName": "Chiqim", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-02 17:08:12.314219
172	8	public	sale_doc	78	INSERT	\N	{"id": 78, "comment": null, "docDate": "2026-06-29T16:42:30.843443", "stateId": 1, "products": [{"id": 26, "amount": 18975.00000000, "tables": [], "unitId": 1, "quantity": 1.000000, "unitName": "Dona", "costPrice": 17250.00000000, "productId": 24, "unitPrice": 18975.00000000, "vatAmount": 2277.00000000, "vatRateId": 2, "productName": "ARTEL, икки камерали HD 341 FND ECO FROST ёмғирли-асфалт ранг", "totalAmount": 21252.00000000, "vatRateName": "QQS 12%"}], "statusId": 1, "docNumber": "100000076", "stateName": "Aktiv", "vatAmount": 2277.00000000, "contractId": 9, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-29T16:42:30.843443", "finalAmount": 21252.00000000, "totalAmount": 18975.00000000, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "contractNumber": "100000009", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-29 16:42:31.817327
173	8	public	sale_doc	78	UPDATE	\N	{"id": 78, "comment": null, "docDate": "2026-06-29T16:42:30.843443", "stateId": 1, "products": [{"id": 26, "amount": 18975.00000000, "tables": [{"id": 98, "amount": 18975.00000000, "costPrice": 17250.00000000, "vatAmount": 2277.00000000, "vatRateId": 2, "totalAmount": 21252.00000000, "serialNumber": null, "markingNumber": "fwliejfoiwjfopwjp'ef856", "productTableId": 462}], "unitId": 1, "quantity": 1.000000, "unitName": "Dona", "costPrice": 17250.00000000, "productId": 24, "unitPrice": 18975.00000000, "vatAmount": 2277.00000000, "vatRateId": 2, "productName": "ARTEL, икки камерали HD 341 FND ECO FROST ёмғирли-асфалт ранг", "totalAmount": 21252.00000000, "vatRateName": "QQS 12%"}], "statusId": 4, "docNumber": "100000076", "stateName": "Aktiv", "vatAmount": 2277.00000000, "contractId": 9, "currencyId": 1, "statusName": "Kutilmoqda", "createdDate": "2026-06-29T16:42:30.843443", "finalAmount": 21252.00000000, "totalAmount": 18975.00000000, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "contractNumber": "100000009", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-29 16:43:23.139744
174	8	public	sale_doc	79	INSERT	\N	{"id": 79, "comment": "FIFO bo'yicha sotilyapti", "docDate": "2026-06-29T17:21:54.01643", "stateId": 1, "products": [{"id": 27, "amount": 65296.00000000, "tables": [], "unitId": 1, "quantity": 5.000000, "unitName": "Dona", "costPrice": 11872.00000000, "productId": 23, "unitPrice": 13059.20000000, "vatAmount": 7835.52000000, "vatRateId": 2, "productName": "ARTEL, икки камерали HD 316 FND ECO FROST қора-жилосиз ранг", "totalAmount": 73131.52000000, "vatRateName": "QQS 12%"}], "statusId": 1, "docNumber": "100000077", "stateName": "Aktiv", "vatAmount": 7835.52000000, "contractId": 9, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-29T17:21:54.01643", "finalAmount": 73131.52000000, "totalAmount": 65296.00000000, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "contractNumber": "100000009", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-29 17:21:54.10886
175	8	public	sale_doc	79	DELETE	{"id": 79, "comment": "FIFO bo'yicha sotilyapti", "docDate": "2026-06-29T17:21:54.01643", "stateId": 1, "products": [{"id": 27, "amount": 65296.00000000, "tables": [], "unitId": 1, "quantity": 5.000000, "unitName": "Dona", "costPrice": 11872.00000000, "productId": 23, "unitPrice": 13059.20000000, "vatAmount": 7835.52000000, "vatRateId": 2, "productName": "ARTEL, икки камерали HD 316 FND ECO FROST қора-жилосиз ранг", "totalAmount": 73131.52000000, "vatRateName": "QQS 12%"}], "statusId": 1, "docNumber": "100000077", "stateName": "Aktiv", "vatAmount": 7835.52000000, "contractId": 9, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-29T17:21:54.01643", "finalAmount": 73131.52000000, "totalAmount": 65296.00000000, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "contractNumber": "100000009", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	{"id": 79, "comment": "FIFO bo'yicha sotilyapti", "docDate": "2026-06-29T17:21:54.01643", "stateId": 2, "products": [], "statusId": 1, "docNumber": "100000077", "stateName": "Passiv", "vatAmount": 7835.52000000, "contractId": 9, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-29T17:21:54.01643", "finalAmount": 73131.52000000, "totalAmount": 65296.00000000, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "contractNumber": "100000009", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-29 17:23:40.739959
176	8	public	sale_doc	80	INSERT	\N	{"id": 80, "comment": null, "docDate": "2026-06-29T17:36:29.055026", "stateId": 1, "products": [{"id": 28, "amount": 65296.00000000, "tables": [], "unitId": 1, "quantity": 5.000000, "unitName": "Dona", "costPrice": 11872.00000000, "productId": 23, "unitPrice": 13059.20000000, "vatAmount": 7835.52000000, "vatRateId": 2, "productName": "ARTEL, икки камерали HD 316 FND ECO FROST қора-жилосиз ранг", "totalAmount": 73131.52000000, "vatRateName": "QQS 12%"}], "statusId": 1, "docNumber": "100000078", "stateName": "Aktiv", "vatAmount": 7835.52000000, "contractId": 10, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-29T17:36:29.055026", "finalAmount": 73131.52000000, "totalAmount": 65296.00000000, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "contractNumber": "100000010", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-29 17:36:29.093782
189	8	public	cash_operation	5	INSERT	\N	{"id": 5, "amount": 10000000.00, "comment": "create test", "docDate": "2026-07-03T11:37:07", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000003", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-03T11:37:32.607436", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": null, "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "operationTypeId": 1, "paymentTypeName": null, "counterpartyName": "Artel", "organizationName": "baraka_market", "cancelledByUserId": null, "operationTypeName": "Kirim", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-03 11:37:32.683396
177	8	public	pur_doc	97	INSERT	\N	{"id": 97, "lines": [{"id": 19, "items": [], "amount": 20000.00000000, "unitId": 5, "ownerId": 97, "quantity": 2.000000, "unitName": "Xizmat", "productId": 25, "unitPrice": 10000.00000000, "vatAmount": 2400.00000000, "vatRateId": 2, "productMxik": "09903001001000000", "productName": "Газ таъминоти хизматлари", "totalAmount": 22400.00000000, "vatRateName": "QQS 12%"}], "comment": null, "docDate": "2026-06-29T17:55:00", "stateId": 1, "statusId": 1, "docNumber": "100000079", "stateName": "Aktiv", "vatAmount": 2400.00000000, "contractId": 9, "currencyId": 1, "statusName": "Qoralama", "createdDate": "2026-06-29T17:57:51.597146", "finalAmount": 22400.00000000, "totalAmount": 20000.00000000, "warehouseId": 7, "currencyName": "Uzbek so'm", "warehouseName": "amonov", "contractNumber": "100000009", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-29 17:57:53.167841
178	8	public	sale_doc	78	UPDATE	{"id": 78, "lines": [{"id": 26, "items": [{"id": 98, "amount": 18975.00000000, "costPrice": 17250.00000000, "vatAmount": 2277.00000000, "vatRateId": 2, "totalAmount": 21252.00000000, "serialNumber": null, "markingNumber": "fwliejfoiwjfopwjp'ef856", "productTableId": 462}], "amount": 18975.00000000, "unitId": 1, "quantity": 1.000000, "unitName": "Dona", "costPrice": 17250.00000000, "productId": 24, "unitPrice": 18975.00000000, "vatAmount": 2277.00000000, "vatRateId": 2, "productMxik": "08418001001005223", "productName": "ARTEL, икки камерали HD 341 FND ECO FROST ёмғирли-асфалт ранг", "totalAmount": 21252.00000000, "vatRateName": "QQS 12%"}], "comment": null, "docDate": "2026-06-29T16:42:30.843443", "stateId": 1, "statusId": 4, "docNumber": "100000076", "stateName": "Aktiv", "vatAmount": 2277.00000000, "contractId": 9, "currencyId": 1, "statusName": "Kutilmoqda", "createdDate": "2026-06-29T16:42:30.843443", "finalAmount": 21252.00000000, "totalAmount": 18975.00000000, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "contractNumber": "100000009", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	{"id": 78, "lines": [{"id": 26, "items": [{"id": 98, "amount": 18975.00000000, "costPrice": 17250.00000000, "vatAmount": 2277.00000000, "vatRateId": 2, "totalAmount": 21252.00000000, "serialNumber": null, "markingNumber": "fwliejfoiwjfopwjp'ef856", "productTableId": 462}], "amount": 18975.00000000, "unitId": 1, "quantity": 1.000000, "unitName": "Dona", "costPrice": 17250.00000000, "productId": 24, "unitPrice": 18975.00000000, "vatAmount": 2277.00000000, "vatRateId": 2, "productMxik": "08418001001005223", "productName": "ARTEL, икки камерали HD 341 FND ECO FROST ёмғирли-асфалт ранг", "totalAmount": 21252.00000000, "vatRateName": "QQS 12%"}], "comment": null, "docDate": "2026-06-29T16:42:30.843443", "stateId": 1, "statusId": 3, "docNumber": "100000076", "stateName": "Aktiv", "vatAmount": 2277.00000000, "contractId": 9, "currencyId": 1, "statusName": "Bekor qilingan", "createdDate": "2026-06-29T16:42:30.843443", "finalAmount": 21252.00000000, "totalAmount": 18975.00000000, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "contractNumber": "100000009", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-30 10:32:28.663418
179	8	public	sale_doc	80	UPDATE	\N	{"id": 80, "lines": [{"id": 28, "items": [{"id": 99, "amount": 13059.20000000, "costPrice": 12320.00000000, "vatAmount": 1567.10000000, "vatRateId": 2, "totalAmount": 14626.30000000, "serialNumber": null, "markingNumber": "qwdqdqwdqwdqwdqdqd", "productTableId": 459}, {"id": 100, "amount": 13059.20000000, "costPrice": 12320.00000000, "vatAmount": 1567.10000000, "vatRateId": 2, "totalAmount": 14626.30000000, "serialNumber": null, "markingNumber": "qdqdqdqdqdqdqdqdqdq2855", "productTableId": 460}, {"id": 101, "amount": 13059.20000000, "costPrice": 12320.00000000, "vatAmount": 1567.10000000, "vatRateId": 2, "totalAmount": 14626.30000000, "serialNumber": null, "markingNumber": "qdqdqdqdqdq98d4q8d789qwd7q", "productTableId": 461}, {"id": 102, "amount": 13059.20000000, "costPrice": 11200.00000000, "vatAmount": 1567.10000000, "vatRateId": 2, "totalAmount": 14626.30000000, "serialNumber": null, "markingNumber": "0104780074206893217UkCJ6Gu*gG_j.wf6WnX91XUWh92JkB/xWNpbjVhcmtkWVdsT3lhVTVhYjdUQm8yQgOW3lE=", "productTableId": 455}, {"id": 103, "amount": 13059.20000000, "costPrice": 11200.00000000, "vatAmount": 1567.10000000, "vatRateId": 2, "totalAmount": 14626.30000000, "serialNumber": null, "markingNumber": "010478007420371721NZVF0x+-fnd<lcpI11Wc91UZF092SDB2UZccAGRfCmSPNYJYumzGxmcnTaumTSS1HOIBuOE=", "productTableId": 456}], "amount": 65296.00000000, "unitId": 1, "quantity": 5.000000, "unitName": "Dona", "costPrice": 59360.00000000, "productId": 23, "unitPrice": 13059.20000000, "vatAmount": 7835.52000000, "vatRateId": 2, "productMxik": "08418001001005219", "productName": "ARTEL, икки камерали HD 316 FND ECO FROST қора-жилосиз ранг", "totalAmount": 73131.52000000, "vatRateName": "QQS 12%"}], "comment": null, "docDate": "2026-06-29T17:36:29.055026", "stateId": 1, "statusId": 4, "docNumber": "100000078", "stateName": "Aktiv", "vatAmount": 7835.52000000, "contractId": 10, "currencyId": 1, "statusName": "Kutilmoqda", "createdDate": "2026-06-29T17:36:29.055026", "finalAmount": 73131.52000000, "totalAmount": 65296.00000000, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "contractNumber": "100000010", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-30 11:29:29.640904
180	8	public	sale_doc	80	UPDATE	{"id": 80, "lines": [{"id": 28, "items": [{"id": 99, "amount": 13059.20000000, "costPrice": 12320.00000000, "vatAmount": 1567.10000000, "vatRateId": 2, "totalAmount": 14626.30000000, "serialNumber": null, "markingNumber": "qwdqdqwdqwdqwdqdqd", "productTableId": 459}, {"id": 100, "amount": 13059.20000000, "costPrice": 12320.00000000, "vatAmount": 1567.10000000, "vatRateId": 2, "totalAmount": 14626.30000000, "serialNumber": null, "markingNumber": "qdqdqdqdqdqdqdqdqdq2855", "productTableId": 460}, {"id": 101, "amount": 13059.20000000, "costPrice": 12320.00000000, "vatAmount": 1567.10000000, "vatRateId": 2, "totalAmount": 14626.30000000, "serialNumber": null, "markingNumber": "qdqdqdqdqdq98d4q8d789qwd7q", "productTableId": 461}, {"id": 102, "amount": 13059.20000000, "costPrice": 11200.00000000, "vatAmount": 1567.10000000, "vatRateId": 2, "totalAmount": 14626.30000000, "serialNumber": null, "markingNumber": "0104780074206893217UkCJ6Gu*gG_j.wf6WnX91XUWh92JkB/xWNpbjVhcmtkWVdsT3lhVTVhYjdUQm8yQgOW3lE=", "productTableId": 455}, {"id": 103, "amount": 13059.20000000, "costPrice": 11200.00000000, "vatAmount": 1567.10000000, "vatRateId": 2, "totalAmount": 14626.30000000, "serialNumber": null, "markingNumber": "010478007420371721NZVF0x+-fnd<lcpI11Wc91UZF092SDB2UZccAGRfCmSPNYJYumzGxmcnTaumTSS1HOIBuOE=", "productTableId": 456}], "amount": 65296.00000000, "unitId": 1, "quantity": 5.000000, "unitName": "Dona", "costPrice": 59360.00000000, "productId": 23, "unitPrice": 13059.20000000, "vatAmount": 7835.52000000, "vatRateId": 2, "productMxik": "08418001001005219", "productName": "ARTEL, икки камерали HD 316 FND ECO FROST қора-жилосиз ранг", "totalAmount": 73131.52000000, "vatRateName": "QQS 12%"}], "comment": null, "docDate": "2026-06-29T17:36:29.055026", "stateId": 1, "statusId": 4, "docNumber": "100000078", "stateName": "Aktiv", "vatAmount": 7835.52000000, "contractId": 10, "currencyId": 1, "statusName": "Kutilmoqda", "createdDate": "2026-06-29T17:36:29.055026", "finalAmount": 73131.52000000, "totalAmount": 65296.00000000, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "contractNumber": "100000010", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	{"id": 80, "lines": [{"id": 28, "items": [{"id": 99, "amount": 65296.00000000, "costPrice": 59360.00000000, "vatAmount": 7835.52000000, "vatRateId": 2, "totalAmount": 73131.52000000, "serialNumber": null, "markingNumber": "qwdqdqwdqwdqwdqdqd", "productTableId": 459}, {"id": 100, "amount": 65296.00000000, "costPrice": 59360.00000000, "vatAmount": 7835.52000000, "vatRateId": 2, "totalAmount": 73131.52000000, "serialNumber": null, "markingNumber": "qdqdqdqdqdqdqdqdqdq2855", "productTableId": 460}, {"id": 101, "amount": 65296.00000000, "costPrice": 59360.00000000, "vatAmount": 7835.52000000, "vatRateId": 2, "totalAmount": 73131.52000000, "serialNumber": null, "markingNumber": "qdqdqdqdqdq98d4q8d789qwd7q", "productTableId": 461}, {"id": 102, "amount": 65296.00000000, "costPrice": 59360.00000000, "vatAmount": 7835.52000000, "vatRateId": 2, "totalAmount": 73131.52000000, "serialNumber": null, "markingNumber": "0104780074206893217UkCJ6Gu*gG_j.wf6WnX91XUWh92JkB/xWNpbjVhcmtkWVdsT3lhVTVhYjdUQm8yQgOW3lE=", "productTableId": 455}, {"id": 103, "amount": 65296.00000000, "costPrice": 59360.00000000, "vatAmount": 7835.52000000, "vatRateId": 2, "totalAmount": 73131.52000000, "serialNumber": null, "markingNumber": "010478007420371721NZVF0x+-fnd<lcpI11Wc91UZF092SDB2UZccAGRfCmSPNYJYumzGxmcnTaumTSS1HOIBuOE=", "productTableId": 456}], "amount": 326480.00000000, "unitId": 1, "quantity": 5.000000, "unitName": "Dona", "costPrice": 59360.00000000, "productId": 23, "unitPrice": 65296.00000000, "vatAmount": 39177.60000000, "vatRateId": 2, "productMxik": "08418001001005219", "productName": "ARTEL, икки камерали HD 316 FND ECO FROST қора-жилосиз ранг", "totalAmount": 365657.60000000, "vatRateName": "QQS 12%"}], "comment": null, "docDate": "2026-06-29T17:36:29.055026", "stateId": 1, "statusId": 2, "docNumber": "100000078", "stateName": "Aktiv", "vatAmount": 39177.60000000, "contractId": 10, "currencyId": 1, "statusName": "O'tkazilgan", "createdDate": "2026-06-29T17:36:29.055026", "finalAmount": 365657.60000000, "totalAmount": 326480.00000000, "warehouseId": 7, "currencyCode": "UZS", "currencyName": "Uzbek so'm", "warehouseName": "amonov", "contractNumber": "100000010", "counterpartyId": 18, "organizationId": 8, "counterpartyName": "Artel", "organizationName": "baraka_market"}	4	\N	\N	\N	2026-06-30 12:28:30.425972
181	8	public	bank_operation	36	INSERT	\N	{"id": 36, "amount": 2550000.00, "bankId": 2, "bankInn": null, "bankMfo": null, "comment": "summmm", "docDate": "2026-07-01T12:51:28", "stateId": 1, "bankName": "Ipoteka bank", "postedAt": "2026-07-01T17:52:12.621583", "statusId": 2, "docNumber": "100000035", "stateName": "Aktiv", "contractId": 10, "currencyId": 1, "statusName": "O'tkazilgan", "cancelledAt": null, "createdDate": "2026-07-01T17:52:12.621999", "currencyName": "Uzbek so'm", "exchangeRate": 12000.000000, "bankAccountId": 12, "paymentTypeId": 2, "bankAccountInn": null, "contractNumber": "100000010", "counterpartyId": 19, "organizationId": 8, "postedByUserId": 4, "bankAccountName": "Ipoteka bank", "counterpartyInn": "999888777", "operationTypeId": 1, "paymentTypeName": "Bank", "counterpartyName": "Farrux Tech", "organizationName": "baraka_market", "bankAccountNumber": "20208000005157348001", "cancelledByUserId": null, "operationTypeName": "Kirim", "counterpartyBankAccountId": 1, "counterpartyBankAccountNumber": "a5s831ascxpic13mx90vnaq"}	4	\N	\N	\N	2026-07-01 17:52:13.263118
182	8	public	bank_operation	37	INSERT	\N	{"id": 37, "amount": 2550000.00, "bankId": 2, "bankInn": null, "bankMfo": null, "comment": "summmm", "docDate": "2026-07-02T08:31:56", "stateId": 1, "bankName": "Ipoteka bank", "postedAt": "2026-07-02T15:05:01.90264", "statusId": 2, "docNumber": "100000036", "stateName": "Aktiv", "contractId": 9, "currencyId": 1, "statusName": "O'tkazilgan", "cancelledAt": null, "createdDate": "2026-07-02T15:05:01.903034", "currencyName": "Uzbek so'm", "exchangeRate": 12000.000000, "bankAccountId": 12, "paymentTypeId": 2, "bankAccountInn": null, "contractNumber": "100000009", "counterpartyId": 18, "organizationId": 8, "postedByUserId": 4, "bankAccountName": "Ipoteka bank", "counterpartyInn": "222222222", "operationTypeId": 1, "paymentTypeName": "Bank", "counterpartyName": "Artel", "organizationName": "baraka_market", "bankAccountNumber": "20208000005157348001", "cancelledByUserId": null, "operationTypeName": "Kirim", "counterpartyBankAccountId": 1, "counterpartyBankAccountNumber": "a5s831ascxpic13mx90vnaq"}	4	\N	\N	\N	2026-07-02 15:05:03.307517
184	8	public	cash_operation	3	UPDATE	{"id": 3, "amount": 2550000.00, "comment": "summmm", "docDate": "2026-07-02T17:07:58", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000001", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-02T17:08:12.108429", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": null, "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "operationTypeId": 2, "paymentTypeName": null, "counterpartyName": "Artel", "organizationName": "baraka_market", "cancelledByUserId": null, "operationTypeName": "Chiqim", "destinationCashBoxId": null, "destinationCashBoxName": null}	{"id": 3, "amount": 2550000.00, "comment": "summmm", "docDate": "2026-07-02T17:07:58", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "7878", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-02T17:08:12.108429", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": null, "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "operationTypeId": 2, "paymentTypeName": null, "counterpartyName": "Artel", "organizationName": "baraka_market", "cancelledByUserId": null, "operationTypeName": "Chiqim", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-02 17:14:06.613537
185	8	public	cash_operation	4	INSERT	\N	{"id": 4, "amount": 2550000.00, "comment": "summmm", "docDate": "2026-07-02T17:28:22", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000002", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-02T17:32:42.464594", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": null, "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "operationTypeId": 1, "paymentTypeName": null, "counterpartyName": "Artel", "organizationName": "baraka_market", "cancelledByUserId": null, "operationTypeName": "Kirim", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-02 17:32:42.492241
186	8	public	cash_operation	4	UPDATE	{"id": 4, "amount": 2550000.00, "comment": "summmm", "docDate": "2026-07-02T17:28:22", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000002", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-02T17:32:42.464594", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": null, "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "operationTypeId": 1, "paymentTypeName": null, "counterpartyName": "Artel", "organizationName": "baraka_market", "cancelledByUserId": null, "operationTypeName": "Kirim", "destinationCashBoxId": null, "destinationCashBoxName": null}	{"id": 4, "amount": 2550000.00, "comment": "summmm", "docDate": "2026-07-02T17:28:22", "stateId": 1, "postedAt": "2026-07-02T19:45:36.88631", "statusId": 2, "cashBoxId": 4, "docNumber": "100000002", "stateName": "Aktiv", "currencyId": 1, "statusName": "O'tkazilgan", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-02T17:32:42.464594", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": null, "counterpartyId": 18, "organizationId": 8, "postedByUserId": 4, "operationTypeId": 1, "paymentTypeName": null, "counterpartyName": "Artel", "organizationName": "baraka_market", "cancelledByUserId": null, "operationTypeName": "Kirim", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-02 19:45:36.968685
187	8	public	cash_operation	3	DELETE	{"id": 3, "amount": 2550000.00, "comment": "summmm", "docDate": "2026-07-02T17:07:58", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "7878", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-02T17:08:12.108429", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": null, "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "operationTypeId": 2, "paymentTypeName": null, "counterpartyName": "Artel", "organizationName": "baraka_market", "cancelledByUserId": null, "operationTypeName": "Chiqim", "destinationCashBoxId": null, "destinationCashBoxName": null}	{"id": 3, "amount": 2550000.00, "comment": "summmm", "docDate": "2026-07-02T17:07:58", "stateId": 2, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "7878", "stateName": "Passiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-02T17:08:12.108429", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": null, "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "operationTypeId": 2, "paymentTypeName": null, "counterpartyName": "Artel", "organizationName": "baraka_market", "cancelledByUserId": null, "operationTypeName": "Chiqim", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-03 10:26:59.1616
188	8	public	cash_operation	4	UPDATE	{"id": 4, "amount": 2550000.00, "comment": "summmm", "docDate": "2026-07-02T17:28:22", "stateId": 1, "postedAt": "2026-07-02T19:45:36.88631", "statusId": 2, "cashBoxId": 4, "docNumber": "100000002", "stateName": "Aktiv", "currencyId": 1, "statusName": "O'tkazilgan", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-02T17:32:42.464594", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": null, "counterpartyId": 18, "organizationId": 8, "postedByUserId": 4, "operationTypeId": 1, "paymentTypeName": null, "counterpartyName": "Artel", "organizationName": "baraka_market", "cancelledByUserId": null, "operationTypeName": "Kirim", "destinationCashBoxId": null, "destinationCashBoxName": null}	{"id": 4, "amount": 2550000.00, "comment": "summmm", "docDate": "2026-07-02T17:28:22", "stateId": 1, "postedAt": "2026-07-02T19:45:36.88631", "statusId": 3, "cashBoxId": 4, "docNumber": "100000002", "stateName": "Aktiv", "currencyId": 1, "statusName": "Bekor qilingan", "cancelledAt": "2026-07-03T11:24:40.416112", "cashBoxName": "kassa", "createdDate": "2026-07-02T17:32:42.464594", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": null, "counterpartyId": 18, "organizationId": 8, "postedByUserId": 4, "operationTypeId": 1, "paymentTypeName": null, "counterpartyName": "Artel", "organizationName": "baraka_market", "cancelledByUserId": 4, "operationTypeName": "Kirim", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-03 11:24:40.446282
190	8	public	cash_operation	5	UPDATE	{"id": 5, "amount": 10000000.00, "comment": "create test", "docDate": "2026-07-03T11:37:07", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000003", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-03T11:37:32.607436", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": null, "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "operationTypeId": 1, "paymentTypeName": null, "counterpartyName": "Artel", "organizationName": "baraka_market", "cancelledByUserId": null, "operationTypeName": "Kirim", "destinationCashBoxId": null, "destinationCashBoxName": null}	{"id": 5, "amount": 10000000.00, "comment": "create test", "docDate": "2026-07-03T11:37:07", "stateId": 1, "postedAt": "2026-07-03T11:42:39.642946", "statusId": 2, "cashBoxId": 4, "docNumber": "100000003", "stateName": "Aktiv", "currencyId": 1, "statusName": "O'tkazilgan", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-03T11:37:32.607436", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": null, "counterpartyId": 18, "organizationId": 8, "postedByUserId": 4, "operationTypeId": 1, "paymentTypeName": null, "counterpartyName": "Artel", "organizationName": "baraka_market", "cancelledByUserId": null, "operationTypeName": "Kirim", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-03 11:42:39.671855
191	8	public	cash_operation	5	UPDATE	{"id": 5, "amount": 10000000.00, "comment": "create test", "docDate": "2026-07-03T11:37:07", "stateId": 1, "postedAt": "2026-07-03T11:42:39.642946", "statusId": 2, "cashBoxId": 4, "docNumber": "100000003", "stateName": "Aktiv", "currencyId": 1, "statusName": "O'tkazilgan", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-03T11:37:32.607436", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": null, "counterpartyId": 18, "organizationId": 8, "postedByUserId": 4, "operationTypeId": 1, "paymentTypeName": null, "counterpartyName": "Artel", "organizationName": "baraka_market", "cancelledByUserId": null, "operationTypeName": "Kirim", "destinationCashBoxId": null, "destinationCashBoxName": null}	{"id": 5, "amount": 10000000.00, "comment": "create test", "docDate": "2026-07-03T11:37:07", "stateId": 1, "postedAt": "2026-07-03T11:42:39.642946", "statusId": 3, "cashBoxId": 4, "docNumber": "100000003", "stateName": "Aktiv", "currencyId": 1, "statusName": "Bekor qilingan", "cancelledAt": "2026-07-03T11:42:49.006074", "cashBoxName": "kassa", "createdDate": "2026-07-03T11:37:32.607436", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": null, "counterpartyId": 18, "organizationId": 8, "postedByUserId": 4, "operationTypeId": 1, "paymentTypeName": null, "counterpartyName": "Artel", "organizationName": "baraka_market", "cancelledByUserId": 4, "operationTypeName": "Kirim", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-03 11:42:49.033905
192	8	public	cash_operation	6	INSERT	\N	{"id": 6, "amount": 2550000.00, "comment": "summmm", "docDate": "2026-07-03T11:55:01", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000004", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-03T11:55:32.793748", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": null, "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "operationTypeId": 1, "paymentTypeName": null, "counterpartyName": "Artel", "organizationName": "baraka_market", "cancelledByUserId": null, "operationTypeName": "Kirim", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-03 11:55:32.810334
193	8	public	cash_operation	6	UPDATE	{"id": 6, "amount": 2550000.00, "comment": "summmm", "docDate": "2026-07-03T11:55:01", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000004", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-03T11:55:32.793748", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": null, "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "operationTypeId": 1, "paymentTypeName": null, "counterpartyName": "Artel", "organizationName": "baraka_market", "cancelledByUserId": null, "operationTypeName": "Kirim", "destinationCashBoxId": null, "destinationCashBoxName": null}	{"id": 6, "amount": 2550000.00, "comment": "summmm", "docDate": "2026-07-03T11:55:01", "stateId": 1, "postedAt": "2026-07-03T11:55:54.065379", "statusId": 2, "cashBoxId": 4, "docNumber": "100000004", "stateName": "Aktiv", "currencyId": 1, "statusName": "O'tkazilgan", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-03T11:55:32.793748", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": null, "counterpartyId": 18, "organizationId": 8, "postedByUserId": 4, "operationTypeId": 1, "paymentTypeName": null, "counterpartyName": "Artel", "organizationName": "baraka_market", "cancelledByUserId": null, "operationTypeName": "Kirim", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-03 11:55:54.088288
194	8	public	cash_operation	7	INSERT	\N	{"id": 7, "amount": 1000.00, "comment": "create test", "docDate": "2026-07-03T12:03:25", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000005", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-03T12:04:08.984397", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": null, "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "operationTypeId": 1, "paymentTypeName": null, "counterpartyName": "Artel", "organizationName": "baraka_market", "cancelledByUserId": null, "operationTypeName": "Kirim", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-03 12:04:09.010161
205	8	public	bank_operation	40	INSERT	\N	{"id": 40, "amount": 3550000.00, "bankId": 2, "bankInn": null, "bankMfo": null, "comment": null, "docDate": "2026-07-03T11:33:03", "stateId": 1, "bankName": "Ipoteka bank", "postedAt": null, "statusId": 1, "docNumber": "100000039", "stateName": "Aktiv", "contractId": 14, "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "createdDate": "2026-07-03T16:38:12.389208", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "bankAccountId": 12, "paymentTypeId": null, "bankAccountInn": null, "contractNumber": "100000014", "counterpartyId": 19, "organizationId": 8, "postedByUserId": null, "bankAccountName": "Ipoteka bank", "counterpartyInn": "999888777", "operationTypeId": 1, "paymentTypeName": null, "counterpartyName": "Farrux Tech", "organizationName": "baraka_market", "bankAccountNumber": "20208000005157348001", "cancelledByUserId": null, "operationTypeName": "Kirim", "counterpartyBankAccountId": 1, "counterpartyBankAccountNumber": "a5s831ascxpic13mx90vnaq"}	4	\N	\N	\N	2026-07-03 16:38:12.437062
195	8	public	cash_operation	7	UPDATE	{"id": 7, "amount": 1000.00, "comment": "create test", "docDate": "2026-07-03T12:03:25", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000005", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-03T12:04:08.984397", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": null, "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "operationTypeId": 1, "paymentTypeName": null, "counterpartyName": "Artel", "organizationName": "baraka_market", "cancelledByUserId": null, "operationTypeName": "Kirim", "destinationCashBoxId": null, "destinationCashBoxName": null}	{"id": 7, "amount": 1000.00, "comment": "create test", "docDate": "2026-07-03T12:03:25", "stateId": 1, "postedAt": "2026-07-03T12:04:18.190119", "statusId": 2, "cashBoxId": 4, "docNumber": "100000005", "stateName": "Aktiv", "currencyId": 1, "statusName": "O'tkazilgan", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-03T12:04:08.984397", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": null, "counterpartyId": 18, "organizationId": 8, "postedByUserId": 4, "operationTypeId": 1, "paymentTypeName": null, "counterpartyName": "Artel", "organizationName": "baraka_market", "cancelledByUserId": null, "operationTypeName": "Kirim", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-03 12:04:18.20115
196	8	public	cash_operation	7	UPDATE	{"id": 7, "amount": 1000.00, "comment": "create test", "docDate": "2026-07-03T12:03:25", "stateId": 1, "postedAt": "2026-07-03T12:04:18.190119", "statusId": 2, "cashBoxId": 4, "docNumber": "100000005", "stateName": "Aktiv", "currencyId": 1, "statusName": "O'tkazilgan", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-03T12:04:08.984397", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": null, "counterpartyId": 18, "organizationId": 8, "postedByUserId": 4, "operationTypeId": 1, "paymentTypeName": null, "counterpartyName": "Artel", "organizationName": "baraka_market", "cancelledByUserId": null, "operationTypeName": "Kirim", "destinationCashBoxId": null, "destinationCashBoxName": null}	{"id": 7, "amount": 1000.00, "comment": "create test", "docDate": "2026-07-03T12:03:25", "stateId": 1, "postedAt": "2026-07-03T12:04:18.190119", "statusId": 3, "cashBoxId": 4, "docNumber": "100000005", "stateName": "Aktiv", "currencyId": 1, "statusName": "Bekor qilingan", "cancelledAt": "2026-07-03T12:04:51.394741", "cashBoxName": "kassa", "createdDate": "2026-07-03T12:04:08.984397", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": null, "counterpartyId": 18, "organizationId": 8, "postedByUserId": 4, "operationTypeId": 1, "paymentTypeName": null, "counterpartyName": "Artel", "organizationName": "baraka_market", "cancelledByUserId": 4, "operationTypeName": "Kirim", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-03 12:04:51.478498
197	8	public	bank_operation	38	INSERT	\N	{"id": 38, "amount": 3550000.00, "bankId": 2, "bankInn": null, "bankMfo": null, "comment": "create test", "docDate": "2026-07-03T07:03:25", "stateId": 1, "bankName": "Ipoteka bank", "postedAt": null, "statusId": 1, "docNumber": "100000037", "stateName": "Aktiv", "contractId": 10, "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "createdDate": "2026-07-03T12:19:26.631772", "currencyName": "Uzbek so'm", "exchangeRate": 12000.000000, "bankAccountId": 12, "paymentTypeId": null, "bankAccountInn": null, "contractNumber": "100000010", "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "bankAccountName": "Ipoteka bank", "counterpartyInn": "222222222", "operationTypeId": 1, "paymentTypeName": null, "counterpartyName": "Artel", "organizationName": "baraka_market", "bankAccountNumber": "20208000005157348001", "cancelledByUserId": null, "operationTypeName": "Kirim", "counterpartyBankAccountId": 1, "counterpartyBankAccountNumber": "a5s831ascxpic13mx90vnaq"}	4	\N	\N	\N	2026-07-03 12:19:26.824218
198	8	public	pur_doc	98	INSERT	\N	{"id": 98, "lines": [{"id": 20, "items": [{"id": 24, "amount": 10000.00000000, "vatAmount": 1200.00000000, "vatRateId": 2, "totalAmount": 11200.00000000, "vatRateName": "QQS 12%", "serialNumber": null, "markingNumber": "qsqsqsqsqsqs874q7sq7s", "productTableId": 464}], "amount": 10000.00000000, "unitId": 1, "ownerId": 98, "quantity": 1.000000, "unitName": "Dona", "productId": 24, "unitPrice": 10000.00000000, "vatAmount": 1200.00000000, "vatRateId": 2, "productMxik": "08418001001005223", "productName": "ARTEL, икки камерали HD 341 FND ECO FROST ёмғирли-асфалт ранг", "totalAmount": 11200.00000000, "vatRateName": "QQS 12%"}], "comment": null, "docDate": "2026-07-03T15:47:49", "stateId": 1, "postedAt": null, "statusId": 1, "docNumber": "100000080", "stateName": "Aktiv", "vatAmount": 1200.00000000, "contractId": 9, "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "createdDate": "2026-07-03T15:48:13.121681", "finalAmount": 11200.00000000, "totalAmount": 10000.00000000, "warehouseId": 7, "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "warehouseName": "amonov", "contractNumber": "100000009", "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "counterpartyName": "Artel", "organizationName": "baraka_market", "cancelledByUserId": null}	4	\N	\N	\N	2026-07-03 15:48:13.43702
199	8	public	pur_doc	98	UPDATE	{"id": 98, "lines": [{"id": 20, "items": [{"id": 24, "amount": 10000.00000000, "vatAmount": 1200.00000000, "vatRateId": 2, "totalAmount": 11200.00000000, "vatRateName": "QQS 12%", "serialNumber": null, "markingNumber": "qsqsqsqsqsqs874q7sq7s", "productTableId": 464}], "amount": 10000.00000000, "unitId": 1, "ownerId": 98, "quantity": 1.000000, "unitName": "Dona", "productId": 24, "unitPrice": 10000.00000000, "vatAmount": 1200.00000000, "vatRateId": 2, "productMxik": "08418001001005223", "productName": "ARTEL, икки камерали HD 341 FND ECO FROST ёмғирли-асфалт ранг", "totalAmount": 11200.00000000, "vatRateName": "QQS 12%"}], "comment": null, "docDate": "2026-07-03T15:47:49", "stateId": 1, "postedAt": null, "statusId": 1, "docNumber": "100000080", "stateName": "Aktiv", "vatAmount": 1200.00000000, "contractId": 9, "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "createdDate": "2026-07-03T15:48:13.121681", "finalAmount": 11200.00000000, "totalAmount": 10000.00000000, "warehouseId": 7, "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "warehouseName": "amonov", "contractNumber": "100000009", "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "counterpartyName": "Artel", "organizationName": "baraka_market", "cancelledByUserId": null}	{"id": 98, "lines": [{"id": 20, "items": [], "amount": 10000.00000000, "unitId": 1, "ownerId": 98, "quantity": 1.000000, "unitName": "Dona", "productId": 24, "unitPrice": 10000.00000000, "vatAmount": 1200.00000000, "vatRateId": 2, "productMxik": "08418001001005223", "productName": "ARTEL, икки камерали HD 341 FND ECO FROST ёмғирли-асфалт ранг", "totalAmount": 11200.00000000, "vatRateName": "QQS 12%"}], "comment": null, "docDate": "2026-07-03T15:47:49", "stateId": 1, "postedAt": null, "statusId": 3, "docNumber": "100000080", "stateName": "Aktiv", "vatAmount": 1200.00000000, "contractId": 9, "currencyId": 1, "statusName": "Bekor qilingan", "cancelledAt": "2026-07-03T15:48:35.667066", "createdDate": "2026-07-03T15:48:13.121681", "finalAmount": 11200.00000000, "totalAmount": 10000.00000000, "warehouseId": 7, "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "warehouseName": "amonov", "contractNumber": "100000009", "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "counterpartyName": "Artel", "organizationName": "baraka_market", "cancelledByUserId": 4}	4	\N	\N	\N	2026-07-03 15:48:35.726721
200	8	public	pur_doc	99	INSERT	\N	{"id": 99, "lines": [{"id": 21, "items": [{"id": 25, "amount": 10000.00000000, "vatAmount": 1200.00000000, "vatRateId": 2, "totalAmount": 11200.00000000, "vatRateName": "QQS 12%", "serialNumber": null, "markingNumber": "qa1qq1q1", "productTableId": 465}], "amount": 10000.00000000, "unitId": 1, "ownerId": 99, "quantity": 1.000000, "unitName": "Dona", "productId": 24, "unitPrice": 10000.00000000, "vatAmount": 1200.00000000, "vatRateId": 2, "productMxik": "08418001001005223", "productName": "ARTEL, икки камерали HD 341 FND ECO FROST ёмғирли-асфалт ранг", "totalAmount": 11200.00000000, "vatRateName": "QQS 12%"}], "comment": null, "docDate": "2026-07-03T15:59:16", "stateId": 1, "postedAt": null, "statusId": 1, "docNumber": "100000081", "stateName": "Aktiv", "vatAmount": 1200.00000000, "contractId": 9, "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "createdDate": "2026-07-03T16:15:55.260463", "finalAmount": 11200.00000000, "totalAmount": 10000.00000000, "warehouseId": 7, "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "warehouseName": "amonov", "contractNumber": "100000009", "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "counterpartyName": "Artel", "organizationName": "baraka_market", "cancelledByUserId": null}	4	\N	\N	\N	2026-07-03 16:15:55.313106
201	8	public	pur_doc	99	UPDATE	{"id": 99, "lines": [{"id": 21, "items": [{"id": 25, "amount": 10000.00000000, "vatAmount": 1200.00000000, "vatRateId": 2, "totalAmount": 11200.00000000, "vatRateName": "QQS 12%", "serialNumber": null, "markingNumber": "qa1qq1q1", "productTableId": 465}], "amount": 10000.00000000, "unitId": 1, "ownerId": 99, "quantity": 1.000000, "unitName": "Dona", "productId": 24, "unitPrice": 10000.00000000, "vatAmount": 1200.00000000, "vatRateId": 2, "productMxik": "08418001001005223", "productName": "ARTEL, икки камерали HD 341 FND ECO FROST ёмғирли-асфалт ранг", "totalAmount": 11200.00000000, "vatRateName": "QQS 12%"}], "comment": null, "docDate": "2026-07-03T15:59:16", "stateId": 1, "postedAt": null, "statusId": 1, "docNumber": "100000081", "stateName": "Aktiv", "vatAmount": 1200.00000000, "contractId": 9, "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "createdDate": "2026-07-03T16:15:55.260463", "finalAmount": 11200.00000000, "totalAmount": 10000.00000000, "warehouseId": 7, "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "warehouseName": "amonov", "contractNumber": "100000009", "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "counterpartyName": "Artel", "organizationName": "baraka_market", "cancelledByUserId": null}	{"id": 99, "lines": [{"id": 21, "items": [], "amount": 10000.00000000, "unitId": 1, "ownerId": 99, "quantity": 1.000000, "unitName": "Dona", "productId": 24, "unitPrice": 10000.00000000, "vatAmount": 1200.00000000, "vatRateId": 2, "productMxik": "08418001001005223", "productName": "ARTEL, икки камерали HD 341 FND ECO FROST ёмғирли-асфалт ранг", "totalAmount": 11200.00000000, "vatRateName": "QQS 12%"}], "comment": null, "docDate": "2026-07-03T15:59:16", "stateId": 1, "postedAt": null, "statusId": 3, "docNumber": "100000081", "stateName": "Aktiv", "vatAmount": 1200.00000000, "contractId": 9, "currencyId": 1, "statusName": "Bekor qilingan", "cancelledAt": "2026-07-03T16:18:01.551278", "createdDate": "2026-07-03T16:15:55.260463", "finalAmount": 11200.00000000, "totalAmount": 10000.00000000, "warehouseId": 7, "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "warehouseName": "amonov", "contractNumber": "100000009", "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "counterpartyName": "Artel", "organizationName": "baraka_market", "cancelledByUserId": 4}	4	\N	\N	\N	2026-07-03 16:18:01.582163
202	8	public	bank_operation	39	INSERT	\N	{"id": 39, "amount": 10000000.00, "bankId": 4, "bankInn": null, "bankMfo": null, "comment": "summmm", "docDate": "2026-07-03T11:19:50", "stateId": 1, "bankName": "Hamkorbank", "postedAt": null, "statusId": 1, "docNumber": "100000038", "stateName": "Aktiv", "contractId": 9, "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "createdDate": "2026-07-03T16:20:09.88283", "currencyName": "Uzbek so'm", "exchangeRate": 12000.000000, "bankAccountId": 9, "paymentTypeId": null, "bankAccountInn": null, "contractNumber": "100000009", "counterpartyId": 19, "organizationId": 8, "postedByUserId": null, "bankAccountName": "Hamkorbank", "counterpartyInn": "999888777", "operationTypeId": 1, "paymentTypeName": null, "counterpartyName": "Farrux Tech", "organizationName": "baraka_market", "bankAccountNumber": "0099855144117", "cancelledByUserId": null, "operationTypeName": "Kirim", "counterpartyBankAccountId": 1, "counterpartyBankAccountNumber": "a5s831ascxpic13mx90vnaq"}	4	\N	\N	\N	2026-07-03 16:20:09.908561
203	8	public	bank_operation	39	UPDATE	{"id": 39, "amount": 10000000.00, "bankId": 4, "bankInn": null, "bankMfo": null, "comment": "summmm", "docDate": "2026-07-03T11:19:50", "stateId": 1, "bankName": "Hamkorbank", "postedAt": null, "statusId": 1, "docNumber": "100000038", "stateName": "Aktiv", "contractId": 9, "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "createdDate": "2026-07-03T16:20:09.88283", "currencyName": "Uzbek so'm", "exchangeRate": 12000.000000, "bankAccountId": 9, "paymentTypeId": null, "bankAccountInn": null, "contractNumber": "100000009", "counterpartyId": 19, "organizationId": 8, "postedByUserId": null, "bankAccountName": "Hamkorbank", "counterpartyInn": "999888777", "operationTypeId": 1, "paymentTypeName": null, "counterpartyName": "Farrux Tech", "organizationName": "baraka_market", "bankAccountNumber": "0099855144117", "cancelledByUserId": null, "operationTypeName": "Kirim", "counterpartyBankAccountId": 1, "counterpartyBankAccountNumber": "a5s831ascxpic13mx90vnaq"}	{"id": 39, "amount": 10000000.00, "bankId": 4, "bankInn": null, "bankMfo": null, "comment": "summmm", "docDate": "2026-07-03T06:19:50", "stateId": 1, "bankName": "Hamkorbank", "postedAt": null, "statusId": 1, "docNumber": "100000038", "stateName": "Aktiv", "contractId": 9, "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "createdDate": "2026-07-03T16:20:09.88283", "currencyName": "Uzbek so'm", "exchangeRate": 12000.000000, "bankAccountId": 9, "paymentTypeId": null, "bankAccountInn": null, "contractNumber": "100000009", "counterpartyId": 19, "organizationId": 8, "postedByUserId": null, "bankAccountName": "Hamkorbank", "counterpartyInn": "999888777", "operationTypeId": 1, "paymentTypeName": null, "counterpartyName": "Farrux Tech", "organizationName": "baraka_market", "bankAccountNumber": "0099855144117", "cancelledByUserId": null, "operationTypeName": "Kirim", "counterpartyBankAccountId": 1, "counterpartyBankAccountNumber": "a5s831ascxpic13mx90vnaq"}	4	\N	\N	\N	2026-07-03 16:23:03.401887
204	8	public	pur_doc	100	INSERT	\N	{"id": 100, "lines": [{"id": 23, "items": [{"id": 27, "amount": 10000.00000000, "vatAmount": 1200.00000000, "vatRateId": 2, "totalAmount": 11200.00000000, "vatRateName": "QQS 12%", "serialNumber": null, "markingNumber": "qa1qq1q1", "productTableId": 467}], "amount": 10000.00000000, "unitId": 1, "ownerId": 100, "quantity": 1.000000, "unitName": "Dona", "productId": 24, "unitPrice": 10000.00000000, "vatAmount": 1200.00000000, "vatRateId": 2, "productMxik": "08418001001005223", "productName": "ARTEL, икки камерали HD 341 FND ECO FROST ёмғирли-асфалт ранг", "totalAmount": 11200.00000000, "vatRateName": "QQS 12%"}], "comment": null, "docDate": "2026-07-03T15:59:16", "stateId": 1, "postedAt": null, "statusId": 1, "docNumber": "100000082", "stateName": "Aktiv", "vatAmount": 1200.00000000, "contractId": 9, "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "createdDate": "2026-07-03T16:25:23.358861", "finalAmount": 11200.00000000, "totalAmount": 10000.00000000, "warehouseId": 7, "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "warehouseName": "amonov", "contractNumber": "100000009", "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "counterpartyName": "Artel", "organizationName": "baraka_market", "cancelledByUserId": null}	4	\N	\N	\N	2026-07-03 16:25:23.402246
206	8	public	pur_doc	100	UPDATE	{"id": 100, "lines": [{"id": 23, "items": [{"id": 27, "amount": 10000.00000000, "vatAmount": 1200.00000000, "vatRateId": 2, "totalAmount": 11200.00000000, "vatRateName": "QQS 12%", "serialNumber": null, "markingNumber": "qa1qq1q1", "productTableId": 467}], "amount": 10000.00000000, "unitId": 1, "ownerId": 100, "quantity": 1.000000, "unitName": "Dona", "productId": 24, "unitPrice": 10000.00000000, "vatAmount": 1200.00000000, "vatRateId": 2, "productMxik": "08418001001005223", "productName": "ARTEL, икки камерали HD 341 FND ECO FROST ёмғирли-асфалт ранг", "totalAmount": 11200.00000000, "vatRateName": "QQS 12%"}], "comment": null, "docDate": "2026-07-03T15:59:16", "stateId": 1, "postedAt": null, "statusId": 1, "docNumber": "100000082", "stateName": "Aktiv", "vatAmount": 1200.00000000, "contractId": 9, "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "createdDate": "2026-07-03T16:25:23.358861", "finalAmount": 11200.00000000, "totalAmount": 10000.00000000, "warehouseId": 7, "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "warehouseName": "amonov", "contractNumber": "100000009", "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "counterpartyName": "Artel", "organizationName": "baraka_market", "cancelledByUserId": null}	{"id": 100, "lines": [{"id": 23, "items": [], "amount": 10000.00000000, "unitId": 1, "ownerId": 100, "quantity": 1.000000, "unitName": "Dona", "productId": 24, "unitPrice": 10000.00000000, "vatAmount": 1200.00000000, "vatRateId": 2, "productMxik": "08418001001005223", "productName": "ARTEL, икки камерали HD 341 FND ECO FROST ёмғирли-асфалт ранг", "totalAmount": 11200.00000000, "vatRateName": "QQS 12%"}], "comment": null, "docDate": "2026-07-03T15:59:16", "stateId": 1, "postedAt": null, "statusId": 3, "docNumber": "100000082", "stateName": "Aktiv", "vatAmount": 1200.00000000, "contractId": 9, "currencyId": 1, "statusName": "Bekor qilingan", "cancelledAt": "2026-07-03T17:35:35.247432", "createdDate": "2026-07-03T16:25:23.358861", "finalAmount": 11200.00000000, "totalAmount": 10000.00000000, "warehouseId": 7, "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "warehouseName": "amonov", "contractNumber": "100000009", "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "counterpartyName": "Artel", "organizationName": "baraka_market", "cancelledByUserId": 4}	4	\N	\N	\N	2026-07-03 17:35:35.262196
207	8	public	pur_doc	101	INSERT	\N	{"id": 101, "lines": [{"id": 31, "items": [{"id": 35, "amount": 10000.00000000, "vatAmount": 1200.00000000, "vatRateId": 2, "totalAmount": 11200.00000000, "vatRateName": "QQS 12%", "serialNumber": null, "markingNumber": "qweqeqeqeq", "productTableId": 475}], "amount": 10000.00000000, "unitId": 1, "ownerId": 101, "quantity": 1.000000, "unitName": "Dona", "productId": 24, "unitPrice": 10000.00000000, "vatAmount": 1200.00000000, "vatRateId": 2, "productMxik": "08418001001005223", "productName": "ARTEL, икки камерали HD 341 FND ECO FROST ёмғирли-асфалт ранг", "totalAmount": 11200.00000000, "vatRateName": "QQS 12%"}], "comment": null, "docDate": "2026-07-03T17:21:43", "stateId": 1, "postedAt": null, "statusId": 1, "docNumber": "100000083", "stateName": "Aktiv", "vatAmount": 1200.00000000, "contractId": 9, "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "createdDate": "2026-07-03T17:37:48.377285", "finalAmount": 11200.00000000, "totalAmount": 10000.00000000, "warehouseId": 7, "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "warehouseName": "amonov", "contractNumber": "100000009", "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "counterpartyName": "Artel", "organizationName": "baraka_market", "cancelledByUserId": null}	4	\N	\N	\N	2026-07-03 17:37:48.409776
208	8	public	bank_operation	41	INSERT	\N	{"id": 41, "lines": [{"id": 7, "amount": 3550000.00, "comment": "create test", "orderNumber": 1, "counterpartyId": 19, "paymentPurposeId": 4, "paymentPurposeCode": "CUSTOMER_ADVANCE", "paymentPurposeName": "Advance from customer"}], "amount": 3550000.00, "bankId": 2, "bankInn": null, "bankMfo": null, "comment": "create test", "docDate": "2026-07-04T03:27:53", "stateId": 1, "bankName": "Ipoteka bank", "postedAt": null, "statusId": 1, "docNumber": "100000040", "stateName": "Aktiv", "contractId": 11, "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "createdDate": "2026-07-04T08:30:10.372839", "currencyName": "Uzbek so'm", "exchangeRate": 12000.000000, "bankAccountId": 12, "paymentTypeId": 2, "bankAccountInn": null, "contractNumber": "100000011", "counterpartyId": 19, "organizationId": 8, "postedByUserId": null, "bankAccountName": "Ipoteka bank", "counterpartyInn": "999888777", "operationTypeId": 1, "paymentTypeName": "Bank", "counterpartyName": "Farrux Tech", "organizationName": "baraka_market", "bankAccountNumber": "20208000005157348001", "cancelledByUserId": null, "operationTypeName": "Kirim", "counterpartyBankAccountId": 1, "counterpartyBankAccountNumber": "a5s831ascxpic13mx90vnaq"}	4	\N	\N	\N	2026-07-04 08:30:10.537802
209	8	public	bank_operation	41	UPDATE	{"id": 41, "lines": [{"id": 7, "amount": 3550000.00, "comment": "create test", "orderNumber": 1, "counterpartyId": 19, "paymentPurposeId": 4, "paymentPurposeCode": "CUSTOMER_ADVANCE", "paymentPurposeName": "Advance from customer"}], "amount": 3550000.00, "bankId": 2, "bankInn": null, "bankMfo": null, "comment": "create test", "docDate": "2026-07-04T03:27:53", "stateId": 1, "bankName": "Ipoteka bank", "postedAt": null, "statusId": 1, "docNumber": "100000040", "stateName": "Aktiv", "contractId": 11, "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "createdDate": "2026-07-04T08:30:10.372839", "currencyName": "Uzbek so'm", "exchangeRate": 12000.000000, "bankAccountId": 12, "paymentTypeId": 2, "bankAccountInn": null, "contractNumber": "100000011", "counterpartyId": 19, "organizationId": 8, "postedByUserId": null, "bankAccountName": "Ipoteka bank", "counterpartyInn": "999888777", "operationTypeId": 1, "paymentTypeName": "Bank", "counterpartyName": "Farrux Tech", "organizationName": "baraka_market", "bankAccountNumber": "20208000005157348001", "cancelledByUserId": null, "operationTypeName": "Kirim", "counterpartyBankAccountId": 1, "counterpartyBankAccountNumber": "a5s831ascxpic13mx90vnaq"}	{"id": 41, "lines": [{"id": 7, "amount": 3550000.00, "comment": "create test", "orderNumber": 1, "counterpartyId": 19, "paymentPurposeId": 4, "paymentPurposeCode": "CUSTOMER_ADVANCE", "paymentPurposeName": "Advance from customer"}], "amount": 3550000.00, "bankId": 2, "bankInn": null, "bankMfo": null, "comment": "create test", "docDate": "2026-07-04T03:27:53", "stateId": 1, "bankName": "Ipoteka bank", "postedAt": "2026-07-04T08:30:34.909001", "statusId": 2, "docNumber": "100000040", "stateName": "Aktiv", "contractId": 11, "currencyId": 1, "statusName": "O'tkazilgan", "cancelledAt": null, "createdDate": "2026-07-04T08:30:10.372839", "currencyName": "Uzbek so'm", "exchangeRate": 12000.000000, "bankAccountId": 12, "paymentTypeId": 2, "bankAccountInn": null, "contractNumber": "100000011", "counterpartyId": 19, "organizationId": 8, "postedByUserId": 4, "bankAccountName": "Ipoteka bank", "counterpartyInn": "999888777", "operationTypeId": 1, "paymentTypeName": "Bank", "counterpartyName": "Farrux Tech", "organizationName": "baraka_market", "bankAccountNumber": "20208000005157348001", "cancelledByUserId": null, "operationTypeName": "Kirim", "counterpartyBankAccountId": 1, "counterpartyBankAccountNumber": "a5s831ascxpic13mx90vnaq"}	4	\N	\N	\N	2026-07-04 08:30:34.939742
210	8	public	bank_operation	40	UPDATE	{"id": 40, "lines": [{"id": 6, "amount": 3550000.00, "comment": null, "orderNumber": 1, "counterpartyId": 19, "paymentPurposeId": 9, "paymentPurposeCode": "LOAN_REPAYMENT", "paymentPurposeName": "Loan repayment"}], "amount": 3550000.00, "bankId": 2, "bankInn": null, "bankMfo": null, "comment": null, "docDate": "2026-07-03T11:33:03", "stateId": 1, "bankName": "Ipoteka bank", "postedAt": null, "statusId": 1, "docNumber": "100000039", "stateName": "Aktiv", "contractId": 14, "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "createdDate": "2026-07-03T16:38:12.389208", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "bankAccountId": 12, "paymentTypeId": null, "bankAccountInn": null, "contractNumber": "100000014", "counterpartyId": 19, "organizationId": 8, "postedByUserId": null, "bankAccountName": "Ipoteka bank", "counterpartyInn": "999888777", "operationTypeId": 1, "paymentTypeName": null, "counterpartyName": "Farrux Tech", "organizationName": "baraka_market", "bankAccountNumber": "20208000005157348001", "cancelledByUserId": null, "operationTypeName": "Kirim", "counterpartyBankAccountId": 1, "counterpartyBankAccountNumber": "a5s831ascxpic13mx90vnaq"}	{"id": 40, "lines": [{"id": 6, "amount": 3550000.00, "comment": null, "orderNumber": 1, "counterpartyId": 19, "paymentPurposeId": 9, "paymentPurposeCode": "LOAN_REPAYMENT", "paymentPurposeName": "Loan repayment"}], "amount": 3550000.00, "bankId": 2, "bankInn": null, "bankMfo": null, "comment": null, "docDate": "2026-07-03T11:33:03", "stateId": 1, "bankName": "Ipoteka bank", "postedAt": null, "statusId": 3, "docNumber": "100000039", "stateName": "Aktiv", "contractId": 14, "currencyId": 1, "statusName": "Bekor qilingan", "cancelledAt": "2026-07-04T08:31:28.978349", "createdDate": "2026-07-03T16:38:12.389208", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "bankAccountId": 12, "paymentTypeId": null, "bankAccountInn": null, "contractNumber": "100000014", "counterpartyId": 19, "organizationId": 8, "postedByUserId": null, "bankAccountName": "Ipoteka bank", "counterpartyInn": "999888777", "operationTypeId": 1, "paymentTypeName": null, "counterpartyName": "Farrux Tech", "organizationName": "baraka_market", "bankAccountNumber": "20208000005157348001", "cancelledByUserId": 4, "operationTypeName": "Kirim", "counterpartyBankAccountId": 1, "counterpartyBankAccountNumber": "a5s831ascxpic13mx90vnaq"}	4	\N	\N	\N	2026-07-04 08:31:29.013044
211	8	public	bank_operation	39	UPDATE	{"id": 39, "lines": [{"id": 5, "amount": 10000000.00, "comment": "summmm", "orderNumber": 1, "counterpartyId": 19, "paymentPurposeId": 9, "paymentPurposeCode": "LOAN_REPAYMENT", "paymentPurposeName": "Loan repayment"}], "amount": 10000000.00, "bankId": 4, "bankInn": null, "bankMfo": null, "comment": "summmm", "docDate": "2026-07-03T06:19:50", "stateId": 1, "bankName": "Hamkorbank", "postedAt": null, "statusId": 1, "docNumber": "100000038", "stateName": "Aktiv", "contractId": 9, "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "createdDate": "2026-07-03T16:20:09.88283", "currencyName": "Uzbek so'm", "exchangeRate": 12000.000000, "bankAccountId": 9, "paymentTypeId": null, "bankAccountInn": null, "contractNumber": "100000009", "counterpartyId": 19, "organizationId": 8, "postedByUserId": null, "bankAccountName": "Hamkorbank", "counterpartyInn": "999888777", "operationTypeId": 1, "paymentTypeName": null, "counterpartyName": "Farrux Tech", "organizationName": "baraka_market", "bankAccountNumber": "0099855144117", "cancelledByUserId": null, "operationTypeName": "Kirim", "counterpartyBankAccountId": 1, "counterpartyBankAccountNumber": "a5s831ascxpic13mx90vnaq"}	{"id": 39, "lines": [{"id": 5, "amount": 10000000.00, "comment": "summmm", "orderNumber": 1, "counterpartyId": 19, "paymentPurposeId": 9, "paymentPurposeCode": "LOAN_REPAYMENT", "paymentPurposeName": "Loan repayment"}], "amount": 10000000.00, "bankId": 4, "bankInn": null, "bankMfo": null, "comment": "summmm", "docDate": "2026-07-03T06:19:50", "stateId": 1, "bankName": "Hamkorbank", "postedAt": null, "statusId": 3, "docNumber": "100000038", "stateName": "Aktiv", "contractId": 9, "currencyId": 1, "statusName": "Bekor qilingan", "cancelledAt": "2026-07-04T08:31:49.192506", "createdDate": "2026-07-03T16:20:09.88283", "currencyName": "Uzbek so'm", "exchangeRate": 12000.000000, "bankAccountId": 9, "paymentTypeId": null, "bankAccountInn": null, "contractNumber": "100000009", "counterpartyId": 19, "organizationId": 8, "postedByUserId": null, "bankAccountName": "Hamkorbank", "counterpartyInn": "999888777", "operationTypeId": 1, "paymentTypeName": null, "counterpartyName": "Farrux Tech", "organizationName": "baraka_market", "bankAccountNumber": "0099855144117", "cancelledByUserId": 4, "operationTypeName": "Kirim", "counterpartyBankAccountId": 1, "counterpartyBankAccountNumber": "a5s831ascxpic13mx90vnaq"}	4	\N	\N	\N	2026-07-04 08:31:49.219951
212	8	public	pur_doc	101	UPDATE	{"id": 101, "lines": [{"id": 31, "items": [{"id": 35, "amount": 10000.00000000, "vatAmount": 1200.00000000, "vatRateId": 2, "totalAmount": 11200.00000000, "vatRateName": "QQS 12%", "serialNumber": null, "markingNumber": "qweqeqeqeq", "productTableId": 475}], "amount": 10000.00000000, "unitId": 1, "ownerId": 101, "quantity": 1.000000, "unitName": "Dona", "productId": 24, "unitPrice": 10000.00000000, "vatAmount": 1200.00000000, "vatRateId": 2, "productMxik": "08418001001005223", "productName": "ARTEL, икки камерали HD 341 FND ECO FROST ёмғирли-асфалт ранг", "totalAmount": 11200.00000000, "vatRateName": "QQS 12%"}], "comment": null, "docDate": "2026-07-03T17:21:43", "stateId": 1, "postedAt": null, "statusId": 1, "docNumber": "100000083", "stateName": "Aktiv", "vatAmount": 1200.00000000, "contractId": 9, "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "createdDate": "2026-07-03T17:37:48.377285", "finalAmount": 11200.00000000, "totalAmount": 10000.00000000, "warehouseId": 7, "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "warehouseName": "amonov", "contractNumber": "100000009", "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "counterpartyName": "Artel", "organizationName": "baraka_market", "cancelledByUserId": null}	{"id": 101, "lines": [{"id": 31, "items": [], "amount": 10000.00000000, "unitId": 1, "ownerId": 101, "quantity": 1.000000, "unitName": "Dona", "productId": 24, "unitPrice": 10000.00000000, "vatAmount": 1200.00000000, "vatRateId": 2, "productMxik": "08418001001005223", "productName": "ARTEL, икки камерали HD 341 FND ECO FROST ёмғирли-асфалт ранг", "totalAmount": 11200.00000000, "vatRateName": "QQS 12%"}], "comment": null, "docDate": "2026-07-03T17:21:43", "stateId": 1, "postedAt": null, "statusId": 3, "docNumber": "100000083", "stateName": "Aktiv", "vatAmount": 1200.00000000, "contractId": 9, "currencyId": 1, "statusName": "Bekor qilingan", "cancelledAt": "2026-07-04T08:32:38.653805", "createdDate": "2026-07-03T17:37:48.377285", "finalAmount": 11200.00000000, "totalAmount": 10000.00000000, "warehouseId": 7, "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "warehouseName": "amonov", "contractNumber": "100000009", "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "counterpartyName": "Artel", "organizationName": "baraka_market", "cancelledByUserId": 4}	4	\N	\N	\N	2026-07-04 08:32:38.689399
213	8	public	pur_doc	102	INSERT	\N	{"id": 102, "lines": [{"id": 32, "items": [{"id": 36, "amount": 10000.00000000, "vatAmount": 1200.00000000, "vatRateId": 2, "totalAmount": 11200.00000000, "vatRateName": "QQS 12%", "serialNumber": null, "markingNumber": "accounting\\\\src\\\\modules\\\\bank\\\\pages\\\\statement\\\\screens\\\\BankOperationDetailPage.tsx", "productTableId": 476}], "amount": 10000.00000000, "unitId": 1, "ownerId": 102, "quantity": 1.000000, "unitName": "Dona", "productId": 24, "unitPrice": 10000.00000000, "vatAmount": 1200.00000000, "vatRateId": 2, "productMxik": "08418001001005223", "productName": "ARTEL, икки камерали HD 341 FND ECO FROST ёмғирли-асфалт ранг", "totalAmount": 11200.00000000, "vatRateName": "QQS 12%"}], "comment": null, "docDate": "2026-07-04T08:32:26", "stateId": 1, "postedAt": null, "statusId": 1, "docNumber": "100000084", "stateName": "Aktiv", "vatAmount": 1200.00000000, "contractId": 9, "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "createdDate": "2026-07-04T08:33:22.100738", "finalAmount": 11200.00000000, "totalAmount": 10000.00000000, "warehouseId": 7, "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "warehouseName": "amonov", "contractNumber": "100000009", "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "counterpartyName": "Artel", "organizationName": "baraka_market", "cancelledByUserId": null}	4	\N	\N	\N	2026-07-04 08:33:22.186115
214	8	public	pur_doc	102	UPDATE	{"id": 102, "lines": [{"id": 32, "items": [{"id": 36, "amount": 10000.00000000, "vatAmount": 1200.00000000, "vatRateId": 2, "totalAmount": 11200.00000000, "vatRateName": "QQS 12%", "serialNumber": null, "markingNumber": "accounting\\\\src\\\\modules\\\\bank\\\\pages\\\\statement\\\\screens\\\\BankOperationDetailPage.tsx", "productTableId": 476}], "amount": 10000.00000000, "unitId": 1, "ownerId": 102, "quantity": 1.000000, "unitName": "Dona", "productId": 24, "unitPrice": 10000.00000000, "vatAmount": 1200.00000000, "vatRateId": 2, "productMxik": "08418001001005223", "productName": "ARTEL, икки камерали HD 341 FND ECO FROST ёмғирли-асфалт ранг", "totalAmount": 11200.00000000, "vatRateName": "QQS 12%"}], "comment": null, "docDate": "2026-07-04T08:32:26", "stateId": 1, "postedAt": null, "statusId": 1, "docNumber": "100000084", "stateName": "Aktiv", "vatAmount": 1200.00000000, "contractId": 9, "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "createdDate": "2026-07-04T08:33:22.100738", "finalAmount": 11200.00000000, "totalAmount": 10000.00000000, "warehouseId": 7, "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "warehouseName": "amonov", "contractNumber": "100000009", "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "counterpartyName": "Artel", "organizationName": "baraka_market", "cancelledByUserId": null}	{"id": 102, "lines": [{"id": 32, "items": [], "amount": 10000.00000000, "unitId": 1, "ownerId": 102, "quantity": 1.000000, "unitName": "Dona", "productId": 24, "unitPrice": 10000.00000000, "vatAmount": 1200.00000000, "vatRateId": 2, "productMxik": "08418001001005223", "productName": "ARTEL, икки камерали HD 341 FND ECO FROST ёмғирли-асфалт ранг", "totalAmount": 11200.00000000, "vatRateName": "QQS 12%"}], "comment": null, "docDate": "2026-07-04T08:32:26", "stateId": 1, "postedAt": null, "statusId": 3, "docNumber": "100000084", "stateName": "Aktiv", "vatAmount": 1200.00000000, "contractId": 9, "currencyId": 1, "statusName": "Bekor qilingan", "cancelledAt": "2026-07-04T08:47:52.115002", "createdDate": "2026-07-04T08:33:22.100738", "finalAmount": 11200.00000000, "totalAmount": 10000.00000000, "warehouseId": 7, "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "warehouseName": "amonov", "contractNumber": "100000009", "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "counterpartyName": "Artel", "organizationName": "baraka_market", "cancelledByUserId": 4}	4	\N	\N	\N	2026-07-04 08:47:52.149781
220	8	public	cash_operation	9	INSERT	\N	{"id": 9, "amount": 1000.00, "comment": "", "docDate": "2026-07-04T10:53:00", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000007", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T11:04:41.396372", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": 2, "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "operationTypeId": 2, "paymentTypeName": "Bank", "counterpartyName": "Artel", "organizationName": "baraka_market", "paymentPurposeId": 7, "cancelledByUserId": null, "operationTypeName": "Chiqim", "paymentPurposeName": "Dividend payment", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-04 11:04:41.428545
215	8	public	pur_doc	103	INSERT	\N	{"id": 103, "lines": [{"id": 33, "items": [{"id": 37, "amount": 10000.00000000, "vatAmount": 1200.00000000, "vatRateId": 2, "totalAmount": 11200.00000000, "vatRateName": "QQS 12%", "serialNumber": null, "markingNumber": "/api/manuals/counterparties", "productTableId": 477}], "amount": 10000.00000000, "unitId": 1, "ownerId": 103, "quantity": 1.000000, "unitName": "Dona", "productId": 24, "unitPrice": 10000.00000000, "vatAmount": 1200.00000000, "vatRateId": 2, "productMxik": "08418001001005223", "productName": "ARTEL, икки камерали HD 341 FND ECO FROST ёмғирли-асфалт ранг", "totalAmount": 11200.00000000, "vatRateName": "QQS 12%"}], "comment": null, "docDate": "2026-07-04T08:47:40", "stateId": 1, "postedAt": null, "statusId": 1, "docNumber": "100000085", "stateName": "Aktiv", "vatAmount": 1200.00000000, "contractId": 10, "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "createdDate": "2026-07-04T08:57:17.324885", "finalAmount": 11200.00000000, "totalAmount": 10000.00000000, "warehouseId": 7, "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "warehouseName": "amonov", "contractNumber": "100000010", "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "counterpartyName": "Artel", "organizationName": "baraka_market", "cancelledByUserId": null}	4	\N	\N	\N	2026-07-04 08:57:18.22828
216	8	public	cash_operation	8	INSERT	\N	{"id": 8, "amount": 3550000.00, "comment": "summmm", "docDate": "2026-07-04T10:53:00", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000006", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T11:02:35.793746", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": null, "counterpartyId": 17, "organizationId": 8, "postedByUserId": null, "operationTypeId": 2, "paymentTypeName": null, "counterpartyName": "aaaa", "organizationName": "baraka_market", "paymentPurposeId": 18, "cancelledByUserId": null, "operationTypeName": "Chiqim", "paymentPurposeName": "Bank fee", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-04 11:02:35.93788
217	8	public	cash_operation	8	UPDATE	{"id": 8, "amount": 3550000.00, "comment": "summmm", "docDate": "2026-07-04T10:53:00", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000006", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T11:02:35.793746", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": null, "counterpartyId": 17, "organizationId": 8, "postedByUserId": null, "operationTypeId": 2, "paymentTypeName": null, "counterpartyName": "aaaa", "organizationName": "baraka_market", "paymentPurposeId": 18, "cancelledByUserId": null, "operationTypeName": "Chiqim", "paymentPurposeName": "Bank fee", "destinationCashBoxId": null, "destinationCashBoxName": null}	{"id": 8, "amount": 3550000.00, "comment": "summmm", "docDate": "2026-07-04T10:53:00", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000006", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T11:02:35.793746", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": null, "counterpartyId": 17, "organizationId": 8, "postedByUserId": null, "operationTypeId": 2, "paymentTypeName": null, "counterpartyName": "aaaa", "organizationName": "baraka_market", "paymentPurposeId": 23, "cancelledByUserId": null, "operationTypeName": "Chiqim", "paymentPurposeName": "Cash collection to transit", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-04 11:03:14.936824
218	8	public	cash_operation	8	UPDATE	{"id": 8, "amount": 3550000.00, "comment": "summmm", "docDate": "2026-07-04T10:53:00", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000006", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T11:02:35.793746", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": null, "counterpartyId": 17, "organizationId": 8, "postedByUserId": null, "operationTypeId": 2, "paymentTypeName": null, "counterpartyName": "aaaa", "organizationName": "baraka_market", "paymentPurposeId": 23, "cancelledByUserId": null, "operationTypeName": "Chiqim", "paymentPurposeName": "Cash collection to transit", "destinationCashBoxId": null, "destinationCashBoxName": null}	{"id": 8, "amount": 1550000.00, "comment": "summmm", "docDate": "2026-07-04T10:53:00", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000006", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T11:02:35.793746", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": null, "counterpartyId": 17, "organizationId": 8, "postedByUserId": null, "operationTypeId": 2, "paymentTypeName": null, "counterpartyName": "aaaa", "organizationName": "baraka_market", "paymentPurposeId": 23, "cancelledByUserId": null, "operationTypeName": "Chiqim", "paymentPurposeName": "Cash collection to transit", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-04 11:03:33.752375
219	8	public	cash_operation	8	UPDATE	{"id": 8, "amount": 1550000.00, "comment": "summmm", "docDate": "2026-07-04T10:53:00", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000006", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T11:02:35.793746", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": null, "counterpartyId": 17, "organizationId": 8, "postedByUserId": null, "operationTypeId": 2, "paymentTypeName": null, "counterpartyName": "aaaa", "organizationName": "baraka_market", "paymentPurposeId": 23, "cancelledByUserId": null, "operationTypeName": "Chiqim", "paymentPurposeName": "Cash collection to transit", "destinationCashBoxId": null, "destinationCashBoxName": null}	{"id": 8, "amount": 1550000.00, "comment": "summmm", "docDate": "2026-07-04T10:53:00", "stateId": 1, "postedAt": "2026-07-04T11:03:34.212499", "statusId": 2, "cashBoxId": 4, "docNumber": "100000006", "stateName": "Aktiv", "currencyId": 1, "statusName": "O'tkazilgan", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T11:02:35.793746", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": null, "counterpartyId": 17, "organizationId": 8, "postedByUserId": 4, "operationTypeId": 2, "paymentTypeName": null, "counterpartyName": "aaaa", "organizationName": "baraka_market", "paymentPurposeId": 23, "cancelledByUserId": null, "operationTypeName": "Chiqim", "paymentPurposeName": "Cash collection to transit", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-04 11:03:34.229871
221	8	public	cash_operation	9	UPDATE	{"id": 9, "amount": 1000.00, "comment": "", "docDate": "2026-07-04T10:53:00", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000007", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T11:04:41.396372", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": 2, "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "operationTypeId": 2, "paymentTypeName": "Bank", "counterpartyName": "Artel", "organizationName": "baraka_market", "paymentPurposeId": 7, "cancelledByUserId": null, "operationTypeName": "Chiqim", "paymentPurposeName": "Dividend payment", "destinationCashBoxId": null, "destinationCashBoxName": null}	{"id": 9, "amount": 1000.00, "comment": "", "docDate": "2026-07-04T10:53:00", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000007", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T11:04:41.396372", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": 2, "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "operationTypeId": 2, "paymentTypeName": "Bank", "counterpartyName": "Artel", "organizationName": "baraka_market", "paymentPurposeId": 17, "cancelledByUserId": null, "operationTypeName": "Chiqim", "paymentPurposeName": "Pension fund", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-04 11:05:18.740479
222	8	public	cash_operation	9	UPDATE	{"id": 9, "amount": 1000.00, "comment": "", "docDate": "2026-07-04T10:53:00", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000007", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T11:04:41.396372", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": 2, "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "operationTypeId": 2, "paymentTypeName": "Bank", "counterpartyName": "Artel", "organizationName": "baraka_market", "paymentPurposeId": 17, "cancelledByUserId": null, "operationTypeName": "Chiqim", "paymentPurposeName": "Pension fund", "destinationCashBoxId": null, "destinationCashBoxName": null}	{"id": 9, "amount": 1000.00, "comment": "", "docDate": "2026-07-04T10:53:00", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000007", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T11:04:41.396372", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": 2, "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "operationTypeId": 2, "paymentTypeName": "Bank", "counterpartyName": "Artel", "organizationName": "baraka_market", "paymentPurposeId": 13, "cancelledByUserId": null, "operationTypeName": "Chiqim", "paymentPurposeName": "Profit tax", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-04 11:05:26.301062
223	8	public	cash_operation	9	UPDATE	{"id": 9, "amount": 1000.00, "comment": "", "docDate": "2026-07-04T10:53:00", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000007", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T11:04:41.396372", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": 2, "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "operationTypeId": 2, "paymentTypeName": "Bank", "counterpartyName": "Artel", "organizationName": "baraka_market", "paymentPurposeId": 13, "cancelledByUserId": null, "operationTypeName": "Chiqim", "paymentPurposeName": "Profit tax", "destinationCashBoxId": null, "destinationCashBoxName": null}	{"id": 9, "amount": 1000.00, "comment": "", "docDate": "2026-07-04T10:53:00", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000007", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T11:04:41.396372", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": 2, "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "operationTypeId": 2, "paymentTypeName": "Bank", "counterpartyName": "Artel", "organizationName": "baraka_market", "paymentPurposeId": 6, "cancelledByUserId": null, "operationTypeName": "Chiqim", "paymentPurposeName": "Accountable amounts", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-04 11:05:40.392924
224	8	public	cash_operation	10	INSERT	\N	{"id": 10, "amount": 100000000.00, "comment": "summmm", "docDate": "2026-07-04T10:53:00", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000008", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T12:05:20.893702", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": null, "counterpartyId": 19, "organizationId": 8, "postedByUserId": null, "operationTypeId": 1, "paymentTypeName": null, "counterpartyName": "Farrux Tech", "organizationName": "baraka_market", "paymentPurposeId": 3, "cancelledByUserId": null, "operationTypeName": "Kirim", "paymentPurposeName": "Payment from customer", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-04 12:05:20.918954
225	8	public	cash_operation	10	UPDATE	{"id": 10, "amount": 100000000.00, "comment": "summmm", "docDate": "2026-07-04T10:53:00", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000008", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T12:05:20.893702", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": null, "counterpartyId": 19, "organizationId": 8, "postedByUserId": null, "operationTypeId": 1, "paymentTypeName": null, "counterpartyName": "Farrux Tech", "organizationName": "baraka_market", "paymentPurposeId": 3, "cancelledByUserId": null, "operationTypeName": "Kirim", "paymentPurposeName": "Payment from customer", "destinationCashBoxId": null, "destinationCashBoxName": null}	{"id": 10, "amount": 100000000.00, "comment": "summmm", "docDate": "2026-07-04T10:53:00", "stateId": 1, "postedAt": "2026-07-04T12:05:26.580645", "statusId": 2, "cashBoxId": 4, "docNumber": "100000008", "stateName": "Aktiv", "currencyId": 1, "statusName": "O'tkazilgan", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T12:05:20.893702", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": null, "counterpartyId": 19, "organizationId": 8, "postedByUserId": 4, "operationTypeId": 1, "paymentTypeName": null, "counterpartyName": "Farrux Tech", "organizationName": "baraka_market", "paymentPurposeId": 3, "cancelledByUserId": null, "operationTypeName": "Kirim", "paymentPurposeName": "Payment from customer", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-04 12:05:26.597678
226	8	public	cash_operation	11	INSERT	\N	{"id": 11, "amount": 40000000.00, "comment": "create test", "docDate": "2026-07-04T11:09:31", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000009", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T12:06:21.540743", "currencyName": "Uzbek so'm", "exchangeRate": 12000.000000, "paymentTypeId": 1, "counterpartyId": 19, "organizationId": 8, "postedByUserId": null, "operationTypeId": 2, "paymentTypeName": "Naqd", "counterpartyName": "Farrux Tech", "organizationName": "baraka_market", "paymentPurposeId": 15, "cancelledByUserId": null, "operationTypeName": "Chiqim", "paymentPurposeName": "Land tax", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-04 12:06:21.554957
227	8	public	cash_operation	11	UPDATE	{"id": 11, "amount": 40000000.00, "comment": "create test", "docDate": "2026-07-04T11:09:31", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000009", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T12:06:21.540743", "currencyName": "Uzbek so'm", "exchangeRate": 12000.000000, "paymentTypeId": 1, "counterpartyId": 19, "organizationId": 8, "postedByUserId": null, "operationTypeId": 2, "paymentTypeName": "Naqd", "counterpartyName": "Farrux Tech", "organizationName": "baraka_market", "paymentPurposeId": 15, "cancelledByUserId": null, "operationTypeName": "Chiqim", "paymentPurposeName": "Land tax", "destinationCashBoxId": null, "destinationCashBoxName": null}	{"id": 11, "amount": 40000000.00, "comment": "create test", "docDate": "2026-07-04T11:09:31", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000009", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T12:06:21.540743", "currencyName": "Uzbek so'm", "exchangeRate": 12000.000000, "paymentTypeId": 1, "counterpartyId": 19, "organizationId": 8, "postedByUserId": null, "operationTypeId": 2, "paymentTypeName": "Naqd", "counterpartyName": "Farrux Tech", "organizationName": "baraka_market", "paymentPurposeId": 18, "cancelledByUserId": null, "operationTypeName": "Chiqim", "paymentPurposeName": "Bank fee", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-04 12:07:09.677575
228	8	public	cash_operation	11	UPDATE	{"id": 11, "amount": 40000000.00, "comment": "create test", "docDate": "2026-07-04T11:09:31", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000009", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T12:06:21.540743", "currencyName": "Uzbek so'm", "exchangeRate": 12000.000000, "paymentTypeId": 1, "counterpartyId": 19, "organizationId": 8, "postedByUserId": null, "operationTypeId": 2, "paymentTypeName": "Naqd", "counterpartyName": "Farrux Tech", "organizationName": "baraka_market", "paymentPurposeId": 18, "cancelledByUserId": null, "operationTypeName": "Chiqim", "paymentPurposeName": "Bank fee", "destinationCashBoxId": null, "destinationCashBoxName": null}	{"id": 11, "amount": 40000000.00, "comment": "create test", "docDate": "2026-07-04T11:09:31", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000009", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T12:06:21.540743", "currencyName": "Uzbek so'm", "exchangeRate": 12000.000000, "paymentTypeId": 1, "counterpartyId": 19, "organizationId": 8, "postedByUserId": null, "operationTypeId": 2, "paymentTypeName": "Naqd", "counterpartyName": "Farrux Tech", "organizationName": "baraka_market", "paymentPurposeId": 7, "cancelledByUserId": null, "operationTypeName": "Chiqim", "paymentPurposeName": "Dividend payment", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-04 12:07:23.833216
229	8	public	cash_operation	11	UPDATE	{"id": 11, "amount": 40000000.00, "comment": "create test", "docDate": "2026-07-04T11:09:31", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000009", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T12:06:21.540743", "currencyName": "Uzbek so'm", "exchangeRate": 12000.000000, "paymentTypeId": 1, "counterpartyId": 19, "organizationId": 8, "postedByUserId": null, "operationTypeId": 2, "paymentTypeName": "Naqd", "counterpartyName": "Farrux Tech", "organizationName": "baraka_market", "paymentPurposeId": 7, "cancelledByUserId": null, "operationTypeName": "Chiqim", "paymentPurposeName": "Dividend payment", "destinationCashBoxId": null, "destinationCashBoxName": null}	{"id": 11, "amount": 40000000.00, "comment": "create test", "docDate": "2026-07-04T11:09:31", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000009", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T12:06:21.540743", "currencyName": "Uzbek so'm", "exchangeRate": 12000.000000, "paymentTypeId": 1, "counterpartyId": 19, "organizationId": 8, "postedByUserId": null, "operationTypeId": 2, "paymentTypeName": "Naqd", "counterpartyName": "Farrux Tech", "organizationName": "baraka_market", "paymentPurposeId": 10, "cancelledByUserId": null, "operationTypeName": "Chiqim", "paymentPurposeName": "Loan received", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-04 12:07:37.02051
231	8	public	cash_operation	11	UPDATE	{"id": 11, "amount": 40000000.00, "comment": "create test", "docDate": "2026-07-04T11:09:31", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000009", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T12:06:21.540743", "currencyName": "Uzbek so'm", "exchangeRate": 12000.000000, "paymentTypeId": 1, "counterpartyId": 19, "organizationId": 8, "postedByUserId": null, "operationTypeId": 2, "paymentTypeName": "Naqd", "counterpartyName": "Farrux Tech", "organizationName": "baraka_market", "paymentPurposeId": 8, "cancelledByUserId": null, "operationTypeName": "Chiqim", "paymentPurposeName": "Loan given", "destinationCashBoxId": null, "destinationCashBoxName": null}	{"id": 11, "amount": 40000000.00, "comment": "create test", "docDate": "2026-07-04T11:09:31", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000009", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T12:06:21.540743", "currencyName": "Uzbek so'm", "exchangeRate": 12000.000000, "paymentTypeId": 1, "counterpartyId": 19, "organizationId": 8, "postedByUserId": null, "operationTypeId": 2, "paymentTypeName": "Naqd", "counterpartyName": "Farrux Tech", "organizationName": "baraka_market", "paymentPurposeId": 12, "cancelledByUserId": null, "operationTypeName": "Chiqim", "paymentPurposeName": "Personal income tax", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-04 12:08:13.693532
230	8	public	cash_operation	11	UPDATE	{"id": 11, "amount": 40000000.00, "comment": "create test", "docDate": "2026-07-04T11:09:31", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000009", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T12:06:21.540743", "currencyName": "Uzbek so'm", "exchangeRate": 12000.000000, "paymentTypeId": 1, "counterpartyId": 19, "organizationId": 8, "postedByUserId": null, "operationTypeId": 2, "paymentTypeName": "Naqd", "counterpartyName": "Farrux Tech", "organizationName": "baraka_market", "paymentPurposeId": 10, "cancelledByUserId": null, "operationTypeName": "Chiqim", "paymentPurposeName": "Loan received", "destinationCashBoxId": null, "destinationCashBoxName": null}	{"id": 11, "amount": 40000000.00, "comment": "create test", "docDate": "2026-07-04T11:09:31", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000009", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T12:06:21.540743", "currencyName": "Uzbek so'm", "exchangeRate": 12000.000000, "paymentTypeId": 1, "counterpartyId": 19, "organizationId": 8, "postedByUserId": null, "operationTypeId": 2, "paymentTypeName": "Naqd", "counterpartyName": "Farrux Tech", "organizationName": "baraka_market", "paymentPurposeId": 8, "cancelledByUserId": null, "operationTypeName": "Chiqim", "paymentPurposeName": "Loan given", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-04 12:08:02.683295
232	8	public	cash_operation	11	UPDATE	{"id": 11, "amount": 40000000.00, "comment": "create test", "docDate": "2026-07-04T11:09:31", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000009", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T12:06:21.540743", "currencyName": "Uzbek so'm", "exchangeRate": 12000.000000, "paymentTypeId": 1, "counterpartyId": 19, "organizationId": 8, "postedByUserId": null, "operationTypeId": 2, "paymentTypeName": "Naqd", "counterpartyName": "Farrux Tech", "organizationName": "baraka_market", "paymentPurposeId": 12, "cancelledByUserId": null, "operationTypeName": "Chiqim", "paymentPurposeName": "Personal income tax", "destinationCashBoxId": null, "destinationCashBoxName": null}	{"id": 11, "amount": 40000000.00, "comment": "create test", "docDate": "2026-07-04T11:09:31", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000009", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T12:06:21.540743", "currencyName": "Uzbek so'm", "exchangeRate": 12000.000000, "paymentTypeId": 1, "counterpartyId": 19, "organizationId": 8, "postedByUserId": null, "operationTypeId": 2, "paymentTypeName": "Naqd", "counterpartyName": "Farrux Tech", "organizationName": "baraka_market", "paymentPurposeId": 1, "cancelledByUserId": null, "operationTypeName": "Chiqim", "paymentPurposeName": "Supplier payment", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-04 12:08:18.545228
233	8	public	cash_operation	11	UPDATE	{"id": 11, "amount": 40000000.00, "comment": "create test", "docDate": "2026-07-04T11:09:31", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000009", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T12:06:21.540743", "currencyName": "Uzbek so'm", "exchangeRate": 12000.000000, "paymentTypeId": 1, "counterpartyId": 19, "organizationId": 8, "postedByUserId": null, "operationTypeId": 2, "paymentTypeName": "Naqd", "counterpartyName": "Farrux Tech", "organizationName": "baraka_market", "paymentPurposeId": 1, "cancelledByUserId": null, "operationTypeName": "Chiqim", "paymentPurposeName": "Supplier payment", "destinationCashBoxId": null, "destinationCashBoxName": null}	{"id": 11, "amount": 40000000.00, "comment": "create test", "docDate": "2026-07-04T11:09:31", "stateId": 1, "postedAt": "2026-07-04T12:08:18.646542", "statusId": 2, "cashBoxId": 4, "docNumber": "100000009", "stateName": "Aktiv", "currencyId": 1, "statusName": "O'tkazilgan", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T12:06:21.540743", "currencyName": "Uzbek so'm", "exchangeRate": 12000.000000, "paymentTypeId": 1, "counterpartyId": 19, "organizationId": 8, "postedByUserId": 4, "operationTypeId": 2, "paymentTypeName": "Naqd", "counterpartyName": "Farrux Tech", "organizationName": "baraka_market", "paymentPurposeId": 1, "cancelledByUserId": null, "operationTypeName": "Chiqim", "paymentPurposeName": "Supplier payment", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-04 12:08:18.668987
234	8	public	cash_operation	12	INSERT	\N	{"id": 12, "amount": 10000000.00, "comment": "", "docDate": "2026-07-04T12:10:12", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000010", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T12:11:59.254284", "currencyName": "Uzbek so'm", "exchangeRate": 12000.000000, "paymentTypeId": 1, "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "operationTypeId": 1, "paymentTypeName": "Naqd", "counterpartyName": "Artel", "organizationName": "baraka_market", "paymentPurposeId": 9, "cancelledByUserId": null, "operationTypeName": "Kirim", "paymentPurposeName": "Loan repayment", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-04 12:11:59.296171
235	8	public	cash_operation	12	UPDATE	{"id": 12, "amount": 10000000.00, "comment": "", "docDate": "2026-07-04T12:10:12", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000010", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T12:11:59.254284", "currencyName": "Uzbek so'm", "exchangeRate": 12000.000000, "paymentTypeId": 1, "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "operationTypeId": 1, "paymentTypeName": "Naqd", "counterpartyName": "Artel", "organizationName": "baraka_market", "paymentPurposeId": 9, "cancelledByUserId": null, "operationTypeName": "Kirim", "paymentPurposeName": "Loan repayment", "destinationCashBoxId": null, "destinationCashBoxName": null}	{"id": 12, "amount": 10000000.00, "comment": "", "docDate": "2026-07-04T12:10:12", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000010", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T12:11:59.254284", "currencyName": "Uzbek so'm", "exchangeRate": 12000.000000, "paymentTypeId": 1, "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "operationTypeId": 1, "paymentTypeName": "Naqd", "counterpartyName": "Artel", "organizationName": "baraka_market", "paymentPurposeId": 24, "cancelledByUserId": null, "operationTypeName": "Kirim", "paymentPurposeName": "Cash collection from transit", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-04 12:19:35.996609
236	8	public	cash_operation	12	UPDATE	{"id": 12, "amount": 10000000.00, "comment": "", "docDate": "2026-07-04T12:10:12", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000010", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T12:11:59.254284", "currencyName": "Uzbek so'm", "exchangeRate": 12000.000000, "paymentTypeId": 1, "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "operationTypeId": 1, "paymentTypeName": "Naqd", "counterpartyName": "Artel", "organizationName": "baraka_market", "paymentPurposeId": 24, "cancelledByUserId": null, "operationTypeName": "Kirim", "paymentPurposeName": "Cash collection from transit", "destinationCashBoxId": null, "destinationCashBoxName": null}	{"id": 12, "amount": 10000000.00, "comment": "", "docDate": "2026-07-04T12:10:12", "stateId": 1, "postedAt": "2026-07-04T12:19:36.147352", "statusId": 2, "cashBoxId": 4, "docNumber": "100000010", "stateName": "Aktiv", "currencyId": 1, "statusName": "O'tkazilgan", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T12:11:59.254284", "currencyName": "Uzbek so'm", "exchangeRate": 12000.000000, "paymentTypeId": 1, "counterpartyId": 18, "organizationId": 8, "postedByUserId": 4, "operationTypeId": 1, "paymentTypeName": "Naqd", "counterpartyName": "Artel", "organizationName": "baraka_market", "paymentPurposeId": 24, "cancelledByUserId": null, "operationTypeName": "Kirim", "paymentPurposeName": "Cash collection from transit", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-04 12:19:36.174067
237	8	public	cash_operation	13	INSERT	\N	{"id": 13, "amount": 3550000.00, "comment": "", "docDate": "2026-07-04T12:19:59", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000011", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T12:20:14.818983", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": 1, "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "operationTypeId": 1, "paymentTypeName": "Naqd", "counterpartyName": "Artel", "organizationName": "baraka_market", "paymentPurposeId": 24, "cancelledByUserId": null, "operationTypeName": "Kirim", "paymentPurposeName": "Cash collection from transit", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-04 12:20:14.83524
238	8	public	cash_operation	14	INSERT	\N	{"id": 14, "amount": 10000000.00, "comment": "", "docDate": "2026-07-04T12:21:58", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000012", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T12:22:16.880025", "currencyName": "Uzbek so'm", "exchangeRate": 12000.000000, "paymentTypeId": 1, "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "operationTypeId": 1, "paymentTypeName": "Naqd", "counterpartyName": "Artel", "organizationName": "baraka_market", "paymentPurposeId": 24, "cancelledByUserId": null, "operationTypeName": "Kirim", "paymentPurposeName": "Cash collection from transit", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-04 12:22:16.89664
246	8	public	cash_operation	18	INSERT	\N	{"id": 18, "amount": 1000.00, "comment": "", "docDate": "2026-07-04T12:25:48", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000016", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T12:26:04.642746", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": 1, "counterpartyId": 20, "organizationId": 8, "postedByUserId": null, "operationTypeId": 1, "paymentTypeName": "Naqd", "counterpartyName": "Ava", "organizationName": "baraka_market", "paymentPurposeId": 24, "cancelledByUserId": null, "operationTypeName": "Kirim", "paymentPurposeName": "Cash collection from transit", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-04 12:26:04.65573
248	8	public	cash_operation	9	UPDATE	{"id": 9, "amount": 1000.00, "comment": "", "docDate": "2026-07-04T10:53:00", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000007", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T11:04:41.396372", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": 2, "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "operationTypeId": 2, "paymentTypeName": "Bank", "counterpartyName": "Artel", "organizationName": "baraka_market", "paymentPurposeId": 6, "cancelledByUserId": null, "operationTypeName": "Chiqim", "paymentPurposeName": "Accountable amounts", "destinationCashBoxId": null, "destinationCashBoxName": null}	{"id": 9, "amount": 1000.00, "comment": "", "docDate": "2026-07-04T10:53:00", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000007", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T11:04:41.396372", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": 2, "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "operationTypeId": 2, "paymentTypeName": "Bank", "counterpartyName": "Artel", "organizationName": "baraka_market", "paymentPurposeId": 18, "cancelledByUserId": null, "operationTypeName": "Chiqim", "paymentPurposeName": "Bank fee", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-04 12:27:13.776974
239	8	public	cash_operation	15	INSERT	\N	{"id": 15, "amount": 2550000.00, "comment": "", "docDate": "2026-07-04T12:21:58", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000013", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T12:22:34.200745", "currencyName": "Uzbek so'm", "exchangeRate": 12000.000000, "paymentTypeId": 1, "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "operationTypeId": 1, "paymentTypeName": "Naqd", "counterpartyName": "Artel", "organizationName": "baraka_market", "paymentPurposeId": 24, "cancelledByUserId": null, "operationTypeName": "Kirim", "paymentPurposeName": "Cash collection from transit", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-04 12:22:34.214546
240	8	public	cash_operation	15	UPDATE	{"id": 15, "amount": 2550000.00, "comment": "", "docDate": "2026-07-04T12:21:58", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000013", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T12:22:34.200745", "currencyName": "Uzbek so'm", "exchangeRate": 12000.000000, "paymentTypeId": 1, "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "operationTypeId": 1, "paymentTypeName": "Naqd", "counterpartyName": "Artel", "organizationName": "baraka_market", "paymentPurposeId": 24, "cancelledByUserId": null, "operationTypeName": "Kirim", "paymentPurposeName": "Cash collection from transit", "destinationCashBoxId": null, "destinationCashBoxName": null}	{"id": 15, "amount": 2550000.00, "comment": "", "docDate": "2026-07-04T12:21:58", "stateId": 1, "postedAt": "2026-07-04T12:22:53.918155", "statusId": 2, "cashBoxId": 4, "docNumber": "100000013", "stateName": "Aktiv", "currencyId": 1, "statusName": "O'tkazilgan", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T12:22:34.200745", "currencyName": "Uzbek so'm", "exchangeRate": 12000.000000, "paymentTypeId": 1, "counterpartyId": 18, "organizationId": 8, "postedByUserId": 4, "operationTypeId": 1, "paymentTypeName": "Naqd", "counterpartyName": "Artel", "organizationName": "baraka_market", "paymentPurposeId": 24, "cancelledByUserId": null, "operationTypeName": "Kirim", "paymentPurposeName": "Cash collection from transit", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-04 12:22:53.935262
241	8	public	cash_operation	14	UPDATE	{"id": 14, "amount": 10000000.00, "comment": "", "docDate": "2026-07-04T12:21:58", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000012", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T12:22:16.880025", "currencyName": "Uzbek so'm", "exchangeRate": 12000.000000, "paymentTypeId": 1, "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "operationTypeId": 1, "paymentTypeName": "Naqd", "counterpartyName": "Artel", "organizationName": "baraka_market", "paymentPurposeId": 24, "cancelledByUserId": null, "operationTypeName": "Kirim", "paymentPurposeName": "Cash collection from transit", "destinationCashBoxId": null, "destinationCashBoxName": null}	{"id": 14, "amount": 10000000.00, "comment": "", "docDate": "2026-07-04T12:21:58", "stateId": 1, "postedAt": "2026-07-04T12:23:18.054785", "statusId": 2, "cashBoxId": 4, "docNumber": "100000012", "stateName": "Aktiv", "currencyId": 1, "statusName": "O'tkazilgan", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T12:22:16.880025", "currencyName": "Uzbek so'm", "exchangeRate": 12000.000000, "paymentTypeId": 1, "counterpartyId": 18, "organizationId": 8, "postedByUserId": 4, "operationTypeId": 1, "paymentTypeName": "Naqd", "counterpartyName": "Artel", "organizationName": "baraka_market", "paymentPurposeId": 24, "cancelledByUserId": null, "operationTypeName": "Kirim", "paymentPurposeName": "Cash collection from transit", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-04 12:23:18.071047
242	8	public	cash_operation	13	UPDATE	{"id": 13, "amount": 3550000.00, "comment": "", "docDate": "2026-07-04T12:19:59", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000011", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T12:20:14.818983", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": 1, "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "operationTypeId": 1, "paymentTypeName": "Naqd", "counterpartyName": "Artel", "organizationName": "baraka_market", "paymentPurposeId": 24, "cancelledByUserId": null, "operationTypeName": "Kirim", "paymentPurposeName": "Cash collection from transit", "destinationCashBoxId": null, "destinationCashBoxName": null}	{"id": 13, "amount": 3550000.00, "comment": "", "docDate": "2026-07-04T12:19:59", "stateId": 1, "postedAt": "2026-07-04T12:24:02.058818", "statusId": 2, "cashBoxId": 4, "docNumber": "100000011", "stateName": "Aktiv", "currencyId": 1, "statusName": "O'tkazilgan", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T12:20:14.818983", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": 1, "counterpartyId": 18, "organizationId": 8, "postedByUserId": 4, "operationTypeId": 1, "paymentTypeName": "Naqd", "counterpartyName": "Artel", "organizationName": "baraka_market", "paymentPurposeId": 24, "cancelledByUserId": null, "operationTypeName": "Kirim", "paymentPurposeName": "Cash collection from transit", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-04 12:24:02.069162
243	8	public	cash_operation	16	INSERT	\N	{"id": 16, "amount": 1000.00, "comment": "", "docDate": "2026-07-04T12:24:15", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000014", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T12:24:30.863325", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": 1, "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "operationTypeId": 1, "paymentTypeName": "Naqd", "counterpartyName": "Artel", "organizationName": "baraka_market", "paymentPurposeId": 24, "cancelledByUserId": null, "operationTypeName": "Kirim", "paymentPurposeName": "Cash collection from transit", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-04 12:24:30.890235
244	8	public	cash_operation	16	UPDATE	{"id": 16, "amount": 1000.00, "comment": "", "docDate": "2026-07-04T12:24:15", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000014", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T12:24:30.863325", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": 1, "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "operationTypeId": 1, "paymentTypeName": "Naqd", "counterpartyName": "Artel", "organizationName": "baraka_market", "paymentPurposeId": 24, "cancelledByUserId": null, "operationTypeName": "Kirim", "paymentPurposeName": "Cash collection from transit", "destinationCashBoxId": null, "destinationCashBoxName": null}	{"id": 16, "amount": 1000.00, "comment": "", "docDate": "2026-07-04T12:24:15", "stateId": 1, "postedAt": "2026-07-04T12:24:35.324369", "statusId": 2, "cashBoxId": 4, "docNumber": "100000014", "stateName": "Aktiv", "currencyId": 1, "statusName": "O'tkazilgan", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T12:24:30.863325", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": 1, "counterpartyId": 18, "organizationId": 8, "postedByUserId": 4, "operationTypeId": 1, "paymentTypeName": "Naqd", "counterpartyName": "Artel", "organizationName": "baraka_market", "paymentPurposeId": 24, "cancelledByUserId": null, "operationTypeName": "Kirim", "paymentPurposeName": "Cash collection from transit", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-04 12:24:35.346431
245	8	public	cash_operation	17	INSERT	\N	{"id": 17, "amount": 1000.00, "comment": "", "docDate": "2026-07-04T12:24:54", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000015", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T12:25:18.545664", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": 2, "counterpartyId": 17, "organizationId": 8, "postedByUserId": null, "operationTypeId": 1, "paymentTypeName": "Bank", "counterpartyName": "aaaa", "organizationName": "baraka_market", "paymentPurposeId": 24, "cancelledByUserId": null, "operationTypeName": "Kirim", "paymentPurposeName": "Cash collection from transit", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-04 12:25:18.559258
247	8	public	cash_operation	17	UPDATE	{"id": 17, "amount": 1000.00, "comment": "", "docDate": "2026-07-04T12:24:54", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000015", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T12:25:18.545664", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": 2, "counterpartyId": 17, "organizationId": 8, "postedByUserId": null, "operationTypeId": 1, "paymentTypeName": "Bank", "counterpartyName": "aaaa", "organizationName": "baraka_market", "paymentPurposeId": 24, "cancelledByUserId": null, "operationTypeName": "Kirim", "paymentPurposeName": "Cash collection from transit", "destinationCashBoxId": null, "destinationCashBoxName": null}	{"id": 17, "amount": 1000.00, "comment": "", "docDate": "2026-07-04T12:24:54", "stateId": 1, "postedAt": null, "statusId": 3, "cashBoxId": 4, "docNumber": "100000015", "stateName": "Aktiv", "currencyId": 1, "statusName": "Bekor qilingan", "cancelledAt": "2026-07-04T12:26:20.004595", "cashBoxName": "kassa", "createdDate": "2026-07-04T12:25:18.545664", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": 2, "counterpartyId": 17, "organizationId": 8, "postedByUserId": null, "operationTypeId": 1, "paymentTypeName": "Bank", "counterpartyName": "aaaa", "organizationName": "baraka_market", "paymentPurposeId": 24, "cancelledByUserId": 4, "operationTypeName": "Kirim", "paymentPurposeName": "Cash collection from transit", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-04 12:26:20.030422
249	8	public	cash_operation	9	UPDATE	{"id": 9, "amount": 1000.00, "comment": "", "docDate": "2026-07-04T10:53:00", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000007", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T11:04:41.396372", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": 2, "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "operationTypeId": 2, "paymentTypeName": "Bank", "counterpartyName": "Artel", "organizationName": "baraka_market", "paymentPurposeId": 18, "cancelledByUserId": null, "operationTypeName": "Chiqim", "paymentPurposeName": "Bank fee", "destinationCashBoxId": null, "destinationCashBoxName": null}	{"id": 9, "amount": 1000.00, "comment": "", "docDate": "2026-07-04T10:53:00", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000007", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T11:04:41.396372", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": 2, "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "operationTypeId": 2, "paymentTypeName": "Bank", "counterpartyName": "Artel", "organizationName": "baraka_market", "paymentPurposeId": 8, "cancelledByUserId": null, "operationTypeName": "Chiqim", "paymentPurposeName": "Loan given", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-04 12:27:26.443987
250	8	public	cash_operation	9	UPDATE	{"id": 9, "amount": 1000.00, "comment": "", "docDate": "2026-07-04T10:53:00", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000007", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T11:04:41.396372", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": 2, "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "operationTypeId": 2, "paymentTypeName": "Bank", "counterpartyName": "Artel", "organizationName": "baraka_market", "paymentPurposeId": 8, "cancelledByUserId": null, "operationTypeName": "Chiqim", "paymentPurposeName": "Loan given", "destinationCashBoxId": null, "destinationCashBoxName": null}	{"id": 9, "amount": 1000.00, "comment": "", "docDate": "2026-07-04T10:53:00", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000007", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T11:04:41.396372", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": 2, "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "operationTypeId": 2, "paymentTypeName": "Bank", "counterpartyName": "Artel", "organizationName": "baraka_market", "paymentPurposeId": 1, "cancelledByUserId": null, "operationTypeName": "Chiqim", "paymentPurposeName": "Supplier payment", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-04 12:27:31.873761
251	8	public	cash_operation	9	UPDATE	{"id": 9, "amount": 1000.00, "comment": "", "docDate": "2026-07-04T10:53:00", "stateId": 1, "postedAt": null, "statusId": 1, "cashBoxId": 4, "docNumber": "100000007", "stateName": "Aktiv", "currencyId": 1, "statusName": "Qoralama", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T11:04:41.396372", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": 2, "counterpartyId": 18, "organizationId": 8, "postedByUserId": null, "operationTypeId": 2, "paymentTypeName": "Bank", "counterpartyName": "Artel", "organizationName": "baraka_market", "paymentPurposeId": 1, "cancelledByUserId": null, "operationTypeName": "Chiqim", "paymentPurposeName": "Supplier payment", "destinationCashBoxId": null, "destinationCashBoxName": null}	{"id": 9, "amount": 1000.00, "comment": "", "docDate": "2026-07-04T10:53:00", "stateId": 1, "postedAt": "2026-07-04T12:27:31.974301", "statusId": 2, "cashBoxId": 4, "docNumber": "100000007", "stateName": "Aktiv", "currencyId": 1, "statusName": "O'tkazilgan", "cancelledAt": null, "cashBoxName": "kassa", "createdDate": "2026-07-04T11:04:41.396372", "currencyName": "Uzbek so'm", "exchangeRate": 1.000000, "paymentTypeId": 2, "counterpartyId": 18, "organizationId": 8, "postedByUserId": 4, "operationTypeId": 2, "paymentTypeName": "Bank", "counterpartyName": "Artel", "organizationName": "baraka_market", "paymentPurposeId": 1, "cancelledByUserId": null, "operationTypeName": "Chiqim", "paymentPurposeName": "Supplier payment", "destinationCashBoxId": null, "destinationCashBoxName": null}	4	\N	\N	\N	2026-07-04 12:27:31.983263
\.


--
-- Data for Name: sys_email_verification_token; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.sys_email_verification_token (id, user_id, token_hash, email, expires_at, verified_at, created_date) FROM stdin;
\.


--
-- Data for Name: sys_module; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.sys_module (id, code, short_name, full_name, sub_group_id, state_id, created_date, parent_id, route, icon, sort_order, is_visible) FROM stdin;
1102	WAREHOUSE_TRANSFER_VIEW	Ombor ko'chirishlar	Ro'yxat	15	1	2026-07-03 00:00:00	\N	\N	\N	0	t
1103	WAREHOUSE_TRANSFER_VIEW_DETAIL	Ombor ko'chirish detail	Batafsil	15	1	2026-07-03 00:00:00	\N	\N	\N	0	t
1104	WAREHOUSE_TRANSFER_CREATE	Ombor ko'chirish yaratish	Yangi	15	1	2026-07-03 00:00:00	\N	\N	\N	0	t
1105	WAREHOUSE_TRANSFER_UPDATE	Ombor ko'chirish tahrirlash	Tahrirlash	15	1	2026-07-03 00:00:00	\N	\N	\N	0	t
1106	WAREHOUSE_TRANSFER_DELETE	Ombor ko'chirish o'chirish	O'chirish	15	1	2026-07-03 00:00:00	\N	\N	\N	0	t
1107	CONFIRM_WAREHOUSE_TRANSFER	Ombor ko'chirishni tasdiqlash	Tasdiqlash	15	1	2026-07-03 00:00:00	\N	\N	\N	0	t
1108	CANCEL_WAREHOUSE_TRANSFER	Ombor ko'chirishni bekor qilish	Bekor qilish	15	1	2026-07-03 00:00:00	\N	\N	\N	0	t
1109	INVENTORY_ADJUSTMENT_VIEW	Inventar tuzatishlar	Ro'yxat	16	1	2026-07-03 00:00:00	\N	\N	\N	0	t
1110	INVENTORY_ADJUSTMENT_VIEW_DETAIL	Inventar tuzatish detail	Batafsil	16	1	2026-07-03 00:00:00	\N	\N	\N	0	t
1111	INVENTORY_ADJUSTMENT_CREATE	Inventar tuzatish yaratish	Yangi	16	1	2026-07-03 00:00:00	\N	\N	\N	0	t
1112	INVENTORY_ADJUSTMENT_UPDATE	Inventar tuzatish tahrirlash	Tahrirlash	16	1	2026-07-03 00:00:00	\N	\N	\N	0	t
1113	INVENTORY_ADJUSTMENT_DELETE	Inventar tuzatish o'chirish	O'chirish	16	1	2026-07-03 00:00:00	\N	\N	\N	0	t
1114	CONFIRM_INVENTORY_ADJUSTMENT	Inventar tuzatishni tasdiqlash	Tasdiqlash	16	1	2026-07-03 00:00:00	\N	\N	\N	0	t
1115	CANCEL_INVENTORY_ADJUSTMENT	Inventar tuzatishni bekor qilish	Bekor qilish	16	1	2026-07-03 00:00:00	\N	\N	\N	0	t
1116	INVENTORY_COUNT_VIEW	Inventar sanog'i	Ro'yxat	17	1	2026-07-03 00:00:00	\N	\N	\N	0	t
1117	INVENTORY_COUNT_VIEW_DETAIL	Inventar sanog'i detail	Batafsil	17	1	2026-07-03 00:00:00	\N	\N	\N	0	t
1118	INVENTORY_COUNT_CREATE	Inventar sanog'i yaratish	Yangi	17	1	2026-07-03 00:00:00	\N	\N	\N	0	t
1119	INVENTORY_COUNT_UPDATE	Inventar sanog'i tahrirlash	Tahrirlash	17	1	2026-07-03 00:00:00	\N	\N	\N	0	t
1120	INVENTORY_COUNT_DELETE	Inventar sanog'i o'chirish	O'chirish	17	1	2026-07-03 00:00:00	\N	\N	\N	0	t
1121	CONFIRM_INVENTORY_COUNT	Inventar sanog'ini tasdiqlash	Tasdiqlash	17	1	2026-07-03 00:00:00	\N	\N	\N	0	t
1122	CANCEL_INVENTORY_COUNT	Inventar sanog'ini bekor qilish	Bekor qilish	17	1	2026-07-03 00:00:00	\N	\N	\N	0	t
516	CONFIRM_BANK_OPERATION	Bank operatsiyasini tasdiqlash	Tasdiqlash	5	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
517	CANCEL_BANK_OPERATION	Bank operatsiyasini bekor qilish	Bekor qilish	5	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
1139	AUDIT_LOG_VIEW	Audit log	Audit loglarni ko'rish	1	1	2026-07-05 00:00:00	\N	/audit-logs	history	90	t
1140	SETTINGS_MANAGE	Tizim sozlamalari	Tizim sozlamalarini boshqarish	1	1	2026-07-05 00:00:00	\N	/settings	settings	92	t
1141	DASHBOARD_VIEW	Dashboard	Super Admin dashboard statistikasi	1	1	2026-07-05 00:00:00	\N	/dashboard	dashboard	91	t
441	PRODUCT_TABLE_VIEW	Tovar kartochkalari	Tovar kartochkalarini ko'rish	4	1	2026-06-20 10:15:52.743562	\N	\N	\N	0	t
442	PRODUCT_TABLE_VIEW_DETAIL	Tovar kartochkasi detail	Tovar kartochkasini batafsil	4	1	2026-06-20 10:15:52.743562	\N	\N	\N	0	t
101	ROLE_VIEW	Rollar ro'yxati	Rollarni ko'rish	1	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
102	ROLE_VIEW_DETAIL	Rol detail	Rolni batafsil ko'rish	1	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
103	ROLE_CREATE	Rol yaratish	Yangi rol qo'shish	1	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
104	ROLE_UPDATE	Rol tahrirlash	Rolni tahrirlash	1	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
105	ROLE_DELETE	Rol o'chirish	Rolni o'chirish	1	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
111	USER_VIEW	Foydalanuvchilar	Foydalanuvchilarni ko'rish	1	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
112	USER_VIEW_DETAIL	Foydalanuvchi detail	Foydalanuvchini batafsil	1	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
113	USER_CREATE	Foydalanuvchi yaratish	Yangi foydalanuvchi	1	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
114	USER_UPDATE	Foydalanuvchi tahrir	Foydalanuvchini tahrirlash	1	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
115	USER_DELETE	Foydalanuvchi o'chirish	Foydalanuvchini o'chirish	1	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
201	ORGANIZATION_VIEW	Tashkilotlar	Tashkilotlar ro'yxati	2	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
202	ORGANIZATION_VIEW_DETAIL	Tashkilot detail	Tashkilotni batafsil ko'rish	2	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
203	ORGANIZATION_CREATE	Tashkilot yaratish	Yangi tashkilot	2	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
204	ORGANIZATION_UPDATE	Tashkilot tahrirlash	Tashkilotni tahrirlash	2	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
205	ORGANIZATION_DELETE	Tashkilot o'chirish	Tashkilotni o'chirish	2	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
211	BRANCH_VIEW	Filiallar	Filiallar ro'yxati	2	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
212	BRANCH_VIEW_DETAIL	Filial detail	Filialni batafsil ko'rish	2	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
213	BRANCH_CREATE	Filial yaratish	Yangi filial	2	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
214	BRANCH_UPDATE	Filial tahrirlash	Filialni tahrirlash	2	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
215	BRANCH_DELETE	Filial o'chirish	Filialni o'chirish	2	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
221	DEPARTMENT_VIEW	Bo'limlar	Bo'limlar ro'yxati	2	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
222	DEPARTMENT_VIEW_DETAIL	Bo'lim detail	Bo'limni batafsil ko'rish	2	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
223	DEPARTMENT_CREATE	Bo'lim yaratish	Yangi bo'lim	2	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
224	DEPARTMENT_UPDATE	Bo'lim tahrirlash	Bo'limni tahrirlash	2	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
225	DEPARTMENT_DELETE	Bo'lim o'chirish	Bo'limni o'chirish	2	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
231	POSITION_VIEW	Lavozimlar	Lavozimlar ro'yxati	2	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
232	POSITION_VIEW_DETAIL	Lavozim detail	Lavozimni batafsil ko'rish	2	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
233	POSITION_CREATE	Lavozim yaratish	Yangi lavozim	2	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
234	POSITION_UPDATE	Lavozim tahrirlash	Lavozimni tahrirlash	2	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
235	POSITION_DELETE	Lavozim o'chirish	Lavozimni o'chirish	2	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
301	COUNTERPARTY_CARD_VIEW	Kontragentlar	Kontragentlar ro'yxati	3	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
302	COUNTERPARTY_CARD_VIEW_DETAIL	Kontragent detail	Kontragentni batafsil ko'rish	3	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
303	COUNTERPARTY_CARD_CREATE	Kontragent yaratish	Yangi kontragent	3	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
304	COUNTERPARTY_CARD_UPDATE	Kontragent tahrirlash	Kontragentni tahrirlash	3	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
305	COUNTERPARTY_CARD_DELETE	Kontragent o'chirish	Kontragentni o'chirish	3	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
311	COUNTERPARTY_BANK_ACCOUNT_VIEW	Kontragent bank hisoblari	Ro'yxat	3	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
312	COUNTERPARTY_BANK_ACCOUNT_VIEW_DETAIL	Kontragent bank hisobi detail	Batafsil	3	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
313	COUNTERPARTY_BANK_ACCOUNT_CREATE	Kontragent bank hisobi yaratish	Yangi	3	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
314	COUNTERPARTY_BANK_ACCOUNT_UPDATE	Kontragent bank hisobi tahrirlash	Tahrirlash	3	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
315	COUNTERPARTY_BANK_ACCOUNT_DELETE	Kontragent bank hisobi o'chirish	O'chirish	3	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
321	COUNTERPARTY_CONTACT_VIEW	Kontragent kontaktlari	Ro'yxat	3	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
322	COUNTERPARTY_CONTACT_VIEW_DETAIL	Kontragent kontakt detail	Batafsil	3	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
323	COUNTERPARTY_CONTACT_CREATE	Kontragent kontakt yaratish	Yangi	3	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
324	COUNTERPARTY_CONTACT_UPDATE	Kontragent kontakt tahrirlash	Tahrirlash	3	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
325	COUNTERPARTY_CONTACT_DELETE	Kontragent kontakt o'chirish	O'chirish	3	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
401	PRODUCT_GROUP_VIEW	Tovar guruhlari	Ro'yxat	4	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
402	PRODUCT_GROUP_VIEW_DETAIL	Tovar guruhi detail	Batafsil	4	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
403	PRODUCT_GROUP_CREATE	Tovar guruhi yaratish	Yangi	4	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
404	PRODUCT_GROUP_UPDATE	Tovar guruhi tahrirlash	Tahrirlash	4	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
405	PRODUCT_GROUP_DELETE	Tovar guruhi o'chirish	O'chirish	4	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
411	PRODUCT_VIEW	Tovarlar	Ro'yxat	4	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
412	PRODUCT_VIEW_DETAIL	Tovar detail	Batafsil	4	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
413	PRODUCT_CREATE	Tovar yaratish	Yangi	4	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
414	PRODUCT_UPDATE	Tovar tahrirlash	Tahrirlash	4	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
415	PRODUCT_DELETE	Tovar o'chirish	O'chirish	4	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
421	PRODUCT_PRICE_VIEW	Tovar narxlari	Ro'yxat	4	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
422	PRODUCT_PRICE_VIEW_DETAIL	Tovar narxi detail	Batafsil	4	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
423	PRODUCT_PRICE_CREATE	Tovar narxi yaratish	Yangi	4	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
424	PRODUCT_PRICE_UPDATE	Tovar narxi tahrirlash	Tahrirlash	4	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
425	PRODUCT_PRICE_DELETE	Tovar narxi o'chirish	O'chirish	4	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
431	WAREHOUSE_VIEW	Omborlar	Ro'yxat	4	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
432	WAREHOUSE_VIEW_DETAIL	Ombor detail	Batafsil	4	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
433	WAREHOUSE_CREATE	Ombor yaratish	Yangi	4	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
434	WAREHOUSE_UPDATE	Ombor tahrirlash	Tahrirlash	4	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
435	WAREHOUSE_DELETE	Ombor o'chirish	O'chirish	4	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
501	ORG_BANK_ACCOUNT_VIEW	Tashkilot bank hisoblari	Ro'yxat	5	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
502	ORG_BANK_ACCOUNT_VIEW_DETAIL	Tashkilot bank hisobi detail	Batafsil	5	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
503	ORG_BANK_ACCOUNT_CREATE	Tashkilot bank hisobi yaratish	Yangi	5	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
504	ORG_BANK_ACCOUNT_UPDATE	Tashkilot bank hisobi tahrirlash	Tahrirlash	5	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
505	ORG_BANK_ACCOUNT_DELETE	Tashkilot bank hisobi o'chirish	O'chirish	5	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
511	BANK_OPERATION_VIEW	Bank operatsiyalari	Ro'yxat	5	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
512	BANK_OPERATION_VIEW_DETAIL	Bank operatsiyasi detail	Batafsil	5	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
513	BANK_OPERATION_CREATE	Bank operatsiyasi yaratish	Yangi	5	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
514	BANK_OPERATION_UPDATE	Bank operatsiyasi tahrirlash	Tahrirlash	5	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
515	BANK_OPERATION_DELETE	Bank operatsiyasi o'chirish	O'chirish	5	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
601	CASH_BOX_VIEW	Kassalar	Ro'yxat	6	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
602	CASH_BOX_VIEW_DETAIL	Kassa detail	Batafsil	6	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
603	CASH_BOX_CREATE	Kassa yaratish	Yangi	6	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
604	CASH_BOX_UPDATE	Kassa tahrirlash	Tahrirlash	6	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
605	CASH_BOX_DELETE	Kassa o'chirish	O'chirish	6	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
611	CASH_OPERATION_VIEW	Kassa operatsiyalari	Ro'yxat	6	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
612	CASH_OPERATION_VIEW_DETAIL	Kassa operatsiyasi detail	Batafsil	6	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
613	CASH_OPERATION_CREATE	Kassa operatsiyasi yaratish	Yangi	6	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
614	CASH_OPERATION_UPDATE	Kassa operatsiyasi tahrirlash	Tahrirlash	6	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
615	CASH_OPERATION_DELETE	Kassa operatsiyasi o'chirish	O'chirish	6	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
616	CONFIRM_CASH_OPERATION	Kassa operatsiyasini tasdiqlash	Tasdiqlash	6	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
617	CANCEL_CASH_OPERATION	Kassa operatsiyasini bekor qilish	Bekor qilish	6	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
701	PURCHASE_DOC_VIEW	Xarid hujjatlari	Ro'yxat	7	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
702	PURCHASE_DOC_VIEW_DETAIL	Xarid hujjati detail	Batafsil	7	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
703	PURCHASE_DOC_CREATE	Xarid hujjati yaratish	Yangi	7	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
704	PURCHASE_DOC_UPDATE	Xarid hujjati tahrirlash	Tahrirlash	7	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
705	PURCHASE_DOC_DELETE	Xarid hujjati o'chirish	O'chirish	7	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
706	CONFIRM_PURCHASE	Xaridni tasdiqlash	Tasdiqlash	7	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
707	CANCEL_PURCHASE	Xaridni bekor qilish	Bekor qilish	7	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
711	PURCHASE_DOC_TABLE_VIEW	Xarid satrlari	Ro'yxat	7	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
712	PURCHASE_DOC_TABLE_VIEW_DETAIL	Xarid satri detail	Batafsil	7	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
713	PURCHASE_DOC_TABLE_CREATE	Xarid satri yaratish	Yangi	7	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
714	PURCHASE_DOC_TABLE_UPDATE	Xarid satri tahrirlash	Tahrirlash	7	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
715	PURCHASE_DOC_TABLE_DELETE	Xarid satri o'chirish	O'chirish	7	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
721	CONTRACT_VIEW	Shartnomalar	Shartnomalar ro'yxati	7	1	2026-06-16 17:32:01.442269	\N	\N	\N	0	t
722	CONTRACT_VIEW_DETAIL	Shartnoma detail	Shartnomani batafsil ko'rish	7	1	2026-06-16 17:32:01.442269	\N	\N	\N	0	t
723	CONTRACT_CREATE	Shartnoma yaratish	Yangi shartnoma qo'shish	7	1	2026-06-16 17:32:01.442269	\N	\N	\N	0	t
724	CONTRACT_UPDATE	Shartnoma tahrirlash	Shartnomani tahrirlash	7	1	2026-06-16 17:32:01.442269	\N	\N	\N	0	t
725	CONTRACT_DELETE	Shartnoma o'chirish	Shartnomani o'chirish	7	1	2026-06-16 17:32:01.442269	\N	\N	\N	0	t
731	PURCHASE_SERVICE_VIEW	Xarid xizmatlari	Ro'yxat	7	1	2026-06-24 11:25:25.034628	\N	\N	\N	0	t
732	PURCHASE_SERVICE_VIEW_DETAIL	Xarid xizmati detail	Batafsil	7	1	2026-06-24 11:25:25.034628	\N	\N	\N	0	t
733	PURCHASE_SERVICE_CREATE	Xarid xizmati yaratish	Yangi	7	1	2026-06-24 11:25:25.034628	\N	\N	\N	0	t
734	PURCHASE_SERVICE_UPDATE	Xarid xizmati tahrirlash	Tahrirlash	7	1	2026-06-24 11:25:25.034628	\N	\N	\N	0	t
735	PURCHASE_SERVICE_DELETE	Xarid xizmati o'chirish	O'chirish	7	1	2026-06-24 11:25:25.034628	\N	\N	\N	0	t
801	SALE_DOC_VIEW	Sotuv hujjatlari	Ro'yxat	8	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
802	SALE_DOC_VIEW_DETAIL	Sotuv hujjati detail	Batafsil	8	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
803	SALE_DOC_CREATE	Sotuv hujjati yaratish	Yangi	8	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
804	SALE_DOC_UPDATE	Sotuv hujjati tahrirlash	Tahrirlash	8	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
805	SALE_DOC_DELETE	Sotuv hujjati o'chirish	O'chirish	8	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
806	CONFIRM_SALE	Sotuvni tasdiqlash	Tasdiqlash	8	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
807	CANCEL_SALE	Sotuvni bekor qilish	Bekor qilish	8	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
811	SALE_DOC_TABLE_VIEW	Sotuv satrlari	Ro'yxat	8	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
812	SALE_DOC_TABLE_VIEW_DETAIL	Sotuv satri detail	Batafsil	8	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
813	SALE_DOC_TABLE_CREATE	Sotuv satri yaratish	Yangi	8	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
814	SALE_DOC_TABLE_UPDATE	Sotuv satri tahrirlash	Tahrirlash	8	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
815	SALE_DOC_TABLE_DELETE	Sotuv satri o'chirish	O'chirish	8	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
901	CHART_ACCOUNT_VIEW	Hisoblar rejasi	Ro'yxat	9	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
902	CHART_ACCOUNT_VIEW_DETAIL	Hisoblar rejasi detail	Batafsil	9	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
903	CHART_ACCOUNT_CREATE	Hisob yaratish	Yangi	9	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
904	CHART_ACCOUNT_UPDATE	Hisob tahrirlash	Tahrirlash	9	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
905	CHART_ACCOUNT_DELETE	Hisob o'chirish	O'chirish	9	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
911	ACC_REG_ENTRY_VIEW	Buxg. yozuvlar	Ro'yxat	9	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
912	ACC_REG_ENTRY_VIEW_DETAIL	Buxg. yozuv detail	Batafsil	9	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
913	ACC_REG_ENTRY_CREATE	Buxg. yozuv yaratish	Yangi	9	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
914	ACC_REG_ENTRY_UPDATE	Buxg. yozuv tahrirlash	Tahrirlash	9	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
915	ACC_REG_ENTRY_DELETE	Buxg. yozuv o'chirish	O'chirish	9	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
1001	COUNTERPARTY_REG_BALANCE_VIEW	Kontragent qoldiqlari	Ro'yxat	10	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
1002	COUNTERPARTY_REG_BALANCE_VIEW_DETAIL	Kontragent qoldig'i detail	Batafsil	10	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
1003	COUNTERPARTY_REG_BALANCE_CREATE	Kontragent qoldig'i yaratish	Yangi	10	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
1004	COUNTERPARTY_REG_BALANCE_UPDATE	Kontragent qoldig'i tahrirlash	Tahrirlash	10	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
1005	COUNTERPARTY_REG_BALANCE_DELETE	Kontragent qoldig'i o'chirish	O'chirish	10	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
1011	INVENTORY_REG_BALANCE_VIEW	Inventar qoldiqlari	Ro'yxat	10	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
1012	INVENTORY_REG_BALANCE_VIEW_DETAIL	Inventar qoldig'i detail	Batafsil	10	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
1013	INVENTORY_REG_BALANCE_CREATE	Inventar qoldig'i yaratish	Yangi	10	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
1014	INVENTORY_REG_BALANCE_UPDATE	Inventar qoldig'i tahrirlash	Tahrirlash	10	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
1015	INVENTORY_REG_BALANCE_DELETE	Inventar qoldig'i o'chirish	O'chirish	10	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
1021	MONEY_REG_BALANCE_VIEW	Pul qoldiqlari	Ro'yxat	10	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
1022	MONEY_REG_BALANCE_VIEW_DETAIL	Pul qoldig'i detail	Batafsil	10	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
1023	MONEY_REG_BALANCE_CREATE	Pul qoldig'i yaratish	Yangi	10	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
1024	MONEY_REG_BALANCE_UPDATE	Pul qoldig'i tahrirlash	Tahrirlash	10	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
1025	MONEY_REG_BALANCE_DELETE	Pul qoldig'i o'chirish	O'chirish	10	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
1026	PRICING_CONDITION_VIEW	Narxlash qoidasi	Ro'yxat	12	1	2026-06-27 16:26:12.016132	\N	\N	\N	0	t
1027	PRICING_CONDITION_VIEW_DETAIL	Narxlash qoidasi detail	Batafsil	12	1	2026-06-27 16:26:12.016132	\N	\N	\N	0	t
1028	PRICING_CONDITION_CREATE	Narxlash qoidasi yaratish	Yangi	12	1	2026-06-27 16:26:12.016132	\N	\N	\N	0	t
1029	PRICING_CONDITION_UPDATE	Narxlash qoidasi tahrirlash	Tahrirlash	12	1	2026-06-27 16:26:12.016132	\N	\N	\N	0	t
1030	PRICING_CONDITION_DELETE	Narxlash qoidasi o'chirish	O'chirish	12	1	2026-06-27 16:26:12.016132	\N	\N	\N	0	t
1031	SALE_CONDITION_VIEW	Sotuv qoidasi	Ro'yxat	13	1	2026-06-27 16:26:12.016132	\N	\N	\N	0	t
1032	SALE_CONDITION_VIEW_DETAIL	Sotuv qoidasi detail	Batafsil	13	1	2026-06-27 16:26:12.016132	\N	\N	\N	0	t
1033	SALE_CONDITION_CREATE	Sotuv qoidasi yaratish	Yangi	13	1	2026-06-27 16:26:12.016132	\N	\N	\N	0	t
1034	SALE_CONDITION_UPDATE	Sotuv qoidasi tahrirlash	Tahrirlash	13	1	2026-06-27 16:26:12.016132	\N	\N	\N	0	t
1035	SALE_CONDITION_DELETE	Sotuv qoidasi o'chirish	O'chirish	13	1	2026-06-27 16:26:12.016132	\N	\N	\N	0	t
1036	POSTING_RULE_VIEW	Postings qoidasi	Ro'yxat	14	1	2026-06-27 16:26:12.016132	\N	\N	\N	0	t
1037	POSTING_RULE_VIEW_DETAIL	Postings qoidasi detail	Batafsil	14	1	2026-06-27 16:26:12.016132	\N	\N	\N	0	t
1101	MANUAL_VIEW	Ma'lumotnoma	Ma'lumotnoma ma'lumotlarini ko'rish	11	1	2026-06-08 11:46:35.397683	\N	\N	\N	0	t
521	BANK_STATEMENT_PARSE	Bank statement import	Bank Excel statement faylini JSON qilib parse qilish	5	1	2026-06-24 13:12:30.3099	\N	\N	\N	0	t
531	BANK_VIEW	Banklar	Banklar ro'yxati	5	1	2026-06-24 19:08:36.998172	\N	\N	\N	0	t
532	BANK_VIEW_DETAIL	Bank detail	Bankni batafsil ko'rish	5	1	2026-06-24 19:08:36.998172	\N	\N	\N	0	t
533	BANK_CREATE	Bank yaratish	Yangi bank qo'shish	5	1	2026-06-24 19:08:36.998172	\N	\N	\N	0	t
534	BANK_UPDATE	Bank tahrirlash	Bankni tahrirlash	5	1	2026-06-24 19:08:36.998172	\N	\N	\N	0	t
535	BANK_DELETE	Bank o'chirish	Bankni o'chirish	5	1	2026-06-24 19:08:36.998172	\N	\N	\N	0	t
\.


--
-- Data for Name: sys_module_sub_group; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.sys_module_sub_group (id, code, short_name, full_name, created_date) FROM stdin;
1	SYS	Tizim	Tizim sozlamalari	2026-06-08 11:44:36.687616
2	ORG	Tashkilot	Tashkilot boshqaruvi	2026-06-08 11:44:36.687616
3	COUNTERPARTY	Kontragent	Kontragentlar	2026-06-08 11:44:36.687616
4	INVENTORY	Inventar	Tovar va ombor	2026-06-08 11:44:36.687616
5	BANK	Bank	Bank operatsiyalari	2026-06-08 11:44:36.687616
6	CASH	Kassa	Kassa operatsiyalari	2026-06-08 11:44:36.687616
7	PURCHASE	Xarid	Xarid hujjatlari	2026-06-08 11:44:36.687616
8	SALE	Sotuv	Sotuv hujjatlari	2026-06-08 11:44:36.687616
9	ACCOUNTING	Buxgalteriya	Buxgalteriya registrlari	2026-06-08 11:44:36.687616
10	REGISTER	Registrlar	Qoldiq registrlari	2026-06-08 11:44:36.687616
11	MANUAL	Ma'lumotnoma	Ma'lumotnoma ma'lumotlari	2026-06-08 11:44:36.687616
12	PRICING_CONDITION	Narxlash qoidasi	Narxlash qoidasi	2026-06-27 16:21:38.259971
13	SALE_CONDITION	Sotuv qoidasi	Sotuv qoidasi	2026-06-27 16:21:38.259971
14	POSTING_RULE	Postings qoidasi	Postings qoidasi	2026-06-27 16:21:38.259971
15	WAREHOUSE_TRANSFER	Ombor ko'chirish	Omborlar o'rtasida ko'chirish	2026-07-03 00:00:00
16	INVENTORY_ADJUSTMENT	Inventar tuzatish	Inventar tuzatish hujjatlari	2026-07-03 00:00:00
17	INVENTORY_COUNT	Inventar sanog'i	Inventar sanog'i hujjatlari	2026-07-03 00:00:00
\.


--
-- Data for Name: sys_password_reset_token; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.sys_password_reset_token (id, user_id, token_hash, expires_at, used_at, created_date) FROM stdin;
\.


--
-- Data for Name: sys_refresh_token; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.sys_refresh_token (id, user_id, token_hash, jwt_id, device_name, ip_address, user_agent, expires_at, revoked_at, replaced_by_token_hash, created_date) FROM stdin;
\.


--
-- Data for Name: sys_role; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.sys_role (id, short_name, full_name, state_id, created_date, organization_id, has_global_access, code, description, is_system, is_owner_role, sort_order) FROM stdin;
3	Kamilahon	Kamilichka	1	2026-06-11 18:23:06.708355	\N	f	\N	\N	f	f	0
2	AsadbekBux	AsadbekBux	1	2026-06-11 18:18:43.452469	\N	f	\N	\N	f	f	0
4	super_admin	Super Admin	1	2026-06-19 11:15:16.697264	\N	t	\N	\N	f	f	0
5	adminka	Adminka	1	2026-06-19 14:37:38.409257	\N	f	\N	\N	f	f	0
1	admin	Tashkilot admini	1	2026-06-05 16:52:31.75491	\N	f	\N	\N	f	f	0
\.


--
-- Data for Name: sys_role_module; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.sys_role_module (role_id, module_id, created_date) FROM stdin;
4	731	2026-06-24 11:25:36.529027+05
4	732	2026-06-24 11:25:36.529027+05
4	733	2026-06-24 11:25:36.529027+05
4	734	2026-06-24 11:25:36.529027+05
4	735	2026-06-24 11:25:36.529027+05
4	521	2026-06-24 13:12:30.3099+05
4	531	2026-06-24 19:08:36.998172+05
4	1140	2026-07-06 10:19:05.561817+05
4	1141	2026-07-06 10:19:05.561817+05
2	501	2026-06-11 18:28:19.088923+05
2	502	2026-06-11 18:28:19.089236+05
2	503	2026-06-11 18:28:19.089445+05
2	504	2026-06-11 18:28:19.089633+05
2	505	2026-06-11 18:28:19.089849+05
2	511	2026-06-11 18:28:19.08993+05
2	512	2026-06-11 18:28:19.09001+05
2	513	2026-06-11 18:28:19.090187+05
2	514	2026-06-11 18:28:19.090383+05
2	515	2026-06-11 18:28:19.090564+05
2	901	2026-06-11 18:28:19.090791+05
2	902	2026-06-11 18:28:19.090894+05
2	903	2026-06-11 18:28:19.09097+05
2	904	2026-06-11 18:28:19.091042+05
2	905	2026-06-11 18:28:19.091261+05
2	911	2026-06-11 18:28:19.091484+05
2	912	2026-06-11 18:28:19.091672+05
2	913	2026-06-11 18:28:19.09185+05
2	914	2026-06-11 18:28:19.091931+05
2	915	2026-06-11 18:28:19.092008+05
4	101	2026-06-19 11:15:16.779889+05
4	102	2026-06-19 11:15:16.837503+05
4	103	2026-06-19 11:15:16.83832+05
4	104	2026-06-19 11:15:16.838704+05
4	105	2026-06-19 11:15:16.839056+05
4	111	2026-06-19 11:15:16.839274+05
4	112	2026-06-19 11:15:16.839804+05
4	113	2026-06-19 11:15:16.840081+05
4	114	2026-06-19 11:15:16.840331+05
4	115	2026-06-19 11:15:16.840601+05
4	201	2026-06-19 11:15:16.840994+05
4	202	2026-06-19 11:15:16.841245+05
4	203	2026-06-19 11:15:16.841521+05
4	204	2026-06-19 11:15:16.841946+05
4	205	2026-06-19 11:15:16.842199+05
4	211	2026-06-19 11:15:16.842577+05
4	212	2026-06-19 11:15:16.842748+05
4	213	2026-06-19 11:15:16.844025+05
4	214	2026-06-19 11:15:16.844584+05
4	215	2026-06-19 11:15:16.84496+05
4	221	2026-06-19 11:15:16.845319+05
4	222	2026-06-19 11:15:16.845523+05
4	223	2026-06-19 11:15:16.845747+05
4	224	2026-06-19 11:15:16.845999+05
4	225	2026-06-19 11:15:16.846393+05
4	231	2026-06-19 11:15:16.846576+05
4	232	2026-06-19 11:15:16.846899+05
4	233	2026-06-19 11:15:16.847166+05
4	234	2026-06-19 11:15:16.847429+05
4	235	2026-06-19 11:15:16.847703+05
4	301	2026-06-19 11:15:16.847953+05
4	302	2026-06-19 11:15:16.848157+05
4	303	2026-06-19 11:15:16.8484+05
4	304	2026-06-19 11:15:16.848634+05
4	305	2026-06-19 11:15:16.848823+05
4	311	2026-06-19 11:15:16.849187+05
4	312	2026-06-19 11:15:16.849422+05
4	313	2026-06-19 11:15:16.849673+05
4	314	2026-06-19 11:15:16.849998+05
4	315	2026-06-19 11:15:16.850233+05
4	321	2026-06-19 11:15:16.850437+05
4	322	2026-06-19 11:15:16.850758+05
4	323	2026-06-19 11:15:16.850989+05
4	324	2026-06-19 11:15:16.851277+05
4	325	2026-06-19 11:15:16.851536+05
4	401	2026-06-19 11:15:16.851878+05
4	402	2026-06-19 11:15:16.852144+05
4	403	2026-06-19 11:15:16.852326+05
4	404	2026-06-19 11:15:16.852512+05
4	405	2026-06-19 11:15:16.852904+05
4	411	2026-06-19 11:15:16.853248+05
4	412	2026-06-19 11:15:16.853451+05
4	413	2026-06-19 11:15:16.853642+05
4	414	2026-06-19 11:15:16.853833+05
4	415	2026-06-19 11:15:16.854201+05
4	421	2026-06-19 11:15:16.854541+05
4	422	2026-06-19 11:15:16.854738+05
4	423	2026-06-19 11:15:16.855205+05
4	424	2026-06-19 11:15:16.855559+05
4	425	2026-06-19 11:15:16.855815+05
4	431	2026-06-19 11:15:16.856014+05
4	432	2026-06-19 11:15:16.856236+05
4	433	2026-06-19 11:15:16.856563+05
4	434	2026-06-19 11:15:16.856832+05
4	435	2026-06-19 11:15:16.85702+05
4	501	2026-06-19 11:15:16.857239+05
4	502	2026-06-19 11:15:16.85742+05
4	503	2026-06-19 11:15:16.857618+05
4	504	2026-06-19 11:15:16.857859+05
4	505	2026-06-19 11:15:16.858139+05
4	511	2026-06-19 11:15:16.858355+05
4	512	2026-06-19 11:15:16.858564+05
4	513	2026-06-19 11:15:16.858934+05
4	514	2026-06-19 11:15:16.859203+05
4	515	2026-06-19 11:15:16.859448+05
4	601	2026-06-19 11:15:16.860844+05
4	602	2026-06-19 11:15:16.861212+05
4	603	2026-06-19 11:15:16.861523+05
4	604	2026-06-19 11:15:16.861748+05
4	605	2026-06-19 11:15:16.861943+05
4	611	2026-06-19 11:15:16.862161+05
4	612	2026-06-19 11:15:16.862403+05
4	613	2026-06-19 11:15:16.862646+05
4	614	2026-06-19 11:15:16.862938+05
4	615	2026-06-19 11:15:16.863211+05
4	701	2026-06-19 11:15:16.863488+05
4	702	2026-06-19 11:15:16.863706+05
4	703	2026-06-19 11:15:16.863925+05
4	704	2026-06-19 11:15:16.864153+05
4	705	2026-06-19 11:15:16.86441+05
4	711	2026-06-19 11:15:16.864617+05
4	712	2026-06-19 11:15:16.864816+05
4	713	2026-06-19 11:15:16.865002+05
4	714	2026-06-19 11:15:16.86527+05
4	715	2026-06-19 11:15:16.865543+05
4	721	2026-06-19 11:15:16.86574+05
4	722	2026-06-19 11:15:16.86596+05
4	723	2026-06-19 11:15:16.866204+05
4	724	2026-06-19 11:15:16.866455+05
4	725	2026-06-19 11:15:16.866674+05
4	801	2026-06-19 11:15:16.866898+05
4	802	2026-06-19 11:15:16.86711+05
4	803	2026-06-19 11:15:16.867348+05
4	804	2026-06-19 11:15:16.867576+05
4	805	2026-06-19 11:15:16.867807+05
4	811	2026-06-19 11:15:16.868038+05
4	812	2026-06-19 11:15:16.868269+05
4	813	2026-06-19 11:15:16.868542+05
4	814	2026-06-19 11:15:16.868768+05
4	815	2026-06-19 11:15:16.869019+05
4	901	2026-06-19 11:15:16.869284+05
4	902	2026-06-19 11:15:16.869466+05
4	903	2026-06-19 11:15:16.869645+05
4	904	2026-06-19 11:15:16.869832+05
4	905	2026-06-19 11:15:16.870018+05
4	911	2026-06-19 11:15:16.870199+05
4	912	2026-06-19 11:15:16.8704+05
4	913	2026-06-19 11:15:16.870604+05
4	914	2026-06-19 11:15:16.87092+05
4	915	2026-06-19 11:15:16.871168+05
4	1001	2026-06-19 11:15:16.871369+05
4	1002	2026-06-19 11:15:16.87158+05
4	1003	2026-06-19 11:15:16.8718+05
4	1004	2026-06-19 11:15:16.871996+05
4	1005	2026-06-19 11:15:16.872233+05
4	1011	2026-06-19 11:15:16.872429+05
4	1012	2026-06-19 11:15:16.872621+05
4	1013	2026-06-19 11:15:16.872808+05
4	1014	2026-06-19 11:15:16.873024+05
4	1015	2026-06-19 11:15:16.873223+05
4	1021	2026-06-19 11:15:16.873417+05
4	1022	2026-06-19 11:15:16.873714+05
4	1023	2026-06-19 11:15:16.873935+05
4	1024	2026-06-19 11:15:16.874136+05
4	1025	2026-06-19 11:15:16.874349+05
4	1101	2026-06-19 11:15:16.874541+05
5	101	2026-06-19 15:50:13.649015+05
5	102	2026-06-19 15:50:13.649125+05
5	103	2026-06-19 15:50:13.649233+05
5	104	2026-06-19 15:50:13.649339+05
5	105	2026-06-19 15:50:13.649442+05
5	111	2026-06-19 15:50:13.649549+05
5	112	2026-06-19 15:50:13.649657+05
5	113	2026-06-19 15:50:13.649784+05
5	114	2026-06-19 15:50:13.649898+05
5	115	2026-06-19 15:50:13.65001+05
5	201	2026-06-19 15:50:13.646636+05
5	202	2026-06-19 15:50:13.646751+05
5	203	2026-06-19 15:50:13.646886+05
5	204	2026-06-19 15:50:13.647002+05
5	205	2026-06-19 15:50:13.647118+05
5	211	2026-06-19 15:50:13.647235+05
5	212	2026-06-19 15:50:13.647348+05
5	213	2026-06-19 15:50:13.647458+05
5	214	2026-06-19 15:50:13.647573+05
5	215	2026-06-19 15:50:13.647688+05
5	221	2026-06-19 15:50:13.647823+05
5	222	2026-06-19 15:50:13.64794+05
5	223	2026-06-19 15:50:13.648052+05
5	224	2026-06-19 15:50:13.648167+05
5	225	2026-06-19 15:50:13.648284+05
5	231	2026-06-19 15:50:13.6484+05
5	232	2026-06-19 15:50:13.648514+05
5	233	2026-06-19 15:50:13.648629+05
5	234	2026-06-19 15:50:13.648749+05
5	235	2026-06-19 15:50:13.648904+05
5	301	2026-06-19 15:50:13.641417+05
5	302	2026-06-19 15:50:13.641547+05
5	303	2026-06-19 15:50:13.641678+05
5	304	2026-06-19 15:50:13.641826+05
5	305	2026-06-19 15:50:13.641961+05
5	311	2026-06-19 15:50:13.642084+05
5	312	2026-06-19 15:50:13.642213+05
5	313	2026-06-19 15:50:13.642346+05
5	314	2026-06-19 15:50:13.642486+05
5	315	2026-06-19 15:50:13.64262+05
5	321	2026-06-19 15:50:13.642743+05
5	322	2026-06-19 15:50:13.642919+05
5	323	2026-06-19 15:50:13.643043+05
5	324	2026-06-19 15:50:13.643165+05
5	325	2026-06-19 15:50:13.64329+05
5	401	2026-06-19 15:50:13.650127+05
5	402	2026-06-19 15:50:13.650243+05
5	403	2026-06-19 15:50:13.650355+05
5	404	2026-06-19 15:50:13.650468+05
5	405	2026-06-19 15:50:13.650573+05
5	411	2026-06-19 15:50:13.650686+05
5	412	2026-06-19 15:50:13.650817+05
5	413	2026-06-19 15:50:13.650932+05
5	414	2026-06-19 15:50:13.651047+05
5	415	2026-06-19 15:50:13.651156+05
5	421	2026-06-19 15:50:13.65126+05
5	422	2026-06-19 15:50:13.651373+05
5	423	2026-06-19 15:50:13.651483+05
5	424	2026-06-19 15:50:13.651592+05
5	425	2026-06-19 15:50:13.651714+05
5	431	2026-06-19 15:50:13.651861+05
5	432	2026-06-19 15:50:13.651974+05
5	433	2026-06-19 15:50:13.652086+05
5	434	2026-06-19 15:50:13.65219+05
5	435	2026-06-19 15:50:13.652293+05
5	501	2026-06-19 15:50:13.609661+05
5	502	2026-06-19 15:50:13.635668+05
5	503	2026-06-19 15:50:13.636065+05
5	504	2026-06-19 15:50:13.636228+05
5	505	2026-06-19 15:50:13.636385+05
5	511	2026-06-19 15:50:13.636537+05
5	512	2026-06-19 15:50:13.636679+05
5	513	2026-06-19 15:50:13.638032+05
5	514	2026-06-19 15:50:13.638225+05
5	515	2026-06-19 15:50:13.638382+05
5	601	2026-06-19 15:50:13.63999+05
5	602	2026-06-19 15:50:13.640126+05
5	603	2026-06-19 15:50:13.640255+05
5	604	2026-06-19 15:50:13.640438+05
5	605	2026-06-19 15:50:13.640575+05
5	611	2026-06-19 15:50:13.640732+05
5	612	2026-06-19 15:50:13.640898+05
5	613	2026-06-19 15:50:13.64103+05
5	614	2026-06-19 15:50:13.64116+05
5	615	2026-06-19 15:50:13.641285+05
5	701	2026-06-19 15:50:13.652392+05
5	702	2026-06-19 15:50:13.652494+05
5	703	2026-06-19 15:50:13.652595+05
5	704	2026-06-19 15:50:13.652699+05
5	705	2026-06-19 15:50:13.652816+05
5	711	2026-06-19 15:50:13.652921+05
5	712	2026-06-19 15:50:13.65302+05
5	713	2026-06-19 15:50:13.653122+05
5	714	2026-06-19 15:50:13.653228+05
5	715	2026-06-19 15:50:13.653332+05
5	721	2026-06-19 15:50:13.653444+05
5	722	2026-06-19 15:50:13.653548+05
5	723	2026-06-19 15:50:13.653646+05
5	724	2026-06-19 15:50:13.653747+05
5	725	2026-06-19 15:50:13.653868+05
5	801	2026-06-19 15:50:13.645466+05
5	802	2026-06-19 15:50:13.645578+05
5	803	2026-06-19 15:50:13.645695+05
5	804	2026-06-19 15:50:13.645828+05
5	805	2026-06-19 15:50:13.645947+05
5	811	2026-06-19 15:50:13.646064+05
5	812	2026-06-19 15:50:13.646179+05
5	813	2026-06-19 15:50:13.646287+05
5	814	2026-06-19 15:50:13.646401+05
5	815	2026-06-19 15:50:13.646518+05
5	901	2026-06-19 15:50:13.638515+05
5	902	2026-06-19 15:50:13.638646+05
5	903	2026-06-19 15:50:13.638804+05
5	904	2026-06-19 15:50:13.638951+05
5	905	2026-06-19 15:50:13.639102+05
5	911	2026-06-19 15:50:13.639277+05
5	912	2026-06-19 15:50:13.639406+05
5	913	2026-06-19 15:50:13.639537+05
5	914	2026-06-19 15:50:13.639675+05
5	915	2026-06-19 15:50:13.639842+05
5	1001	2026-06-19 15:50:13.643538+05
5	1002	2026-06-19 15:50:13.643668+05
5	1003	2026-06-19 15:50:13.643827+05
5	1004	2026-06-19 15:50:13.643955+05
5	1005	2026-06-19 15:50:13.644077+05
5	1011	2026-06-19 15:50:13.6442+05
5	1012	2026-06-19 15:50:13.644339+05
5	1013	2026-06-19 15:50:13.644466+05
5	1014	2026-06-19 15:50:13.644588+05
5	1015	2026-06-19 15:50:13.644716+05
5	1021	2026-06-19 15:50:13.644867+05
5	1022	2026-06-19 15:50:13.644994+05
5	1023	2026-06-19 15:50:13.645111+05
5	1024	2026-06-19 15:50:13.645228+05
5	1025	2026-06-19 15:50:13.645346+05
5	1101	2026-06-19 15:50:13.643415+05
4	441	2026-06-19 11:15:16.845523+05
4	442	2026-06-20 10:18:09.728522+05
4	532	2026-06-24 19:08:36.998172+05
4	533	2026-06-24 19:08:36.998172+05
4	534	2026-06-24 19:08:36.998172+05
4	535	2026-06-24 19:08:36.998172+05
1	101	2026-07-03 16:32:59.712492+05
1	102	2026-07-03 16:32:59.712615+05
1	103	2026-07-03 16:32:59.712668+05
1	104	2026-07-03 16:32:59.712693+05
1	105	2026-07-03 16:32:59.712729+05
1	111	2026-07-03 16:32:59.712752+05
1	112	2026-07-03 16:32:59.712773+05
1	113	2026-07-03 16:32:59.712796+05
1	114	2026-07-03 16:32:59.712819+05
1	115	2026-07-03 16:32:59.712849+05
1	201	2026-07-03 16:32:59.712871+05
1	202	2026-07-03 16:32:59.712891+05
1	203	2026-07-03 16:32:59.71291+05
1	204	2026-07-03 16:32:59.71293+05
1	205	2026-07-03 16:32:59.71295+05
1	211	2026-07-03 16:32:59.71297+05
1	212	2026-07-03 16:32:59.71299+05
1	213	2026-07-03 16:32:59.71302+05
1	214	2026-07-03 16:32:59.713043+05
1	215	2026-07-03 16:32:59.713064+05
1	221	2026-07-03 16:32:59.713084+05
1	222	2026-07-03 16:32:59.713111+05
1	223	2026-07-03 16:32:59.713132+05
1	224	2026-07-03 16:32:59.713152+05
1	225	2026-07-03 16:32:59.713172+05
1	231	2026-07-03 16:32:59.713192+05
1	232	2026-07-03 16:32:59.713211+05
1	233	2026-07-03 16:32:59.713231+05
1	234	2026-07-03 16:32:59.713258+05
1	235	2026-07-03 16:32:59.713279+05
1	301	2026-07-03 16:32:59.713303+05
1	302	2026-07-03 16:32:59.713323+05
1	303	2026-07-03 16:32:59.713343+05
1	304	2026-07-03 16:32:59.713365+05
1	305	2026-07-03 16:32:59.713385+05
1	311	2026-07-03 16:32:59.713411+05
1	312	2026-07-03 16:32:59.713432+05
1	313	2026-07-03 16:32:59.713455+05
1	314	2026-07-03 16:32:59.713487+05
1	315	2026-07-03 16:32:59.713508+05
1	321	2026-07-03 16:32:59.713529+05
1	322	2026-07-03 16:32:59.713549+05
1	323	2026-07-03 16:32:59.713569+05
1	324	2026-07-03 16:32:59.713595+05
1	325	2026-07-03 16:32:59.713616+05
1	401	2026-07-03 16:32:59.713661+05
1	402	2026-07-03 16:32:59.713686+05
1	403	2026-07-03 16:32:59.713707+05
1	404	2026-07-03 16:32:59.713728+05
1	405	2026-07-03 16:32:59.713748+05
1	411	2026-07-03 16:32:59.713777+05
1	412	2026-07-03 16:32:59.713798+05
1	413	2026-07-03 16:32:59.713818+05
1	414	2026-07-03 16:32:59.713838+05
1	415	2026-07-03 16:32:59.713858+05
1	421	2026-07-03 16:32:59.713877+05
1	422	2026-07-03 16:32:59.713897+05
1	423	2026-07-03 16:32:59.713917+05
1	424	2026-07-03 16:32:59.713944+05
1	425	2026-07-03 16:32:59.713965+05
1	431	2026-07-03 16:32:59.713985+05
1	432	2026-07-03 16:32:59.714004+05
1	433	2026-07-03 16:32:59.714024+05
1	434	2026-07-03 16:32:59.714044+05
1	435	2026-07-03 16:32:59.714064+05
1	441	2026-07-03 16:32:59.714091+05
1	442	2026-07-03 16:32:59.714113+05
1	501	2026-07-03 16:32:59.714133+05
1	502	2026-07-03 16:32:59.714153+05
1	503	2026-07-03 16:32:59.714173+05
1	504	2026-07-03 16:32:59.714193+05
1	505	2026-07-03 16:32:59.714213+05
1	511	2026-07-03 16:32:59.714242+05
1	512	2026-07-03 16:32:59.714263+05
1	513	2026-07-03 16:32:59.714283+05
1	514	2026-07-03 16:32:59.714303+05
1	515	2026-07-03 16:32:59.714322+05
1	516	2026-07-03 16:32:59.714342+05
1	517	2026-07-03 16:32:59.714362+05
1	521	2026-07-03 16:32:59.714388+05
1	531	2026-07-03 16:32:59.714409+05
1	532	2026-07-03 16:32:59.714429+05
1	533	2026-07-03 16:32:59.71445+05
1	534	2026-07-03 16:32:59.71447+05
1	535	2026-07-03 16:32:59.71449+05
1	601	2026-07-03 16:32:59.71451+05
1	602	2026-07-03 16:32:59.714531+05
1	603	2026-07-03 16:32:59.714557+05
1	604	2026-07-03 16:32:59.714579+05
1	605	2026-07-03 16:32:59.714604+05
1	611	2026-07-03 16:32:59.714679+05
1	612	2026-07-03 16:32:59.714706+05
1	613	2026-07-03 16:32:59.714727+05
1	614	2026-07-03 16:32:59.714759+05
1	615	2026-07-03 16:32:59.71478+05
1	616	2026-07-03 16:32:59.714803+05
1	617	2026-07-03 16:32:59.714825+05
1	701	2026-07-03 16:32:59.714845+05
1	702	2026-07-03 16:32:59.714866+05
1	703	2026-07-03 16:32:59.714886+05
1	704	2026-07-03 16:32:59.714914+05
1	705	2026-07-03 16:32:59.714936+05
1	706	2026-07-03 16:32:59.714956+05
1	707	2026-07-03 16:32:59.714976+05
1	711	2026-07-03 16:32:59.714996+05
1	712	2026-07-03 16:32:59.715017+05
1	713	2026-07-03 16:32:59.715037+05
1	714	2026-07-03 16:32:59.715058+05
1	715	2026-07-03 16:32:59.715085+05
1	721	2026-07-03 16:32:59.715106+05
1	722	2026-07-03 16:32:59.715126+05
1	723	2026-07-03 16:32:59.715147+05
1	724	2026-07-03 16:32:59.715167+05
1	725	2026-07-03 16:32:59.715187+05
1	731	2026-07-03 16:32:59.715208+05
1	732	2026-07-03 16:32:59.715235+05
1	733	2026-07-03 16:32:59.715256+05
1	734	2026-07-03 16:32:59.715277+05
1	735	2026-07-03 16:32:59.715298+05
1	801	2026-07-03 16:32:59.715318+05
1	802	2026-07-03 16:32:59.715339+05
1	803	2026-07-03 16:32:59.71536+05
1	804	2026-07-03 16:32:59.715381+05
1	805	2026-07-03 16:32:59.715408+05
1	806	2026-07-03 16:32:59.715429+05
1	807	2026-07-03 16:32:59.715449+05
1	811	2026-07-03 16:32:59.71547+05
1	812	2026-07-03 16:32:59.715491+05
1	813	2026-07-03 16:32:59.715511+05
1	814	2026-07-03 16:32:59.715541+05
1	815	2026-07-03 16:32:59.715563+05
1	901	2026-07-03 16:32:59.715584+05
1	902	2026-07-03 16:32:59.715605+05
1	903	2026-07-03 16:32:59.715626+05
1	904	2026-07-03 16:32:59.715665+05
1	905	2026-07-03 16:32:59.715687+05
1	911	2026-07-03 16:32:59.715716+05
1	912	2026-07-03 16:32:59.715738+05
1	913	2026-07-03 16:32:59.715759+05
1	914	2026-07-03 16:32:59.71578+05
1	915	2026-07-03 16:32:59.715801+05
1	1001	2026-07-03 16:32:59.715822+05
1	1002	2026-07-03 16:32:59.715842+05
1	1003	2026-07-03 16:32:59.715863+05
1	1004	2026-07-03 16:32:59.715891+05
1	1005	2026-07-03 16:32:59.715912+05
1	1011	2026-07-03 16:32:59.715933+05
1	1012	2026-07-03 16:32:59.715954+05
1	1013	2026-07-03 16:32:59.715975+05
1	1014	2026-07-03 16:32:59.715996+05
1	1015	2026-07-03 16:32:59.716017+05
1	1021	2026-07-03 16:32:59.716046+05
1	1022	2026-07-03 16:32:59.716067+05
1	1023	2026-07-03 16:32:59.716088+05
1	1024	2026-07-03 16:32:59.716109+05
1	1025	2026-07-03 16:32:59.71613+05
1	1026	2026-07-03 16:32:59.716151+05
1	1027	2026-07-03 16:32:59.716172+05
1	1028	2026-07-03 16:32:59.716193+05
1	1029	2026-07-03 16:32:59.716221+05
1	1030	2026-07-03 16:32:59.716242+05
1	1031	2026-07-03 16:32:59.716263+05
1	1032	2026-07-03 16:32:59.716284+05
1	1033	2026-07-03 16:32:59.716305+05
1	1034	2026-07-03 16:32:59.716325+05
1	1035	2026-07-03 16:32:59.716346+05
1	1036	2026-07-03 16:32:59.716373+05
1	1037	2026-07-03 16:32:59.716395+05
1	1101	2026-07-03 16:32:59.716416+05
1	1102	2026-07-03 16:32:59.716437+05
1	1103	2026-07-03 16:32:59.716458+05
1	1104	2026-07-03 16:32:59.716479+05
1	1105	2026-07-03 16:32:59.7165+05
1	1106	2026-07-03 16:32:59.71652+05
1	1107	2026-07-03 16:32:59.716548+05
1	1108	2026-07-03 16:32:59.71657+05
1	1109	2026-07-03 16:32:59.716591+05
1	1110	2026-07-03 16:32:59.716612+05
1	1111	2026-07-03 16:32:59.716647+05
1	1112	2026-07-03 16:32:59.716673+05
1	1113	2026-07-03 16:32:59.716696+05
1	1114	2026-07-03 16:32:59.716726+05
1	1115	2026-07-03 16:32:59.716748+05
1	1116	2026-07-03 16:32:59.716769+05
1	1117	2026-07-03 16:32:59.71679+05
1	1118	2026-07-03 16:32:59.716811+05
1	1119	2026-07-03 16:32:59.716832+05
1	1120	2026-07-03 16:32:59.716853+05
1	1121	2026-07-03 16:32:59.716874+05
1	1122	2026-07-03 16:32:59.716903+05
\.


--
-- Data for Name: sys_user; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.sys_user (id, user_name, password_hash, password_salt, phone_number, email, first_name, last_name, role_id, last_access_time, state_id, created_date, language_id, organization_id, email_verified, email_verified_at, last_login_ip, is_platform_admin, timezone) FROM stdin;
1	Mahmudjon Amonovvv	MCxmiJJhujeVeuPjiXeOGT4vQfTm7pzjwMlsGNJvsI0=	c5c3hf87yc3x3nlUMdO4/A==	+998 99 890-08-58	amonovmahmudjon13@gmail.com	Mahmudjon	Amonov	1	2026-06-08 15:33:42.741805	1	2026-06-05 16:54:41.07541	1	2	f	\N	\N	f	\N
2	Mahmudjon Amonov	rCAmiQlaSG2JolJHYYEUChB89dqAQ63Sp9aY9WaKg6k=	biHGSYPE6sCc8w/QJ9XOkw==	+998 99 890-08-58	amonovmahmudjon13@gmail.com	Mahmudjon	Amonov	2	2026-06-18 18:21:32.558673	1	2026-06-08 14:53:31.216633	1	2	f	\N	\N	f	\N
7	test_test	EshZvFfH7DsVO6/HnUI5E/Dj5hWCnLZeMnQO6/hpSdc=	v/F/y1YzCvy+hQMqc/bBIA==	+998 99 455-55-55	testuser@mail.uz	Test	User	1	\N	1	2026-06-12 11:13:27.396974	\N	\N	f	\N	\N	f	\N
9	sarvarbek_hafizov	gyObckIfXCcVCGH8wh8uLP4ORmbf7MwdDzCN8xqowBo=	LcOJh4Zh+ShOwvk6T3nprA==	+998907304643	sarvarbekhafizov@gmail.com	Sarvarbek	Hafizov	1	\N	1	2026-06-13 11:34:36.217917	\N	\N	f	\N	\N	f	\N
4	sardorbek_hafizov	r6Aa04o5vGGuUwNkYGXWN1hZh5duIfR4sRtO6ElN5zY=	LcOJh4Zh+ShOwvk6T3nprA==	+998 99 890-08-58	sardorbekHafizov@gmail.com	Sardorbek	Hafizov	1	2026-07-06 10:56:46.644215	1	2026-06-11 15:51:32.389581	\N	\N	f	\N	\N	f	\N
13	sabinahon	gyObckIfXCcVCGH8wh8uLP4ORmbf7MwdDzCN8xqowBo=	LcOJh4Zh+ShOwvk6T3nprA==	+998 90 730-46-43	sabinahon13@gmail.com	Sabinahon	Soyibovna	5	2026-06-19 15:29:58.574366	1	2026-06-19 14:46:08.293094	\N	\N	f	\N	\N	f	\N
12	sardorbek	gyObckIfXCcVCGH8wh8uLP4ORmbf7MwdDzCN8xqowBo=	LcOJh4Zh+ShOwvk6T3nprA==	+998970642323	sardorbek@gmail.com	Sardorbek	Hafizov	4	2026-07-03 11:05:06.743822	1	2026-06-19 11:32:44.418707	\N	\N	f	\N	\N	f	\N
10	test_user	5vlocv7E6747NqLfIElKiAno7h6LhABv4dBO5AHC04A=	TWOahharOvv3Vu1yQWai1g==	+998901234567	test@gmail.com	Test	User	1	\N	1	2026-06-13 11:48:39.245593	\N	\N	f	\N	\N	f	\N
8	Amonov	ezzess8CQ3L1QREQqeg7iscDs08ijrWjvDPJvZCBt6A=	z9SXjmluYTvffYsAfBMd4A==	+998 99 890-08-58	amonovmahmudjon13@gmail.com	Mahmudjon	Amonov	3	\N	1	2026-06-12 11:44:30.993097	\N	\N	f	\N	\N	f	\N
3	Mahmudjon	9TWnCcdvvf7Yv1m73xpzxFwuK5PY65UpQgV2iDwhNWk=	Y5N8oSECkMiuvFVzhARVSA==	+998 99 890-08-58	amonovmahmudjon13@gmail.com	Mahmudjon	Amonov	1	2026-06-11 12:30:44.960698	1	2026-06-08 16:55:31.159039	1	2	f	\N	\N	f	\N
15	superadmin	NbX6dK2HaOHl+c5Vs2a5S75qYy2WXCG6W4HJ7UCqZnI=	6LSuU2I2rB8GUMB51ly5rA==	+998 99 899-89-00	sardorbekHafizov@gmail.com	Super	Admin	4	2026-06-19 16:07:02.441095	1	2026-06-19 15:39:50.103447	\N	\N	f	\N	\N	f	\N
14	monika	UFS/jbDQnrJRQqwfiKWK1aLzrKaFlCK5QCeCyTeZHFo=	SjRx9lE0hF0DQU2nf4kOVQ==	+998 90 236-90-01	matluba13@gmail.com	Matluba	Farmonova	5	2026-06-19 15:32:58.193463	1	2026-06-19 15:31:51.265437	\N	\N	f	\N	\N	f	\N
\.


--
-- Data for Name: sys_user_organization; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.sys_user_organization (user_id, organization_id, role_id, is_default, state_id, created_date, is_owner, joined_at, invited_by_user_id, last_access_at, blocked_at) FROM stdin;
7	2	1	t	1	2026-06-12 11:13:27.42408	f	2026-06-30 12:21:03.848157	\N	\N	\N
4	8	\N	f	1	2026-06-13 11:02:26.046266	f	2026-06-30 12:21:03.848157	\N	\N	\N
2	2	1	f	1	2026-06-13 11:23:17.800494	f	2026-06-30 12:21:03.848157	\N	\N	\N
2	8	2	f	1	2026-06-13 11:23:17.800494	f	2026-06-30 12:21:03.848157	\N	\N	\N
12	2	4	t	1	2026-06-19 12:03:57.914819	f	2026-06-30 12:21:03.848157	\N	\N	\N
12	8	4	f	1	2026-06-19 12:03:57.917009	f	2026-06-30 12:21:03.848157	\N	\N	\N
15	8	\N	t	1	2026-06-19 15:39:50.109461	f	2026-06-30 12:21:03.848157	\N	\N	\N
\.


--
-- Name: acc_account_resolve_rule_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.acc_account_resolve_rule_id_seq', 71, true);


--
-- Name: acc_account_type_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.acc_account_type_id_seq', 1, false);


--
-- Name: acc_accounting_period_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.acc_accounting_period_id_seq', 1, false);


--
-- Name: acc_accounting_policy_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.acc_accounting_policy_id_seq', 1, false);


--
-- Name: acc_chart_account_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.acc_chart_account_id_seq', 1072, true);


--
-- Name: acc_chart_account_subkonto_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.acc_chart_account_subkonto_id_seq', 30, true);


--
-- Name: acc_payment_purpose_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.acc_payment_purpose_id_seq', 24, true);


--
-- Name: acc_posting_alias_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.acc_posting_alias_id_seq', 34, true);


--
-- Name: acc_posting_batch_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.acc_posting_batch_id_seq', 26, true);


--
-- Name: acc_posting_rule_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.acc_posting_rule_id_seq', 1, false);


--
-- Name: acc_posting_rule_line_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.acc_posting_rule_line_id_seq', 21, true);


--
-- Name: acc_reg_entry_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.acc_reg_entry_id_seq', 288, true);


--
-- Name: acc_reg_entry_subkonto_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.acc_reg_entry_subkonto_id_seq', 859, true);


--
-- Name: acc_subkonto_type_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.acc_subkonto_type_id_seq', 1, false);


--
-- Name: bank_operation_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.bank_operation_id_seq', 41, true);


--
-- Name: bank_operation_line_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.bank_operation_line_id_seq', 7, true);


--
-- Name: cash_box_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.cash_box_id_seq', 4, true);


--
-- Name: cash_operation_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.cash_operation_id_seq', 18, true);


--
-- Name: cmn_bank_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.cmn_bank_id_seq', 6, true);


--
-- Name: cmn_contract_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.cmn_contract_id_seq', 14, true);


--
-- Name: cmn_contract_type_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.cmn_contract_type_id_seq', 1, false);


--
-- Name: cmn_costing_method_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.cmn_costing_method_id_seq', 1, false);


--
-- Name: cmn_counterparty_type_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.cmn_counterparty_type_id_seq', 3, true);


--
-- Name: cmn_currency_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.cmn_currency_id_seq', 4, true);


--
-- Name: cmn_district_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.cmn_district_id_seq', 209, true);


--
-- Name: cmn_document_sequence_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.cmn_document_sequence_id_seq', 1, false);


--
-- Name: cmn_document_status_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.cmn_document_status_id_seq', 3, true);


--
-- Name: cmn_document_type_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.cmn_document_type_id_seq', 6, true);


--
-- Name: cmn_language_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.cmn_language_id_seq', 1, false);


--
-- Name: cmn_operation_type_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.cmn_operation_type_id_seq', 5, true);


--
-- Name: cmn_payment_type_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.cmn_payment_type_id_seq', 4, true);


--
-- Name: cmn_price_rounding_method_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.cmn_price_rounding_method_id_seq', 1, false);


--
-- Name: cmn_pricing_condition_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.cmn_pricing_condition_id_seq', 8, true);


--
-- Name: cmn_product_price_type_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.cmn_product_price_type_id_seq', 1, false);


--
-- Name: cmn_product_table_status_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.cmn_product_table_status_id_seq', 1, false);


--
-- Name: cmn_region_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.cmn_region_id_seq', 14, true);


--
-- Name: cmn_tax_type_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.cmn_tax_type_id_seq', 2, true);


--
-- Name: cmn_translation_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.cmn_translation_id_seq', 57, true);


--
-- Name: cmn_unit_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.cmn_unit_id_seq', 5, true);


--
-- Name: cmn_vat_rate_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.cmn_vat_rate_id_seq', 4, true);


--
-- Name: contract_number_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.contract_number_seq', 100000014, true);


--
-- Name: counterparty_bank_account_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.counterparty_bank_account_id_seq', 1, true);


--
-- Name: counterparty_card_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.counterparty_card_id_seq', 21, true);


--
-- Name: counterparty_contact_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.counterparty_contact_id_seq', 6, true);


--
-- Name: counterparty_reg_balance_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.counterparty_reg_balance_id_seq', 12, true);


--
-- Name: doc_number_bank_operation_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.doc_number_bank_operation_seq', 100000040, true);


--
-- Name: doc_number_cash_operation_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.doc_number_cash_operation_seq', 100000016, true);


--
-- Name: doc_number_purchase_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.doc_number_purchase_seq', 100000085, true);


--
-- Name: doc_number_sale_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.doc_number_sale_seq', 100000078, true);


--
-- Name: inv_product_group_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.inv_product_group_id_seq', 14, true);


--
-- Name: inv_product_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.inv_product_id_seq', 26, true);


--
-- Name: inv_product_price_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.inv_product_price_id_seq', 2, true);


--
-- Name: inv_product_table_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.inv_product_table_id_seq', 477, true);


--
-- Name: inv_reg_balance_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.inv_reg_balance_id_seq', 216, true);


--
-- Name: inv_warehouse_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.inv_warehouse_id_seq', 8, true);


--
-- Name: money_reg_balance_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.money_reg_balance_id_seq', 18, true);


--
-- Name: org_bank_account_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.org_bank_account_id_seq', 12, true);


--
-- Name: org_branch_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.org_branch_id_seq', 6, true);


--
-- Name: org_claim_request_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.org_claim_request_id_seq', 1, false);


--
-- Name: org_defaults_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.org_defaults_id_seq', 1, false);


--
-- Name: org_department_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.org_department_id_seq', 2, true);


--
-- Name: org_organization_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.org_organization_id_seq', 19, true);


--
-- Name: org_position_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.org_position_id_seq', 4, true);


--
-- Name: org_setup_state_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.org_setup_state_id_seq', 2, true);


--
-- Name: org_tax_settings_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.org_tax_settings_id_seq', 6, true);


--
-- Name: org_user_invitation_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.org_user_invitation_id_seq', 1, false);


--
-- Name: platform_tenant_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.platform_tenant_id_seq', 8, true);


--
-- Name: pur_doc_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.pur_doc_id_seq', 103, true);


--
-- Name: pur_doc_product_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.pur_doc_product_id_seq', 33, true);


--
-- Name: pur_doc_table_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.pur_doc_table_id_seq', 37, true);


--
-- Name: sale_condition_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.sale_condition_id_seq', 4, true);


--
-- Name: sale_doc_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.sale_doc_id_seq', 80, true);


--
-- Name: sale_doc_product_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.sale_doc_product_id_seq', 28, true);


--
-- Name: sale_doc_table_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.sale_doc_table_id_seq', 103, true);


--
-- Name: sys_audit_log_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.sys_audit_log_id_seq', 251, true);


--
-- Name: sys_email_verification_token_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.sys_email_verification_token_id_seq', 1, false);


--
-- Name: sys_module_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.sys_module_id_seq', 1141, true);


--
-- Name: sys_module_sub_group_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.sys_module_sub_group_id_seq', 17, true);


--
-- Name: sys_password_reset_token_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.sys_password_reset_token_id_seq', 1, false);


--
-- Name: sys_refresh_token_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.sys_refresh_token_id_seq', 1, false);


--
-- Name: sys_role_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.sys_role_id_seq', 5, true);


--
-- Name: sys_user_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.sys_user_id_seq', 21, true);


--
-- Name: acc_account_resolve_rule acc_account_resolve_rule_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_account_resolve_rule
    ADD CONSTRAINT acc_account_resolve_rule_pkey PRIMARY KEY (id);


--
-- Name: acc_account_type acc_account_type_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_account_type
    ADD CONSTRAINT acc_account_type_pkey PRIMARY KEY (id);


--
-- Name: acc_accounting_period acc_accounting_period_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_accounting_period
    ADD CONSTRAINT acc_accounting_period_pkey PRIMARY KEY (id);


--
-- Name: acc_accounting_period acc_accounting_period_unique_period; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_accounting_period
    ADD CONSTRAINT acc_accounting_period_unique_period UNIQUE (organization_id, year, month);


--
-- Name: acc_accounting_policy acc_accounting_policy_code_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_accounting_policy
    ADD CONSTRAINT acc_accounting_policy_code_key UNIQUE (code);


--
-- Name: acc_accounting_policy acc_accounting_policy_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_accounting_policy
    ADD CONSTRAINT acc_accounting_policy_pkey PRIMARY KEY (id);


--
-- Name: acc_chart_account acc_chart_account_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_chart_account
    ADD CONSTRAINT acc_chart_account_pkey PRIMARY KEY (id);


--
-- Name: acc_chart_account_subkonto acc_chart_account_subkonto_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_chart_account_subkonto
    ADD CONSTRAINT acc_chart_account_subkonto_pkey PRIMARY KEY (id);


--
-- Name: acc_payment_purpose acc_payment_purpose_code_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_payment_purpose
    ADD CONSTRAINT acc_payment_purpose_code_key UNIQUE (code);


--
-- Name: acc_payment_purpose acc_payment_purpose_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_payment_purpose
    ADD CONSTRAINT acc_payment_purpose_pkey PRIMARY KEY (id);


--
-- Name: acc_payment_purpose_translation acc_payment_purpose_translation_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_payment_purpose_translation
    ADD CONSTRAINT acc_payment_purpose_translation_pkey PRIMARY KEY (payment_purpose_id, language_id);


--
-- Name: acc_posting_alias acc_posting_alias_code_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_posting_alias
    ADD CONSTRAINT acc_posting_alias_code_key UNIQUE (code);


--
-- Name: acc_posting_alias acc_posting_alias_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_posting_alias
    ADD CONSTRAINT acc_posting_alias_pkey PRIMARY KEY (id);


--
-- Name: acc_posting_alias_translation acc_posting_alias_translation_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_posting_alias_translation
    ADD CONSTRAINT acc_posting_alias_translation_pkey PRIMARY KEY (posting_alias_id, language_id);


--
-- Name: acc_posting_batch acc_posting_batch_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_posting_batch
    ADD CONSTRAINT acc_posting_batch_pkey PRIMARY KEY (id);


--
-- Name: acc_posting_rule acc_posting_rule_code_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_posting_rule
    ADD CONSTRAINT acc_posting_rule_code_key UNIQUE (code);


--
-- Name: acc_posting_rule_line acc_posting_rule_line_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_posting_rule_line
    ADD CONSTRAINT acc_posting_rule_line_pkey PRIMARY KEY (id);


--
-- Name: acc_posting_rule acc_posting_rule_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_posting_rule
    ADD CONSTRAINT acc_posting_rule_pkey PRIMARY KEY (id);


--
-- Name: acc_reg_entry acc_reg_entry_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_reg_entry
    ADD CONSTRAINT acc_reg_entry_pkey PRIMARY KEY (id);


--
-- Name: acc_reg_entry_subkonto acc_reg_entry_subkonto_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_reg_entry_subkonto
    ADD CONSTRAINT acc_reg_entry_subkonto_pkey PRIMARY KEY (id);


--
-- Name: acc_subkonto_type acc_subkonto_type_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_subkonto_type
    ADD CONSTRAINT acc_subkonto_type_pkey PRIMARY KEY (id);


--
-- Name: bank_operation_line bank_operation_line_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.bank_operation_line
    ADD CONSTRAINT bank_operation_line_pkey PRIMARY KEY (id);


--
-- Name: bank_operation bank_operation_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.bank_operation
    ADD CONSTRAINT bank_operation_pkey PRIMARY KEY (id);


--
-- Name: cash_box cash_box_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cash_box
    ADD CONSTRAINT cash_box_pkey PRIMARY KEY (id);


--
-- Name: cash_operation cash_operation_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cash_operation
    ADD CONSTRAINT cash_operation_pkey PRIMARY KEY (id);


--
-- Name: cmn_bank cmn_bank_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_bank
    ADD CONSTRAINT cmn_bank_pkey PRIMARY KEY (id);


--
-- Name: cmn_contract cmn_contract_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_contract
    ADD CONSTRAINT cmn_contract_pkey PRIMARY KEY (id);


--
-- Name: cmn_contract_type cmn_contract_type_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_contract_type
    ADD CONSTRAINT cmn_contract_type_pkey PRIMARY KEY (id);


--
-- Name: cmn_costing_method cmn_costing_method_code_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_costing_method
    ADD CONSTRAINT cmn_costing_method_code_key UNIQUE (code);


--
-- Name: cmn_costing_method cmn_costing_method_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_costing_method
    ADD CONSTRAINT cmn_costing_method_pkey PRIMARY KEY (id);


--
-- Name: cmn_counterparty_type cmn_counterparty_type_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_counterparty_type
    ADD CONSTRAINT cmn_counterparty_type_pkey PRIMARY KEY (id);


--
-- Name: cmn_currency cmn_currency_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_currency
    ADD CONSTRAINT cmn_currency_pkey PRIMARY KEY (id);


--
-- Name: cmn_district cmn_district_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_district
    ADD CONSTRAINT cmn_district_pkey PRIMARY KEY (id);


--
-- Name: cmn_document_sequence cmn_document_sequence_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_document_sequence
    ADD CONSTRAINT cmn_document_sequence_pkey PRIMARY KEY (id);


--
-- Name: cmn_document_status cmn_document_status_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_document_status
    ADD CONSTRAINT cmn_document_status_pkey PRIMARY KEY (id);


--
-- Name: cmn_document_type cmn_document_type_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_document_type
    ADD CONSTRAINT cmn_document_type_pkey PRIMARY KEY (id);


--
-- Name: cmn_language cmn_language_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_language
    ADD CONSTRAINT cmn_language_pkey PRIMARY KEY (id);


--
-- Name: cmn_operation_type cmn_operation_type_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_operation_type
    ADD CONSTRAINT cmn_operation_type_pkey PRIMARY KEY (id);


--
-- Name: cmn_payment_type cmn_payment_type_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_payment_type
    ADD CONSTRAINT cmn_payment_type_pkey PRIMARY KEY (id);


--
-- Name: cmn_price_rounding_method cmn_price_rounding_method_code_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_price_rounding_method
    ADD CONSTRAINT cmn_price_rounding_method_code_key UNIQUE (code);


--
-- Name: cmn_price_rounding_method cmn_price_rounding_method_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_price_rounding_method
    ADD CONSTRAINT cmn_price_rounding_method_pkey PRIMARY KEY (id);


--
-- Name: cmn_pricing_condition cmn_pricing_condition_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_pricing_condition
    ADD CONSTRAINT cmn_pricing_condition_pkey PRIMARY KEY (id);


--
-- Name: cmn_pricing_method cmn_pricing_method_code_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_pricing_method
    ADD CONSTRAINT cmn_pricing_method_code_key UNIQUE (code);


--
-- Name: cmn_pricing_method cmn_pricing_method_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_pricing_method
    ADD CONSTRAINT cmn_pricing_method_pkey PRIMARY KEY (id);


--
-- Name: cmn_product_price_type cmn_product_price_type_code_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_product_price_type
    ADD CONSTRAINT cmn_product_price_type_code_key UNIQUE (code);


--
-- Name: cmn_product_price_type cmn_product_price_type_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_product_price_type
    ADD CONSTRAINT cmn_product_price_type_pkey PRIMARY KEY (id);


--
-- Name: cmn_product_table_status cmn_product_table_status_code_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_product_table_status
    ADD CONSTRAINT cmn_product_table_status_code_key UNIQUE (code);


--
-- Name: cmn_product_table_status cmn_product_table_status_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_product_table_status
    ADD CONSTRAINT cmn_product_table_status_pkey PRIMARY KEY (id);


--
-- Name: cmn_product_type cmn_product_type_code_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_product_type
    ADD CONSTRAINT cmn_product_type_code_key UNIQUE (code);


--
-- Name: cmn_product_type cmn_product_type_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_product_type
    ADD CONSTRAINT cmn_product_type_pkey PRIMARY KEY (id);


--
-- Name: cmn_product_type_translation cmn_product_type_translation_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_product_type_translation
    ADD CONSTRAINT cmn_product_type_translation_pkey PRIMARY KEY (product_type_id, language_id);


--
-- Name: cmn_region cmn_region_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_region
    ADD CONSTRAINT cmn_region_pkey PRIMARY KEY (id);


--
-- Name: cmn_state cmn_state_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_state
    ADD CONSTRAINT cmn_state_pkey PRIMARY KEY (id);


--
-- Name: cmn_tax_type cmn_tax_type_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_tax_type
    ADD CONSTRAINT cmn_tax_type_pkey PRIMARY KEY (id);


--
-- Name: cmn_translation cmn_translation_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_translation
    ADD CONSTRAINT cmn_translation_pkey PRIMARY KEY (id);


--
-- Name: cmn_unit cmn_unit_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_unit
    ADD CONSTRAINT cmn_unit_pkey PRIMARY KEY (id);


--
-- Name: cmn_vat_rate cmn_vat_rate_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_vat_rate
    ADD CONSTRAINT cmn_vat_rate_pkey PRIMARY KEY (id);


--
-- Name: counterparty_account_payment_purpose_hint counterparty_account_payment_purpose_hint_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.counterparty_account_payment_purpose_hint
    ADD CONSTRAINT counterparty_account_payment_purpose_hint_pkey PRIMARY KEY (counterparty_bank_account_id, payment_purpose_id);


--
-- Name: counterparty_bank_account counterparty_bank_account_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.counterparty_bank_account
    ADD CONSTRAINT counterparty_bank_account_pkey PRIMARY KEY (id);


--
-- Name: counterparty_card counterparty_card_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.counterparty_card
    ADD CONSTRAINT counterparty_card_pkey PRIMARY KEY (id);


--
-- Name: counterparty_contact counterparty_contact_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.counterparty_contact
    ADD CONSTRAINT counterparty_contact_pkey PRIMARY KEY (id);


--
-- Name: counterparty_reg_balance counterparty_reg_balance_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.counterparty_reg_balance
    ADD CONSTRAINT counterparty_reg_balance_pkey PRIMARY KEY (id);


--
-- Name: inv_inventory_adjustment_doc inv_inventory_adjustment_doc_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_inventory_adjustment_doc
    ADD CONSTRAINT inv_inventory_adjustment_doc_pkey PRIMARY KEY (id);


--
-- Name: inv_inventory_adjustment_doc_table inv_inventory_adjustment_doc_table_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_inventory_adjustment_doc_table
    ADD CONSTRAINT inv_inventory_adjustment_doc_table_pkey PRIMARY KEY (id);


--
-- Name: inv_inventory_adjustment_line inv_inventory_adjustment_line_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_inventory_adjustment_line
    ADD CONSTRAINT inv_inventory_adjustment_line_pkey PRIMARY KEY (id);


--
-- Name: inv_inventory_count_doc inv_inventory_count_doc_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_inventory_count_doc
    ADD CONSTRAINT inv_inventory_count_doc_pkey PRIMARY KEY (id);


--
-- Name: inv_inventory_count_doc_table inv_inventory_count_doc_table_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_inventory_count_doc_table
    ADD CONSTRAINT inv_inventory_count_doc_table_pkey PRIMARY KEY (id);


--
-- Name: inv_inventory_count_line inv_inventory_count_line_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_inventory_count_line
    ADD CONSTRAINT inv_inventory_count_line_pkey PRIMARY KEY (id);


--
-- Name: inv_product_group inv_product_group_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_product_group
    ADD CONSTRAINT inv_product_group_pkey PRIMARY KEY (id);


--
-- Name: inv_product inv_product_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_product
    ADD CONSTRAINT inv_product_pkey PRIMARY KEY (id);


--
-- Name: inv_product_price inv_product_price_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_product_price
    ADD CONSTRAINT inv_product_price_pkey PRIMARY KEY (id);


--
-- Name: inv_product_table inv_product_table_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_product_table
    ADD CONSTRAINT inv_product_table_pkey PRIMARY KEY (id);


--
-- Name: inv_reg_balance inv_reg_balance_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_reg_balance
    ADD CONSTRAINT inv_reg_balance_pkey PRIMARY KEY (id);


--
-- Name: inv_transfer_doc inv_transfer_doc_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_transfer_doc
    ADD CONSTRAINT inv_transfer_doc_pkey PRIMARY KEY (id);


--
-- Name: inv_transfer_doc_table inv_transfer_doc_table_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_transfer_doc_table
    ADD CONSTRAINT inv_transfer_doc_table_pkey PRIMARY KEY (id);


--
-- Name: inv_transfer_line inv_transfer_line_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_transfer_line
    ADD CONSTRAINT inv_transfer_line_pkey PRIMARY KEY (id);


--
-- Name: inv_warehouse inv_warehouse_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_warehouse
    ADD CONSTRAINT inv_warehouse_pkey PRIMARY KEY (id);


--
-- Name: money_reg_balance money_reg_balance_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.money_reg_balance
    ADD CONSTRAINT money_reg_balance_pkey PRIMARY KEY (id);


--
-- Name: org_bank_account org_bank_account_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_bank_account
    ADD CONSTRAINT org_bank_account_pkey PRIMARY KEY (id);


--
-- Name: org_branch org_branch_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_branch
    ADD CONSTRAINT org_branch_pkey PRIMARY KEY (id);


--
-- Name: org_claim_request org_claim_request_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_claim_request
    ADD CONSTRAINT org_claim_request_pkey PRIMARY KEY (id);


--
-- Name: org_defaults org_defaults_organization_id_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_defaults
    ADD CONSTRAINT org_defaults_organization_id_key UNIQUE (organization_id);


--
-- Name: org_defaults org_defaults_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_defaults
    ADD CONSTRAINT org_defaults_pkey PRIMARY KEY (id);


--
-- Name: org_department org_department_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_department
    ADD CONSTRAINT org_department_pkey PRIMARY KEY (id);


--
-- Name: org_organization_config org_organization_config_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_organization_config
    ADD CONSTRAINT org_organization_config_pkey PRIMARY KEY (organization_id);


--
-- Name: org_organization org_organization_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_organization
    ADD CONSTRAINT org_organization_pkey PRIMARY KEY (id);


--
-- Name: org_position org_position_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_position
    ADD CONSTRAINT org_position_pkey PRIMARY KEY (id);


--
-- Name: org_setup_state org_setup_state_organization_id_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_setup_state
    ADD CONSTRAINT org_setup_state_organization_id_key UNIQUE (organization_id);


--
-- Name: org_setup_state org_setup_state_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_setup_state
    ADD CONSTRAINT org_setup_state_pkey PRIMARY KEY (id);


--
-- Name: org_tax_settings org_tax_settings_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_tax_settings
    ADD CONSTRAINT org_tax_settings_pkey PRIMARY KEY (id);


--
-- Name: org_user_invitation org_user_invitation_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_user_invitation
    ADD CONSTRAINT org_user_invitation_pkey PRIMARY KEY (id);


--
-- Name: org_user_invitation org_user_invitation_token_hash_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_user_invitation
    ADD CONSTRAINT org_user_invitation_token_hash_key UNIQUE (token_hash);


--
-- Name: platform_tenant platform_tenant_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.platform_tenant
    ADD CONSTRAINT platform_tenant_pkey PRIMARY KEY (id);


--
-- Name: platform_tenant platform_tenant_slug_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.platform_tenant
    ADD CONSTRAINT platform_tenant_slug_key UNIQUE (slug);


--
-- Name: pur_doc pur_doc_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.pur_doc
    ADD CONSTRAINT pur_doc_pkey PRIMARY KEY (id);


--
-- Name: pur_doc_product pur_doc_product_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.pur_doc_product
    ADD CONSTRAINT pur_doc_product_pkey PRIMARY KEY (id);


--
-- Name: pur_doc_table pur_doc_table_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.pur_doc_table
    ADD CONSTRAINT pur_doc_table_pkey PRIMARY KEY (id);


--
-- Name: sale_condition sale_condition_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sale_condition
    ADD CONSTRAINT sale_condition_pkey PRIMARY KEY (id);


--
-- Name: sale_doc sale_doc_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sale_doc
    ADD CONSTRAINT sale_doc_pkey PRIMARY KEY (id);


--
-- Name: sale_doc_product sale_doc_product_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sale_doc_product
    ADD CONSTRAINT sale_doc_product_pkey PRIMARY KEY (id);


--
-- Name: sale_doc_table sale_doc_table_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sale_doc_table
    ADD CONSTRAINT sale_doc_table_pkey PRIMARY KEY (id);


--
-- Name: sys_audit_log sys_audit_log_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sys_audit_log
    ADD CONSTRAINT sys_audit_log_pkey PRIMARY KEY (id);


--
-- Name: sys_email_verification_token sys_email_verification_token_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sys_email_verification_token
    ADD CONSTRAINT sys_email_verification_token_pkey PRIMARY KEY (id);


--
-- Name: sys_email_verification_token sys_email_verification_token_token_hash_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sys_email_verification_token
    ADD CONSTRAINT sys_email_verification_token_token_hash_key UNIQUE (token_hash);


--
-- Name: sys_module sys_module_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sys_module
    ADD CONSTRAINT sys_module_pkey PRIMARY KEY (id);


--
-- Name: sys_module_sub_group sys_module_sub_group_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sys_module_sub_group
    ADD CONSTRAINT sys_module_sub_group_pkey PRIMARY KEY (id);


--
-- Name: sys_password_reset_token sys_password_reset_token_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sys_password_reset_token
    ADD CONSTRAINT sys_password_reset_token_pkey PRIMARY KEY (id);


--
-- Name: sys_password_reset_token sys_password_reset_token_token_hash_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sys_password_reset_token
    ADD CONSTRAINT sys_password_reset_token_token_hash_key UNIQUE (token_hash);


--
-- Name: sys_refresh_token sys_refresh_token_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sys_refresh_token
    ADD CONSTRAINT sys_refresh_token_pkey PRIMARY KEY (id);


--
-- Name: sys_refresh_token sys_refresh_token_token_hash_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sys_refresh_token
    ADD CONSTRAINT sys_refresh_token_token_hash_key UNIQUE (token_hash);


--
-- Name: sys_role_module sys_role_module_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sys_role_module
    ADD CONSTRAINT sys_role_module_pkey PRIMARY KEY (role_id, module_id);


--
-- Name: sys_role sys_role_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sys_role
    ADD CONSTRAINT sys_role_pkey PRIMARY KEY (id);


--
-- Name: sys_user_organization sys_user_organization_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sys_user_organization
    ADD CONSTRAINT sys_user_organization_pkey PRIMARY KEY (user_id, organization_id);


--
-- Name: sys_user sys_user_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sys_user
    ADD CONSTRAINT sys_user_pkey PRIMARY KEY (id);


--
-- Name: acc_chart_account uq_acc_chart_account_code; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_chart_account
    ADD CONSTRAINT uq_acc_chart_account_code UNIQUE (code);


--
-- Name: bank_operation_line uq_bank_operation_line_order; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.bank_operation_line
    ADD CONSTRAINT uq_bank_operation_line_order UNIQUE (bank_operation_id, order_number);


--
-- Name: idx_acc_account_type_code; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX idx_acc_account_type_code ON public.acc_account_type USING btree (code);


--
-- Name: idx_acc_account_type_state_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_acc_account_type_state_id ON public.acc_account_type USING btree (state_id);


--
-- Name: idx_acc_accounting_period_is_closed; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_acc_accounting_period_is_closed ON public.acc_accounting_period USING btree (is_closed);


--
-- Name: idx_acc_accounting_period_organization_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_acc_accounting_period_organization_id ON public.acc_accounting_period USING btree (organization_id);


--
-- Name: idx_acc_chart_account_account_type_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_acc_chart_account_account_type_id ON public.acc_chart_account USING btree (account_type_id);


--
-- Name: idx_acc_chart_account_parent_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_acc_chart_account_parent_id ON public.acc_chart_account USING btree (parent_id);


--
-- Name: idx_acc_chart_account_state_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_acc_chart_account_state_id ON public.acc_chart_account USING btree (state_id);


--
-- Name: idx_acc_chart_account_subkonto_account_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_acc_chart_account_subkonto_account_id ON public.acc_chart_account_subkonto USING btree (account_id);


--
-- Name: idx_acc_chart_account_subkonto_organization_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_acc_chart_account_subkonto_organization_id ON public.acc_chart_account_subkonto USING btree (organization_id);


--
-- Name: idx_acc_chart_account_subkonto_type_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_acc_chart_account_subkonto_type_id ON public.acc_chart_account_subkonto USING btree (subkonto_type_id);


--
-- Name: idx_acc_chart_account_subkonto_unique; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX idx_acc_chart_account_subkonto_unique ON public.acc_chart_account_subkonto USING btree (organization_id, account_id, subkonto_type_id);


--
-- Name: idx_acc_payment_purpose_alias_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_acc_payment_purpose_alias_id ON public.acc_payment_purpose USING btree (alias_id);


--
-- Name: idx_acc_payment_purpose_operation_type_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_acc_payment_purpose_operation_type_id ON public.acc_payment_purpose USING btree (operation_type_id);


--
-- Name: idx_acc_posting_batch_document; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_acc_posting_batch_document ON public.acc_posting_batch USING btree (document_type_id, document_id);


--
-- Name: idx_acc_posting_batch_organization_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_acc_posting_batch_organization_id ON public.acc_posting_batch USING btree (organization_id);


--
-- Name: idx_acc_posting_batch_status; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_acc_posting_batch_status ON public.acc_posting_batch USING btree (status);


--
-- Name: idx_acc_reg_entry_credit_account_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_acc_reg_entry_credit_account_id ON public.acc_reg_entry USING btree (credit_account_id);


--
-- Name: idx_acc_reg_entry_currency_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_acc_reg_entry_currency_id ON public.acc_reg_entry USING btree (currency_id);


--
-- Name: idx_acc_reg_entry_debit_account_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_acc_reg_entry_debit_account_id ON public.acc_reg_entry USING btree (debit_account_id);


--
-- Name: idx_acc_reg_entry_doc_date; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_acc_reg_entry_doc_date ON public.acc_reg_entry USING btree (doc_date);


--
-- Name: idx_acc_reg_entry_document; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_acc_reg_entry_document ON public.acc_reg_entry USING btree (document_type_id, document_id);


--
-- Name: idx_acc_reg_entry_journal_number; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_acc_reg_entry_journal_number ON public.acc_reg_entry USING btree (journal_number);


--
-- Name: idx_acc_reg_entry_operation_type_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_acc_reg_entry_operation_type_id ON public.acc_reg_entry USING btree (operation_type_id);


--
-- Name: idx_acc_reg_entry_org_credit_docdate_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_acc_reg_entry_org_credit_docdate_id ON public.acc_reg_entry USING btree (organization_id, credit_account_id, doc_date, id);


--
-- Name: idx_acc_reg_entry_org_debit_docdate_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_acc_reg_entry_org_debit_docdate_id ON public.acc_reg_entry USING btree (organization_id, debit_account_id, doc_date, id);


--
-- Name: idx_acc_reg_entry_org_docdate_credit_account; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_acc_reg_entry_org_docdate_credit_account ON public.acc_reg_entry USING btree (organization_id, doc_date, credit_account_id);


--
-- Name: idx_acc_reg_entry_org_docdate_debit_account; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_acc_reg_entry_org_docdate_debit_account ON public.acc_reg_entry USING btree (organization_id, doc_date, debit_account_id);


--
-- Name: idx_acc_reg_entry_organization_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_acc_reg_entry_organization_id ON public.acc_reg_entry USING btree (organization_id);


--
-- Name: idx_acc_reg_entry_posting_batch_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_acc_reg_entry_posting_batch_id ON public.acc_reg_entry USING btree (posting_batch_id);


--
-- Name: idx_acc_reg_entry_reversal_entry_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_acc_reg_entry_reversal_entry_id ON public.acc_reg_entry USING btree (reversal_entry_id);


--
-- Name: idx_acc_reg_entry_subkonto_entity; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_acc_reg_entry_subkonto_entity ON public.acc_reg_entry_subkonto USING btree (subkonto_type_id, entity_id);


--
-- Name: idx_acc_reg_entry_subkonto_entry_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_acc_reg_entry_subkonto_entry_id ON public.acc_reg_entry_subkonto USING btree (entry_id);


--
-- Name: idx_acc_reg_entry_subkonto_side; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_acc_reg_entry_subkonto_side ON public.acc_reg_entry_subkonto USING btree (side);


--
-- Name: idx_acc_reg_entry_subkonto_type_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_acc_reg_entry_subkonto_type_id ON public.acc_reg_entry_subkonto USING btree (subkonto_type_id);


--
-- Name: idx_acc_subkonto_type_code; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX idx_acc_subkonto_type_code ON public.acc_subkonto_type USING btree (code);


--
-- Name: idx_acc_subkonto_type_state_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_acc_subkonto_type_state_id ON public.acc_subkonto_type USING btree (state_id);


--
-- Name: idx_bank_operation_bank_account_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_bank_operation_bank_account_id ON public.bank_operation USING btree (bank_account_id);


--
-- Name: idx_bank_operation_cancelled_by_user_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_bank_operation_cancelled_by_user_id ON public.bank_operation USING btree (cancelled_by_user_id);


--
-- Name: idx_bank_operation_contract_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_bank_operation_contract_id ON public.bank_operation USING btree (contract_id);


--
-- Name: idx_bank_operation_counterparty_bank_account_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_bank_operation_counterparty_bank_account_id ON public.bank_operation USING btree (counterparty_bank_account_id);


--
-- Name: idx_bank_operation_counterparty_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_bank_operation_counterparty_id ON public.bank_operation USING btree (counterparty_id);


--
-- Name: idx_bank_operation_doc_date; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_bank_operation_doc_date ON public.bank_operation USING btree (doc_date);


--
-- Name: idx_bank_operation_line_counterparty_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_bank_operation_line_counterparty_id ON public.bank_operation_line USING btree (counterparty_id);


--
-- Name: idx_bank_operation_line_payment_purpose_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_bank_operation_line_payment_purpose_id ON public.bank_operation_line USING btree (payment_purpose_id);


--
-- Name: idx_bank_operation_operation_type_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_bank_operation_operation_type_id ON public.bank_operation USING btree (operation_type_id);


--
-- Name: idx_bank_operation_organization_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_bank_operation_organization_id ON public.bank_operation USING btree (organization_id);


--
-- Name: idx_bank_operation_payment_purpose_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_bank_operation_payment_purpose_id ON public.bank_operation USING btree (payment_purpose_id);


--
-- Name: idx_bank_operation_posted_by_user_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_bank_operation_posted_by_user_id ON public.bank_operation USING btree (posted_by_user_id);


--
-- Name: idx_bank_operation_state_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_bank_operation_state_id ON public.bank_operation USING btree (state_id);


--
-- Name: idx_bank_operation_status_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_bank_operation_status_id ON public.bank_operation USING btree (status_id);


--
-- Name: idx_cash_box_branch_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_cash_box_branch_id ON public.cash_box USING btree (branch_id);


--
-- Name: idx_cash_box_currency_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_cash_box_currency_id ON public.cash_box USING btree (currency_id);


--
-- Name: idx_cash_box_is_main; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_cash_box_is_main ON public.cash_box USING btree (is_main);


--
-- Name: idx_cash_box_org_code; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX idx_cash_box_org_code ON public.cash_box USING btree (organization_id, code);


--
-- Name: idx_cash_box_organization_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_cash_box_organization_id ON public.cash_box USING btree (organization_id);


--
-- Name: idx_cash_box_responsible_user_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_cash_box_responsible_user_id ON public.cash_box USING btree (responsible_user_id);


--
-- Name: idx_cash_box_state_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_cash_box_state_id ON public.cash_box USING btree (state_id);


--
-- Name: idx_cash_operation_cancelled_by_user_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_cash_operation_cancelled_by_user_id ON public.cash_operation USING btree (cancelled_by_user_id);


--
-- Name: idx_cash_operation_cash_box_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_cash_operation_cash_box_id ON public.cash_operation USING btree (cash_box_id);


--
-- Name: idx_cash_operation_counterparty_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_cash_operation_counterparty_id ON public.cash_operation USING btree (counterparty_id);


--
-- Name: idx_cash_operation_destination_cash_box_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_cash_operation_destination_cash_box_id ON public.cash_operation USING btree (destination_cash_box_id);


--
-- Name: idx_cash_operation_doc_date; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_cash_operation_doc_date ON public.cash_operation USING btree (doc_date);


--
-- Name: idx_cash_operation_operation_type_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_cash_operation_operation_type_id ON public.cash_operation USING btree (operation_type_id);


--
-- Name: idx_cash_operation_organization_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_cash_operation_organization_id ON public.cash_operation USING btree (organization_id);


--
-- Name: idx_cash_operation_payment_purpose_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_cash_operation_payment_purpose_id ON public.cash_operation USING btree (payment_purpose_id);


--
-- Name: idx_cash_operation_posted_by_user_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_cash_operation_posted_by_user_id ON public.cash_operation USING btree (posted_by_user_id);


--
-- Name: idx_cash_operation_state_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_cash_operation_state_id ON public.cash_operation USING btree (state_id);


--
-- Name: idx_cash_operation_status_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_cash_operation_status_id ON public.cash_operation USING btree (status_id);


--
-- Name: idx_cmn_bank_code; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX idx_cmn_bank_code ON public.cmn_bank USING btree (code);


--
-- Name: idx_cmn_bank_state_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_cmn_bank_state_id ON public.cmn_bank USING btree (state_id);


--
-- Name: idx_cmn_contract_contract_date; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_cmn_contract_contract_date ON public.cmn_contract USING btree (contract_date);


--
-- Name: idx_cmn_contract_counterparty_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_cmn_contract_counterparty_id ON public.cmn_contract USING btree (counterparty_id);


--
-- Name: idx_cmn_contract_number; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX idx_cmn_contract_number ON public.cmn_contract USING btree (organization_id, counterparty_id, contract_number);


--
-- Name: idx_cmn_contract_organization_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_cmn_contract_organization_id ON public.cmn_contract USING btree (organization_id);


--
-- Name: idx_cmn_contract_state_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_cmn_contract_state_id ON public.cmn_contract USING btree (state_id);


--
-- Name: idx_cmn_contract_type_code; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX idx_cmn_contract_type_code ON public.cmn_contract_type USING btree (code);


--
-- Name: idx_cmn_contract_type_state_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_cmn_contract_type_state_id ON public.cmn_contract_type USING btree (state_id);


--
-- Name: idx_cmn_counterparty_type_code; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX idx_cmn_counterparty_type_code ON public.cmn_counterparty_type USING btree (code);


--
-- Name: idx_cmn_currency_code; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX idx_cmn_currency_code ON public.cmn_currency USING btree (code);


--
-- Name: idx_cmn_district_region_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_cmn_district_region_id ON public.cmn_district USING btree (region_id);


--
-- Name: idx_cmn_document_sequence_document_type_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_cmn_document_sequence_document_type_id ON public.cmn_document_sequence USING btree (document_type_id);


--
-- Name: idx_cmn_document_sequence_state_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_cmn_document_sequence_state_id ON public.cmn_document_sequence USING btree (state_id);


--
-- Name: idx_cmn_document_status_code; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX idx_cmn_document_status_code ON public.cmn_document_status USING btree (code);


--
-- Name: idx_cmn_document_type_code; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX idx_cmn_document_type_code ON public.cmn_document_type USING btree (code);


--
-- Name: idx_cmn_document_type_state_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_cmn_document_type_state_id ON public.cmn_document_type USING btree (state_id);


--
-- Name: idx_cmn_language_code; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX idx_cmn_language_code ON public.cmn_language USING btree (code);


--
-- Name: idx_cmn_language_default; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX idx_cmn_language_default ON public.cmn_language USING btree (is_default) WHERE (is_default = true);


--
-- Name: idx_cmn_language_state_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_cmn_language_state_id ON public.cmn_language USING btree (state_id);


--
-- Name: idx_cmn_operation_type_code; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX idx_cmn_operation_type_code ON public.cmn_operation_type USING btree (code);


--
-- Name: idx_cmn_operation_type_state_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_cmn_operation_type_state_id ON public.cmn_operation_type USING btree (state_id);


--
-- Name: idx_cmn_payment_type_code; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX idx_cmn_payment_type_code ON public.cmn_payment_type USING btree (code);


--
-- Name: idx_cmn_pricing_condition_dates; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_cmn_pricing_condition_dates ON public.cmn_pricing_condition USING btree (start_date, end_date);


--
-- Name: idx_cmn_pricing_condition_organization_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_cmn_pricing_condition_organization_id ON public.cmn_pricing_condition USING btree (organization_id);


--
-- Name: idx_cmn_pricing_condition_pricing_method_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_cmn_pricing_condition_pricing_method_id ON public.cmn_pricing_condition USING btree (pricing_method_id);


--
-- Name: idx_cmn_pricing_condition_rounding_method_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_cmn_pricing_condition_rounding_method_id ON public.cmn_pricing_condition USING btree (rounding_method_id);


--
-- Name: idx_cmn_tax_type_code; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX idx_cmn_tax_type_code ON public.cmn_tax_type USING btree (code);


--
-- Name: idx_cmn_tax_type_state_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_cmn_tax_type_state_id ON public.cmn_tax_type USING btree (state_id);


--
-- Name: idx_cmn_translation_language_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_cmn_translation_language_id ON public.cmn_translation USING btree (language_id);


--
-- Name: idx_cmn_translation_lookup; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_cmn_translation_lookup ON public.cmn_translation USING btree (table_name, record_id, column_name);


--
-- Name: idx_cmn_translation_unique; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX idx_cmn_translation_unique ON public.cmn_translation USING btree (language_id, table_name, record_id, column_name);


--
-- Name: idx_cmn_unit_code; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX idx_cmn_unit_code ON public.cmn_unit USING btree (code);


--
-- Name: idx_cmn_vat_rate_code; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX idx_cmn_vat_rate_code ON public.cmn_vat_rate USING btree (code);


--
-- Name: idx_cmn_vat_rate_effective_dates; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_cmn_vat_rate_effective_dates ON public.cmn_vat_rate USING btree (effective_from, effective_to);


--
-- Name: idx_cmn_vat_rate_state_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_cmn_vat_rate_state_id ON public.cmn_vat_rate USING btree (state_id);


--
-- Name: idx_counterparty_bank_account_bank_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_counterparty_bank_account_bank_id ON public.counterparty_bank_account USING btree (bank_id);


--
-- Name: idx_counterparty_bank_account_counterparty_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_counterparty_bank_account_counterparty_id ON public.counterparty_bank_account USING btree (counterparty_id);


--
-- Name: idx_counterparty_bank_account_currency_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_counterparty_bank_account_currency_id ON public.counterparty_bank_account USING btree (currency_id);


--
-- Name: idx_counterparty_bank_account_organization_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_counterparty_bank_account_organization_id ON public.counterparty_bank_account USING btree (organization_id);


--
-- Name: idx_counterparty_bank_account_state_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_counterparty_bank_account_state_id ON public.counterparty_bank_account USING btree (state_id);


--
-- Name: idx_counterparty_card_code; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_counterparty_card_code ON public.counterparty_card USING btree (code);


--
-- Name: idx_counterparty_card_district_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_counterparty_card_district_id ON public.counterparty_card USING btree (district_id);


--
-- Name: idx_counterparty_card_external_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_counterparty_card_external_id ON public.counterparty_card USING btree (external_id);


--
-- Name: idx_counterparty_card_inn; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_counterparty_card_inn ON public.counterparty_card USING btree (inn);


--
-- Name: idx_counterparty_card_organization_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_counterparty_card_organization_id ON public.counterparty_card USING btree (organization_id);


--
-- Name: idx_counterparty_card_region_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_counterparty_card_region_id ON public.counterparty_card USING btree (region_id);


--
-- Name: idx_counterparty_card_short_name; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_counterparty_card_short_name ON public.counterparty_card USING btree (short_name);


--
-- Name: idx_counterparty_card_state_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_counterparty_card_state_id ON public.counterparty_card USING btree (state_id);


--
-- Name: idx_counterparty_card_type_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_counterparty_card_type_id ON public.counterparty_card USING btree (counterparty_type_id);


--
-- Name: idx_counterparty_contact_counterparty_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_counterparty_contact_counterparty_id ON public.counterparty_contact USING btree (counterparty_id);


--
-- Name: idx_counterparty_contact_organization_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_counterparty_contact_organization_id ON public.counterparty_contact USING btree (organization_id);


--
-- Name: idx_counterparty_contact_state_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_counterparty_contact_state_id ON public.counterparty_contact USING btree (state_id);


--
-- Name: idx_counterparty_reg_balance_counterparty_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_counterparty_reg_balance_counterparty_id ON public.counterparty_reg_balance USING btree (counterparty_id);


--
-- Name: idx_counterparty_reg_balance_currency_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_counterparty_reg_balance_currency_id ON public.counterparty_reg_balance USING btree (currency_id);


--
-- Name: idx_counterparty_reg_balance_doc_date; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_counterparty_reg_balance_doc_date ON public.counterparty_reg_balance USING btree (doc_date);


--
-- Name: idx_counterparty_reg_balance_document; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_counterparty_reg_balance_document ON public.counterparty_reg_balance USING btree (document_type_id, document_id);


--
-- Name: idx_counterparty_reg_balance_organization_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_counterparty_reg_balance_organization_id ON public.counterparty_reg_balance USING btree (organization_id);


--
-- Name: idx_counterparty_reg_balance_posting_batch_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_counterparty_reg_balance_posting_batch_id ON public.counterparty_reg_balance USING btree (posting_batch_id);


--
-- Name: idx_counterparty_reg_balance_reversal_entry_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_counterparty_reg_balance_reversal_entry_id ON public.counterparty_reg_balance USING btree (reversal_entry_id);


--
-- Name: idx_inv_inventory_adjustment_doc_adjustment_type; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_inventory_adjustment_doc_adjustment_type ON public.inv_inventory_adjustment_doc USING btree (adjustment_type);


--
-- Name: idx_inv_inventory_adjustment_doc_cancelled_by_user_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_inventory_adjustment_doc_cancelled_by_user_id ON public.inv_inventory_adjustment_doc USING btree (cancelled_by_user_id);


--
-- Name: idx_inv_inventory_adjustment_doc_doc_date; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_inventory_adjustment_doc_doc_date ON public.inv_inventory_adjustment_doc USING btree (doc_date);


--
-- Name: idx_inv_inventory_adjustment_doc_organization_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_inventory_adjustment_doc_organization_id ON public.inv_inventory_adjustment_doc USING btree (organization_id);


--
-- Name: idx_inv_inventory_adjustment_doc_posted_by_user_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_inventory_adjustment_doc_posted_by_user_id ON public.inv_inventory_adjustment_doc USING btree (posted_by_user_id);


--
-- Name: idx_inv_inventory_adjustment_doc_state_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_inventory_adjustment_doc_state_id ON public.inv_inventory_adjustment_doc USING btree (state_id);


--
-- Name: idx_inv_inventory_adjustment_doc_status_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_inventory_adjustment_doc_status_id ON public.inv_inventory_adjustment_doc USING btree (status_id);


--
-- Name: idx_inv_inventory_adjustment_doc_table_product_table_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_inventory_adjustment_doc_table_product_table_id ON public.inv_inventory_adjustment_doc_table USING btree (product_table_id);


--
-- Name: idx_inv_inventory_adjustment_doc_warehouse_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_inventory_adjustment_doc_warehouse_id ON public.inv_inventory_adjustment_doc USING btree (warehouse_id);


--
-- Name: idx_inv_inventory_adjustment_line_product_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_inventory_adjustment_line_product_id ON public.inv_inventory_adjustment_line USING btree (product_id);


--
-- Name: idx_inv_inventory_adjustment_line_unit_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_inventory_adjustment_line_unit_id ON public.inv_inventory_adjustment_line USING btree (unit_id);


--
-- Name: idx_inv_inventory_count_doc_cancelled_by_user_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_inventory_count_doc_cancelled_by_user_id ON public.inv_inventory_count_doc USING btree (cancelled_by_user_id);


--
-- Name: idx_inv_inventory_count_doc_count_completed_by_user_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_inventory_count_doc_count_completed_by_user_id ON public.inv_inventory_count_doc USING btree (count_completed_by_user_id);


--
-- Name: idx_inv_inventory_count_doc_doc_date; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_inventory_count_doc_doc_date ON public.inv_inventory_count_doc USING btree (doc_date);


--
-- Name: idx_inv_inventory_count_doc_negative_adjustment_doc_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_inventory_count_doc_negative_adjustment_doc_id ON public.inv_inventory_count_doc USING btree (negative_adjustment_doc_id);


--
-- Name: idx_inv_inventory_count_doc_organization_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_inventory_count_doc_organization_id ON public.inv_inventory_count_doc USING btree (organization_id);


--
-- Name: idx_inv_inventory_count_doc_positive_adjustment_doc_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_inventory_count_doc_positive_adjustment_doc_id ON public.inv_inventory_count_doc USING btree (positive_adjustment_doc_id);


--
-- Name: idx_inv_inventory_count_doc_posted_by_user_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_inventory_count_doc_posted_by_user_id ON public.inv_inventory_count_doc USING btree (posted_by_user_id);


--
-- Name: idx_inv_inventory_count_doc_state_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_inventory_count_doc_state_id ON public.inv_inventory_count_doc USING btree (state_id);


--
-- Name: idx_inv_inventory_count_doc_status_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_inventory_count_doc_status_id ON public.inv_inventory_count_doc USING btree (status_id);


--
-- Name: idx_inv_inventory_count_doc_table_product_table_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_inventory_count_doc_table_product_table_id ON public.inv_inventory_count_doc_table USING btree (product_table_id);


--
-- Name: idx_inv_inventory_count_doc_warehouse_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_inventory_count_doc_warehouse_id ON public.inv_inventory_count_doc USING btree (warehouse_id);


--
-- Name: idx_inv_inventory_count_line_product_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_inventory_count_line_product_id ON public.inv_inventory_count_line USING btree (product_id);


--
-- Name: idx_inv_inventory_count_line_unit_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_inventory_count_line_unit_id ON public.inv_inventory_count_line USING btree (unit_id);


--
-- Name: idx_inv_product_article; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_product_article ON public.inv_product USING btree (article);


--
-- Name: idx_inv_product_barcode; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_product_barcode ON public.inv_product USING btree (barcode);


--
-- Name: idx_inv_product_code; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_product_code ON public.inv_product USING btree (code);


--
-- Name: idx_inv_product_default_vat_rate_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_product_default_vat_rate_id ON public.inv_product USING btree (default_vat_rate_id);


--
-- Name: idx_inv_product_group_code; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_product_group_code ON public.inv_product_group USING btree (code);


--
-- Name: idx_inv_product_group_organization_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_product_group_organization_id ON public.inv_product_group USING btree (organization_id);


--
-- Name: idx_inv_product_group_parent_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_product_group_parent_id ON public.inv_product_group USING btree (parent_id);


--
-- Name: idx_inv_product_group_sort_order; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_product_group_sort_order ON public.inv_product_group USING btree (sort_order);


--
-- Name: idx_inv_product_group_state_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_product_group_state_id ON public.inv_product_group USING btree (state_id);


--
-- Name: idx_inv_product_name; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_product_name ON public.inv_product USING btree (name);


--
-- Name: idx_inv_product_organization_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_product_organization_id ON public.inv_product USING btree (organization_id);


--
-- Name: idx_inv_product_price_currency_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_product_price_currency_id ON public.inv_product_price USING btree (currency_id);


--
-- Name: idx_inv_product_price_dates; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_product_price_dates ON public.inv_product_price USING btree (start_date, end_date);


--
-- Name: idx_inv_product_price_organization_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_product_price_organization_id ON public.inv_product_price USING btree (organization_id);


--
-- Name: idx_inv_product_price_price_type_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_product_price_price_type_id ON public.inv_product_price USING btree (price_type_id);


--
-- Name: idx_inv_product_price_product_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_product_price_product_id ON public.inv_product_price USING btree (product_id);


--
-- Name: idx_inv_product_price_product_type_dates; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_product_price_product_type_dates ON public.inv_product_price USING btree (organization_id, product_id, price_type_id, state_id, start_date, end_date);


--
-- Name: idx_inv_product_price_state_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_product_price_state_id ON public.inv_product_price USING btree (state_id);


--
-- Name: idx_inv_product_product_group_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_product_product_group_id ON public.inv_product USING btree (product_group_id);


--
-- Name: idx_inv_product_product_type_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_product_product_type_id ON public.inv_product USING btree (product_type_id);


--
-- Name: idx_inv_product_sku; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_product_sku ON public.inv_product USING btree (sku);


--
-- Name: idx_inv_product_state_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_product_state_id ON public.inv_product USING btree (state_id);


--
-- Name: idx_inv_product_table_current_warehouse_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_product_table_current_warehouse_id ON public.inv_product_table USING btree (current_warehouse_id);


--
-- Name: idx_inv_product_table_org_warehouse_status; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_product_table_org_warehouse_status ON public.inv_product_table USING btree (organization_id, current_warehouse_id, status_id);


--
-- Name: idx_inv_product_table_org_warehouse_status_product; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_product_table_org_warehouse_status_product ON public.inv_product_table USING btree (organization_id, current_warehouse_id, status_id, product_id);


--
-- Name: idx_inv_product_unit_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_product_unit_id ON public.inv_product USING btree (unit_id);


--
-- Name: idx_inv_reg_balance_doc_date; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_reg_balance_doc_date ON public.inv_reg_balance USING btree (doc_date);


--
-- Name: idx_inv_reg_balance_document; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_reg_balance_document ON public.inv_reg_balance USING btree (document_type_id, document_id);


--
-- Name: idx_inv_reg_balance_organization_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_reg_balance_organization_id ON public.inv_reg_balance USING btree (organization_id);


--
-- Name: idx_inv_reg_balance_posting_batch_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_reg_balance_posting_batch_id ON public.inv_reg_balance USING btree (posting_batch_id);


--
-- Name: idx_inv_reg_balance_product_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_reg_balance_product_id ON public.inv_reg_balance USING btree (product_id);


--
-- Name: idx_inv_reg_balance_product_table_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_reg_balance_product_table_id ON public.inv_reg_balance USING btree (product_table_id);


--
-- Name: idx_inv_reg_balance_reversal_entry_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_reg_balance_reversal_entry_id ON public.inv_reg_balance USING btree (reversal_entry_id);


--
-- Name: idx_inv_reg_balance_warehouse_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_reg_balance_warehouse_id ON public.inv_reg_balance USING btree (warehouse_id);


--
-- Name: idx_inv_transfer_doc_cancelled_by_user_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_transfer_doc_cancelled_by_user_id ON public.inv_transfer_doc USING btree (cancelled_by_user_id);


--
-- Name: idx_inv_transfer_doc_destination_warehouse_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_transfer_doc_destination_warehouse_id ON public.inv_transfer_doc USING btree (destination_warehouse_id);


--
-- Name: idx_inv_transfer_doc_doc_date; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_transfer_doc_doc_date ON public.inv_transfer_doc USING btree (doc_date);


--
-- Name: idx_inv_transfer_doc_organization_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_transfer_doc_organization_id ON public.inv_transfer_doc USING btree (organization_id);


--
-- Name: idx_inv_transfer_doc_posted_by_user_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_transfer_doc_posted_by_user_id ON public.inv_transfer_doc USING btree (posted_by_user_id);


--
-- Name: idx_inv_transfer_doc_source_warehouse_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_transfer_doc_source_warehouse_id ON public.inv_transfer_doc USING btree (source_warehouse_id);


--
-- Name: idx_inv_transfer_doc_state_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_transfer_doc_state_id ON public.inv_transfer_doc USING btree (state_id);


--
-- Name: idx_inv_transfer_doc_status_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_transfer_doc_status_id ON public.inv_transfer_doc USING btree (status_id);


--
-- Name: idx_inv_transfer_doc_table_destination_warehouse_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_transfer_doc_table_destination_warehouse_id ON public.inv_transfer_doc_table USING btree (destination_warehouse_id);


--
-- Name: idx_inv_transfer_doc_table_product_table_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_transfer_doc_table_product_table_id ON public.inv_transfer_doc_table USING btree (product_table_id);


--
-- Name: idx_inv_transfer_doc_table_source_warehouse_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_transfer_doc_table_source_warehouse_id ON public.inv_transfer_doc_table USING btree (source_warehouse_id);


--
-- Name: idx_inv_transfer_line_product_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_transfer_line_product_id ON public.inv_transfer_line USING btree (product_id);


--
-- Name: idx_inv_transfer_line_unit_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_transfer_line_unit_id ON public.inv_transfer_line USING btree (unit_id);


--
-- Name: idx_inv_warehouse_branch_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_warehouse_branch_id ON public.inv_warehouse USING btree (branch_id);


--
-- Name: idx_inv_warehouse_code; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_warehouse_code ON public.inv_warehouse USING btree (code);


--
-- Name: idx_inv_warehouse_is_main; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_warehouse_is_main ON public.inv_warehouse USING btree (is_main);


--
-- Name: idx_inv_warehouse_organization_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_warehouse_organization_id ON public.inv_warehouse USING btree (organization_id);


--
-- Name: idx_inv_warehouse_responsible_user_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_warehouse_responsible_user_id ON public.inv_warehouse USING btree (responsible_user_id);


--
-- Name: idx_inv_warehouse_state_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_inv_warehouse_state_id ON public.inv_warehouse USING btree (state_id);


--
-- Name: idx_money_reg_balance_currency_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_money_reg_balance_currency_id ON public.money_reg_balance USING btree (currency_id);


--
-- Name: idx_money_reg_balance_doc_date; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_money_reg_balance_doc_date ON public.money_reg_balance USING btree (doc_date);


--
-- Name: idx_money_reg_balance_document; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_money_reg_balance_document ON public.money_reg_balance USING btree (document_type_id, document_id);


--
-- Name: idx_money_reg_balance_organization_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_money_reg_balance_organization_id ON public.money_reg_balance USING btree (organization_id);


--
-- Name: idx_money_reg_balance_posting_batch_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_money_reg_balance_posting_batch_id ON public.money_reg_balance USING btree (posting_batch_id);


--
-- Name: idx_money_reg_balance_reversal_entry_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_money_reg_balance_reversal_entry_id ON public.money_reg_balance USING btree (reversal_entry_id);


--
-- Name: idx_money_reg_balance_source; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_money_reg_balance_source ON public.money_reg_balance USING btree (source_type, source_id);


--
-- Name: idx_org_bank_account_bank_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_org_bank_account_bank_id ON public.org_bank_account USING btree (bank_id);


--
-- Name: idx_org_bank_account_code; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_org_bank_account_code ON public.org_bank_account USING btree (code);


--
-- Name: idx_org_bank_account_currency_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_org_bank_account_currency_id ON public.org_bank_account USING btree (currency_id);


--
-- Name: idx_org_bank_account_name; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_org_bank_account_name ON public.org_bank_account USING btree (name);


--
-- Name: idx_org_bank_account_organization_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_org_bank_account_organization_id ON public.org_bank_account USING btree (organization_id);


--
-- Name: idx_org_bank_account_state_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_org_bank_account_state_id ON public.org_bank_account USING btree (state_id);


--
-- Name: idx_org_branch_district_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_org_branch_district_id ON public.org_branch USING btree (district_id);


--
-- Name: idx_org_branch_org_code; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX idx_org_branch_org_code ON public.org_branch USING btree (organization_id, code);


--
-- Name: idx_org_branch_organization_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_org_branch_organization_id ON public.org_branch USING btree (organization_id);


--
-- Name: idx_org_branch_region_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_org_branch_region_id ON public.org_branch USING btree (region_id);


--
-- Name: idx_org_branch_state_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_org_branch_state_id ON public.org_branch USING btree (state_id);


--
-- Name: idx_org_claim_request_inn; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_org_claim_request_inn ON public.org_claim_request USING btree (inn);


--
-- Name: idx_org_claim_request_organization_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_org_claim_request_organization_id ON public.org_claim_request USING btree (organization_id);


--
-- Name: idx_org_claim_request_requested_by_user_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_org_claim_request_requested_by_user_id ON public.org_claim_request USING btree (requested_by_user_id);


--
-- Name: idx_org_claim_request_status; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_org_claim_request_status ON public.org_claim_request USING btree (status);


--
-- Name: idx_org_defaults_bank_account_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_org_defaults_bank_account_id ON public.org_defaults USING btree (bank_account_id);


--
-- Name: idx_org_defaults_branch_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_org_defaults_branch_id ON public.org_defaults USING btree (branch_id);


--
-- Name: idx_org_defaults_cash_box_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_org_defaults_cash_box_id ON public.org_defaults USING btree (cash_box_id);


--
-- Name: idx_org_defaults_warehouse_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_org_defaults_warehouse_id ON public.org_defaults USING btree (warehouse_id);


--
-- Name: idx_org_department_branch_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_org_department_branch_id ON public.org_department USING btree (branch_id);


--
-- Name: idx_org_department_org_code; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX idx_org_department_org_code ON public.org_department USING btree (organization_id, code);


--
-- Name: idx_org_department_organization_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_org_department_organization_id ON public.org_department USING btree (organization_id);


--
-- Name: idx_org_department_state_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_org_department_state_id ON public.org_department USING btree (state_id);


--
-- Name: idx_org_organization_config_accounting_policy_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_org_organization_config_accounting_policy_id ON public.org_organization_config USING btree (accounting_policy_id);


--
-- Name: idx_org_organization_config_base_currency_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_org_organization_config_base_currency_id ON public.org_organization_config USING btree (base_currency_id);


--
-- Name: idx_org_organization_default_language_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_org_organization_default_language_id ON public.org_organization USING btree (default_language_id);


--
-- Name: idx_org_organization_district_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_org_organization_district_id ON public.org_organization USING btree (district_id);


--
-- Name: idx_org_organization_full_name; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_org_organization_full_name ON public.org_organization USING btree (full_name);


--
-- Name: idx_org_organization_inn; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_org_organization_inn ON public.org_organization USING btree (inn);


--
-- Name: idx_org_organization_region_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_org_organization_region_id ON public.org_organization USING btree (region_id);


--
-- Name: idx_org_organization_setup_status; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_org_organization_setup_status ON public.org_organization USING btree (setup_status);


--
-- Name: idx_org_organization_short_name; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_org_organization_short_name ON public.org_organization USING btree (short_name);


--
-- Name: idx_org_organization_state_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_org_organization_state_id ON public.org_organization USING btree (state_id);


--
-- Name: idx_org_organization_tenant_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_org_organization_tenant_id ON public.org_organization USING btree (tenant_id);


--
-- Name: idx_org_position_org_code; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX idx_org_position_org_code ON public.org_position USING btree (organization_id, code);


--
-- Name: idx_org_position_organization_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_org_position_organization_id ON public.org_position USING btree (organization_id);


--
-- Name: idx_org_position_state_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_org_position_state_id ON public.org_position USING btree (state_id);


--
-- Name: idx_org_setup_state_is_completed; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_org_setup_state_is_completed ON public.org_setup_state USING btree (is_completed);


--
-- Name: idx_org_tax_settings_effective_dates; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_org_tax_settings_effective_dates ON public.org_tax_settings USING btree (effective_from, effective_to);


--
-- Name: idx_org_tax_settings_organization_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_org_tax_settings_organization_id ON public.org_tax_settings USING btree (organization_id);


--
-- Name: idx_org_tax_settings_tax_type_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_org_tax_settings_tax_type_id ON public.org_tax_settings USING btree (tax_type_id);


--
-- Name: idx_org_user_invitation_email; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_org_user_invitation_email ON public.org_user_invitation USING btree (email);


--
-- Name: idx_org_user_invitation_organization_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_org_user_invitation_organization_id ON public.org_user_invitation USING btree (organization_id);


--
-- Name: idx_org_user_invitation_role_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_org_user_invitation_role_id ON public.org_user_invitation USING btree (role_id);


--
-- Name: idx_platform_tenant_owner_user_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_platform_tenant_owner_user_id ON public.platform_tenant USING btree (owner_user_id);


--
-- Name: idx_platform_tenant_state_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_platform_tenant_state_id ON public.platform_tenant USING btree (state_id);


--
-- Name: idx_pur_doc_cancelled_by_user_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_pur_doc_cancelled_by_user_id ON public.pur_doc USING btree (cancelled_by_user_id);


--
-- Name: idx_pur_doc_contract_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_pur_doc_contract_id ON public.pur_doc USING btree (contract_id);


--
-- Name: idx_pur_doc_counterparty_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_pur_doc_counterparty_id ON public.pur_doc USING btree (counterparty_id);


--
-- Name: idx_pur_doc_doc_date; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_pur_doc_doc_date ON public.pur_doc USING btree (doc_date);


--
-- Name: idx_pur_doc_organization_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_pur_doc_organization_id ON public.pur_doc USING btree (organization_id);


--
-- Name: idx_pur_doc_posted_by_user_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_pur_doc_posted_by_user_id ON public.pur_doc USING btree (posted_by_user_id);


--
-- Name: idx_pur_doc_state_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_pur_doc_state_id ON public.pur_doc USING btree (state_id);


--
-- Name: idx_pur_doc_status_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_pur_doc_status_id ON public.pur_doc USING btree (status_id);


--
-- Name: idx_pur_doc_warehouse_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_pur_doc_warehouse_id ON public.pur_doc USING btree (warehouse_id);


--
-- Name: idx_sale_condition_costing_method_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sale_condition_costing_method_id ON public.sale_condition USING btree (costing_method_id);


--
-- Name: idx_sale_condition_dates; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sale_condition_dates ON public.sale_condition USING btree (start_date, end_date);


--
-- Name: idx_sale_condition_org_state_dates; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sale_condition_org_state_dates ON public.sale_condition USING btree (organization_id, state_id, start_date, end_date);


--
-- Name: idx_sale_condition_organization_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sale_condition_organization_id ON public.sale_condition USING btree (organization_id);


--
-- Name: idx_sale_condition_state_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sale_condition_state_id ON public.sale_condition USING btree (state_id);


--
-- Name: idx_sale_condition_vat_rate_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sale_condition_vat_rate_id ON public.sale_condition USING btree (vat_rate_id);


--
-- Name: idx_sale_doc_cancelled_by_user_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sale_doc_cancelled_by_user_id ON public.sale_doc USING btree (cancelled_by_user_id);


--
-- Name: idx_sale_doc_contract_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sale_doc_contract_id ON public.sale_doc USING btree (contract_id) WHERE (contract_id IS NOT NULL);


--
-- Name: idx_sale_doc_counterparty_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sale_doc_counterparty_id ON public.sale_doc USING btree (counterparty_id);


--
-- Name: idx_sale_doc_doc_date; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sale_doc_doc_date ON public.sale_doc USING btree (doc_date);


--
-- Name: idx_sale_doc_organization_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sale_doc_organization_id ON public.sale_doc USING btree (organization_id);


--
-- Name: idx_sale_doc_posted_by_user_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sale_doc_posted_by_user_id ON public.sale_doc USING btree (posted_by_user_id);


--
-- Name: idx_sale_doc_state_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sale_doc_state_id ON public.sale_doc USING btree (state_id);


--
-- Name: idx_sale_doc_status_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sale_doc_status_id ON public.sale_doc USING btree (status_id);


--
-- Name: idx_sale_doc_table_product_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sale_doc_table_product_id ON public.sale_doc_table USING btree (product_table_id);


--
-- Name: idx_sale_doc_table_vat_rate_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sale_doc_table_vat_rate_id ON public.sale_doc_table USING btree (vat_rate_id);


--
-- Name: idx_sale_doc_warehouse_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sale_doc_warehouse_id ON public.sale_doc USING btree (warehouse_id);


--
-- Name: idx_sys_audit_log_action; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sys_audit_log_action ON public.sys_audit_log USING btree (action);


--
-- Name: idx_sys_audit_log_changed_date; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sys_audit_log_changed_date ON public.sys_audit_log USING btree (changed_date);


--
-- Name: idx_sys_audit_log_changed_user_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sys_audit_log_changed_user_id ON public.sys_audit_log USING btree (changed_user_id);


--
-- Name: idx_sys_audit_log_organization_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sys_audit_log_organization_id ON public.sys_audit_log USING btree (organization_id);


--
-- Name: idx_sys_audit_log_table_record; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sys_audit_log_table_record ON public.sys_audit_log USING btree (table_name, record_id);


--
-- Name: idx_sys_email_verification_token_email; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sys_email_verification_token_email ON public.sys_email_verification_token USING btree (email);


--
-- Name: idx_sys_email_verification_token_user_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sys_email_verification_token_user_id ON public.sys_email_verification_token USING btree (user_id);


--
-- Name: idx_sys_module_is_visible; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sys_module_is_visible ON public.sys_module USING btree (is_visible);


--
-- Name: idx_sys_module_parent_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sys_module_parent_id ON public.sys_module USING btree (parent_id);


--
-- Name: idx_sys_module_sort_order; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sys_module_sort_order ON public.sys_module USING btree (sort_order);


--
-- Name: idx_sys_password_reset_token_expires_at; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sys_password_reset_token_expires_at ON public.sys_password_reset_token USING btree (expires_at);


--
-- Name: idx_sys_password_reset_token_user_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sys_password_reset_token_user_id ON public.sys_password_reset_token USING btree (user_id);


--
-- Name: idx_sys_refresh_token_expires_at; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sys_refresh_token_expires_at ON public.sys_refresh_token USING btree (expires_at);


--
-- Name: idx_sys_refresh_token_revoked_at; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sys_refresh_token_revoked_at ON public.sys_refresh_token USING btree (revoked_at);


--
-- Name: idx_sys_refresh_token_user_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sys_refresh_token_user_id ON public.sys_refresh_token USING btree (user_id);


--
-- Name: idx_sys_role_code; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sys_role_code ON public.sys_role USING btree (code);


--
-- Name: idx_sys_role_is_system; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sys_role_is_system ON public.sys_role USING btree (is_system);


--
-- Name: idx_sys_role_organization_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sys_role_organization_id ON public.sys_role USING btree (organization_id);


--
-- Name: idx_sys_role_sort_order; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sys_role_sort_order ON public.sys_role USING btree (sort_order);


--
-- Name: idx_sys_user_email; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sys_user_email ON public.sys_user USING btree (email);


--
-- Name: idx_sys_user_email_verified; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sys_user_email_verified ON public.sys_user USING btree (email_verified);


--
-- Name: idx_sys_user_is_platform_admin; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sys_user_is_platform_admin ON public.sys_user USING btree (is_platform_admin);


--
-- Name: idx_sys_user_language_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sys_user_language_id ON public.sys_user USING btree (language_id);


--
-- Name: idx_sys_user_organization_default_user; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX idx_sys_user_organization_default_user ON public.sys_user_organization USING btree (user_id) WHERE (is_default = true);


--
-- Name: idx_sys_user_organization_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sys_user_organization_id ON public.sys_user USING btree (organization_id);


--
-- Name: idx_sys_user_organization_invited_by_user_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sys_user_organization_invited_by_user_id ON public.sys_user_organization USING btree (invited_by_user_id);


--
-- Name: idx_sys_user_organization_is_owner; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sys_user_organization_is_owner ON public.sys_user_organization USING btree (is_owner);


--
-- Name: idx_sys_user_organization_organization_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sys_user_organization_organization_id ON public.sys_user_organization USING btree (organization_id);


--
-- Name: idx_sys_user_organization_role_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sys_user_organization_role_id ON public.sys_user_organization USING btree (role_id);


--
-- Name: idx_sys_user_organization_state_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sys_user_organization_state_id ON public.sys_user_organization USING btree (state_id);


--
-- Name: idx_sys_user_phone; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sys_user_phone ON public.sys_user USING btree (phone_number);


--
-- Name: idx_sys_user_role_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_sys_user_role_id ON public.sys_user USING btree (role_id);


--
-- Name: ix_inv_inventory_adjustment_doc_table_owner_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_inv_inventory_adjustment_doc_table_owner_id ON public.inv_inventory_adjustment_doc_table USING btree (owner_id);


--
-- Name: ix_inv_inventory_adjustment_line_owner_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_inv_inventory_adjustment_line_owner_id ON public.inv_inventory_adjustment_line USING btree (owner_id);


--
-- Name: ix_inv_inventory_count_doc_table_owner_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_inv_inventory_count_doc_table_owner_id ON public.inv_inventory_count_doc_table USING btree (owner_id);


--
-- Name: ix_inv_inventory_count_line_owner_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_inv_inventory_count_line_owner_id ON public.inv_inventory_count_line USING btree (owner_id);


--
-- Name: ix_inv_product_mxik; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_inv_product_mxik ON public.inv_product USING btree (mxik) WHERE (mxik IS NOT NULL);


--
-- Name: ix_inv_product_table_status_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_inv_product_table_status_id ON public.inv_product_table USING btree (status_id);


--
-- Name: ix_inv_transfer_doc_table_owner_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_inv_transfer_doc_table_owner_id ON public.inv_transfer_doc_table USING btree (owner_id);


--
-- Name: ix_inv_transfer_line_owner_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_inv_transfer_line_owner_id ON public.inv_transfer_line USING btree (owner_id);


--
-- Name: ix_pur_doc_product_owner_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_pur_doc_product_owner_id ON public.pur_doc_product USING btree (owner_id);


--
-- Name: ix_pur_doc_product_product_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_pur_doc_product_product_id ON public.pur_doc_product USING btree (product_id) WHERE (product_id IS NOT NULL);


--
-- Name: ix_pur_doc_table_owner_id_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_pur_doc_table_owner_id_id ON public.pur_doc_table USING btree (owner_id, id);


--
-- Name: ix_sale_doc_product_owner_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_sale_doc_product_owner_id ON public.sale_doc_product USING btree (owner_id);


--
-- Name: ix_sale_doc_product_product_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_sale_doc_product_product_id ON public.sale_doc_product USING btree (product_id);


--
-- Name: ix_sale_doc_table_owner_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_sale_doc_table_owner_id ON public.sale_doc_table USING btree (owner_id);


--
-- Name: sys_module_sub_group_unique_index_code; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX sys_module_sub_group_unique_index_code ON public.sys_module_sub_group USING btree (code);


--
-- Name: sys_module_unique_index_code; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX sys_module_unique_index_code ON public.sys_module USING btree (code);


--
-- Name: sys_module_unique_index_sub_group_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX sys_module_unique_index_sub_group_id ON public.sys_module USING btree (sub_group_id);


--
-- Name: uidx_cmn_document_sequence_scope; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX uidx_cmn_document_sequence_scope ON public.cmn_document_sequence USING btree (organization_id, document_type_id, COALESCE((year)::integer, 0), COALESCE((month)::integer, 0));


--
-- Name: uidx_counterparty_card_org_code; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX uidx_counterparty_card_org_code ON public.counterparty_card USING btree (organization_id, code) WHERE (code IS NOT NULL);


--
-- Name: uidx_inv_product_group_org_code; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX uidx_inv_product_group_org_code ON public.inv_product_group USING btree (organization_id, code) WHERE (code IS NOT NULL);


--
-- Name: uidx_inv_product_org_code; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX uidx_inv_product_org_code ON public.inv_product USING btree (organization_id, code) WHERE (code IS NOT NULL);


--
-- Name: uidx_inv_warehouse_org_code; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX uidx_inv_warehouse_org_code ON public.inv_warehouse USING btree (organization_id, code) WHERE (code IS NOT NULL);


--
-- Name: uidx_org_bank_account_org_code; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX uidx_org_bank_account_org_code ON public.org_bank_account USING btree (organization_id, code) WHERE (code IS NOT NULL);


--
-- Name: uidx_sys_role_org_code; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX uidx_sys_role_org_code ON public.sys_role USING btree (organization_id, code) WHERE (code IS NOT NULL);


--
-- Name: uidx_sys_user_user_name; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX uidx_sys_user_user_name ON public.sys_user USING btree (user_name);


--
-- Name: ux_inv_inventory_adjustment_doc_org_doc_number; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX ux_inv_inventory_adjustment_doc_org_doc_number ON public.inv_inventory_adjustment_doc USING btree (organization_id, doc_number);


--
-- Name: ux_inv_inventory_adjustment_doc_table_owner_product_table; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX ux_inv_inventory_adjustment_doc_table_owner_product_table ON public.inv_inventory_adjustment_doc_table USING btree (owner_id, product_table_id);


--
-- Name: ux_inv_inventory_count_doc_org_doc_number; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX ux_inv_inventory_count_doc_org_doc_number ON public.inv_inventory_count_doc USING btree (organization_id, doc_number);


--
-- Name: ux_inv_inventory_count_doc_table_owner_product_table; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX ux_inv_inventory_count_doc_table_owner_product_table ON public.inv_inventory_count_doc_table USING btree (owner_id, product_table_id);


--
-- Name: ux_inv_inventory_count_line_owner_product_unit; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX ux_inv_inventory_count_line_owner_product_unit ON public.inv_inventory_count_line USING btree (owner_id, product_id, unit_id);


--
-- Name: ux_inv_product_table_org_marking; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX ux_inv_product_table_org_marking ON public.inv_product_table USING btree (organization_id, marking_number) WHERE (marking_number IS NOT NULL);


--
-- Name: ux_inv_product_table_org_serial; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX ux_inv_product_table_org_serial ON public.inv_product_table USING btree (organization_id, serial_number) WHERE (serial_number IS NOT NULL);


--
-- Name: ux_inv_transfer_doc_doc_number_org; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX ux_inv_transfer_doc_doc_number_org ON public.inv_transfer_doc USING btree (organization_id, doc_number);


--
-- Name: ux_inv_transfer_doc_table_owner_product_table; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX ux_inv_transfer_doc_table_owner_product_table ON public.inv_transfer_doc_table USING btree (owner_id, product_table_id);


--
-- Name: ux_pur_doc_table_owner_id_product_table_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX ux_pur_doc_table_owner_id_product_table_id ON public.pur_doc_table USING btree (owner_id, product_table_id);


--
-- Name: bank_operation set_bank_operation_doc_number_trigger; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER set_bank_operation_doc_number_trigger BEFORE INSERT ON public.bank_operation FOR EACH ROW EXECUTE FUNCTION public.set_bank_operation_doc_number();


--
-- Name: cash_operation set_cash_operation_doc_number_trigger; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER set_cash_operation_doc_number_trigger BEFORE INSERT ON public.cash_operation FOR EACH ROW EXECUTE FUNCTION public.set_cash_operation_doc_number();


--
-- Name: cmn_contract set_cmn_contract_number_trigger; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER set_cmn_contract_number_trigger BEFORE INSERT ON public.cmn_contract FOR EACH ROW EXECUTE FUNCTION public.set_cmn_contract_number();


--
-- Name: pur_doc set_pur_doc_number_trigger; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER set_pur_doc_number_trigger BEFORE INSERT ON public.pur_doc FOR EACH ROW EXECUTE FUNCTION public.set_pur_doc_number();


--
-- Name: sale_doc set_sale_doc_number_trigger; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER set_sale_doc_number_trigger BEFORE INSERT ON public.sale_doc FOR EACH ROW EXECUTE FUNCTION public.set_sale_doc_number();


--
-- Name: acc_account_resolve_rule acc_account_resolve_rule_account_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_account_resolve_rule
    ADD CONSTRAINT acc_account_resolve_rule_account_id_fkey FOREIGN KEY (account_id) REFERENCES public.acc_chart_account(id);


--
-- Name: acc_account_resolve_rule acc_account_resolve_rule_policy_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_account_resolve_rule
    ADD CONSTRAINT acc_account_resolve_rule_policy_id_fkey FOREIGN KEY (policy_id) REFERENCES public.acc_accounting_policy(id);


--
-- Name: acc_account_type acc_account_type_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_account_type
    ADD CONSTRAINT acc_account_type_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: acc_accounting_period acc_accounting_period_closed_by_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_accounting_period
    ADD CONSTRAINT acc_accounting_period_closed_by_user_id_fkey FOREIGN KEY (closed_by_user_id) REFERENCES public.sys_user(id);


--
-- Name: acc_accounting_period acc_accounting_period_organization_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_accounting_period
    ADD CONSTRAINT acc_accounting_period_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);


--
-- Name: acc_accounting_policy acc_accounting_policy_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_accounting_policy
    ADD CONSTRAINT acc_accounting_policy_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: acc_chart_account acc_chart_account_account_type_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_chart_account
    ADD CONSTRAINT acc_chart_account_account_type_id_fkey FOREIGN KEY (account_type_id) REFERENCES public.acc_account_type(id);


--
-- Name: acc_chart_account acc_chart_account_parent_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_chart_account
    ADD CONSTRAINT acc_chart_account_parent_id_fkey FOREIGN KEY (parent_id) REFERENCES public.acc_chart_account(id);


--
-- Name: acc_chart_account acc_chart_account_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_chart_account
    ADD CONSTRAINT acc_chart_account_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: acc_chart_account_subkonto acc_chart_account_subkonto_account_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_chart_account_subkonto
    ADD CONSTRAINT acc_chart_account_subkonto_account_id_fkey FOREIGN KEY (account_id) REFERENCES public.acc_chart_account(id) ON DELETE CASCADE;


--
-- Name: acc_chart_account_subkonto acc_chart_account_subkonto_organization_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_chart_account_subkonto
    ADD CONSTRAINT acc_chart_account_subkonto_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);


--
-- Name: acc_chart_account_subkonto acc_chart_account_subkonto_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_chart_account_subkonto
    ADD CONSTRAINT acc_chart_account_subkonto_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: acc_chart_account_subkonto acc_chart_account_subkonto_subkonto_type_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_chart_account_subkonto
    ADD CONSTRAINT acc_chart_account_subkonto_subkonto_type_id_fkey FOREIGN KEY (subkonto_type_id) REFERENCES public.acc_subkonto_type(id);


--
-- Name: acc_payment_purpose acc_payment_purpose_alias_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_payment_purpose
    ADD CONSTRAINT acc_payment_purpose_alias_id_fkey FOREIGN KEY (alias_id) REFERENCES public.acc_posting_alias(id);


--
-- Name: acc_payment_purpose acc_payment_purpose_operation_type_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_payment_purpose
    ADD CONSTRAINT acc_payment_purpose_operation_type_id_fkey FOREIGN KEY (operation_type_id) REFERENCES public.cmn_operation_type(id);


--
-- Name: acc_payment_purpose_translation acc_payment_purpose_translation_language_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_payment_purpose_translation
    ADD CONSTRAINT acc_payment_purpose_translation_language_id_fkey FOREIGN KEY (language_id) REFERENCES public.cmn_language(id);


--
-- Name: acc_payment_purpose_translation acc_payment_purpose_translation_payment_purpose_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_payment_purpose_translation
    ADD CONSTRAINT acc_payment_purpose_translation_payment_purpose_id_fkey FOREIGN KEY (payment_purpose_id) REFERENCES public.acc_payment_purpose(id);


--
-- Name: acc_posting_alias_translation acc_posting_alias_translation_language_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_posting_alias_translation
    ADD CONSTRAINT acc_posting_alias_translation_language_id_fkey FOREIGN KEY (language_id) REFERENCES public.cmn_language(id);


--
-- Name: acc_posting_alias_translation acc_posting_alias_translation_posting_alias_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_posting_alias_translation
    ADD CONSTRAINT acc_posting_alias_translation_posting_alias_id_fkey FOREIGN KEY (posting_alias_id) REFERENCES public.acc_posting_alias(id);


--
-- Name: acc_posting_batch acc_posting_batch_document_type_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_posting_batch
    ADD CONSTRAINT acc_posting_batch_document_type_id_fkey FOREIGN KEY (document_type_id) REFERENCES public.cmn_document_type(id);


--
-- Name: acc_posting_batch acc_posting_batch_organization_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_posting_batch
    ADD CONSTRAINT acc_posting_batch_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);


--
-- Name: acc_posting_batch acc_posting_batch_posted_by_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_posting_batch
    ADD CONSTRAINT acc_posting_batch_posted_by_user_id_fkey FOREIGN KEY (posted_by_user_id) REFERENCES public.sys_user(id);


--
-- Name: acc_posting_batch acc_posting_batch_reversed_by_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_posting_batch
    ADD CONSTRAINT acc_posting_batch_reversed_by_user_id_fkey FOREIGN KEY (reversed_by_user_id) REFERENCES public.sys_user(id);


--
-- Name: acc_posting_rule_line acc_posting_rule_line_template_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_posting_rule_line
    ADD CONSTRAINT acc_posting_rule_line_template_id_fkey FOREIGN KEY (template_id) REFERENCES public.acc_posting_rule(id);


--
-- Name: acc_reg_entry acc_reg_entry_credit_account_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_reg_entry
    ADD CONSTRAINT acc_reg_entry_credit_account_id_fkey FOREIGN KEY (credit_account_id) REFERENCES public.acc_chart_account(id);


--
-- Name: acc_reg_entry acc_reg_entry_currency_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_reg_entry
    ADD CONSTRAINT acc_reg_entry_currency_id_fkey FOREIGN KEY (currency_id) REFERENCES public.cmn_currency(id);


--
-- Name: acc_reg_entry acc_reg_entry_debit_account_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_reg_entry
    ADD CONSTRAINT acc_reg_entry_debit_account_id_fkey FOREIGN KEY (debit_account_id) REFERENCES public.acc_chart_account(id);


--
-- Name: acc_reg_entry acc_reg_entry_document_type_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_reg_entry
    ADD CONSTRAINT acc_reg_entry_document_type_id_fkey FOREIGN KEY (document_type_id) REFERENCES public.cmn_document_type(id);


--
-- Name: acc_reg_entry acc_reg_entry_operation_type_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_reg_entry
    ADD CONSTRAINT acc_reg_entry_operation_type_id_fkey FOREIGN KEY (operation_type_id) REFERENCES public.cmn_operation_type(id);


--
-- Name: acc_reg_entry acc_reg_entry_organization_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_reg_entry
    ADD CONSTRAINT acc_reg_entry_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);


--
-- Name: acc_reg_entry acc_reg_entry_posting_batch_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_reg_entry
    ADD CONSTRAINT acc_reg_entry_posting_batch_id_fkey FOREIGN KEY (posting_batch_id) REFERENCES public.acc_posting_batch(id);


--
-- Name: acc_reg_entry acc_reg_entry_reversal_entry_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_reg_entry
    ADD CONSTRAINT acc_reg_entry_reversal_entry_id_fkey FOREIGN KEY (reversal_entry_id) REFERENCES public.acc_reg_entry(id);


--
-- Name: acc_reg_entry_subkonto acc_reg_entry_subkonto_entry_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_reg_entry_subkonto
    ADD CONSTRAINT acc_reg_entry_subkonto_entry_id_fkey FOREIGN KEY (entry_id) REFERENCES public.acc_reg_entry(id) ON DELETE CASCADE;


--
-- Name: acc_reg_entry_subkonto acc_reg_entry_subkonto_subkonto_type_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_reg_entry_subkonto
    ADD CONSTRAINT acc_reg_entry_subkonto_subkonto_type_id_fkey FOREIGN KEY (subkonto_type_id) REFERENCES public.acc_subkonto_type(id);


--
-- Name: acc_subkonto_type acc_subkonto_type_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_subkonto_type
    ADD CONSTRAINT acc_subkonto_type_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: bank_operation bank_operation_bank_account_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.bank_operation
    ADD CONSTRAINT bank_operation_bank_account_id_fkey FOREIGN KEY (bank_account_id) REFERENCES public.org_bank_account(id);


--
-- Name: bank_operation bank_operation_cancelled_by_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.bank_operation
    ADD CONSTRAINT bank_operation_cancelled_by_user_id_fkey FOREIGN KEY (cancelled_by_user_id) REFERENCES public.sys_user(id);


--
-- Name: bank_operation bank_operation_contract_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.bank_operation
    ADD CONSTRAINT bank_operation_contract_id_fkey FOREIGN KEY (contract_id) REFERENCES public.cmn_contract(id);


--
-- Name: bank_operation bank_operation_counterparty_bank_account_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.bank_operation
    ADD CONSTRAINT bank_operation_counterparty_bank_account_id_fkey FOREIGN KEY (counterparty_bank_account_id) REFERENCES public.counterparty_bank_account(id);


--
-- Name: bank_operation bank_operation_counterparty_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.bank_operation
    ADD CONSTRAINT bank_operation_counterparty_id_fkey FOREIGN KEY (counterparty_id) REFERENCES public.counterparty_card(id);


--
-- Name: bank_operation bank_operation_currency_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.bank_operation
    ADD CONSTRAINT bank_operation_currency_id_fkey FOREIGN KEY (currency_id) REFERENCES public.cmn_currency(id);


--
-- Name: bank_operation_line bank_operation_line_bank_operation_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.bank_operation_line
    ADD CONSTRAINT bank_operation_line_bank_operation_id_fkey FOREIGN KEY (bank_operation_id) REFERENCES public.bank_operation(id) ON DELETE CASCADE;


--
-- Name: bank_operation_line bank_operation_line_counterparty_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.bank_operation_line
    ADD CONSTRAINT bank_operation_line_counterparty_id_fkey FOREIGN KEY (counterparty_id) REFERENCES public.counterparty_card(id);


--
-- Name: bank_operation_line bank_operation_line_payment_purpose_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.bank_operation_line
    ADD CONSTRAINT bank_operation_line_payment_purpose_id_fkey FOREIGN KEY (payment_purpose_id) REFERENCES public.acc_payment_purpose(id);


--
-- Name: bank_operation bank_operation_operation_type_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.bank_operation
    ADD CONSTRAINT bank_operation_operation_type_id_fkey FOREIGN KEY (operation_type_id) REFERENCES public.cmn_operation_type(id);


--
-- Name: bank_operation bank_operation_organization_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.bank_operation
    ADD CONSTRAINT bank_operation_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);


--
-- Name: bank_operation bank_operation_payment_type_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.bank_operation
    ADD CONSTRAINT bank_operation_payment_type_id_fkey FOREIGN KEY (payment_type_id) REFERENCES public.cmn_payment_type(id);


--
-- Name: bank_operation bank_operation_posted_by_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.bank_operation
    ADD CONSTRAINT bank_operation_posted_by_user_id_fkey FOREIGN KEY (posted_by_user_id) REFERENCES public.sys_user(id);


--
-- Name: bank_operation bank_operation_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.bank_operation
    ADD CONSTRAINT bank_operation_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: bank_operation bank_operation_status_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.bank_operation
    ADD CONSTRAINT bank_operation_status_id_fkey FOREIGN KEY (status_id) REFERENCES public.cmn_document_status(id);


--
-- Name: cash_box cash_box_branch_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cash_box
    ADD CONSTRAINT cash_box_branch_id_fkey FOREIGN KEY (branch_id) REFERENCES public.org_branch(id);


--
-- Name: cash_box cash_box_currency_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cash_box
    ADD CONSTRAINT cash_box_currency_id_fkey FOREIGN KEY (currency_id) REFERENCES public.cmn_currency(id);


--
-- Name: cash_box cash_box_organization_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cash_box
    ADD CONSTRAINT cash_box_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);


--
-- Name: cash_box cash_box_responsible_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cash_box
    ADD CONSTRAINT cash_box_responsible_user_id_fkey FOREIGN KEY (responsible_user_id) REFERENCES public.sys_user(id);


--
-- Name: cash_box cash_box_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cash_box
    ADD CONSTRAINT cash_box_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: cash_operation cash_operation_cancelled_by_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cash_operation
    ADD CONSTRAINT cash_operation_cancelled_by_user_id_fkey FOREIGN KEY (cancelled_by_user_id) REFERENCES public.sys_user(id);


--
-- Name: cash_operation cash_operation_cash_box_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cash_operation
    ADD CONSTRAINT cash_operation_cash_box_id_fkey FOREIGN KEY (cash_box_id) REFERENCES public.cash_box(id);


--
-- Name: cash_operation cash_operation_counterparty_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cash_operation
    ADD CONSTRAINT cash_operation_counterparty_id_fkey FOREIGN KEY (counterparty_id) REFERENCES public.counterparty_card(id);


--
-- Name: cash_operation cash_operation_currency_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cash_operation
    ADD CONSTRAINT cash_operation_currency_id_fkey FOREIGN KEY (currency_id) REFERENCES public.cmn_currency(id);


--
-- Name: cash_operation cash_operation_destination_cash_box_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cash_operation
    ADD CONSTRAINT cash_operation_destination_cash_box_id_fkey FOREIGN KEY (destination_cash_box_id) REFERENCES public.cash_box(id);


--
-- Name: cash_operation cash_operation_operation_type_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cash_operation
    ADD CONSTRAINT cash_operation_operation_type_id_fkey FOREIGN KEY (operation_type_id) REFERENCES public.cmn_operation_type(id);


--
-- Name: cash_operation cash_operation_organization_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cash_operation
    ADD CONSTRAINT cash_operation_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);


--
-- Name: cash_operation cash_operation_payment_type_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cash_operation
    ADD CONSTRAINT cash_operation_payment_type_id_fkey FOREIGN KEY (payment_type_id) REFERENCES public.cmn_payment_type(id);


--
-- Name: cash_operation cash_operation_posted_by_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cash_operation
    ADD CONSTRAINT cash_operation_posted_by_user_id_fkey FOREIGN KEY (posted_by_user_id) REFERENCES public.sys_user(id);


--
-- Name: cash_operation cash_operation_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cash_operation
    ADD CONSTRAINT cash_operation_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: cash_operation cash_operation_status_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cash_operation
    ADD CONSTRAINT cash_operation_status_id_fkey FOREIGN KEY (status_id) REFERENCES public.cmn_document_status(id);


--
-- Name: cmn_bank cmn_bank_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_bank
    ADD CONSTRAINT cmn_bank_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: cmn_contract cmn_contract_contract_type_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_contract
    ADD CONSTRAINT cmn_contract_contract_type_id_fkey FOREIGN KEY (contract_type_id) REFERENCES public.cmn_contract_type(id);


--
-- Name: cmn_contract cmn_contract_counterparty_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_contract
    ADD CONSTRAINT cmn_contract_counterparty_id_fkey FOREIGN KEY (counterparty_id) REFERENCES public.counterparty_card(id);


--
-- Name: cmn_contract cmn_contract_organization_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_contract
    ADD CONSTRAINT cmn_contract_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);


--
-- Name: cmn_contract cmn_contract_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_contract
    ADD CONSTRAINT cmn_contract_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: cmn_contract_type cmn_contract_type_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_contract_type
    ADD CONSTRAINT cmn_contract_type_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: cmn_counterparty_type cmn_counterparty_type_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_counterparty_type
    ADD CONSTRAINT cmn_counterparty_type_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: cmn_currency cmn_currency_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_currency
    ADD CONSTRAINT cmn_currency_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: cmn_document_sequence cmn_document_sequence_document_type_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_document_sequence
    ADD CONSTRAINT cmn_document_sequence_document_type_id_fkey FOREIGN KEY (document_type_id) REFERENCES public.cmn_document_type(id);


--
-- Name: cmn_document_sequence cmn_document_sequence_organization_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_document_sequence
    ADD CONSTRAINT cmn_document_sequence_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);


--
-- Name: cmn_document_sequence cmn_document_sequence_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_document_sequence
    ADD CONSTRAINT cmn_document_sequence_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: cmn_document_status cmn_document_status_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_document_status
    ADD CONSTRAINT cmn_document_status_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: cmn_document_type cmn_document_type_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_document_type
    ADD CONSTRAINT cmn_document_type_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: cmn_language cmn_language_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_language
    ADD CONSTRAINT cmn_language_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: cmn_operation_type cmn_operation_type_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_operation_type
    ADD CONSTRAINT cmn_operation_type_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: cmn_payment_type cmn_payment_type_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_payment_type
    ADD CONSTRAINT cmn_payment_type_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: cmn_pricing_condition cmn_pricing_condition_organization_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_pricing_condition
    ADD CONSTRAINT cmn_pricing_condition_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);


--
-- Name: cmn_pricing_condition cmn_pricing_condition_pricing_method_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_pricing_condition
    ADD CONSTRAINT cmn_pricing_condition_pricing_method_id_fkey FOREIGN KEY (pricing_method_id) REFERENCES public.cmn_pricing_method(id);


--
-- Name: cmn_pricing_condition cmn_pricing_condition_rounding_method_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_pricing_condition
    ADD CONSTRAINT cmn_pricing_condition_rounding_method_id_fkey FOREIGN KEY (rounding_method_id) REFERENCES public.cmn_price_rounding_method(id);


--
-- Name: cmn_pricing_condition cmn_pricing_condition_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_pricing_condition
    ADD CONSTRAINT cmn_pricing_condition_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: cmn_product_table_status cmn_product_table_status_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_product_table_status
    ADD CONSTRAINT cmn_product_table_status_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: cmn_product_type_translation cmn_product_type_translation_language_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_product_type_translation
    ADD CONSTRAINT cmn_product_type_translation_language_id_fkey FOREIGN KEY (language_id) REFERENCES public.cmn_language(id);


--
-- Name: cmn_product_type_translation cmn_product_type_translation_product_type_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_product_type_translation
    ADD CONSTRAINT cmn_product_type_translation_product_type_id_fkey FOREIGN KEY (product_type_id) REFERENCES public.cmn_product_type(id);


--
-- Name: cmn_region cmn_region_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_region
    ADD CONSTRAINT cmn_region_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: cmn_tax_type cmn_tax_type_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_tax_type
    ADD CONSTRAINT cmn_tax_type_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: cmn_translation cmn_translation_language_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_translation
    ADD CONSTRAINT cmn_translation_language_id_fkey FOREIGN KEY (language_id) REFERENCES public.cmn_language(id);


--
-- Name: cmn_unit cmn_unit_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_unit
    ADD CONSTRAINT cmn_unit_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: cmn_vat_rate cmn_vat_rate_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cmn_vat_rate
    ADD CONSTRAINT cmn_vat_rate_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: counterparty_account_payment_purpose_hint counterparty_account_payment__counterparty_bank_account_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.counterparty_account_payment_purpose_hint
    ADD CONSTRAINT counterparty_account_payment__counterparty_bank_account_id_fkey FOREIGN KEY (counterparty_bank_account_id) REFERENCES public.counterparty_bank_account(id);


--
-- Name: counterparty_account_payment_purpose_hint counterparty_account_payment_purpose_hi_payment_purpose_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.counterparty_account_payment_purpose_hint
    ADD CONSTRAINT counterparty_account_payment_purpose_hi_payment_purpose_id_fkey FOREIGN KEY (payment_purpose_id) REFERENCES public.acc_payment_purpose(id);


--
-- Name: counterparty_bank_account counterparty_bank_account_bank_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.counterparty_bank_account
    ADD CONSTRAINT counterparty_bank_account_bank_id_fkey FOREIGN KEY (bank_id) REFERENCES public.cmn_bank(id);


--
-- Name: counterparty_bank_account counterparty_bank_account_counterparty_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.counterparty_bank_account
    ADD CONSTRAINT counterparty_bank_account_counterparty_id_fkey FOREIGN KEY (counterparty_id) REFERENCES public.counterparty_card(id);


--
-- Name: counterparty_bank_account counterparty_bank_account_currency_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.counterparty_bank_account
    ADD CONSTRAINT counterparty_bank_account_currency_id_fkey FOREIGN KEY (currency_id) REFERENCES public.cmn_currency(id);


--
-- Name: counterparty_bank_account counterparty_bank_account_organization_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.counterparty_bank_account
    ADD CONSTRAINT counterparty_bank_account_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);


--
-- Name: counterparty_bank_account counterparty_bank_account_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.counterparty_bank_account
    ADD CONSTRAINT counterparty_bank_account_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: counterparty_card counterparty_card_counterparty_type_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.counterparty_card
    ADD CONSTRAINT counterparty_card_counterparty_type_id_fkey FOREIGN KEY (counterparty_type_id) REFERENCES public.cmn_counterparty_type(id);


--
-- Name: counterparty_card counterparty_card_district_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.counterparty_card
    ADD CONSTRAINT counterparty_card_district_id_fkey FOREIGN KEY (district_id) REFERENCES public.cmn_district(id);


--
-- Name: counterparty_card counterparty_card_organization_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.counterparty_card
    ADD CONSTRAINT counterparty_card_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);


--
-- Name: counterparty_card counterparty_card_region_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.counterparty_card
    ADD CONSTRAINT counterparty_card_region_id_fkey FOREIGN KEY (region_id) REFERENCES public.cmn_region(id);


--
-- Name: counterparty_card counterparty_card_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.counterparty_card
    ADD CONSTRAINT counterparty_card_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: counterparty_contact counterparty_contact_counterparty_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.counterparty_contact
    ADD CONSTRAINT counterparty_contact_counterparty_id_fkey FOREIGN KEY (counterparty_id) REFERENCES public.counterparty_card(id);


--
-- Name: counterparty_contact counterparty_contact_organization_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.counterparty_contact
    ADD CONSTRAINT counterparty_contact_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);


--
-- Name: counterparty_contact counterparty_contact_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.counterparty_contact
    ADD CONSTRAINT counterparty_contact_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: counterparty_reg_balance counterparty_reg_balance_counterparty_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.counterparty_reg_balance
    ADD CONSTRAINT counterparty_reg_balance_counterparty_id_fkey FOREIGN KEY (counterparty_id) REFERENCES public.counterparty_card(id);


--
-- Name: counterparty_reg_balance counterparty_reg_balance_currency_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.counterparty_reg_balance
    ADD CONSTRAINT counterparty_reg_balance_currency_id_fkey FOREIGN KEY (currency_id) REFERENCES public.cmn_currency(id);


--
-- Name: counterparty_reg_balance counterparty_reg_balance_document_type_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.counterparty_reg_balance
    ADD CONSTRAINT counterparty_reg_balance_document_type_id_fkey FOREIGN KEY (document_type_id) REFERENCES public.cmn_document_type(id);


--
-- Name: counterparty_reg_balance counterparty_reg_balance_operation_type_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.counterparty_reg_balance
    ADD CONSTRAINT counterparty_reg_balance_operation_type_id_fkey FOREIGN KEY (operation_type_id) REFERENCES public.cmn_operation_type(id);


--
-- Name: counterparty_reg_balance counterparty_reg_balance_organization_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.counterparty_reg_balance
    ADD CONSTRAINT counterparty_reg_balance_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);


--
-- Name: counterparty_reg_balance counterparty_reg_balance_posting_batch_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.counterparty_reg_balance
    ADD CONSTRAINT counterparty_reg_balance_posting_batch_id_fkey FOREIGN KEY (posting_batch_id) REFERENCES public.acc_posting_batch(id);


--
-- Name: acc_posting_rule_line fk_acc_posting_rule_line_credit_alias; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_posting_rule_line
    ADD CONSTRAINT fk_acc_posting_rule_line_credit_alias FOREIGN KEY (credit_alias_id) REFERENCES public.acc_posting_alias(id);


--
-- Name: acc_posting_rule_line fk_acc_posting_rule_line_debit_alias; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.acc_posting_rule_line
    ADD CONSTRAINT fk_acc_posting_rule_line_debit_alias FOREIGN KEY (debit_alias_id) REFERENCES public.acc_posting_alias(id);


--
-- Name: bank_operation fk_bank_operation_payment_purpose; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.bank_operation
    ADD CONSTRAINT fk_bank_operation_payment_purpose FOREIGN KEY (payment_purpose_id) REFERENCES public.acc_payment_purpose(id);


--
-- Name: cash_operation fk_cash_operation_payment_purpose; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cash_operation
    ADD CONSTRAINT fk_cash_operation_payment_purpose FOREIGN KEY (payment_purpose_id) REFERENCES public.acc_payment_purpose(id);


--
-- Name: inv_inventory_adjustment_doc inv_inventory_adjustment_doc_cancelled_by_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_inventory_adjustment_doc
    ADD CONSTRAINT inv_inventory_adjustment_doc_cancelled_by_user_id_fkey FOREIGN KEY (cancelled_by_user_id) REFERENCES public.sys_user(id);


--
-- Name: inv_inventory_adjustment_doc inv_inventory_adjustment_doc_organization_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_inventory_adjustment_doc
    ADD CONSTRAINT inv_inventory_adjustment_doc_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);


--
-- Name: inv_inventory_adjustment_doc inv_inventory_adjustment_doc_posted_by_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_inventory_adjustment_doc
    ADD CONSTRAINT inv_inventory_adjustment_doc_posted_by_user_id_fkey FOREIGN KEY (posted_by_user_id) REFERENCES public.sys_user(id);


--
-- Name: inv_inventory_adjustment_doc inv_inventory_adjustment_doc_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_inventory_adjustment_doc
    ADD CONSTRAINT inv_inventory_adjustment_doc_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: inv_inventory_adjustment_doc inv_inventory_adjustment_doc_status_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_inventory_adjustment_doc
    ADD CONSTRAINT inv_inventory_adjustment_doc_status_id_fkey FOREIGN KEY (status_id) REFERENCES public.cmn_document_status(id);


--
-- Name: inv_inventory_adjustment_doc_table inv_inventory_adjustment_doc_table_owner_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_inventory_adjustment_doc_table
    ADD CONSTRAINT inv_inventory_adjustment_doc_table_owner_id_fkey FOREIGN KEY (owner_id) REFERENCES public.inv_inventory_adjustment_line(id) ON DELETE CASCADE;


--
-- Name: inv_inventory_adjustment_doc_table inv_inventory_adjustment_doc_table_product_table_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_inventory_adjustment_doc_table
    ADD CONSTRAINT inv_inventory_adjustment_doc_table_product_table_id_fkey FOREIGN KEY (product_table_id) REFERENCES public.inv_product_table(id);


--
-- Name: inv_inventory_adjustment_doc inv_inventory_adjustment_doc_warehouse_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_inventory_adjustment_doc
    ADD CONSTRAINT inv_inventory_adjustment_doc_warehouse_id_fkey FOREIGN KEY (warehouse_id) REFERENCES public.inv_warehouse(id);


--
-- Name: inv_inventory_adjustment_line inv_inventory_adjustment_line_owner_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_inventory_adjustment_line
    ADD CONSTRAINT inv_inventory_adjustment_line_owner_id_fkey FOREIGN KEY (owner_id) REFERENCES public.inv_inventory_adjustment_doc(id) ON DELETE CASCADE;


--
-- Name: inv_inventory_adjustment_line inv_inventory_adjustment_line_product_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_inventory_adjustment_line
    ADD CONSTRAINT inv_inventory_adjustment_line_product_id_fkey FOREIGN KEY (product_id) REFERENCES public.inv_product(id);


--
-- Name: inv_inventory_adjustment_line inv_inventory_adjustment_line_unit_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_inventory_adjustment_line
    ADD CONSTRAINT inv_inventory_adjustment_line_unit_id_fkey FOREIGN KEY (unit_id) REFERENCES public.cmn_unit(id);


--
-- Name: inv_inventory_count_doc inv_inventory_count_doc_cancelled_by_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_inventory_count_doc
    ADD CONSTRAINT inv_inventory_count_doc_cancelled_by_user_id_fkey FOREIGN KEY (cancelled_by_user_id) REFERENCES public.sys_user(id);


--
-- Name: inv_inventory_count_doc inv_inventory_count_doc_count_completed_by_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_inventory_count_doc
    ADD CONSTRAINT inv_inventory_count_doc_count_completed_by_user_id_fkey FOREIGN KEY (count_completed_by_user_id) REFERENCES public.sys_user(id);


--
-- Name: inv_inventory_count_doc inv_inventory_count_doc_negative_adjustment_doc_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_inventory_count_doc
    ADD CONSTRAINT inv_inventory_count_doc_negative_adjustment_doc_id_fkey FOREIGN KEY (negative_adjustment_doc_id) REFERENCES public.inv_inventory_adjustment_doc(id);


--
-- Name: inv_inventory_count_doc inv_inventory_count_doc_organization_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_inventory_count_doc
    ADD CONSTRAINT inv_inventory_count_doc_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);


--
-- Name: inv_inventory_count_doc inv_inventory_count_doc_positive_adjustment_doc_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_inventory_count_doc
    ADD CONSTRAINT inv_inventory_count_doc_positive_adjustment_doc_id_fkey FOREIGN KEY (positive_adjustment_doc_id) REFERENCES public.inv_inventory_adjustment_doc(id);


--
-- Name: inv_inventory_count_doc inv_inventory_count_doc_posted_by_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_inventory_count_doc
    ADD CONSTRAINT inv_inventory_count_doc_posted_by_user_id_fkey FOREIGN KEY (posted_by_user_id) REFERENCES public.sys_user(id);


--
-- Name: inv_inventory_count_doc inv_inventory_count_doc_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_inventory_count_doc
    ADD CONSTRAINT inv_inventory_count_doc_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: inv_inventory_count_doc inv_inventory_count_doc_status_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_inventory_count_doc
    ADD CONSTRAINT inv_inventory_count_doc_status_id_fkey FOREIGN KEY (status_id) REFERENCES public.cmn_document_status(id);


--
-- Name: inv_inventory_count_doc_table inv_inventory_count_doc_table_owner_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_inventory_count_doc_table
    ADD CONSTRAINT inv_inventory_count_doc_table_owner_id_fkey FOREIGN KEY (owner_id) REFERENCES public.inv_inventory_count_line(id) ON DELETE CASCADE;


--
-- Name: inv_inventory_count_doc_table inv_inventory_count_doc_table_product_table_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_inventory_count_doc_table
    ADD CONSTRAINT inv_inventory_count_doc_table_product_table_id_fkey FOREIGN KEY (product_table_id) REFERENCES public.inv_product_table(id);


--
-- Name: inv_inventory_count_doc inv_inventory_count_doc_warehouse_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_inventory_count_doc
    ADD CONSTRAINT inv_inventory_count_doc_warehouse_id_fkey FOREIGN KEY (warehouse_id) REFERENCES public.inv_warehouse(id);


--
-- Name: inv_inventory_count_line inv_inventory_count_line_owner_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_inventory_count_line
    ADD CONSTRAINT inv_inventory_count_line_owner_id_fkey FOREIGN KEY (owner_id) REFERENCES public.inv_inventory_count_doc(id) ON DELETE CASCADE;


--
-- Name: inv_inventory_count_line inv_inventory_count_line_product_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_inventory_count_line
    ADD CONSTRAINT inv_inventory_count_line_product_id_fkey FOREIGN KEY (product_id) REFERENCES public.inv_product(id);


--
-- Name: inv_inventory_count_line inv_inventory_count_line_unit_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_inventory_count_line
    ADD CONSTRAINT inv_inventory_count_line_unit_id_fkey FOREIGN KEY (unit_id) REFERENCES public.cmn_unit(id);


--
-- Name: inv_product inv_product_default_vat_rate_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_product
    ADD CONSTRAINT inv_product_default_vat_rate_id_fkey FOREIGN KEY (default_vat_rate_id) REFERENCES public.cmn_vat_rate(id);


--
-- Name: inv_product_group inv_product_group_organization_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_product_group
    ADD CONSTRAINT inv_product_group_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);


--
-- Name: inv_product_group inv_product_group_parent_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_product_group
    ADD CONSTRAINT inv_product_group_parent_id_fkey FOREIGN KEY (parent_id) REFERENCES public.inv_product_group(id);


--
-- Name: inv_product_group inv_product_group_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_product_group
    ADD CONSTRAINT inv_product_group_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: inv_product inv_product_organization_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_product
    ADD CONSTRAINT inv_product_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);


--
-- Name: inv_product_price inv_product_price_currency_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_product_price
    ADD CONSTRAINT inv_product_price_currency_id_fkey FOREIGN KEY (currency_id) REFERENCES public.cmn_currency(id);


--
-- Name: inv_product_price inv_product_price_organization_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_product_price
    ADD CONSTRAINT inv_product_price_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);


--
-- Name: inv_product_price inv_product_price_price_type_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_product_price
    ADD CONSTRAINT inv_product_price_price_type_id_fkey FOREIGN KEY (price_type_id) REFERENCES public.cmn_product_price_type(id);


--
-- Name: inv_product_price inv_product_price_product_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_product_price
    ADD CONSTRAINT inv_product_price_product_id_fkey FOREIGN KEY (product_id) REFERENCES public.inv_product(id);


--
-- Name: inv_product_price inv_product_price_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_product_price
    ADD CONSTRAINT inv_product_price_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: inv_product_price inv_product_price_unit_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_product_price
    ADD CONSTRAINT inv_product_price_unit_id_fkey FOREIGN KEY (unit_id) REFERENCES public.cmn_unit(id);


--
-- Name: inv_product inv_product_product_group_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_product
    ADD CONSTRAINT inv_product_product_group_id_fkey FOREIGN KEY (product_group_id) REFERENCES public.inv_product_group(id);


--
-- Name: inv_product inv_product_product_type_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_product
    ADD CONSTRAINT inv_product_product_type_id_fkey FOREIGN KEY (product_type_id) REFERENCES public.cmn_product_type(id);


--
-- Name: inv_product inv_product_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_product
    ADD CONSTRAINT inv_product_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: inv_product_table inv_product_table_current_warehouse_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_product_table
    ADD CONSTRAINT inv_product_table_current_warehouse_id_fkey FOREIGN KEY (current_warehouse_id) REFERENCES public.inv_warehouse(id);


--
-- Name: inv_product_table inv_product_table_organization_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_product_table
    ADD CONSTRAINT inv_product_table_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);


--
-- Name: inv_product_table inv_product_table_product_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_product_table
    ADD CONSTRAINT inv_product_table_product_id_fkey FOREIGN KEY (product_id) REFERENCES public.inv_product(id);


--
-- Name: inv_product_table inv_product_table_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_product_table
    ADD CONSTRAINT inv_product_table_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: inv_product_table inv_product_table_status_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_product_table
    ADD CONSTRAINT inv_product_table_status_id_fkey FOREIGN KEY (status_id) REFERENCES public.cmn_product_table_status(id);


--
-- Name: inv_product inv_product_unit_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_product
    ADD CONSTRAINT inv_product_unit_id_fkey FOREIGN KEY (unit_id) REFERENCES public.cmn_unit(id);


--
-- Name: inv_reg_balance inv_reg_balance_document_type_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_reg_balance
    ADD CONSTRAINT inv_reg_balance_document_type_id_fkey FOREIGN KEY (document_type_id) REFERENCES public.cmn_document_type(id);


--
-- Name: inv_reg_balance inv_reg_balance_operation_type_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_reg_balance
    ADD CONSTRAINT inv_reg_balance_operation_type_id_fkey FOREIGN KEY (operation_type_id) REFERENCES public.cmn_operation_type(id);


--
-- Name: inv_reg_balance inv_reg_balance_organization_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_reg_balance
    ADD CONSTRAINT inv_reg_balance_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);


--
-- Name: inv_reg_balance inv_reg_balance_posting_batch_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_reg_balance
    ADD CONSTRAINT inv_reg_balance_posting_batch_id_fkey FOREIGN KEY (posting_batch_id) REFERENCES public.acc_posting_batch(id);


--
-- Name: inv_reg_balance inv_reg_balance_product_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_reg_balance
    ADD CONSTRAINT inv_reg_balance_product_id_fkey FOREIGN KEY (product_id) REFERENCES public.inv_product(id);


--
-- Name: inv_reg_balance inv_reg_balance_product_table_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_reg_balance
    ADD CONSTRAINT inv_reg_balance_product_table_id_fkey FOREIGN KEY (product_table_id) REFERENCES public.inv_product_table(id);


--
-- Name: inv_reg_balance inv_reg_balance_warehouse_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_reg_balance
    ADD CONSTRAINT inv_reg_balance_warehouse_id_fkey FOREIGN KEY (warehouse_id) REFERENCES public.inv_warehouse(id);


--
-- Name: inv_transfer_doc inv_transfer_doc_cancelled_by_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_transfer_doc
    ADD CONSTRAINT inv_transfer_doc_cancelled_by_user_id_fkey FOREIGN KEY (cancelled_by_user_id) REFERENCES public.sys_user(id);


--
-- Name: inv_transfer_doc inv_transfer_doc_destination_warehouse_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_transfer_doc
    ADD CONSTRAINT inv_transfer_doc_destination_warehouse_id_fkey FOREIGN KEY (destination_warehouse_id) REFERENCES public.inv_warehouse(id);


--
-- Name: inv_transfer_doc inv_transfer_doc_organization_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_transfer_doc
    ADD CONSTRAINT inv_transfer_doc_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);


--
-- Name: inv_transfer_doc inv_transfer_doc_posted_by_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_transfer_doc
    ADD CONSTRAINT inv_transfer_doc_posted_by_user_id_fkey FOREIGN KEY (posted_by_user_id) REFERENCES public.sys_user(id);


--
-- Name: inv_transfer_doc inv_transfer_doc_source_warehouse_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_transfer_doc
    ADD CONSTRAINT inv_transfer_doc_source_warehouse_id_fkey FOREIGN KEY (source_warehouse_id) REFERENCES public.inv_warehouse(id);


--
-- Name: inv_transfer_doc inv_transfer_doc_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_transfer_doc
    ADD CONSTRAINT inv_transfer_doc_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: inv_transfer_doc inv_transfer_doc_status_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_transfer_doc
    ADD CONSTRAINT inv_transfer_doc_status_id_fkey FOREIGN KEY (status_id) REFERENCES public.cmn_document_status(id);


--
-- Name: inv_transfer_doc_table inv_transfer_doc_table_destination_warehouse_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_transfer_doc_table
    ADD CONSTRAINT inv_transfer_doc_table_destination_warehouse_id_fkey FOREIGN KEY (destination_warehouse_id) REFERENCES public.inv_warehouse(id);


--
-- Name: inv_transfer_doc_table inv_transfer_doc_table_owner_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_transfer_doc_table
    ADD CONSTRAINT inv_transfer_doc_table_owner_id_fkey FOREIGN KEY (owner_id) REFERENCES public.inv_transfer_line(id) ON DELETE CASCADE;


--
-- Name: inv_transfer_doc_table inv_transfer_doc_table_product_table_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_transfer_doc_table
    ADD CONSTRAINT inv_transfer_doc_table_product_table_id_fkey FOREIGN KEY (product_table_id) REFERENCES public.inv_product_table(id);


--
-- Name: inv_transfer_doc_table inv_transfer_doc_table_source_warehouse_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_transfer_doc_table
    ADD CONSTRAINT inv_transfer_doc_table_source_warehouse_id_fkey FOREIGN KEY (source_warehouse_id) REFERENCES public.inv_warehouse(id);


--
-- Name: inv_transfer_line inv_transfer_line_owner_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_transfer_line
    ADD CONSTRAINT inv_transfer_line_owner_id_fkey FOREIGN KEY (owner_id) REFERENCES public.inv_transfer_doc(id) ON DELETE CASCADE;


--
-- Name: inv_transfer_line inv_transfer_line_product_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_transfer_line
    ADD CONSTRAINT inv_transfer_line_product_id_fkey FOREIGN KEY (product_id) REFERENCES public.inv_product(id);


--
-- Name: inv_transfer_line inv_transfer_line_unit_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_transfer_line
    ADD CONSTRAINT inv_transfer_line_unit_id_fkey FOREIGN KEY (unit_id) REFERENCES public.cmn_unit(id);


--
-- Name: inv_warehouse inv_warehouse_branch_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_warehouse
    ADD CONSTRAINT inv_warehouse_branch_id_fkey FOREIGN KEY (branch_id) REFERENCES public.org_branch(id);


--
-- Name: inv_warehouse inv_warehouse_organization_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_warehouse
    ADD CONSTRAINT inv_warehouse_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);


--
-- Name: inv_warehouse inv_warehouse_responsible_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_warehouse
    ADD CONSTRAINT inv_warehouse_responsible_user_id_fkey FOREIGN KEY (responsible_user_id) REFERENCES public.sys_user(id);


--
-- Name: inv_warehouse inv_warehouse_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.inv_warehouse
    ADD CONSTRAINT inv_warehouse_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: money_reg_balance money_reg_balance_currency_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.money_reg_balance
    ADD CONSTRAINT money_reg_balance_currency_id_fkey FOREIGN KEY (currency_id) REFERENCES public.cmn_currency(id);


--
-- Name: money_reg_balance money_reg_balance_document_type_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.money_reg_balance
    ADD CONSTRAINT money_reg_balance_document_type_id_fkey FOREIGN KEY (document_type_id) REFERENCES public.cmn_document_type(id);


--
-- Name: money_reg_balance money_reg_balance_operation_type_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.money_reg_balance
    ADD CONSTRAINT money_reg_balance_operation_type_id_fkey FOREIGN KEY (operation_type_id) REFERENCES public.cmn_operation_type(id);


--
-- Name: money_reg_balance money_reg_balance_organization_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.money_reg_balance
    ADD CONSTRAINT money_reg_balance_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);


--
-- Name: money_reg_balance money_reg_balance_posting_batch_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.money_reg_balance
    ADD CONSTRAINT money_reg_balance_posting_batch_id_fkey FOREIGN KEY (posting_batch_id) REFERENCES public.acc_posting_batch(id);


--
-- Name: org_bank_account org_bank_account_bank_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_bank_account
    ADD CONSTRAINT org_bank_account_bank_id_fkey FOREIGN KEY (bank_id) REFERENCES public.cmn_bank(id);


--
-- Name: org_bank_account org_bank_account_currency_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_bank_account
    ADD CONSTRAINT org_bank_account_currency_id_fkey FOREIGN KEY (currency_id) REFERENCES public.cmn_currency(id);


--
-- Name: org_bank_account org_bank_account_organization_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_bank_account
    ADD CONSTRAINT org_bank_account_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);


--
-- Name: org_bank_account org_bank_account_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_bank_account
    ADD CONSTRAINT org_bank_account_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: org_branch org_branch_district_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_branch
    ADD CONSTRAINT org_branch_district_id_fkey FOREIGN KEY (district_id) REFERENCES public.cmn_district(id);


--
-- Name: org_branch org_branch_organization_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_branch
    ADD CONSTRAINT org_branch_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);


--
-- Name: org_branch org_branch_region_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_branch
    ADD CONSTRAINT org_branch_region_id_fkey FOREIGN KEY (region_id) REFERENCES public.cmn_region(id);


--
-- Name: org_branch org_branch_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_branch
    ADD CONSTRAINT org_branch_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: org_claim_request org_claim_request_organization_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_claim_request
    ADD CONSTRAINT org_claim_request_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);


--
-- Name: org_claim_request org_claim_request_requested_by_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_claim_request
    ADD CONSTRAINT org_claim_request_requested_by_user_id_fkey FOREIGN KEY (requested_by_user_id) REFERENCES public.sys_user(id);


--
-- Name: org_claim_request org_claim_request_reviewed_by_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_claim_request
    ADD CONSTRAINT org_claim_request_reviewed_by_user_id_fkey FOREIGN KEY (reviewed_by_user_id) REFERENCES public.sys_user(id);


--
-- Name: org_defaults org_defaults_bank_account_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_defaults
    ADD CONSTRAINT org_defaults_bank_account_id_fkey FOREIGN KEY (bank_account_id) REFERENCES public.org_bank_account(id);


--
-- Name: org_defaults org_defaults_bank_accounting_account_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_defaults
    ADD CONSTRAINT org_defaults_bank_accounting_account_id_fkey FOREIGN KEY (bank_accounting_account_id) REFERENCES public.acc_chart_account(id);


--
-- Name: org_defaults org_defaults_branch_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_defaults
    ADD CONSTRAINT org_defaults_branch_id_fkey FOREIGN KEY (branch_id) REFERENCES public.org_branch(id);


--
-- Name: org_defaults org_defaults_cash_account_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_defaults
    ADD CONSTRAINT org_defaults_cash_account_id_fkey FOREIGN KEY (cash_account_id) REFERENCES public.acc_chart_account(id);


--
-- Name: org_defaults org_defaults_cash_box_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_defaults
    ADD CONSTRAINT org_defaults_cash_box_id_fkey FOREIGN KEY (cash_box_id) REFERENCES public.cash_box(id);


--
-- Name: org_defaults org_defaults_cogs_account_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_defaults
    ADD CONSTRAINT org_defaults_cogs_account_id_fkey FOREIGN KEY (cogs_account_id) REFERENCES public.acc_chart_account(id);


--
-- Name: org_defaults org_defaults_expense_account_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_defaults
    ADD CONSTRAINT org_defaults_expense_account_id_fkey FOREIGN KEY (expense_account_id) REFERENCES public.acc_chart_account(id);


--
-- Name: org_defaults org_defaults_inventory_account_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_defaults
    ADD CONSTRAINT org_defaults_inventory_account_id_fkey FOREIGN KEY (inventory_account_id) REFERENCES public.acc_chart_account(id);


--
-- Name: org_defaults org_defaults_organization_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_defaults
    ADD CONSTRAINT org_defaults_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);


--
-- Name: org_defaults org_defaults_payable_account_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_defaults
    ADD CONSTRAINT org_defaults_payable_account_id_fkey FOREIGN KEY (payable_account_id) REFERENCES public.acc_chart_account(id);


--
-- Name: org_defaults org_defaults_receivable_account_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_defaults
    ADD CONSTRAINT org_defaults_receivable_account_id_fkey FOREIGN KEY (receivable_account_id) REFERENCES public.acc_chart_account(id);


--
-- Name: org_defaults org_defaults_revenue_account_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_defaults
    ADD CONSTRAINT org_defaults_revenue_account_id_fkey FOREIGN KEY (revenue_account_id) REFERENCES public.acc_chart_account(id);


--
-- Name: org_defaults org_defaults_warehouse_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_defaults
    ADD CONSTRAINT org_defaults_warehouse_id_fkey FOREIGN KEY (warehouse_id) REFERENCES public.inv_warehouse(id);


--
-- Name: org_department org_department_branch_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_department
    ADD CONSTRAINT org_department_branch_id_fkey FOREIGN KEY (branch_id) REFERENCES public.org_branch(id);


--
-- Name: org_department org_department_organization_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_department
    ADD CONSTRAINT org_department_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);


--
-- Name: org_department org_department_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_department
    ADD CONSTRAINT org_department_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: org_organization_config org_organization_config_accounting_policy_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_organization_config
    ADD CONSTRAINT org_organization_config_accounting_policy_id_fkey FOREIGN KEY (accounting_policy_id) REFERENCES public.acc_accounting_policy(id);


--
-- Name: org_organization_config org_organization_config_base_currency_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_organization_config
    ADD CONSTRAINT org_organization_config_base_currency_id_fkey FOREIGN KEY (base_currency_id) REFERENCES public.cmn_currency(id);


--
-- Name: org_organization_config org_organization_config_organization_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_organization_config
    ADD CONSTRAINT org_organization_config_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);


--
-- Name: org_organization org_organization_default_language_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_organization
    ADD CONSTRAINT org_organization_default_language_id_fkey FOREIGN KEY (default_language_id) REFERENCES public.cmn_language(id);


--
-- Name: org_organization org_organization_district_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_organization
    ADD CONSTRAINT org_organization_district_id_fkey FOREIGN KEY (district_id) REFERENCES public.cmn_district(id);


--
-- Name: org_organization org_organization_region_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_organization
    ADD CONSTRAINT org_organization_region_id_fkey FOREIGN KEY (region_id) REFERENCES public.cmn_region(id);


--
-- Name: org_organization org_organization_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_organization
    ADD CONSTRAINT org_organization_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: org_organization org_organization_tenant_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_organization
    ADD CONSTRAINT org_organization_tenant_id_fkey FOREIGN KEY (tenant_id) REFERENCES public.platform_tenant(id);


--
-- Name: org_position org_position_organization_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_position
    ADD CONSTRAINT org_position_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);


--
-- Name: org_position org_position_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_position
    ADD CONSTRAINT org_position_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: org_setup_state org_setup_state_organization_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_setup_state
    ADD CONSTRAINT org_setup_state_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);


--
-- Name: org_tax_settings org_tax_settings_organization_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_tax_settings
    ADD CONSTRAINT org_tax_settings_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);


--
-- Name: org_tax_settings org_tax_settings_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_tax_settings
    ADD CONSTRAINT org_tax_settings_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: org_tax_settings org_tax_settings_tax_type_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_tax_settings
    ADD CONSTRAINT org_tax_settings_tax_type_id_fkey FOREIGN KEY (tax_type_id) REFERENCES public.cmn_tax_type(id);


--
-- Name: org_user_invitation org_user_invitation_accepted_by_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_user_invitation
    ADD CONSTRAINT org_user_invitation_accepted_by_user_id_fkey FOREIGN KEY (accepted_by_user_id) REFERENCES public.sys_user(id);


--
-- Name: org_user_invitation org_user_invitation_invited_by_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_user_invitation
    ADD CONSTRAINT org_user_invitation_invited_by_user_id_fkey FOREIGN KEY (invited_by_user_id) REFERENCES public.sys_user(id);


--
-- Name: org_user_invitation org_user_invitation_organization_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_user_invitation
    ADD CONSTRAINT org_user_invitation_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);


--
-- Name: org_user_invitation org_user_invitation_role_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_user_invitation
    ADD CONSTRAINT org_user_invitation_role_id_fkey FOREIGN KEY (role_id) REFERENCES public.sys_role(id);


--
-- Name: org_user_invitation org_user_invitation_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.org_user_invitation
    ADD CONSTRAINT org_user_invitation_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: platform_tenant platform_tenant_owner_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.platform_tenant
    ADD CONSTRAINT platform_tenant_owner_user_id_fkey FOREIGN KEY (owner_user_id) REFERENCES public.sys_user(id);


--
-- Name: platform_tenant platform_tenant_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.platform_tenant
    ADD CONSTRAINT platform_tenant_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: pur_doc pur_doc_cancelled_by_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.pur_doc
    ADD CONSTRAINT pur_doc_cancelled_by_user_id_fkey FOREIGN KEY (cancelled_by_user_id) REFERENCES public.sys_user(id);


--
-- Name: pur_doc pur_doc_contract_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.pur_doc
    ADD CONSTRAINT pur_doc_contract_id_fkey FOREIGN KEY (contract_id) REFERENCES public.cmn_contract(id);


--
-- Name: pur_doc pur_doc_counterparty_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.pur_doc
    ADD CONSTRAINT pur_doc_counterparty_id_fkey FOREIGN KEY (counterparty_id) REFERENCES public.counterparty_card(id);


--
-- Name: pur_doc pur_doc_currency_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.pur_doc
    ADD CONSTRAINT pur_doc_currency_id_fkey FOREIGN KEY (currency_id) REFERENCES public.cmn_currency(id);


--
-- Name: pur_doc pur_doc_organization_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.pur_doc
    ADD CONSTRAINT pur_doc_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);


--
-- Name: pur_doc pur_doc_posted_by_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.pur_doc
    ADD CONSTRAINT pur_doc_posted_by_user_id_fkey FOREIGN KEY (posted_by_user_id) REFERENCES public.sys_user(id);


--
-- Name: pur_doc_product pur_doc_product_owner_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.pur_doc_product
    ADD CONSTRAINT pur_doc_product_owner_id_fkey FOREIGN KEY (owner_id) REFERENCES public.pur_doc(id);


--
-- Name: pur_doc_product pur_doc_product_product_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.pur_doc_product
    ADD CONSTRAINT pur_doc_product_product_id_fkey FOREIGN KEY (product_id) REFERENCES public.inv_product(id);


--
-- Name: pur_doc_product pur_doc_product_unit_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.pur_doc_product
    ADD CONSTRAINT pur_doc_product_unit_id_fkey FOREIGN KEY (unit_id) REFERENCES public.cmn_unit(id);


--
-- Name: pur_doc_product pur_doc_product_vat_rate_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.pur_doc_product
    ADD CONSTRAINT pur_doc_product_vat_rate_id_fkey FOREIGN KEY (vat_rate_id) REFERENCES public.cmn_vat_rate(id);


--
-- Name: pur_doc pur_doc_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.pur_doc
    ADD CONSTRAINT pur_doc_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: pur_doc pur_doc_status_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.pur_doc
    ADD CONSTRAINT pur_doc_status_id_fkey FOREIGN KEY (status_id) REFERENCES public.cmn_document_status(id);


--
-- Name: pur_doc_table pur_doc_table_owner_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.pur_doc_table
    ADD CONSTRAINT pur_doc_table_owner_id_fkey FOREIGN KEY (owner_id) REFERENCES public.pur_doc_product(id);


--
-- Name: pur_doc_table pur_doc_table_product_table_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.pur_doc_table
    ADD CONSTRAINT pur_doc_table_product_table_id_fkey FOREIGN KEY (product_table_id) REFERENCES public.inv_product_table(id);


--
-- Name: pur_doc_table pur_doc_table_vat_rate_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.pur_doc_table
    ADD CONSTRAINT pur_doc_table_vat_rate_id_fkey FOREIGN KEY (vat_rate_id) REFERENCES public.cmn_vat_rate(id);


--
-- Name: pur_doc pur_doc_warehouse_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.pur_doc
    ADD CONSTRAINT pur_doc_warehouse_id_fkey FOREIGN KEY (warehouse_id) REFERENCES public.inv_warehouse(id);


--
-- Name: sale_condition sale_condition_costing_method_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sale_condition
    ADD CONSTRAINT sale_condition_costing_method_id_fkey FOREIGN KEY (costing_method_id) REFERENCES public.cmn_costing_method(id);


--
-- Name: sale_condition sale_condition_organization_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sale_condition
    ADD CONSTRAINT sale_condition_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);


--
-- Name: sale_condition sale_condition_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sale_condition
    ADD CONSTRAINT sale_condition_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: sale_condition sale_condition_vat_rate_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sale_condition
    ADD CONSTRAINT sale_condition_vat_rate_id_fkey FOREIGN KEY (vat_rate_id) REFERENCES public.cmn_vat_rate(id);


--
-- Name: sale_doc sale_doc_cancelled_by_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sale_doc
    ADD CONSTRAINT sale_doc_cancelled_by_user_id_fkey FOREIGN KEY (cancelled_by_user_id) REFERENCES public.sys_user(id);


--
-- Name: sale_doc sale_doc_contract_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sale_doc
    ADD CONSTRAINT sale_doc_contract_id_fkey FOREIGN KEY (contract_id) REFERENCES public.cmn_contract(id);


--
-- Name: sale_doc sale_doc_counterparty_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sale_doc
    ADD CONSTRAINT sale_doc_counterparty_id_fkey FOREIGN KEY (counterparty_id) REFERENCES public.counterparty_card(id);


--
-- Name: sale_doc sale_doc_currency_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sale_doc
    ADD CONSTRAINT sale_doc_currency_id_fkey FOREIGN KEY (currency_id) REFERENCES public.cmn_currency(id);


--
-- Name: sale_doc sale_doc_organization_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sale_doc
    ADD CONSTRAINT sale_doc_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);


--
-- Name: sale_doc sale_doc_posted_by_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sale_doc
    ADD CONSTRAINT sale_doc_posted_by_user_id_fkey FOREIGN KEY (posted_by_user_id) REFERENCES public.sys_user(id);


--
-- Name: sale_doc_product sale_doc_product_owner_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sale_doc_product
    ADD CONSTRAINT sale_doc_product_owner_id_fkey FOREIGN KEY (owner_id) REFERENCES public.sale_doc(id) ON DELETE CASCADE;


--
-- Name: sale_doc_product sale_doc_product_product_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sale_doc_product
    ADD CONSTRAINT sale_doc_product_product_id_fkey FOREIGN KEY (product_id) REFERENCES public.inv_product(id);


--
-- Name: sale_doc_product sale_doc_product_unit_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sale_doc_product
    ADD CONSTRAINT sale_doc_product_unit_id_fkey FOREIGN KEY (unit_id) REFERENCES public.cmn_unit(id);


--
-- Name: sale_doc_product sale_doc_product_vat_rate_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sale_doc_product
    ADD CONSTRAINT sale_doc_product_vat_rate_id_fkey FOREIGN KEY (vat_rate_id) REFERENCES public.cmn_vat_rate(id);


--
-- Name: sale_doc sale_doc_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sale_doc
    ADD CONSTRAINT sale_doc_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: sale_doc sale_doc_status_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sale_doc
    ADD CONSTRAINT sale_doc_status_id_fkey FOREIGN KEY (status_id) REFERENCES public.cmn_document_status(id);


--
-- Name: sale_doc_table sale_doc_table_owner_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sale_doc_table
    ADD CONSTRAINT sale_doc_table_owner_id_fkey FOREIGN KEY (owner_id) REFERENCES public.sale_doc_product(id);


--
-- Name: sale_doc_table sale_doc_table_product_table_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sale_doc_table
    ADD CONSTRAINT sale_doc_table_product_table_id_fkey FOREIGN KEY (product_table_id) REFERENCES public.inv_product_table(id);


--
-- Name: sale_doc_table sale_doc_table_vat_rate_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sale_doc_table
    ADD CONSTRAINT sale_doc_table_vat_rate_id_fkey FOREIGN KEY (vat_rate_id) REFERENCES public.cmn_vat_rate(id);


--
-- Name: sale_doc sale_doc_warehouse_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sale_doc
    ADD CONSTRAINT sale_doc_warehouse_id_fkey FOREIGN KEY (warehouse_id) REFERENCES public.inv_warehouse(id);


--
-- Name: sys_email_verification_token sys_email_verification_token_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sys_email_verification_token
    ADD CONSTRAINT sys_email_verification_token_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.sys_user(id);


--
-- Name: sys_module sys_module_parent_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sys_module
    ADD CONSTRAINT sys_module_parent_id_fkey FOREIGN KEY (parent_id) REFERENCES public.sys_module(id);


--
-- Name: sys_module sys_module_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sys_module
    ADD CONSTRAINT sys_module_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: sys_module sys_module_sub_group_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sys_module
    ADD CONSTRAINT sys_module_sub_group_id_fkey FOREIGN KEY (sub_group_id) REFERENCES public.sys_module_sub_group(id);


--
-- Name: sys_password_reset_token sys_password_reset_token_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sys_password_reset_token
    ADD CONSTRAINT sys_password_reset_token_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.sys_user(id);


--
-- Name: sys_refresh_token sys_refresh_token_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sys_refresh_token
    ADD CONSTRAINT sys_refresh_token_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.sys_user(id);


--
-- Name: sys_role_module sys_role_module_module_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sys_role_module
    ADD CONSTRAINT sys_role_module_module_id_fkey FOREIGN KEY (module_id) REFERENCES public.sys_module(id);


--
-- Name: sys_role_module sys_role_module_role_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sys_role_module
    ADD CONSTRAINT sys_role_module_role_id_fkey FOREIGN KEY (role_id) REFERENCES public.sys_role(id);


--
-- Name: sys_role sys_role_organization_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sys_role
    ADD CONSTRAINT sys_role_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);


--
-- Name: sys_role sys_role_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sys_role
    ADD CONSTRAINT sys_role_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: sys_user sys_user_language_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sys_user
    ADD CONSTRAINT sys_user_language_id_fkey FOREIGN KEY (language_id) REFERENCES public.cmn_language(id);


--
-- Name: sys_user sys_user_organization_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sys_user
    ADD CONSTRAINT sys_user_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id);


--
-- Name: sys_user_organization sys_user_organization_invited_by_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sys_user_organization
    ADD CONSTRAINT sys_user_organization_invited_by_user_id_fkey FOREIGN KEY (invited_by_user_id) REFERENCES public.sys_user(id);


--
-- Name: sys_user_organization sys_user_organization_organization_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sys_user_organization
    ADD CONSTRAINT sys_user_organization_organization_id_fkey FOREIGN KEY (organization_id) REFERENCES public.org_organization(id) ON DELETE CASCADE;


--
-- Name: sys_user_organization sys_user_organization_role_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sys_user_organization
    ADD CONSTRAINT sys_user_organization_role_id_fkey FOREIGN KEY (role_id) REFERENCES public.sys_role(id);


--
-- Name: sys_user_organization sys_user_organization_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sys_user_organization
    ADD CONSTRAINT sys_user_organization_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- Name: sys_user_organization sys_user_organization_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sys_user_organization
    ADD CONSTRAINT sys_user_organization_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.sys_user(id) ON DELETE CASCADE;


--
-- Name: sys_user sys_user_role_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sys_user
    ADD CONSTRAINT sys_user_role_id_fkey FOREIGN KEY (role_id) REFERENCES public.sys_role(id);


--
-- Name: sys_user sys_user_state_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sys_user
    ADD CONSTRAINT sys_user_state_id_fkey FOREIGN KEY (state_id) REFERENCES public.cmn_state(id);


--
-- PostgreSQL database dump complete
--

\unrestrict dHKCrvK9c7j3jVKobcEFvhE4FoIfEfGX7OnpzYTpHSRckBob0CsBq22rKTrmmBt

