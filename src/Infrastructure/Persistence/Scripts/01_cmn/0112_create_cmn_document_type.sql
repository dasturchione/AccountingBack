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

insert into cmn_document_type (id, code, name, state_id, created_date) values
    ('1', 'purchase', 'Xarid / Kirim', '1', '2026-06-06 16:40:07.223429'),
    ('2', 'sale', 'Sotuv', '1', '2026-06-06 16:40:07.223429'),
    ('3', 'bank_operation', 'Bank operatsiyasi', '1', '2026-06-06 16:40:07.223429'),
    ('4', 'cash_operation', 'Kassa operatsiyasi', '1', '2026-06-06 16:40:07.223429'),
    ('5', 'salary', 'Ish haqi', '1', '2026-06-06 16:40:07.223429'),
    ('6', 'expense', 'Xarajat', '1', '2026-06-06 16:40:07.223429'),
    ('10', 'currency_revaluation', 'Currency revaluation', '1', '2026-07-04 00:00:00'),
    ('11', 'fa_receipt', 'Asosiy vosita qabuli', '1', '2026-07-06 00:00:00'),
    ('12', 'fa_movement', 'Asosiy vosita ko''chirishi', '1', '2026-07-07 00:00:00'),
    ('13', 'fa_depreciation', 'Asosiy vosita amortizatsiyasi', '1', '2026-07-07 00:00:00'),
    ('14', 'fa_disposal', 'Asosiy vosita chiqib ketishi', '1', '2026-07-07 00:00:00'),
    ('15', 'fa_revaluation', 'Asosiy vosita qayta baholash', '1', '2026-07-07 00:00:00');
