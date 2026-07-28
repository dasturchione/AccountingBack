create table cmn_document_status 
(
    id smallint   not null,
    code character varying(50) not null,
    name character varying(100) not null,
    state_id smallint not null,
    constraint cmn_document_status_pkey primary key (id),
    constraint cmn_document_status_state_id_fkey foreign key (state_id) references cmn_state(id)
);

create unique index idx_cmn_document_status_code on cmn_document_status using btree (code);

insert into cmn_document_status (id, code, name, state_id) values
    ('1', 'draft', 'Qoralama', '1'),
    ('2', 'posted', 'O''tkazilgan', '1'),
    ('3', 'cancelled', 'Bekor qilingan', '1'),
    ('4', 'pending', 'Kutilmoqda', '1');
