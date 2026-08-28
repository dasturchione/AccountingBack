create or replace function cmn_sync_document_registry()
returns trigger
language plpgsql
as
$$
declare
    row_data            jsonb;
    source_id           bigint;
    source_number       varchar(100);
    source_date         timestamp without time zone;
    source_amount       numeric(24, 8);
    source_currency_id  smallint;
    source_status_id    smallint;
    source_state_id     smallint;
    source_created_date timestamp without time zone;
begin
    if tg_op = 'DELETE' then
        source_id := (to_jsonb(old) ->> 'id')::bigint;

        delete from cmn_document_registry
        where document_type_id = tg_argv[0]::smallint
          and document_id = source_id;

        return old;
    end if;

    row_data := to_jsonb(new);
    source_id := (row_data ->> 'id')::bigint;

    if tg_argv[1] <> '' then
        source_number := nullif(btrim(row_data ->> tg_argv[1]), '');
    end if;
    source_number := coalesce(source_number, source_id::varchar);

    source_date := (row_data ->> tg_argv[2])::timestamp without time zone;

    if tg_argv[3] <> '' then
        source_amount := coalesce((row_data ->> tg_argv[3])::numeric(24, 8), 0);
    else
        source_amount := 0;
    end if;

    if tg_argv[4] <> '' then
        source_currency_id := (row_data ->> tg_argv[4])::smallint;
    end if;

    if tg_argv[5] <> '' then
        source_status_id := (row_data ->> tg_argv[5])::smallint;
    end if;

    if tg_argv[6] <> '' then
        source_state_id := coalesce((row_data ->> tg_argv[6])::smallint, 1);
    else
        source_state_id := 1;
    end if;

    source_created_date := coalesce(
        (row_data ->> 'created_date')::timestamp without time zone,
        now());

    insert into cmn_document_registry
    (
        organization_id,
        document_type_id,
        document_id,
        doc_number,
        doc_date,
        amount,
        currency_id,
        status_id,
        state_id,
        created_date,
        updated_date
    )
    values
    (
        (row_data ->> 'organization_id')::int,
        tg_argv[0]::smallint,
        source_id,
        source_number,
        source_date,
        source_amount,
        source_currency_id,
        source_status_id,
        source_state_id,
        source_created_date,
        null
    )
    on conflict (document_type_id, document_id) do update
    set organization_id = excluded.organization_id,
        doc_number = excluded.doc_number,
        doc_date = excluded.doc_date,
        amount = case
            when tg_argv[3] = '' then cmn_document_registry.amount
            else excluded.amount
        end,
        currency_id = excluded.currency_id,
        status_id = excluded.status_id,
        state_id = excluded.state_id,
        updated_date = now();

    return new;
end;
$$;

create or replace function cmn_refresh_document_registry_amount()
returns trigger
language plpgsql
as
$$
declare
    new_parent_id bigint;
    old_parent_id bigint;
    refreshed_amount numeric(24, 8);
begin
    if tg_op <> 'DELETE' then
        new_parent_id := (to_jsonb(new) ->> tg_argv[1])::bigint;
    end if;

    if tg_op <> 'INSERT' then
        old_parent_id := (to_jsonb(old) ->> tg_argv[1])::bigint;
    end if;

    if old_parent_id is not null and old_parent_id is distinct from new_parent_id then
        execute format(
            'select coalesce(sum(%s), 0)::numeric(24, 8) from %I.%I where %I = $1',
            tg_argv[2],
            tg_table_schema,
            tg_table_name,
            tg_argv[1])
        into refreshed_amount
        using old_parent_id;

        update cmn_document_registry
        set amount = refreshed_amount,
            updated_date = now()
        where document_type_id = tg_argv[0]::smallint
          and document_id = old_parent_id;
    end if;

    if new_parent_id is not null then
        execute format(
            'select coalesce(sum(%s), 0)::numeric(24, 8) from %I.%I where %I = $1',
            tg_argv[2],
            tg_table_schema,
            tg_table_name,
            tg_argv[1])
        into refreshed_amount
        using new_parent_id;

        update cmn_document_registry
        set amount = refreshed_amount,
            updated_date = now()
        where document_type_id = tg_argv[0]::smallint
          and document_id = new_parent_id;
    end if;

    if tg_op = 'DELETE' then
        return old;
    end if;

    return new;
