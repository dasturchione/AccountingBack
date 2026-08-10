create table bank_terminal 
(
	id                      serial primary key,

    organization_id         int not null references org_organization(id),

    -- На какой банковский счет поступают деньги
    bank_account_id         int references org_bank_account(id),

    name                    varchar(200) not null,

    -- Идентификаторы, полученные от банка / процессинга
    merchant_id             varchar(100),
    external_terminal_id    varchar(100),

    -- Серийный номер физического устройства
    serial_number           varchar(100),

    state_id                smallint not null references cmn_state(id),

    created_date            timestamp without time zone not null default now()
);

create index ix_bank_terminal_organization_id
    on bank_terminal(organization_id);

create index ix_bank_terminal_bank_account_id
    on bank_terminal(bank_account_id);

create index ix_bank_terminal_state_id
    on bank_terminal(state_id);


-- Для списков терминалов организации
create index ix_bank_terminal_organization_id_state_id
    on bank_terminal(organization_id, state_id);


-- Один Merchant может иметь несколько терминалов
create index ix_bank_terminal_merchant_id
    on bank_terminal(merchant_id)
    where merchant_id is not null;


-- Terminal ID должен быть уникален внутри организации
create unique index ux_bank_terminal_organization_external_terminal_id
    on bank_terminal(organization_id, external_terminal_id)
    where external_terminal_id is not null;


-- Один физический терминал не должен дублироваться внутри организации
create unique index ux_bank_terminal_organization_serial_number
    on bank_terminal(organization_id, serial_number)
    where serial_number is not null;

create or replace function check_bank_terminal_organization()
returns trigger
language plpgsql
as $$
begin
    if new.bank_account_id is not null
       and not exists (
            select 1
            from org_bank_account a
            where a.id = new.bank_account_id
              and a.organization_id = new.organization_id
       )
    then
        raise exception
            'Bank account % does not belong to organization %',
            new.bank_account_id,
            new.organization_id;
    end if;

    return new;
end;
$$;