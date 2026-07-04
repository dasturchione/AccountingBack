do $$
begin
    if exists (
        select 1
        from acc_chart_account
        group by code
        having count(*) > 1
    ) then
        raise exception 'Cannot add uq_acc_chart_account_code: duplicate acc_chart_account.code values exist.';
    end if;

    if not exists (
        select 1
        from pg_constraint
        where conname = 'uq_acc_chart_account_code'
    ) then
        alter table acc_chart_account
            add constraint uq_acc_chart_account_code unique (code);
    end if;
end $$;

create index if not exists idx_acc_reg_entry_org_debit_docdate_id
    on acc_reg_entry using btree (organization_id, debit_account_id, doc_date, id);

create index if not exists idx_acc_reg_entry_org_credit_docdate_id
    on acc_reg_entry using btree (organization_id, credit_account_id, doc_date, id);

create index if not exists idx_acc_reg_entry_org_docdate_debit_account
    on acc_reg_entry using btree (organization_id, doc_date, debit_account_id);

create index if not exists idx_acc_reg_entry_org_docdate_credit_account
    on acc_reg_entry using btree (organization_id, doc_date, credit_account_id);

-- =====================================================================
-- Bank & cash operations: required header payment_purpose_id.
-- Rows created before the column existed (and any without derivable
-- purpose) are backfilled with a neutral system purpose so the FK and
-- NOT NULL can be enforced without aborting the migration or losing data.
-- Reassign the correct purposes from the UI afterwards.
-- =====================================================================

-- Neutral system payment purposes. IN uses the Customer alias, OUT uses the
-- Supplier alias — both are in the supported posting-alias set and need no
-- counterparty, so a legacy draft can still be confirmed after reassignment.
insert into acc_payment_purpose (code, alias_id, name, operation_type_id, requires_counterparty)
select 'UNSPECIFIED_IN', a.id, 'Назначение не указано (приход)', 1, false
from acc_posting_alias a
where a.code = 'Customer'
on conflict (code) do nothing;

insert into acc_payment_purpose (code, alias_id, name, operation_type_id, requires_counterparty)
select 'UNSPECIFIED_OUT', a.id, 'Назначение не указано (расход)', 2, false
from acc_posting_alias a
where a.code = 'Supplier'
on conflict (code) do nothing;

-- ---- bank_operation ----
alter table bank_operation
    add column if not exists payment_purpose_id smallint;

-- 1) derive the header purpose from operation lines where present
update bank_operation bo
set payment_purpose_id = source.payment_purpose_id
from (
    select distinct on (bank_operation_id)
           bank_operation_id,
           payment_purpose_id
    from bank_operation_line
    order by bank_operation_id, order_number, id
) source
where bo.id = source.bank_operation_id
  and bo.payment_purpose_id is null;

-- 2) neutral fallback for remaining legacy rows, by operation direction
update bank_operation bo
set payment_purpose_id = pp.id
from acc_payment_purpose pp
where bo.payment_purpose_id is null
  and pp.code = case when bo.operation_type_id = 1 then 'UNSPECIFIED_IN' else 'UNSPECIFIED_OUT' end;

do $$
begin
    if not exists (
        select 1
        from pg_constraint
        where conname = 'fk_bank_operation_payment_purpose'
    ) then
        alter table bank_operation
            add constraint fk_bank_operation_payment_purpose
            foreign key (payment_purpose_id) references acc_payment_purpose(id);
    end if;
end $$;

alter table bank_operation
    alter column payment_purpose_id set not null;

create index if not exists idx_bank_operation_payment_purpose_id
    on bank_operation using btree (payment_purpose_id);

-- ---- cash_operation ----
alter table cash_operation
    add column if not exists payment_purpose_id smallint;

-- neutral fallback for legacy rows, by operation direction (IN → приход, else → расход)
update cash_operation co
set payment_purpose_id = pp.id
from acc_payment_purpose pp
where co.payment_purpose_id is null
  and pp.code = case when co.operation_type_id = 1 then 'UNSPECIFIED_IN' else 'UNSPECIFIED_OUT' end;

do $$
begin
    if not exists (
        select 1
        from pg_constraint
        where conname = 'fk_cash_operation_payment_purpose'
    ) then
        alter table cash_operation
            add constraint fk_cash_operation_payment_purpose
            foreign key (payment_purpose_id) references acc_payment_purpose(id);
    end if;
end $$;

alter table cash_operation
    alter column payment_purpose_id set not null;

create index if not exists idx_cash_operation_payment_purpose_id
    on cash_operation using btree (payment_purpose_id);

do $$
declare
    v_chart_account_id integer;
    v_alias_id smallint;