end;
$$;

-- Existing documents.
insert into cmn_document_registry
    (organization_id, document_type_id, document_id, doc_number, doc_date, amount, currency_id, status_id, state_id, created_date)
select organization_id, 1, id, doc_number, doc_date, final_amount, currency_id, status_id, state_id, created_date from pur_doc
union all
select organization_id, 2, id, doc_number, doc_date, final_amount, currency_id, status_id, state_id, created_date from sale_doc
union all
select organization_id, 3, id, doc_number, doc_date, amount, currency_id, status_id, state_id, created_date from bank_operation
union all
select organization_id, 4, id, doc_number, doc_date, amount, currency_id, status_id, state_id, created_date from cash_operation
union all
select organization_id, 5, id, doc_number, doc_date, payable_amount, currency_id, status_id, state_id, created_date from pay_payroll_doc
union all
select organization_id, 7, id, doc_number, doc_date, final_amount, currency_id, status_id, state_id, created_date from rtl_sale_doc
union all
select organization_id, 8, id, doc_number, doc_date, 0, null, status_id, state_id, created_date from inv_inventory_adjustment_doc
union all
select organization_id, 9, id, doc_number, doc_date,
       coalesce((select sum(line.counted_quantity * line.default_cost_price) from inv_inventory_count_line line where line.owner_id = doc.id), 0),
       null, status_id, state_id, created_date
from inv_inventory_count_doc doc
union all
select organization_id, 10, id, id::varchar, revaluation_date,
       coalesce((select sum(case when line.state_id = 1 then line.difference_amount else 0 end) from cmn_currency_revaluation_line line where line.revaluation_id = doc.id), 0),
       null, status_id, state_id, created_date
from cmn_currency_revaluation doc
union all
select organization_id, 11, id, doc_number, doc_date, final_amount, currency_id, status_id, state_id, created_date from fa_receipt_doc
union all
select organization_id, 12, id, doc_number, doc_date, 0, null, status_id, state_id, created_date from fa_movement_doc
union all
select organization_id, 13, id, doc_number, period_month,
       coalesce((select sum(line.amount) from fa_depreciation_run_line line where line.depreciation_run_id = doc.id), 0),
       null, status_id, state_id, created_date
from fa_depreciation_run doc
union all
select organization_id, 14, id, doc_number, disposal_date,
       coalesce((select sum(line.sale_amount) from fa_disposal_doc_line line where line.disposal_doc_id = doc.id), 0),
       null, status_id, state_id, created_date
from fa_disposal_doc doc
union all
select organization_id, 15, id, doc_number, revaluation_date,
       coalesce((select sum(line.revaluation_amount) from fa_revaluation_doc_line line where line.revaluation_doc_id = doc.id), 0),
       null, status_id, state_id, created_date
from fa_revaluation_doc doc
union all
select organization_id, 16, id, doc_number, doc_date, total_amount, null, status_id, state_id, created_date from inv_opening_inventory
union all
select organization_id, 17, id, doc_number, doc_date,
       coalesce((select sum(line.capitalized_amount) from fa_commissioning_doc_line line where line.commissioning_doc_id = doc.id), 0),
       null, status_id, state_id, created_date
