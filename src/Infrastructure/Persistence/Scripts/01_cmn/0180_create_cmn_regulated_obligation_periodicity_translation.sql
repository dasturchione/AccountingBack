create table cmn_regulated_obligation_periodicity_translation
(
    periodicity_id    smallint not null references cmn_regulated_obligation_periodicity(id),
    language_id       smallint not null references cmn_language(id),
    name              varchar(150) not null,

    constraint cmn_regulated_obligation_periodicity_translation_pkey
        primary key (periodicity_id, language_id),
    constraint ck_cmn_regulated_obligation_periodicity_translation_name
        check (nullif(btrim(name), '') is not null)
);

create index ix_cmn_regulated_obligation_periodicity_translation_language_id
    on cmn_regulated_obligation_periodicity_translation(language_id);