begin
    if not exists (
        select 1
        from acc_chart_account
        where code = '5710'
    ) then
        insert into acc_chart_account (
            id,
            parent_id,
            code,
            name,
            is_group,
            state_id,
            created_date,
            account_type_id,
            is_quantity,
            is_currency
        )
        select coalesce(max(id), 0) + 1,
               null,
               '5710',
               'Переводы в пути',
               false,
               1,
               now(),
               1,
               false,
               false
        from acc_chart_account;
    end if;

    select id
    into v_chart_account_id
    from acc_chart_account
    where code = '5710';

    insert into acc_posting_alias (code, name)
    values ('CashInTransit', 'Денежные средства в пути')
    on conflict (code) do nothing;

    select id
    into v_alias_id
    from acc_posting_alias
    where code = 'CashInTransit';

    insert into acc_posting_alias_translation (posting_alias_id, language_id, name)
    values
        (v_alias_id, 1, 'Yo''ldagi pul mablag''lari'),
        (v_alias_id, 2, 'Денежные средства в пути'),
        (v_alias_id, 3, 'Cash in transit')
    on conflict do nothing;

    insert into acc_account_resolve_rule (policy_id, alias, dimension_key, dimension_value, account_id, priority)
    select policy.id, 'CashInTransit', '_none', '_default', v_chart_account_id, 100
    from acc_accounting_policy policy
    where not exists (
        select 1
        from acc_account_resolve_rule rule
        where rule.policy_id = policy.id
          and rule.alias = 'CashInTransit'
          and rule.dimension_key = '_none'
          and rule.dimension_value = '_default'
    );

    insert into acc_posting_rule_line (id, template_id, order_number, debit_alias_id, credit_alias_id, amount_source, is_optional)
    select coalesce(max(line.id), 0) + 1,
           5,
           1,
           (select id from acc_posting_alias where code = 'PaymentAccount'),
           v_alias_id,
           'Total',
           true
    from acc_posting_rule_line line
    where not exists (
        select 1
        from acc_posting_rule_line existing
        where existing.template_id = 5
          and existing.debit_alias_id = (select id from acc_posting_alias where code = 'PaymentAccount')
          and existing.credit_alias_id = v_alias_id
          and existing.amount_source = 'Total'
    );

    insert into acc_posting_rule_line (id, template_id, order_number, debit_alias_id, credit_alias_id, amount_source, is_optional)
    select coalesce(max(line.id), 0) + 1,
           6,
           1,
           v_alias_id,
           (select id from acc_posting_alias where code = 'PaymentAccount'),
           'Total',
           true
    from acc_posting_rule_line line
    where not exists (
        select 1
        from acc_posting_rule_line existing
        where existing.template_id = 6
          and existing.debit_alias_id = v_alias_id
          and existing.credit_alias_id = (select id from acc_posting_alias where code = 'PaymentAccount')
          and existing.amount_source = 'Total'
    );

    insert into acc_payment_purpose (code, alias_id, name, operation_type_id, requires_counterparty)
    values ('CASH_COLLECTION_SENT', v_alias_id, 'Cash collection to transit', 2, false)
    on conflict (code) do nothing;

    insert into acc_payment_purpose (code, alias_id, name, operation_type_id, requires_counterparty)
    values ('CASH_COLLECTION_RECEIVED', v_alias_id, 'Cash collection from transit', 1, false)
    on conflict (code) do nothing;
end $$;

-- =====================================================================
-- Posting account fix: resolve rules must select POSTABLE (leaf) accounts.
-- Group (header) accounts aggregate their children and cannot receive a
-- direct posting, which caused Purchase/Sale Confirm to fail with
-- AccountingPosting.GroupAccountNotPostable (e.g. group account 4410).
-- =====================================================================

-- 1) Accounts flagged as groups but without any child accounts are in fact
--    postable synthetic accounts — clear the erroneous group flag.
update acc_chart_account
set is_group = false
where code in ('6710', '6810', '6970')
  and is_group = true;

-- 2) Repoint single-account aliases from group accounts to their postable
--    leaf subaccount (main taxation system where several subaccounts exist).
update acc_account_resolve_rule r set account_id = a.id
from acc_chart_account a where a.code = '6410.1' and r.alias = 'VATOut'          and r.account_id <> a.id;
update acc_account_resolve_rule r set account_id = a.id
from acc_chart_account a where a.code = '9020.1' and r.alias = 'SalesRevenue'    and r.account_id <> a.id;
update acc_account_resolve_rule r set account_id = a.id
from acc_chart_account a where a.code = '9030.1' and r.alias = 'ServiceRevenue'  and r.account_id <> a.id;
update acc_account_resolve_rule r set account_id = a.id
from acc_chart_account a where a.code = '9120.1' and r.alias = 'CostOfGoods'     and r.account_id <> a.id;
update acc_account_resolve_rule r set account_id = a.id
from acc_chart_account a where a.code = '9130.1' and r.alias = 'CostOfService'   and r.account_id <> a.id;
update acc_account_resolve_rule r set account_id = a.id
from acc_chart_account a where a.code = '6410.1' and r.alias = 'TaxVAT'          and r.account_id <> a.id;
update acc_account_resolve_rule r set account_id = a.id
from acc_chart_account a where a.code = '6420.1' and r.alias = 'TaxNDFL'         and r.account_id <> a.id;
update acc_account_resolve_rule r set account_id = a.id
from acc_chart_account a where a.code = '6510.1' and r.alias = 'SocialInsurance' and r.account_id <> a.id;
update acc_account_resolve_rule r set account_id = a.id
from acc_chart_account a where a.code = '6530.1' and r.alias = 'PensionFund'     and r.account_id <> a.id;

-- 3) Input VAT (4410 is a group): resolve per purchase kind onto postable
--    subaccounts — goods/MPZ → 4410.3, services → 4410.4, default → 4410.3.
delete from acc_account_resolve_rule where alias = 'VATIn';
insert into acc_account_resolve_rule (policy_id, alias, dimension_key, dimension_value, account_id, priority)
select 1, 'VATIn', 'vatKind', 'goods',    id, 10  from acc_chart_account where code = '4410.3'
    union all
select 1, 'VATIn', 'vatKind', 'services', id, 10  from acc_chart_account where code = '4410.4'
    union all
select 1, 'VATIn', 'vatKind', '_default', id, 100 from acc_chart_account where code = '4410.3';