from fa_commissioning_doc doc
union all
select organization_id, 18, id, doc_number, doc_date, 0, null, status_id, state_id, created_date from inv_transfer_doc
union all
select organization_id, 19, id, coalesce(nullif(btrim(doc_number), ''), id::varchar), doc_date, 0, null, status_id, 1, created_date from sale_shipment_doc
union all
select organization_id, 20, id, doc_number, doc_date, 0, null, status_id, state_id, created_date from pay_timesheet
union all
select organization_id, 21, id, doc_number, doc_date, total_amount, currency_id, status_id, state_id, created_date from pay_payment_batch
union all
select organization_id, 22, id, doc_number, doc_date::timestamp without time zone, 0, null, null, state_id, created_date from hr_absence
union all
select organization_id, 23, id, doc_number, doc_date, amount, currency_id, status_id, state_id, created_date from cash_fiscal_transfer_doc
union all
select organization_id, 24, id, doc_number, doc_date, amount, currency_id, status_id, state_id, created_date from cash_collection_doc
where true
on conflict (document_type_id, document_id) do update
set organization_id = excluded.organization_id,
    doc_number = excluded.doc_number,
    doc_date = excluded.doc_date,
    amount = excluded.amount,
    currency_id = excluded.currency_id,
    status_id = excluded.status_id,
    state_id = excluded.state_id,
    updated_date = now();

-- Header synchronization. Arguments:
-- document type, number column, date column, amount column, currency column,
-- status column, state column.
create trigger trg_pur_doc_document_registry after insert or update or delete on pur_doc
    for each row execute function cmn_sync_document_registry('1', 'doc_number', 'doc_date', 'final_amount', 'currency_id', 'status_id', 'state_id');
create trigger trg_sale_doc_document_registry after insert or update or delete on sale_doc
    for each row execute function cmn_sync_document_registry('2', 'doc_number', 'doc_date', 'final_amount', 'currency_id', 'status_id', 'state_id');
create trigger trg_bank_operation_document_registry after insert or update or delete on bank_operation
    for each row execute function cmn_sync_document_registry('3', 'doc_number', 'doc_date', 'amount', 'currency_id', 'status_id', 'state_id');
create trigger trg_cash_operation_document_registry after insert or update or delete on cash_operation
    for each row execute function cmn_sync_document_registry('4', 'doc_number', 'doc_date', 'amount', 'currency_id', 'status_id', 'state_id');
create trigger trg_pay_payroll_doc_document_registry after insert or update or delete on pay_payroll_doc
    for each row execute function cmn_sync_document_registry('5', 'doc_number', 'doc_date', 'payable_amount', 'currency_id', 'status_id', 'state_id');
create trigger trg_rtl_sale_doc_document_registry after insert or update or delete on rtl_sale_doc
    for each row execute function cmn_sync_document_registry('7', 'doc_number', 'doc_date', 'final_amount', 'currency_id', 'status_id', 'state_id');
create trigger trg_inv_inventory_adjustment_doc_document_registry after insert or update or delete on inv_inventory_adjustment_doc
    for each row execute function cmn_sync_document_registry('8', 'doc_number', 'doc_date', '', '', 'status_id', 'state_id');
create trigger trg_inv_inventory_count_doc_document_registry after insert or update or delete on inv_inventory_count_doc
    for each row execute function cmn_sync_document_registry('9', 'doc_number', 'doc_date', '', '', 'status_id', 'state_id');
create trigger trg_cmn_currency_revaluation_document_registry after insert or update or delete on cmn_currency_revaluation
    for each row execute function cmn_sync_document_registry('10', '', 'revaluation_date', '', '', 'status_id', 'state_id');
create trigger trg_fa_receipt_doc_document_registry after insert or update or delete on fa_receipt_doc
    for each row execute function cmn_sync_document_registry('11', 'doc_number', 'doc_date', 'final_amount', 'currency_id', 'status_id', 'state_id');
create trigger trg_fa_movement_doc_document_registry after insert or update or delete on fa_movement_doc
    for each row execute function cmn_sync_document_registry('12', 'doc_number', 'doc_date', '', '', 'status_id', 'state_id');
create trigger trg_fa_depreciation_run_document_registry after insert or update or delete on fa_depreciation_run
    for each row execute function cmn_sync_document_registry('13', 'doc_number', 'period_month', '', '', 'status_id', 'state_id');
