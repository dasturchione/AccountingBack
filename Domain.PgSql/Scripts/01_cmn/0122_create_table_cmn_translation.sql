create table cmn_translation
(
	id bigserial primary key,
	language_id smallint not null references cmn_language(id),
	table_name varchar(100) not null,
	record_id bigint not null,
	column_name varchar(100) not null,
	value text not null,
	created_date timestamp without time zone default now() not null
);

create unique index idx_cmn_translation_unique
on cmn_translation (language_id, table_name, record_id, column_name);

create index idx_cmn_translation_lookup
on cmn_translation (table_name, record_id, column_name);

create index idx_cmn_translation_language_id
on cmn_translation (language_id);
