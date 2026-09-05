begin;

create table rnt_lessor
(
    id                      bigserial primary key,
    organization_id         int not null references org_organization(id),
    lessor_kind_code        varchar(20) not null,
    counterparty_id         int references counterparty_card(id),
    full_name               varchar(500) not null,
    inn                     varchar(20),
    pinfl                   varchar(14),
    phone_number            varchar(50),
    registered_address      varchar(1000),
    residential_address     varchar(1000),
    state_id                smallint not null default 1 references cmn_state(id),
    created_date            timestamp without time zone not null default now(),
    updated_date            timestamp without time zone,

    constraint ck_rnt_lessor_kind
        check (lessor_kind_code in ('INDIVIDUAL', 'LEGAL_ENTITY')),
    constraint ck_rnt_lessor_counterparty
        check
        (
            (lessor_kind_code = 'INDIVIDUAL' and counterparty_id is null)
            or
            (lessor_kind_code = 'LEGAL_ENTITY' and counterparty_id is not null)
        ),
    constraint ck_rnt_lessor_identifier
        check
        (
            nullif(btrim(inn), '') is not null
            or nullif(btrim(pinfl), '') is not null
        )
);

create table rnt_contract_lessor
(
    contract_id    bigint not null references rnt_contract(id) on delete cascade,
    lessor_id      bigint not null references rnt_lessor(id),

    primary key (contract_id, lessor_id)
);

do $$
declare
    contract_row         record;
    resolved_lessor_id   bigint;
begin
    for contract_row in
        select contract.id,
               contract.organization_id,
               contract.lessor_full_name,
               nullif(btrim(contract.lessor_inn), '') as lessor_inn,
               nullif(btrim(contract.lessor_pinfl), '') as lessor_pinfl
        from rnt_contract contract
        order by contract.id
    loop
        resolved_lessor_id := null;

        if contract_row.lessor_pinfl is not null then
            select lessor.id
            into resolved_lessor_id
            from rnt_lessor lessor
            where lessor.organization_id = contract_row.organization_id
              and lessor.lessor_kind_code = 'INDIVIDUAL'
              and btrim(lessor.pinfl) = contract_row.lessor_pinfl
              and lessor.state_id = 1
            order by lessor.id
            limit 1;
        end if;

        if resolved_lessor_id is null and contract_row.lessor_inn is not null then
            select lessor.id
            into resolved_lessor_id
            from rnt_lessor lessor
            where lessor.organization_id = contract_row.organization_id
              and btrim(lessor.inn) = contract_row.lessor_inn
              and lessor.state_id = 1
            order by lessor.id
            limit 1;
        end if;

        if resolved_lessor_id is null then
            insert into rnt_lessor
                (organization_id, lessor_kind_code, full_name, inn, pinfl, state_id)
            values
                (contract_row.organization_id, 'INDIVIDUAL', contract_row.lessor_full_name,
                 contract_row.lessor_inn, contract_row.lessor_pinfl, 1)
            returning id into resolved_lessor_id;
        else
            update rnt_lessor
            set inn = coalesce(nullif(btrim(inn), ''), contract_row.lessor_inn),
                pinfl = coalesce(nullif(btrim(pinfl), ''), contract_row.lessor_pinfl)
            where id = resolved_lessor_id;
        end if;

        insert into rnt_contract_lessor (contract_id, lessor_id)
        values (contract_row.id, resolved_lessor_id)
        on conflict (contract_id, lessor_id) do nothing;
    end loop;
end
$$;

create index ix_rnt_lessor_organization_id
    on rnt_lessor (organization_id);

create unique index ux_rnt_lessor_active_pinfl
    on rnt_lessor (organization_id, btrim(pinfl))
    where state_id = 1
      and lessor_kind_code = 'INDIVIDUAL'
      and nullif(btrim(pinfl), '') is not null;

create unique index ux_rnt_lessor_active_inn
    on rnt_lessor (organization_id, btrim(inn))
    where state_id = 1
      and nullif(btrim(inn), '') is not null;

create unique index ux_rnt_lessor_active_counterparty
    on rnt_lessor (organization_id, counterparty_id)
    where state_id = 1
      and counterparty_id is not null;

create index ix_rnt_contract_lessor_lessor_id
    on rnt_contract_lessor (lessor_id);

alter table rnt_contract
    add column is_free_of_charge boolean not null default false;

alter table rnt_contract
    drop column lessor_full_name,
    drop column lessor_inn,
    drop column lessor_pinfl;

alter table rnt_contract_object
    add column total_area numeric(24, 8),
    add column rented_area numeric(24, 8);

alter table rnt_contract_object
    drop constraint if exists rnt_contract_object_contract_amount_check;

alter table rnt_contract_object
    add constraint ck_rnt_contract_object_contract_amount
        check (contract_amount >= 0),
    add constraint ck_rnt_contract_object_total_area
        check (total_area is null or total_area > 0),
    add constraint ck_rnt_contract_object_rented_area
        check (rented_area is null or rented_area > 0),
    add constraint ck_rnt_contract_object_area_relation
        check (total_area is null or rented_area is null or rented_area <= total_area);

create table rnt_contract_object_utility
(
    id                      bigserial primary key,
    contract_object_id      bigint not null references rnt_contract_object(id) on delete cascade,
    utility_service_id      smallint not null references cmn_utility_service(id),
    payer_code              varchar(20) not null,

    constraint ux_rnt_contract_object_utility
        unique (contract_object_id, utility_service_id),
    constraint ck_rnt_contract_object_utility_payer
        check (payer_code in ('LESSOR', 'LESSEE'))
);

create index ix_rnt_contract_object_utility_service_id
    on rnt_contract_object_utility (utility_service_id);

commit;
