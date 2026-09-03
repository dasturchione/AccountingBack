alter table cmn_bank
    add column if not exists legal_name character varying(500),
    add column if not exists license_number character varying(50),
    add column if not exists license_date date,
    add column if not exists address character varying(500),
    add column if not exists opened_date date,
    add column if not exists source_updated_date date,
    add column if not exists website character varying(250),
    add column if not exists latitude numeric(9,6),
    add column if not exists longitude numeric(9,6);

do $$
begin
    if not exists
    (
        select 1
        from pg_constraint
        where conname = 'ck_cmn_bank_latitude'
          and conrelid = 'cmn_bank'::regclass
    ) then
        alter table cmn_bank
            add constraint ck_cmn_bank_latitude
            check (latitude is null or latitude between -90 and 90);
    end if;

    if not exists
    (
        select 1
        from pg_constraint
        where conname = 'ck_cmn_bank_longitude'
          and conrelid = 'cmn_bank'::regclass
    ) then
        alter table cmn_bank
            add constraint ck_cmn_bank_longitude
            check (longitude is null or longitude between -180 and 180);
    end if;
end
$$;

create index if not exists idx_cmn_bank_inn
    on cmn_bank using btree (inn);

create index if not exists idx_cmn_bank_license_number
    on cmn_bank using btree (license_number);
