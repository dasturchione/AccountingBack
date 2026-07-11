create table acc_chart_account_preset_account
(
    id                          serial primary key,
    preset_id                   smallint not null                   references acc_chart_account_preset(id) on delete cascade,
    code                        varchar(20) null,
    number                      varchar(20) not null,
    parent_preset_account_id    int null,
    account_type_id             smallint not null                   references acc_account_type(id),
    is_currency                 boolean not null default false,
    is_quantity                 boolean not null default false,
    is_department               boolean not null default false,
    is_tax_accounting           boolean not null default false,
    is_off_balance              boolean not null default false,
    display_order               int not null default 1,
    state_id                    smallint not null                   references cmn_state(id),
    created_date                timestamp without time zone not null default now(),

    constraint uq_acc_chart_account_preset_account_preset_number unique (preset_id, number),

    constraint uq_acc_chart_account_preset_account_preset_id unique (preset_id, id),

    constraint fk_acc_chart_account_preset_account_parent
        foreign key (preset_id, parent_preset_account_id)
        references acc_chart_account_preset_account(preset_id, id),

    constraint chk_acc_chart_account_preset_account_not_self_parent
        check (
            parent_preset_account_id is null
            or parent_preset_account_id <> id
        )
);

create table acc_chart_account_preset_account_translation
(
    preset_account_id   int not null            references acc_chart_account_preset_account(id) on delete cascade,
    language_id         smallint not null       references cmn_language(id),
    name                varchar(255) not null,

    primary key (preset_account_id, language_id)
);

create index idx_acc_chart_account_preset_account_preset_id
on acc_chart_account_preset_account(preset_id);

create index idx_acc_chart_account_preset_account_parent_id
on acc_chart_account_preset_account(parent_preset_account_id);

create index idx_acc_chart_account_preset_account_account_type_id
on acc_chart_account_preset_account(account_type_id);

create index idx_acc_chart_account_preset_account_translation_language_id
on acc_chart_account_preset_account_translation(language_id);
