create table acc_document_account_setting
(
    id                              bigserial primary key,

    organization_id                 int not null references org_organization(id),

    document_account_type_role_id   int not null references acc_document_account_type_role(id),

    chart_account_id                int not null references acc_chart_account(id),

    is_default                      boolean not null default false,
    can_change                      boolean not null default true,

    sort_order                      int not null default 1,

    state_id                        smallint not null references cmn_state(id),

    created_date                    timestamp without time zone not null default now(),
    updated_date                    timestamp without time zone null,

    constraint chk_acc_document_account_setting_sort_order 
        check (sort_order > 0),

    constraint uq_acc_document_account_setting
        unique (
            organization_id,
            document_account_type_role_id,
            chart_account_id
        )
);

create index idx_acc_document_account_setting_organization_id
on acc_document_account_setting (organization_id);

create index idx_acc_document_account_setting_type_role_id
on acc_document_account_setting (document_account_type_role_id);

create index idx_acc_document_account_setting_chart_account_id
on acc_document_account_setting (chart_account_id);

create unique index ux_acc_document_account_setting_default
on acc_document_account_setting
(
    organization_id,
    document_account_type_role_id
)
where is_default = true and state_id = 1;