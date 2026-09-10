begin;

alter table pay_payroll_doc
    add column if not exists correction_payout_mode varchar(20);

update pay_payroll_doc
set correction_payout_mode = case
    when document_kind = 'CORRECTION' then 'SEPARATE'
    else 'WITH_SALARY'
end
where correction_payout_mode is null;

alter table pay_payroll_doc
    alter column correction_payout_mode set default 'WITH_SALARY',
    alter column correction_payout_mode set not null;

alter table pay_payroll_doc
    drop constraint if exists ck_pay_payroll_doc_correction_payout_mode;

alter table pay_payroll_doc
    add constraint ck_pay_payroll_doc_correction_payout_mode
    check (correction_payout_mode in ('WITH_SALARY', 'WITH_ADVANCE', 'SEPARATE'));

create index if not exists idx_pay_payroll_doc_correction_payout_mode
    on pay_payroll_doc (correction_payout_mode);

commit;
