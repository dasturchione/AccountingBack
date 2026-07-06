create table cmn_tax_type 
(
    id smallint   not null,
    code character varying(50) not null,
    name character varying(150) not null,
    state_id smallint not null,
    created_date timestamp without time zone default now() not null,
    constraint cmn_tax_type_pkey primary key (id),
    constraint cmn_tax_type_state_id_fkey foreign key (state_id) references cmn_state(id)
);

create unique index idx_cmn_tax_type_code on cmn_tax_type using btree (code);
create index idx_cmn_tax_type_state_id on cmn_tax_type using btree (state_id);

insert into cmn_tax_type (id, code, name, state_id, created_date) values
    ('1', 'none', 'Soliqsiz', '1', '2026-06-06 16:40:32.193179'),
    ('2', 'vat', 'QQS', '1', '2026-06-06 16:40:32.193179');
