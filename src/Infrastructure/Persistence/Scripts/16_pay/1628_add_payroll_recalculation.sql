begin;

create table if not exists pay_payroll_recalculation
(
    id                    bigserial primary key,
    organization_id       integer not null references org_organization(id),
    payroll_doc_id        bigint not null references pay_payroll_doc(id) on delete restrict,
    status                varchar(20) not null default 'PENDING',
    reason                varchar(1000) not null,
    source_revision       varchar(64),
    requested_date        timestamp without time zone not null default now(),
    requested_by_user_id  integer references sys_user(id) on delete set null,
    completed_date        timestamp without time zone,
    correction_doc_id     bigint references pay_payroll_doc(id) on delete set null,
    error_message         varchar(2000),
    constraint ck_pay_payroll_recalculation_status
        check (status in ('PENDING', 'PROCESSING', 'COMPLETED', 'FAILED')),
    constraint ck_pay_payroll_recalculation_dates
        check (completed_date is null or completed_date >= requested_date)
);

create index if not exists idx_pay_payroll_recalculation_org_status
    on pay_payroll_recalculation (organization_id, status);
create index if not exists idx_pay_payroll_recalculation_payroll_doc_id
    on pay_payroll_recalculation (payroll_doc_id);
create unique index if not exists ux_pay_payroll_recalculation_active_doc
    on pay_payroll_recalculation (payroll_doc_id)
    where status in ('PENDING', 'PROCESSING');

commit;
