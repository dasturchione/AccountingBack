create table inv_product_group_translation 
(
	product_group_id		int not null references inv_product_group(id) on delete cascade,
	language_id				smallint not null references cmn_language(id) on delete cascade,
	name					varchar(250) not null,

	primary key (product_group_id, language_id)
);

create index if not exists ix_inv_product_group_translation_language_id
    on inv_product_group_translation(language_id);