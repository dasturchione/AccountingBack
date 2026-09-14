-- Rewrites the accounting reversals that were written before the storno fix.
--
-- Legacy shape (produced by all 15 document lifecycles): cancelling a document inserted a
-- row with the debit/credit accounts SWAPPED, a POSITIVE amount, doc_date set to the
-- moment of cancellation, quantities swapped and every subkonto moved to the opposite
-- side. The closing balance came out right, but the period turnover of BOTH accounts was
-- inflated by the full document amount, and the correction landed in whatever month the
-- cancellation happened in instead of the month being corrected.
--
-- Target shape (Application.Features.Register.AccountingRegisterEntries
-- .AccountingRegisterEntryReversalFactory): the ORIGINAL correspondence with a NEGATED
-- amount, dated on the original entry's doc_date, quantities negated on their own side,
-- and subkonto rows on the side they were originally recorded on.
--
-- Only rows still in the legacy shape are touched (a reversal row with a positive
-- amount), so re-running the script is a no-op. The pre-migration values are copied into
-- backup tables first, so the conversion can be undone.

begin;

create table if not exists acc_reg_entry_storno_backup_20260914
(
    entry_id bigint not null,
    debit_account_id integer,
    credit_account_id integer,
    amount numeric(18, 2) not null,
    doc_date timestamp without time zone not null,
    debit_quantity numeric(18, 3),
    credit_quantity numeric(18, 3),
    backed_up_at timestamp without time zone not null default now(),
    constraint acc_reg_entry_storno_backup_20260914_pkey primary key (entry_id)
);

create table if not exists acc_reg_entry_subkonto_storno_backup_20260914
(
    subkonto_id bigint not null,
    entry_id bigint not null,
    side character varying(2) not null,
    backed_up_at timestamp without time zone not null default now(),
    constraint acc_reg_entry_subkonto_storno_backup_20260914_pkey primary key (subkonto_id)
);

-- Legacy reversal rows resolved against the entry each one reverses. Everything is taken
-- from the original entry, so the result does not depend on how the legacy row was built.
create temporary table tmp_legacy_storno on commit drop as
select r.id               as reversal_id,
       o.debit_account_id as debit_account_id,
       o.credit_account_id as credit_account_id,
       o.amount           as original_amount,
       o.doc_date         as original_doc_date,
       o.debit_quantity   as original_debit_quantity,
       o.credit_quantity  as original_credit_quantity
from acc_reg_entry r
join acc_reg_entry o on o.id = r.reversal_entry_id
where r.reversal_entry_id is not null
  and r.amount > 0;

insert into acc_reg_entry_storno_backup_20260914
    (entry_id, debit_account_id, credit_account_id, amount, doc_date, debit_quantity, credit_quantity)
select r.id, r.debit_account_id, r.credit_account_id, r.amount, r.doc_date, r.debit_quantity, r.credit_quantity
from acc_reg_entry r
join tmp_legacy_storno t on t.reversal_id = r.id
on conflict (entry_id) do nothing;

insert into acc_reg_entry_subkonto_storno_backup_20260914 (subkonto_id, entry_id, side)
select s.id, s.entry_id, s.side
from acc_reg_entry_subkonto s
join tmp_legacy_storno t on t.reversal_id = s.entry_id
on conflict (subkonto_id) do nothing;

update acc_reg_entry r
set debit_account_id = t.debit_account_id,
    credit_account_id = t.credit_account_id,
    amount = -t.original_amount,
    doc_date = t.original_doc_date,
    debit_quantity = -t.original_debit_quantity,
    credit_quantity = -t.original_credit_quantity
from tmp_legacy_storno t
where r.id = t.reversal_id;

-- The legacy writer flipped each subkonto side along with the accounts; flip it back so
-- every subkonto sits on the side of the account it actually belongs to.
update acc_reg_entry_subkonto s
set side = case s.side
               when 'DR' then 'CR'
               when 'CR' then 'DR'
               else s.side
           end
from tmp_legacy_storno t
where t.reversal_id = s.entry_id;

do $$
declare
    converted bigint;
    orphaned bigint;
    inconsistent bigint;
begin
    select count(*) into converted from tmp_legacy_storno;

    -- reversal_entry_id has no foreign key, so a reversal whose original row is gone
    -- cannot be rebuilt. Report it instead of failing the whole migration.
    select count(*) into orphaned
    from acc_reg_entry r
    where r.reversal_entry_id is not null
      and r.amount > 0
      and not exists (select 1 from acc_reg_entry o where o.id = r.reversal_entry_id);

    select count(*) into inconsistent
    from acc_reg_entry r
    join acc_reg_entry o on o.id = r.reversal_entry_id
    where r.amount <> -o.amount
       or r.doc_date <> o.doc_date
       or r.debit_account_id is distinct from o.debit_account_id
       or r.credit_account_id is distinct from o.credit_account_id;

    if inconsistent > 0 then
        raise exception 'acc_reg_entry storno conversion incomplete: % reversal row(s) still disagree with the entry they reverse', inconsistent;
    end if;

    raise notice 'acc_reg_entry storno conversion: % row(s) converted, % orphaned reversal row(s) left untouched', converted, orphaned;
end
$$;

commit;
