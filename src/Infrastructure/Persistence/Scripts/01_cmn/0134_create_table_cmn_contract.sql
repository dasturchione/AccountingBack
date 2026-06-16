create table cmn_contract
(
	id bigserial primary key,
	organization_id int not null references org_organization(id),
	counterparty_id int not null references counterparty_card(id),
	contract_type varchar(50) not null,
	contract_number varchar(100) not null,
	contract_date timestamp without time zone not null,
	start_date timestamp without time zone null,
	end_date timestamp without time zone null,
	comment varchar(1000) null,
	state_id smallint not null references cmn_state(id),
	created_date timestamp without time zone default now() not null,
	constraint chk_cmn_contract_type
		check (contract_type in ('supplier', 'customer'))
);

create unique index idx_cmn_contract_number
on cmn_contract (organization_id, counterparty_id, contract_number);

create index idx_cmn_contract_organization_id on cmn_contract (organization_id);
create index idx_cmn_contract_counterparty_id on cmn_contract (counterparty_id);
create index idx_cmn_contract_contract_type on cmn_contract (contract_type);
create index idx_cmn_contract_contract_date on cmn_contract (contract_date);
create index idx_cmn_contract_state_id on cmn_contract (state_id);
