create table cash_box
(
	id serial primary key,
	organization_id int not null references org_organization(id),
	branch_id int null references org_branch(id),
	code varchar(50) not null,
	name varchar(250) not null,
	currency_id smallint not null references cmn_currency(id),
	state_id smallint not null references cmn_state(id),
	created_date timestamp without time zone default now() not null
);

create unique index idx_cash_box_org_code on cash_box (organization_id, code);
create index idx_cash_box_organization_id on cash_box (organization_id);
create index idx_cash_box_branch_id on cash_box (branch_id);
create index idx_cash_box_currency_id on cash_box (currency_id);
create index idx_cash_box_state_id on cash_box (state_id);
