create table cmn_language
(
	id smallserial primary key,
	code varchar(10) not null,
	name varchar(100) not null,
	native_name varchar(100) not null,
	is_default boolean not null default false,
	sort_order int not null default 0,
	state_id smallint not null references cmn_state(id),
	created_date timestamp without time zone default now() not null
);

create unique index idx_cmn_language_code on cmn_language (code);
create index idx_cmn_language_state_id on cmn_language (state_id);
create unique index idx_cmn_language_default on cmn_language (is_default)
where is_default = true;
