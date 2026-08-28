create table counterparty_card 
(
    id                  serial primary key,
    organization_id     int not null references org_organization(id),
    short_name          varchar(250) not null,
    full_name           varchar(500),
    inn                 varchar(20),
    phone_number        varchar(50),
    email               varchar(250),
    region_id           int references cmn_region(id),
    district_id         int references cmn_district(id),
    address             varchar(1000),
    state_id            smallint not null references cmn_state(id),
    created_date        timestamp without time zone not null default now(),
    code                varchar(100),
    is_vat_payer        boolean not null default false,
    oked                varchar(20),
    external_id         varchar(100),
    crpt_participant_id int
);

create index idx_counterparty_card_district_id on counterparty_card using btree (district_id);
create index idx_counterparty_card_inn on counterparty_card using btree (inn);
create index idx_counterparty_card_organization_id on counterparty_card using btree (organization_id);
create index idx_counterparty_card_region_id on counterparty_card using btree (region_id);
create index idx_counterparty_card_short_name on counterparty_card using btree (short_name);
create index idx_counterparty_card_state_id on counterparty_card using btree (state_id);
create index idx_counterparty_card_code on counterparty_card using btree (code);
create index idx_counterparty_card_external_id on counterparty_card using btree (external_id);
create unique index uidx_counterparty_card_org_code on counterparty_card using btree (organization_id, code) WHERE (code IS not null);


insert into counterparty_card (id, organization_id, short_name, full_name, inn, phone_number, email, region_id, district_id, address, state_id, created_date) values
    ('16', '8', 'as', 'Asta', '12132145631', '+998 99 890-08-58', null, '4', '77', 'Navoiy, Uzbekistan', '1', '2026-06-20 15:37:24.758641'),
    ('17', '8', 'aaaa', 'Shaxriddinbek', '12132145631', '+998 99 890-08-58', null, '3', '58', 'Navoiy, Uzbekistan', '1', '2026-06-20 16:03:26.590196'),
    ('18', '8', 'Artel', 'Artel', '222222222', '+998 00 222-00-22', null, '2', '51', '', '1', '2026-06-20 17:54:14.649893'),
    ('19', '8', 'Farrux Tech', 'Farrux Tech', '999888777', '+998 00 111-44-11', null, '8', '129', '', '1', '2026-06-20 18:20:49.024257'),
    ('20', '8', 'Ava', 'Avalon', '22618000562088110001', '+998 99 556-56-88', null, '4', '78', 'Navoiy, Uzbekistan', '1', '2026-06-24 18:15:21.1843'),
    ('21', '8', 'Aval', 'Avaloncha', '20208000005157348001', '+998 99 890-08-58', null, '3', '59', 'Navoiy, Uzbekistan', '1', '2026-06-25 11:14:04.759077');
