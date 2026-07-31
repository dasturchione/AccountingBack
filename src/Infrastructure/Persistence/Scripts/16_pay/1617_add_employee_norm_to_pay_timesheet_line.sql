alter table pay_timesheet_line
    add column if not exists norm_work_days numeric(6,2) not null default 0,
    add column if not exists norm_work_hours numeric(8,2) not null default 0;

update pay_timesheet_line line
set norm_work_days = period.norm_work_days,
    norm_work_hours = period.norm_work_hours
from pay_timesheet timesheet
join pay_period period on period.id = timesheet.period_id
where line.timesheet_id = timesheet.id
  and line.norm_work_days = 0
  and line.norm_work_hours = 0;

alter table pay_timesheet_line
    drop constraint if exists ck_pay_timesheet_line_employee_norm;

alter table pay_timesheet_line
    add constraint ck_pay_timesheet_line_employee_norm
        check (norm_work_days >= 0 and norm_work_hours >= 0);
