begin;

alter table pay_timesheet_line
    add column if not exists paid_leave_days numeric(6,2) not null default 0,
    add column if not exists paid_sick_days numeric(6,2) not null default 0;

commit;
