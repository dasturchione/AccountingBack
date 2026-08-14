create table acc_document_account_role_translation
(
	document_account_role_id	smallint not null references acc_document_account_role(id),
	language_id					smallint not null references cmn_language(id),
	name						varchar(250) not null,
	description					varchar(500),

	primary key (document_account_role_id, language_id)
);
