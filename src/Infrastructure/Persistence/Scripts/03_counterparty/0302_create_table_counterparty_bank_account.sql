create table counterparty_bank_account
(
	id serial primary key,
	organization_id int not null references org_organization(id),
	counterparty_id int not null references counterparty_card(id),
	bank_id int not null references cmn_bank(id),
	account_number varchar(50) not null,
	currency_id smallint not null references cmn_currency(id),
	is_main boolean not null default false,
	state_id smallint not null references cmn_state(id),
	created_date timestamp without time zone default now() not null
);

create index idx_counterparty_bank_account_organization_id on counterparty_bank_account (organization_id);
create index idx_counterparty_bank_account_counterparty_id on counterparty_bank_account (counterparty_id);
create index idx_counterparty_bank_account_bank_id on counterparty_bank_account (bank_id);
create index idx_counterparty_bank_account_currency_id on counterparty_bank_account (currency_id);
create index idx_counterparty_bank_account_state_id on counterparty_bank_account (state_id);
