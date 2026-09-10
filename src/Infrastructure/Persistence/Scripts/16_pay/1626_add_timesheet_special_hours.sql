begin;

alter table pay_timesheet_line
    add column if not exists night_hours numeric(8,2) not null default 0,
    add column if not exists holiday_hours numeric(8,2) not null default 0,
    add column if not exists weekend_hours numeric(8,2) not null default 0;

alter table pay_timesheet_line_day
    add column if not exists overtime_hours numeric(8,2) not null default 0,
    add column if not exists night_hours numeric(8,2) not null default 0,
    add column if not exists holiday_hours numeric(8,2) not null default 0,
    add column if not exists weekend_hours numeric(8,2) not null default 0;

commit;
