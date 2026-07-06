create table pur_doc 
(
    id bigint   not null,
    organization_id integer not null,
    doc_number character varying(100) not null,
    doc_date timestamp without time zone not null,
    counterparty_id integer not null,
    warehouse_id integer not null,
    currency_id smallint not null,
    total_amount numeric(24,8) default 0 not null,
    vat_amount numeric(24,8) default 0 not null,
    final_amount numeric(24,8) default 0 not null,
    status_id smallint not null,
    comment character varying(1000),
    state_id smallint not null,
    created_date timestamp without time zone default now() not null,
    contract_id bigint,
    exchange_rate numeric(18,6) default 1 not null,
    posted_at timestamp without time zone,
    posted_by_user_id integer,
    cancelled_at timestamp without time zone,
    cancelled_by_user_id integer,
    constraint pur_doc_pkey primary key (id),
    constraint pur_doc_contract_id_fkey foreign key (contract_id) references cmn_contract(id),
    constraint pur_doc_counterparty_id_fkey foreign key (counterparty_id) references counterparty_card(id),
    constraint pur_doc_currency_id_fkey foreign key (currency_id) references cmn_currency(id),
    constraint pur_doc_organization_id_fkey foreign key (organization_id) references org_organization(id),
    constraint pur_doc_state_id_fkey foreign key (state_id) references cmn_state(id),
    constraint pur_doc_status_id_fkey foreign key (status_id) references cmn_document_status(id),
    constraint pur_doc_warehouse_id_fkey foreign key (warehouse_id) references inv_warehouse(id)
);

create index idx_pur_doc_contract_id on pur_doc using btree (contract_id);
create index idx_pur_doc_counterparty_id on pur_doc using btree (counterparty_id);
create index idx_pur_doc_doc_date on pur_doc using btree (doc_date);
create index idx_pur_doc_organization_id on pur_doc using btree (organization_id);
create index idx_pur_doc_state_id on pur_doc using btree (state_id);
create index idx_pur_doc_status_id on pur_doc using btree (status_id);
create index idx_pur_doc_warehouse_id on pur_doc using btree (warehouse_id);
create index idx_pur_doc_posted_by_user_id on pur_doc using btree (posted_by_user_id);
create index idx_pur_doc_cancelled_by_user_id on pur_doc using btree (cancelled_by_user_id);

create function set_pur_doc_number() returns trigger
    language plpgsql
    as $$
begin
    perform pg_advisory_xact_lock(hashtext('pur_doc_number'));
    new.doc_number := lpad((coalesce((select max(doc_number::bigint) from pur_doc), 100000000) + 1)::text, 9, '0');
    return new;
end;
$$;

insert into pur_doc (id, organization_id, doc_number, doc_date, counterparty_id, warehouse_id, currency_id, total_amount, vat_amount, final_amount, status_id, comment, state_id, created_date, contract_id) values
    ('92', '8', '100000074', '2026-06-27 18:05:43', '18', '7', '1', '40000.00000000', '4800.00000000', '44800.00000000', '1', null, '1', '2026-06-27 18:07:17.501272', '9'),
    ('93', '8', '100000075', '2026-06-27 18:07:17', '18', '7', '1', '10000.00000000', '1200.00000000', '11200.00000000', '1', null, '1', '2026-06-27 18:07:50.767417', '10'),
    ('94', '8', '100000076', '2026-06-29 10:06:32', '18', '7', '1', '10000.00000000', '1200.00000000', '11200.00000000', '1', null, '1', '2026-06-29 10:13:54.392765', '9'),
    ('95', '8', '100000077', '2026-06-15 14:58:00', '18', '7', '1', '33000.00000000', '3960.00000000', '36960.00000000', '1', null, '1', '2026-06-29 14:59:14.796206', '9'),
    ('96', '8', '100000078', '2026-06-29 14:59:15', '18', '7', '1', '30000.00000000', '4500.00000000', '34500.00000000', '1', null, '1', '2026-06-29 15:01:54.008454', '10'),
    ('97', '8', '100000079', '2026-06-29 17:55:00', '18', '7', '1', '20000.00000000', '2400.00000000', '22400.00000000', '1', null, '1', '2026-06-29 17:57:51.597146', '9');
