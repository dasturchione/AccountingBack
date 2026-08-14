alter table cmn_contract
    add column provider_code character varying(20),
    add column provider_contract_number character varying(100),
    add column provider_contract_date date;

alter table cmn_contract
    add constraint ck_cmn_contract_provider_code
        check (provider_code is null or provider_code in ('EDOCS', 'DIDOX'));

create unique index ux_cmn_contract_provider_identity
    on cmn_contract (
        organization_id,
        counterparty_id,
        provider_code,
        provider_contract_number,
        provider_contract_date)
    where provider_code is not null
      and provider_contract_number is not null
      and provider_contract_date is not null;

comment on column cmn_contract.provider_code is
    'EDO provider identity; nullable for contracts not explicitly reconciled with provider data.';
comment on column cmn_contract.provider_contract_number is
    'Exact provider contract number; separate from the locally generated contract_number.';
comment on column cmn_contract.provider_contract_date is
    'Exact provider contract date used with provider code and number for idempotency.';
