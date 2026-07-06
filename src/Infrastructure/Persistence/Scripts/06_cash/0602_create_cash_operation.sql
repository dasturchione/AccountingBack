create table cash_operation (
    id bigint   not null,
    organization_id integer not null,
    cash_box_id integer not null,
    destination_cash_box_id integer,
    operation_type_id smallint not null,
    payment_type_id smallint,
    payment_purpose_id smallint,
    counterparty_id integer,
    doc_number character varying(100) not null,
    doc_date timestamp without time zone not null,
    currency_id smallint not null,
    amount numeric(18,2) not null,
    comment character varying(1000),
    status_id smallint not null,
    state_id smallint not null,
    created_date timestamp without time zone default now() not null,
    exchange_rate numeric(18,6) default 1 not null,
    posted_at timestamp without time zone,
    posted_by_user_id integer,
    cancelled_at timestamp without time zone,
    cancelled_by_user_id integer,
    constraint cash_operation_pkey primary key (id),
    constraint cash_operation_cash_box_id_fkey foreign key (cash_box_id) references cash_box(id),
    constraint cash_operation_destination_cash_box_id_fkey foreign key (destination_cash_box_id) references cash_box(id),
    constraint cash_operation_counterparty_id_fkey foreign key (counterparty_id) references counterparty_card(id),
    constraint cash_operation_currency_id_fkey foreign key (currency_id) references cmn_currency(id),
    constraint cash_operation_operation_type_id_fkey foreign key (operation_type_id) references cmn_operation_type(id),
    constraint cash_operation_organization_id_fkey foreign key (organization_id) references org_organization(id),
    constraint cash_operation_payment_type_id_fkey foreign key (payment_type_id) references cmn_payment_type(id),
    constraint cash_operation_state_id_fkey foreign key (state_id) references cmn_state(id),
    constraint cash_operation_status_id_fkey foreign key (status_id) references cmn_document_status(id),
    constraint cash_operation_posted_by_user_id_fkey foreign key (posted_by_user_id) references sys_user(id),
    constraint cash_operation_cancelled_by_user_id_fkey foreign key (cancelled_by_user_id) references sys_user(id)
);

create index idx_cash_operation_cash_box_id on cash_operation using btree (cash_box_id);
create index idx_cash_operation_destination_cash_box_id on cash_operation using btree (destination_cash_box_id);
create index idx_cash_operation_counterparty_id on cash_operation using btree (counterparty_id);
create index idx_cash_operation_doc_date on cash_operation using btree (doc_date);
create index idx_cash_operation_operation_type_id on cash_operation using btree (operation_type_id);
create index idx_cash_operation_organization_id on cash_operation using btree (organization_id);
create index idx_cash_operation_state_id on cash_operation using btree (state_id);
create index idx_cash_operation_status_id on cash_operation using btree (status_id);
create index idx_cash_operation_posted_by_user_id on cash_operation using btree (posted_by_user_id);
create index idx_cash_operation_cancelled_by_user_id on cash_operation using btree (cancelled_by_user_id);

create function set_cash_operation_doc_number() returns trigger
    language plpgsql
    as $$
begin
    perform pg_advisory_xact_lock(hashtext('cash_operation_doc_number'));
    new.doc_number := lpad((coalesce((select max(doc_number::bigint) from cash_operation), 100000000) + 1)::text, 9, '0');
    return new;
end;
$$;
