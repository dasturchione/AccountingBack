create table acc_document_account_type_role
(
	id							serial primary key,
	document_account_type_id	smallint not null references acc_document_account_type(id),
	document_account_role_id	smallint not null references acc_document_account_role(id),

	account_side				varchar(10) not null,

	is_required					boolean not null default true,
	sort_order					int not null default 1,

	constraint chk_acc_document_type_account_role_side check (account_side in ('debit', 'credit')),

	unique (document_account_type_id, document_account_role_id)
);
