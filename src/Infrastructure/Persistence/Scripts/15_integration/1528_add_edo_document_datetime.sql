alter table edo_document
    add column if not exists document_date_time timestamp without time zone;

alter table edo_import_candidate
    add column if not exists document_date_time timestamp without time zone;

update edo_document
set document_date_time = document_date::timestamp
where document_date_time is null
  and document_date is not null;

update edo_import_candidate
set document_date_time = document_date::timestamp
where document_date_time is null
  and document_date is not null;
