begin;

alter table pay_timesheet_line_day
    add column if not exists worked_hours numeric(8,2) default 0;

do $$
begin
    if exists (
        select 1
        from pay_timesheet_line_day day
        join pay_timesheet_line line on line.id = day.timesheet_line_id
        join pay_timesheet timesheet on timesheet.id = line.timesheet_id
        join pay_period period on period.id = timesheet.period_id
        where day.status_code = 'WORKED'
          and period.daily_work_hours is null
    ) then
        raise exception 'Cannot initialize worked_hours because one or more payroll periods have no daily_work_hours.';
    end if;
end $$;

update pay_timesheet_line_day day
set worked_hours = period.daily_work_hours
from pay_timesheet_line line
join pay_timesheet timesheet on timesheet.id = line.timesheet_id
join pay_period period on period.id = timesheet.period_id
where day.timesheet_line_id = line.id
  and day.status_code = 'WORKED'
  and coalesce(day.worked_hours, 0) = 0;

update pay_timesheet_line_day
set worked_hours = 0
where status_code <> 'WORKED'
  and worked_hours is null;

alter table pay_timesheet_line_day
    alter column worked_hours set default 0,
    alter column worked_hours set not null;

do $$
begin
    if not exists (
        select 1
        from pg_constraint
        where conname = 'ck_pay_timesheet_line_day_worked_hours'
          and conrelid = 'pay_timesheet_line_day'::regclass
    ) then
        alter table pay_timesheet_line_day
            add constraint ck_pay_timesheet_line_day_worked_hours check (
                (status_code = 'WORKED' and worked_hours >= 1) or
                (status_code <> 'WORKED' and worked_hours = 0)
            );
    end if;
end $$;

commit;
