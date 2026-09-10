-- Payroll/HR test-data cleanup.
--
-- This removes transactional payroll data and employee-specific setup while
-- preserving master configuration (pay_component, pay_tax_definition,
-- hr_absence_type, accounts, organizations and users).
--
-- Safety: this script intentionally runs only on the local AccountingTest
-- database. Change the database name below only after an explicit review.

begin;

set local lock_timeout = '5s';
set local statement_timeout = '60s';

do $$
begin
    if current_database() <> 'AccountingTest' then
        raise exception 'Safety stop: cleanup is allowed only on AccountingTest (current database: %)', current_database();
    end if;
end;
$$;

-- Review the number of rows before deletion.
select 'pay_employee' as table_name, count(*) as row_count from pay_employee
union all select 'pay_employment', count(*) from pay_employment
union all select 'pay_employee_component', count(*) from pay_employee_component
union all select 'hr_employee_work_schedule', count(*) from hr_employee_work_schedule
union all select 'hr_absence', count(*) from hr_absence
union all select 'pay_period', count(*) from pay_period
union all select 'pay_timesheet', count(*) from pay_timesheet
union all select 'pay_timesheet_line', count(*) from pay_timesheet_line
union all select 'pay_timesheet_line_day', count(*) from pay_timesheet_line_day
union all select 'pay_payroll_doc', count(*) from pay_payroll_doc
union all select 'pay_payroll_line', count(*) from pay_payroll_line
union all select 'pay_payroll_calc_line', count(*) from pay_payroll_calc_line
union all select 'pay_payroll_tax_line', count(*) from pay_payroll_tax_line
union all select 'pay_payroll_line_segment', count(*) from pay_payroll_line_segment
union all select 'pay_payroll_recalculation', count(*) from pay_payroll_recalculation
union all select 'pay_payment_batch', count(*) from pay_payment_batch;

-- Unlink banking/payment-acceptance operations from registry rows that will
-- disappear with payroll documents. The operations themselves are preserved.
update bank_operation operation
set related_document_id = null
where related_document_id in (
    select id
    from cmn_document_registry
    where document_type_id in (5, 20, 21, 22)
);

update payment_acceptance_point_operation operation
set related_document_id = null
where related_document_id in (
    select id
    from cmn_document_registry
    where document_type_id in (5, 20, 21, 22)
);

-- Daily snapshots must be removed before their source absences/schedules.
delete from pay_timesheet_line_day;
delete from hr_absence_attachment;
delete from hr_absence;

-- Recalculation requests and payments reference payroll documents/lines.
delete from pay_payroll_recalculation;
delete from pay_payment_line;
delete from pay_payment_batch;

-- Delete correction documents from the leaves upward. Clearing the source
-- column first would violate ck_pay_payroll_doc_correction.
do $$
declare
    deleted_count integer;
begin
    loop
        delete from pay_payroll_doc document
        where document.document_kind = 'CORRECTION'
          and not exists (
              select 1
              from pay_payroll_doc child
              where child.correction_of_doc_id = document.id
          );

        get diagnostics deleted_count = row_count;
        exit when deleted_count = 0;
    end loop;

    if exists (select 1 from pay_payroll_doc where document_kind = 'CORRECTION') then
        raise exception 'Cannot clear payroll documents: correction source cycle detected.';
    end if;
end;
$$;
delete from pay_payroll_doc;

-- Tabel and period data.
delete from pay_timesheet_line;
delete from pay_timesheet;
delete from pay_period_work_day;
delete from pay_period;

-- Employee-specific setup. Master components and absence types remain.
delete from hr_employee_work_schedule_day;
delete from hr_employee_work_schedule;
delete from pay_employee_component;
delete from pay_employment;
delete from pay_employee;

-- Remove any registry rows left behind by old data or disabled triggers.
delete from cmn_document_registry
where document_type_id in (5, 20, 21, 22);

-- Final verification inside the transaction. Review these zeros, then commit.
select 'pay_employee' as table_name, count(*) as row_count from pay_employee
union all select 'pay_employment', count(*) from pay_employment
union all select 'pay_employee_component', count(*) from pay_employee_component
union all select 'hr_employee_work_schedule', count(*) from hr_employee_work_schedule
union all select 'hr_absence', count(*) from hr_absence
union all select 'pay_period', count(*) from pay_period
union all select 'pay_timesheet', count(*) from pay_timesheet
union all select 'pay_timesheet_line', count(*) from pay_timesheet_line
union all select 'pay_timesheet_line_day', count(*) from pay_timesheet_line_day
union all select 'pay_payroll_doc', count(*) from pay_payroll_doc
union all select 'pay_payroll_line', count(*) from pay_payroll_line
union all select 'pay_payroll_calc_line', count(*) from pay_payroll_calc_line
union all select 'pay_payroll_tax_line', count(*) from pay_payroll_tax_line
union all select 'pay_payroll_line_segment', count(*) from pay_payroll_line_segment
union all select 'pay_payroll_recalculation', count(*) from pay_payroll_recalculation
union all select 'pay_payment_batch', count(*) from pay_payment_batch;

commit;
