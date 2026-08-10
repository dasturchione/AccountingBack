create table fiscal_cash_register
(
    id                      serial primary key,

    organization_id         int not null
        references org_organization(id),

    -- Торговая точка / склад, если касса привязана к конкретной точке
    warehouse_id            int
        references inv_warehouse(id),

    register_type_id        smallint not null
        references fiscal_cash_register_type(id),

    name                    varchar(250) not null,

    -- Идентификатор кассы во внешней / налоговой системе
    external_register_id    varchar(100),

    -- Модель онлайн-ККМ
    model                   varchar(100),

    -- Заводской номер физической ККМ
    serial_number           varchar(100),

    -- Номер фискального модуля
    fiscal_module_number    varchar(100),

    state_id                smallint not null
        references cmn_state(id),

    created_date            timestamp without time zone not null default now(),

    constraint ck_fiscal_cash_register_name_not_empty
        check (btrim(name) <> ''),

    constraint ck_fiscal_cash_register_external_id_not_empty
        check (
            external_register_id is null
            or btrim(external_register_id) <> ''
        ),

    constraint ck_fiscal_cash_register_model_not_empty
        check (
            model is null
            or btrim(model) <> ''
        ),

    constraint ck_fiscal_cash_register_serial_number_not_empty
        check (
            serial_number is null
            or btrim(serial_number) <> ''
        ),

    constraint ck_fiscal_cash_register_fiscal_module_number_not_empty
        check (
            fiscal_module_number is null
            or btrim(fiscal_module_number) <> ''
        )
);

create index ix_fiscal_cash_register_organization_id
    on fiscal_cash_register(organization_id);

create index ix_fiscal_cash_register_warehouse_id
    on fiscal_cash_register(warehouse_id);

create index ix_fiscal_cash_register_register_type_id
    on fiscal_cash_register(register_type_id);

create index ix_fiscal_cash_register_state_id
    on fiscal_cash_register(state_id);

create index ix_fiscal_cash_register_organization_state_id
    on fiscal_cash_register(organization_id, state_id);

create unique index ux_fiscal_cash_register_org_external_register_id
    on fiscal_cash_register(organization_id, external_register_id)
    where external_register_id is not null;

create unique index ux_fiscal_cash_register_org_serial_number
    on fiscal_cash_register(organization_id, serial_number)
    where serial_number is not null;

create unique index ux_fiscal_cash_register_org_fiscal_module_number
    on fiscal_cash_register(organization_id, fiscal_module_number)
    where fiscal_module_number is not null;