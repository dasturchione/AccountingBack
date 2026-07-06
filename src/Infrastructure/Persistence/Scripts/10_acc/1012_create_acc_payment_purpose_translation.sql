create table acc_payment_purpose_translation
(
    payment_purpose_id smallint not null references acc_payment_purpose(id),
    language_id smallint not null references cmn_language(id),
    name varchar(250) not null,
    primary key (payment_purpose_id, language_id)
);
