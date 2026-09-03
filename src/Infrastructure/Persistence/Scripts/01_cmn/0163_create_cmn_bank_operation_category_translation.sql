create table cmn_bank_operation_category_translation
(
    category_id    smallint not null references cmn_bank_operation_category(id),
    language_id    smallint not null references cmn_language(id),
    name           varchar(250) not null,

    constraint cmn_bank_operation_category_translation_pkey
        primary key (category_id, language_id),
    constraint ck_cmn_bank_operation_category_translation_name
        check (nullif(btrim(name), '') is not null)
);

create index ix_cmn_bank_operation_category_translation_language_id
    on cmn_bank_operation_category_translation (language_id);