create trigger trg_fa_disposal_doc_document_registry after insert or update or delete on fa_disposal_doc
    for each row execute function cmn_sync_document_registry('14', 'doc_number', 'disposal_date', '', '', 'status_id', 'state_id');
create trigger trg_fa_revaluation_doc_document_registry after insert or update or delete on fa_revaluation_doc
    for each row execute function cmn_sync_document_registry('15', 'doc_number', 'revaluation_date', '', '', 'status_id', 'state_id');
create trigger trg_inv_opening_inventory_document_registry after insert or update or delete on inv_opening_inventory
    for each row execute function cmn_sync_document_registry('16', 'doc_number', 'doc_date', 'total_amount', '', 'status_id', 'state_id');
create trigger trg_fa_commissioning_doc_document_registry after insert or update or delete on fa_commissioning_doc
    for each row execute function cmn_sync_document_registry('17', 'doc_number', 'doc_date', '', '', 'status_id', 'state_id');
create trigger trg_inv_transfer_doc_document_registry after insert or update or delete on inv_transfer_doc
    for each row execute function cmn_sync_document_registry('18', 'doc_number', 'doc_date', '', '', 'status_id', 'state_id');
create trigger trg_sale_shipment_doc_document_registry after insert or update or delete on sale_shipment_doc
    for each row execute function cmn_sync_document_registry('19', 'doc_number', 'doc_date', '', '', 'status_id', '');
create trigger trg_pay_timesheet_document_registry after insert or update or delete on pay_timesheet
    for each row execute function cmn_sync_document_registry('20', 'doc_number', 'doc_date', '', '', 'status_id', 'state_id');
create trigger trg_pay_payment_batch_document_registry after insert or update or delete on pay_payment_batch
    for each row execute function cmn_sync_document_registry('21', 'doc_number', 'doc_date', 'total_amount', 'currency_id', 'status_id', 'state_id');
create trigger trg_hr_absence_document_registry after insert or update or delete on hr_absence
    for each row execute function cmn_sync_document_registry('22', 'doc_number', 'doc_date', '', '', '', 'state_id');
create trigger trg_cash_fiscal_transfer_doc_document_registry after insert or update or delete on cash_fiscal_transfer_doc
    for each row execute function cmn_sync_document_registry('23', 'doc_number', 'doc_date', 'amount', 'currency_id', 'status_id', 'state_id');
create trigger trg_cash_collection_doc_document_registry after insert or update or delete on cash_collection_doc
    for each row execute function cmn_sync_document_registry('24', 'doc_number', 'doc_date', 'amount', 'currency_id', 'status_id', 'state_id');

-- Monetary amounts stored in document lines.
create trigger trg_inv_inventory_count_line_document_registry_amount after insert or update or delete on inv_inventory_count_line
    for each row execute function cmn_refresh_document_registry_amount('9', 'owner_id', 'counted_quantity * default_cost_price');
create trigger trg_cmn_currency_revaluation_line_document_registry_amount after insert or update or delete on cmn_currency_revaluation_line
    for each row execute function cmn_refresh_document_registry_amount('10', 'revaluation_id', 'case when state_id = 1 then difference_amount else 0 end');
create trigger trg_fa_depreciation_run_line_document_registry_amount after insert or update or delete on fa_depreciation_run_line
    for each row execute function cmn_refresh_document_registry_amount('13', 'depreciation_run_id', 'amount');
create trigger trg_fa_disposal_doc_line_document_registry_amount after insert or update or delete on fa_disposal_doc_line
    for each row execute function cmn_refresh_document_registry_amount('14', 'disposal_doc_id', 'sale_amount');
create trigger trg_fa_revaluation_doc_line_document_registry_amount after insert or update or delete on fa_revaluation_doc_line
    for each row execute function cmn_refresh_document_registry_amount('15', 'revaluation_doc_id', 'revaluation_amount');
create trigger trg_fa_commissioning_doc_line_document_registry_amount after insert or update or delete on fa_commissioning_doc_line
    for each row execute function cmn_refresh_document_registry_amount('17', 'commissioning_doc_id', 'capitalized_amount');
