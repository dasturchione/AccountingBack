begin;

alter table pay_payroll_line
    add column if not exists paid_leave_days numeric(6,2) not null default 0,
    add column if not exists paid_sick_days numeric(6,2) not null default 0,
    add column if not exists overtime_hours numeric(8,2) not null default 0,
    add column if not exists night_hours numeric(8,2) not null default 0,
    add column if not exists holiday_hours numeric(8,2) not null default 0,
    add column if not exists weekend_hours numeric(8,2) not null default 0;

alter table pay_payroll_line
    drop constraint if exists ck_pay_payroll_line_attendance_totals;

alter table pay_payroll_line
    add constraint ck_pay_payroll_line_attendance_totals check (
        paid_leave_days >= 0 and paid_sick_days >= 0 and overtime_hours >= 0 and
        night_hours >= 0 and holiday_hours >= 0 and weekend_hours >= 0
    );

commit;
