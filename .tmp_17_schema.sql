--
-- PostgreSQL database dump
--

\restrict B20TlbTlLp82GhIQjANS4c5oC4p9jYXUuYDxXXQai8CFrQwNCWDvwI9TDuoFqKE

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

\unrestrict B20TlbTlLp82GhIQjANS4c5oC4p9jYXUuYDxXXQai8CFrQwNCWDvwI9TDuoFqKE

