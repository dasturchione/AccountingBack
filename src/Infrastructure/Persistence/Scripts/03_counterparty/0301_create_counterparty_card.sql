
create table counterparty_card 
(
    id integer   not null,
    organization_id integer not null,
    counterparty_type_id smallint not null,
    short_name character varying(250) not null,
    full_name character varying(500),
    inn character varying(20),
    phone_number character varying(50),
    email character varying(250),
    region_id integer,
    district_id integer,
    address character varying(1000),
    state_id smallint not null,
    created_date timestamp without time zone default now() not null,
    code character varying(100),
    is_customer boolean default true not null,
    is_supplier boolean default true not null,
    is_vat_payer boolean default false not null,
    oked character varying(20),
    external_id character varying(100),
    constraint counterparty_card_pkey primary key (id),
    constraint counterparty_card_counterparty_type_id_fkey foreign key (counterparty_type_id) references cmn_counterparty_type(id),
    constraint counterparty_card_district_id_fkey foreign key (district_id) references cmn_district(id),
    constraint counterparty_card_organization_id_fkey foreign key (organization_id) references org_organization(id),
    constraint counterparty_card_region_id_fkey foreign key (region_id) references cmn_region(id),
    constraint counterparty_card_state_id_fkey foreign key (state_id) references cmn_state(id)
);

create index idx_counterparty_card_district_id on counterparty_card using btree (district_id);
create index idx_counterparty_card_inn on counterparty_card using btree (inn);
create index idx_counterparty_card_organization_id on counterparty_card using btree (organization_id);
create index idx_counterparty_card_region_id on counterparty_card using btree (region_id);
create index idx_counterparty_card_short_name on counterparty_card using btree (short_name);
create index idx_counterparty_card_state_id on counterparty_card using btree (state_id);
create index idx_counterparty_card_type_id on counterparty_card using btree (counterparty_type_id);
create index idx_counterparty_card_code on counterparty_card using btree (code);
create index idx_counterparty_card_external_id on counterparty_card using btree (external_id);
create unique index uidx_counterparty_card_org_code on counterparty_card using btree (organization_id, code) WHERE (code IS not null);


insert into counterparty_card (id, organization_id, counterparty_type_id, short_name, full_name, inn, phone_number, email, region_id, district_id, address, state_id, created_date) values
    ('16', '8', '1', 'as', 'Asta', '12132145631', '+998 99 890-08-58', null, '4', '77', 'Navoiy, Uzbekistan', '1', '2026-06-20 15:37:24.758641'),
    ('17', '8', '2', 'aaaa', 'Shaxriddinbek', '12132145631', '+998 99 890-08-58', null, '3', '58', 'Navoiy, Uzbekistan', '1', '2026-06-20 16:03:26.590196'),
    ('18', '8', '2', 'Artel', 'Artel', '222222222', '+998 00 222-00-22', null, '2', '51', '', '1', '2026-06-20 17:54:14.649893'),
    ('19', '8', '1', 'Farrux Tech', 'Farrux Tech', '999888777', '+998 00 111-44-11', null, '8', '129', '', '1', '2026-06-20 18:20:49.024257'),
    ('20', '8', '3', 'Ava', 'Avalon', '22618000562088110001', '+998 99 556-56-88', null, '4', '78', 'Navoiy, Uzbekistan', '1', '2026-06-24 18:15:21.1843'),
    ('21', '8', '1', 'Aval', 'Avaloncha', '20208000005157348001', '+998 99 890-08-58', null, '3', '59', 'Navoiy, Uzbekistan', '1', '2026-06-25 11:14:04.759077');
