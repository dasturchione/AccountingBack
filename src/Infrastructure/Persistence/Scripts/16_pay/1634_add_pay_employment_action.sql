begin;

set local lock_timeout = '5s';
set local statement_timeout = '60s';

-- Turn pay_employment into an interval-versioned personnel record. Every
-- personnel action (hire / transfer / pay change / dismissal) now opens a new
-- interval instead of overwriting the previous one, so history is preserved.
alter table pay_employment
    add column if not exists action_type varchar(20) not null default 'HIRE',
    add column if not exists note varchar(500),
    add column if not exists created_by_user_id integer references sys_user(id),
    add column if not exists updated_by_user_id integer references sys_user(id);

-- Backfill: the earliest interval per employee is the hire; later intervals are
-- treated as transfers (the closest existing semantic before this migration).
with ranked as (
    select id,
           row_number() over (partition by employee_id order by start_date, id) as rn
    from pay_employment
)
update pay_employment e
set action_type = case when r.rn = 1 then 'HIRE' else 'TRANSFER' end
from ranked r
where e.id = r.id;

alter table pay_employment
    drop constraint if exists ck_pay_employment_action_type;

alter table pay_employment
    add constraint ck_pay_employment_action_type
    check (action_type in ('HIRE', 'TRANSFER', 'PAY_CHANGE', 'DISMISSAL'));

create index if not exists idx_pay_employment_created_by_user_id
    on pay_employment (created_by_user_id);

commit;
