create table counterparty_card
(
	id serial primary key,
	organization_id int not null references org_organization(id),
	counterparty_type_id smallint not null references cmn_counterparty_type(id),
	short_name varchar(250) not null,
	full_name varchar(500) null,
	inn varchar(20) null,
	phone_number varchar(50) null,
	email varchar(250) null,
	region_id int null references cmn_region(id),
	district_id int null references cmn_district(id),
	address varchar(1000) null,
	state_id smallint not null references cmn_state(id),
	created_date timestamp without time zone default now() not null
);

create index idx_counterparty_card_organization_id on counterparty_card (organization_id);
create index idx_counterparty_card_type_id on counterparty_card (counterparty_type_id);
create index idx_counterparty_card_short_name on counterparty_card (short_name);
create index idx_counterparty_card_inn on counterparty_card (inn);
create index idx_counterparty_card_region_id on counterparty_card (region_id);
create index idx_counterparty_card_district_id on counterparty_card (district_id);
create index idx_counterparty_card_state_id on counterparty_card (state_id);
