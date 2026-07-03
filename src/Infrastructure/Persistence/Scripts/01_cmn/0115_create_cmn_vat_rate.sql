
create table cmn_vat_rate (
    id smallint   not null,
    code character varying(50) not null,
    name character varying(150) not null,
    rate numeric(5,2) not null,
    state_id smallint not null,
    created_date timestamp without time zone default now() not null,
    effective_from date,
    effective_to date,
    constraint cmn_vat_rate_pkey primary key (id),
    constraint cmn_vat_rate_state_id_fkey foreign key (state_id) references cmn_state(id),
    constraint cmn_vat_rate_effective_dates_check CHECK (((effective_to IS null) OR (effective_from IS null) OR (effective_to >= effective_from)))
);

insert into cmn_vat_rate (id, code, name, rate, state_id, created_date) values
    ('1', 'vat_0', 'QQS 0%', '0.00', '1', '2026-06-06 16:40:45.727928'),
    ('2', 'vat_12', 'QQS 12%', '12.00', '1', '2026-06-06 16:40:45.727928'),
    ('3', 'vat_15', 'QQS 15%', '15.00', '1', '2026-06-06 16:40:45.727928'),
    ('4', 'vat_6', 'QQS 6%', '6.00', '1', '2026-06-27 13:53:02.619573');

create unique index idx_cmn_vat_rate_code on cmn_vat_rate using btree (code);

create index idx_cmn_vat_rate_state_id on cmn_vat_rate using btree (state_id);

create index idx_cmn_vat_rate_effective_dates on cmn_vat_rate using btree (effective_from, effective_to);

