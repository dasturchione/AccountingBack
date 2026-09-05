create table cmn_regulated_obligation_translation
(
    regulated_obligation_id    smallint not null references cmn_regulated_obligation(id),
    language_id                smallint not null references cmn_language(id),
    name                       varchar(250) not null,

    constraint cmn_regulated_obligation_translation_pkey
        primary key (regulated_obligation_id, language_id),
    constraint ck_cmn_regulated_obligation_translation_name
        check (nullif(btrim(name), '') is not null)
);

create index ix_cmn_regulated_obligation_translation_language_id
    on cmn_regulated_obligation_translation(language_id);
