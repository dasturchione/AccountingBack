begin;

set local lock_timeout = '5s';
set local statement_timeout = '60s';

alter table pay_period_work_day
    add column if not exists day_type varchar(20) not null default 'NORMAL',
    add column if not exists is_work_day boolean not null default true,
    add column if not exists work_hours numeric(8,4);

update pay_period_work_day work_day
set work_hours = period.daily_work_hours
from pay_period period
where work_day.period_id = period.id
  and work_day.work_hours is null;

alter table pay_period_work_day
    alter column work_hours set not null;

do $$
begin
    if not exists (
        select 1 from pg_constraint
        where conname = 'ck_pay_period_work_day_type'
          and conrelid = 'pay_period_work_day'::regclass
    ) then
        alter table pay_period_work_day
            add constraint ck_pay_period_work_day_type
            check (day_type in ('NORMAL', 'HOLIDAY', 'TRANSFERRED', 'SHORTENED'));
    end if;
    if not exists (
        select 1 from pg_constraint
        where conname = 'ck_pay_period_work_day_hours'
          and conrelid = 'pay_period_work_day'::regclass
    ) then
        alter table pay_period_work_day
            add constraint ck_pay_period_work_day_hours
            check (work_hours >= 0 and work_hours <= 24);
    end if;
end $$;

commit;
