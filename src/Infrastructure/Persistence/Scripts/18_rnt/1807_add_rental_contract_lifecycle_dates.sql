begin;

alter table rnt_contract
    alter column end_date drop not null,
    add column confirmation_date date,
    add column termination_date date;

update rnt_contract
set confirmation_date = greatest(posted_at::date, contract_date)
where status_id in (2, 3)
  and posted_at is not null;

update rnt_contract
set termination_date = greatest(cancelled_at::date, start_date)
where status_id = 3
  and confirmation_date is not null
  and cancelled_at is not null;

alter table rnt_contract
    add constraint ck_rnt_contract_confirmation_date
        check (confirmation_date is null or confirmation_date >= contract_date),
    add constraint ck_rnt_contract_termination_date
        check (termination_date is null or termination_date >= start_date);

alter table rnt_contract_object
    alter column end_date drop not null;

commit;
