create table cmn_fa_depreciation_method
(
    id smallint not null,
    code character varying(50) not null,
    name character varying(150) not null,
    state_id smallint not null,
    constraint cmn_fa_depreciation_method_pkey primary key (id),
    constraint cmn_fa_depreciation_method_state_id_fkey foreign key (state_id) references cmn_state(id)
);

create unique index idx_cmn_fa_depreciation_method_code on cmn_fa_depreciation_method using btree (code);
create index idx_cmn_fa_depreciation_method_state_id on cmn_fa_depreciation_method using btree (state_id);

insert into cmn_fa_depreciation_method (id, code, name, state_id) values
    ('1', 'LINEAR', 'Chiziqli', '1'),
    ('2', 'DECLINING_BALANCE', 'Kamayuvchi qoldiq', '1'),
    ('3', 'UNITS_OF_PRODUCTION', 'Ishlab chiqarish hajmi bo''yicha', '1');
