begin;

set local lock_timeout = '5s';
set local statement_timeout = '60s';

-- Benefit percentage applied to a paid sick absence when computing the sickness
-- benefit from average earnings (e.g. 60/80/100 by service). Paid leave uses 100.
alter table hr_absence_type
    add column if not exists benefit_percent numeric(5,2) not null default 100;

alter table hr_absence_type
    drop constraint if exists ck_hr_absence_type_benefit_percent;
alter table hr_absence_type
    add constraint ck_hr_absence_type_benefit_percent
    check (benefit_percent >= 0 and benefit_percent <= 100);

commit;
