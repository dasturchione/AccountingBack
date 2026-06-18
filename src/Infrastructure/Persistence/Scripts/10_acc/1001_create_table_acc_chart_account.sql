create table acc_chart_account
(
	id serial primary key,
	parent_id int null references acc_chart_account(id),
	code varchar(50) not null unique,
	name varchar(250) not null,
	is_group boolean not null default false,
	state_id smallint not null references cmn_state(id),
	created_date timestamp without time zone default now() not null
);

create unique index idx_acc_chart_account_org_code on acc_chart_account (organization_id, code);
create index idx_acc_chart_account_parent_id on acc_chart_account (parent_id);
create index idx_acc_chart_account_state_id on acc_chart_account (state_id);
