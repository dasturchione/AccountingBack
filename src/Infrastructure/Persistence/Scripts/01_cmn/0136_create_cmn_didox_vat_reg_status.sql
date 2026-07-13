create table cmn_didox_vat_reg_status
(
    code smallint not null,
    name character varying(250) not null,
    state_id smallint not null,
    created_date timestamp without time zone default now() not null,
    constraint cmn_didox_vat_reg_status_pkey primary key (code),
    constraint cmn_didox_vat_reg_status_state_id_fkey foreign key (state_id) references cmn_state(id)
);

create index idx_cmn_didox_vat_reg_status_state_id on cmn_didox_vat_reg_status using btree (state_id);
