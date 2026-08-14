create table acc_document_account_type_translation
(
	language_id					smallint not null references cmn_language(id),
	document_account_type_id	smallint not null references acc_document_account_type(id),
	name						varchar(250) not null,
	description					varchar(500),

	primary key (language_id, document_account_type_id)
);
