create table acc_subkonto_type 
(
    id smallint   not null,
    code character varying(50) not null,
    name character varying(150) not null,
    source_table character varying(100) not null,
    state_id smallint not null,
    created_date timestamp without time zone default now() not null,
    constraint acc_subkonto_type_pkey primary key (id),
    constraint acc_subkonto_type_state_id_fkey foreign key (state_id) references cmn_state(id)
);

create unique index idx_acc_subkonto_type_code on acc_subkonto_type using btree (code);
create index idx_acc_subkonto_type_state_id on acc_subkonto_type using btree (state_id);

insert into acc_subkonto_type (id, code, name, source_table, state_id, created_date) values
    ('1', 'product', 'Tovar / xizmat', 'inv_product', '1', '2026-06-25 10:43:24.6827'),
    ('2', 'warehouse', 'Ombor', 'inv_warehouse', '1', '2026-06-25 10:43:24.6827'),
    ('3', 'counterparty', 'Kontragent', 'counterparty_card', '1', '2026-06-25 10:43:24.6827'),
    ('4', 'bank_account', 'Bank hisobi', 'org_bank_account', '1', '2026-06-25 10:43:24.6827'),
    ('5', 'cash_box', 'Kassa', 'cash_box', '1', '2026-06-25 10:43:24.6827'),
    ('6', 'employee', 'Xodim', 'sys_user', '1', '2026-06-25 10:43:24.6827'),
    ('7', 'tax', 'Soliq', 'cmn_tax_type', '1', '2026-06-25 10:43:24.6827'),
    ('8', 'bank_operation', 'Bank operatsiyasi', 'bank_operation', '1', '2026-06-25 10:43:24.6827'),
    ('9', 'contract', 'Shartnoma', 'cmn_contract', '1', '2026-06-25 10:43:24.6827'),
    ('10', 'puchase', 'Xarid', 'pur_doc', '1', '2026-06-25 10:43:24.6827'),
    ('11', 'sale', 'Sotuv', 'sale_doc', '1', '2026-06-25 10:43:24.6827');
