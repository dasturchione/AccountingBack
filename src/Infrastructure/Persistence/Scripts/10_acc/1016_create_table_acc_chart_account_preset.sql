create table acc_chart_account_preset
(
    id              smallserial primary key,
    code            varchar(50) not null unique,
    name            varchar(255) not null,
    description     varchar(500) null,
    state_id        smallint not null           references cmn_state(id),

    created_date    timestamp without time zone not null default now()
);

create table acc_chart_account_preset_translation
(
    preset_id           smallint not null       references acc_chart_account_preset(id) on delete cascade,
    language_id         smallint not null       references cmn_language(id),
    name                varchar(255) not null,
    description         varchar(500) null,

    primary key (preset_id, language_id)
);

CREATE INDEX idx_acc_chart_account_preset_translation_language_id
ON acc_chart_account_preset_translation(language_id);
