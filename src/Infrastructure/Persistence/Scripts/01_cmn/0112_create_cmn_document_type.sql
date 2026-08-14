create table cmn_document_type 
(
    id smallint   not null,
    code character varying(50) not null,
    name character varying(150) not null,
    state_id smallint not null,
    created_date timestamp without time zone default now() not null,
    constraint cmn_document_type_pkey primary key (id),
    constraint cmn_document_type_state_id_fkey foreign key (state_id) references cmn_state(id)
);

create unique index idx_cmn_document_type_code on cmn_document_type using btree (code);
create index idx_cmn_document_type_state_id on cmn_document_type using btree (state_id);

insert into cmn_document_type 
    (id, state_id, code, name) 
values
    ( 1, 1, 'purchase',                 'Xarid / Kirim'                 ),
    ( 2, 1, 'sale',                     'Sotuv'                         ),
    ( 3, 1, 'bank_operation',           'Bank operatsiyasi'             ),
    ( 4, 1, 'cash_operation',           'Kassa operatsiyasi'            ),
    ( 5, 1, 'salary',                   'Ish haqi'                      ),
    ( 6, 1, 'expense',                  'Xarajat'                       ),
    ( 7, 1, 'retail_sale',              'Chakana sotuv'                 ),
    (10, 1, 'currency_revaluation',     'Currency revaluation'          ),
    (11, 1, 'fa_receipt',               'Asosiy vosita qabuli'          ),
    (12, 1, 'fa_movement',              'Asosiy vosita ko''chirishi'    ),
    (13, 1, 'fa_depreciation',          'Asosiy vosita amortizatsiyasi' ),
    (14, 1, 'fa_disposal',              'Asosiy vosita chiqib ketishi'  ),
    (15, 1, 'fa_revaluation',           'Boshlang''ich tovar qoldig''i' ),
    (16, 1, 'opening_inventory',        'Ochiq inventarizatsiya'        ),
    (17, 1, 'fa_commissioning',         'Asosiy vosita o''rnatish'      );
