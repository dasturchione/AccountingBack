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

alter table bank_operation
    add column if not exists payment_purpose_id smallint;

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

do $$
begin
    if exists (
        select 1
        from bank_operation
        where payment_purpose_id is null
    ) then
        raise exception 'Cannot enforce bank_operation.payment_purpose_id: legacy rows without payment purpose remain.';
    end if;

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

alter table cash_operation
    add column if not exists payment_purpose_id smallint;

do $$
begin
    if exists (
        select 1
        from cash_operation
        where payment_purpose_id is null
    ) then
        raise exception 'Cannot enforce cash_operation.payment_purpose_id: legacy rows without payment purpose require explicit backfill.';
    end if;

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

    insert into acc_posting_rule_line (id, template_id, order_number, debit_alias, credit_alias, amount_source, is_optional)
    select coalesce(max(line.id), 0) + 1,
           5,
           1,
           'PaymentAccount',
           'CashInTransit',
           'Total',
           true
    from acc_posting_rule_line line
    where not exists (
        select 1
        from acc_posting_rule_line existing
        where existing.template_id = 5
          and existing.debit_alias = 'PaymentAccount'
          and existing.credit_alias = 'CashInTransit'
          and existing.amount_source = 'Total'
    );

    insert into acc_posting_rule_line (id, template_id, order_number, debit_alias, credit_alias, amount_source, is_optional)
    select coalesce(max(line.id), 0) + 1,
           6,
           1,
           'CashInTransit',
           'PaymentAccount',
           'Total',
           true
    from acc_posting_rule_line line
    where not exists (
        select 1
        from acc_posting_rule_line existing
        where existing.template_id = 6
          and existing.debit_alias = 'CashInTransit'
          and existing.credit_alias = 'PaymentAccount'
          and existing.amount_source = 'Total'
    );

    insert into acc_payment_purpose (code, alias_id, name, operation_type_id, requires_counterparty)
    values ('CASH_COLLECTION_SENT', v_alias_id, 'Cash collection to transit', 2, false)
    on conflict (code) do nothing;

    insert into acc_payment_purpose (code, alias_id, name, operation_type_id, requires_counterparty)
    values ('CASH_COLLECTION_RECEIVED', v_alias_id, 'Cash collection from transit', 1, false)
    on conflict (code) do nothing;
end $$;
