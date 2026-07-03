create table acc_payment_purpose
(
	id                    smallserial primary key,
	code                  varchar(50) not null unique,
	alias_id              smallint not null references acc_posting_alias(id),
	name                  varchar(250) not null,
	operation_type_id     smallint not null references cmn_operation_type(id),
	requires_counterparty boolean not null default true);

create index idx_acc_payment_purpose_alias_id on acc_payment_purpose (alias_id);
create index idx_acc_payment_purpose_operation_type_id on acc_payment_purpose (operation_type_id);

create table acc_payment_purpose_translation
(
    payment_purpose_id    smallint not null references acc_payment_purpose(id),
    language_id           smallint not null references cmn_language(id),
    name                  varchar(250) not null,
    primary key (payment_purpose_id, language_id));
