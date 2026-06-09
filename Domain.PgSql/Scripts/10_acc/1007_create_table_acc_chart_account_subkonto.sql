create table acc_chart_account_subkonto
(
	id serial primary key,
	organization_id int not null references org_organization(id),
	account_id int not null references acc_chart_account(id) on delete cascade,
	subkonto_type_id smallint not null references acc_subkonto_type(id),
	sort_order int default 0 not null,
	is_required boolean default true not null,
	state_id smallint not null references cmn_state(id),
	created_date timestamp without time zone default now() not null
);

create unique index idx_acc_chart_account_subkonto_unique
on acc_chart_account_subkonto (organization_id, account_id, subkonto_type_id);

create index idx_acc_chart_account_subkonto_organization_id
on acc_chart_account_subkonto (organization_id);

create index idx_acc_chart_account_subkonto_account_id
on acc_chart_account_subkonto (account_id);

create index idx_acc_chart_account_subkonto_type_id
on acc_chart_account_subkonto (subkonto_type_id);
