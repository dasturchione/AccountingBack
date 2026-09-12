begin;

set local lock_timeout = '5s';
set local statement_timeout = '60s';

-- Per-employee advance (first half of month) setting, 1C-style: percentage of the
-- planned salary or a fixed amount. Used to compute the advance vedomost.
alter table pay_employment
    add column if not exists advance_method varchar(10) not null default 'PERCENT',
    add column if not exists advance_value numeric(18,2) not null default 0;

alter table pay_employment
    drop constraint if exists ck_pay_employment_advance_method;
alter table pay_employment
    add constraint ck_pay_employment_advance_method
    check (advance_method in ('PERCENT', 'FIXED'));

alter table pay_employment
    drop constraint if exists ck_pay_employment_advance_value;
alter table pay_employment
    add constraint ck_pay_employment_advance_value
    check (advance_value >= 0);

commit;
