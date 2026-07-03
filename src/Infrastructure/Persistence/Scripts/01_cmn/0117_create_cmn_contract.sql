
create table cmn_contract (
    id bigint   not null,
    organization_id integer not null,
    counterparty_id integer not null,
    contract_number character varying(100) not null,
    contract_date timestamp without time zone not null,
    start_date timestamp without time zone,
    end_date timestamp without time zone,
    comment character varying(1000),
    state_id smallint not null,
    created_date timestamp without time zone default now() not null,
    contract_type_id smallint not null,
    constraint cmn_contract_pkey primary key (id),
    constraint cmn_contract_contract_type_id_fkey foreign key (contract_type_id) references cmn_contract_type(id),
    constraint cmn_contract_counterparty_id_fkey foreign key (counterparty_id) references counterparty_card(id),
    constraint cmn_contract_organization_id_fkey foreign key (organization_id) references org_organization(id),
    constraint cmn_contract_state_id_fkey foreign key (state_id) references cmn_state(id)
);

create function set_cmn_contract_number() returns trigger
    language plpgsql
    as $$
begin
    perform pg_advisory_xact_lock(hashtext('cmn_contract_number'));
    new.contract_number := lpad((coalesce((select max(contract_number::bigint) from cmn_contract), 100000000) + 1)::text, 9, '0');
    return new;
end;
$$;

insert into cmn_contract (id, organization_id, counterparty_id, contract_number, contract_date, start_date, end_date, comment, state_id, created_date, contract_type_id) values
    ('7', '8', '16', '100000007', '2026-06-20 15:35:42', '2026-06-20 00:00:00', '2027-08-26 23:59:59.999999', '', '1', '2026-06-20 15:39:04.060566', '2'),
    ('9', '8', '18', '100000009', '2026-05-20 12:58:18.282', '2026-05-20 00:00:00', '2026-09-20 23:59:59.999999', 'string', '1', '2026-06-20 18:00:25.144259', '1'),
    ('10', '8', '18', '100000010', '2026-06-23 05:58:06', '2026-06-23 05:58:06', '2026-07-31 06:02:00', '', '1', '2026-06-23 06:02:11.011106', '2');

create index idx_cmn_contract_contract_date on cmn_contract using btree (contract_date);

create index idx_cmn_contract_contract_type_id on cmn_contract using btree (contract_type_id);

create index idx_cmn_contract_counterparty_id on cmn_contract using btree (counterparty_id);

create unique index idx_cmn_contract_number on cmn_contract using btree (organization_id, counterparty_id, contract_number);

create index idx_cmn_contract_organization_id on cmn_contract using btree (organization_id);

create index idx_cmn_contract_state_id on cmn_contract using btree (state_id);

