create table acc_chart_account_preset_account_subkonto
(
    id                  bigserial primary key,
    preset_account_id   int not null            references acc_chart_account_preset_account(id) on delete cascade,
    subkonto_type_id    smallint not null       references acc_subkonto_type(id),
    sort_order          int not null,
    created_date        timestamp without time zone not null default now(),

    constraint chk_acc_chart_account_preset_account_subkonto_sort_order
        check (sort_order between 1 and 9),

    constraint uq_acc_chart_account_preset_account_subkonto_order
        unique (preset_account_id, sort_order),

    constraint uq_acc_chart_account_preset_account_subkonto_type
        unique (preset_account_id, subkonto_type_id)
);

create index idx_acc_chart_account_preset_account_subkonto_account_id
on acc_chart_account_preset_account_subkonto(preset_account_id);

create index idx_acc_chart_account_preset_account_subkonto_type_id
on acc_chart_account_preset_account_subkonto(subkonto_type_id);