alter table edo_import_job
    add column bulk_import_status character varying(30),
    add column bulk_batch_size integer,
    add column bulk_line_values_invalid_policy character varying(30),
    add column bulk_marking_already_used_policy character varying(80),
    add column bulk_processed_count integer not null default 0,
    add column bulk_created_draft_count integer not null default 0,
    add column bulk_reused_draft_count integer not null default 0,
    add column bulk_duplicate_count integer not null default 0,
    add column bulk_skipped_count integer not null default 0,
    add column bulk_failed_count integer not null default 0,
    add column bulk_last_safe_error_code character varying(100),
    add column bulk_started_at timestamp without time zone,
    add column bulk_completed_at timestamp without time zone,
    add column bulk_cancel_requested_at timestamp without time zone;

alter table edo_import_job
    add constraint ck_edo_import_job_bulk_status
        check (bulk_import_status is null or bulk_import_status in
            ('QUEUED', 'RUNNING', 'PAUSED', 'CANCEL_REQUESTED', 'CANCELLED', 'COMPLETED')),
    add constraint ck_edo_import_job_bulk_batch_size
        check (bulk_batch_size is null or bulk_batch_size = 50),
    add constraint ck_edo_import_job_bulk_counts
        check (bulk_processed_count >= 0 and bulk_created_draft_count >= 0
            and bulk_reused_draft_count >= 0 and bulk_duplicate_count >= 0
            and bulk_skipped_count >= 0 and bulk_failed_count >= 0);

create index idx_edo_import_job_bulk_status
    on edo_import_job using btree (bulk_import_status, id)
    where bulk_import_status in ('QUEUED', 'RUNNING');
