create table counterparty_contact
(
	id serial primary key,
	organization_id int not null references org_organization(id),
	counterparty_id int not null references counterparty_card(id),
	full_name varchar(250) not null,
	phone_number varchar(50) null,
	email varchar(250) null,
	position varchar(250) null,
	comment varchar(1000) null,
	state_id smallint not null references cmn_state(id),
	created_date timestamp without time zone default now() not null
);

create index idx_counterparty_contact_organization_id on counterparty_contact (organization_id);
create index idx_counterparty_contact_counterparty_id on counterparty_contact (counterparty_id);
create index idx_counterparty_contact_state_id on counterparty_contact (state_id);
