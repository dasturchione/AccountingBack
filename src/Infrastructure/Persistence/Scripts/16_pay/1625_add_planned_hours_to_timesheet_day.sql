begin;

alter table pay_timesheet_line_day
    add column if not exists planned_hours numeric(8,2) not null default 0;

commit;
