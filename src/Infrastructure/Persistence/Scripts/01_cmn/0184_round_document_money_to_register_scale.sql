-- Rounds document money to the two decimals the accounting register stores.
--
-- Document amount columns are numeric(24,8) but acc_reg_entry.amount is numeric(18,2). VAT
-- used to be computed to eight decimals, so a posted line reached the ledger rounded while
-- the document kept the longer number: the document and its own postings disagreed, and the
-- difference accumulated over a document against the ЭСФ.
--
-- The code now decides money once, at two decimals (SharedKernel.Money.DocumentMoney). This
-- brings the rows written before that fix to the same scale:
--   * amount and vat_amount are rounded, and total_amount is rebuilt from them, so each line
--     stays internally consistent;
--   * the VAT of a line's marked items is redistributed with the same largest-remainder rule
--     DocumentMoney.Distribute uses, so the items still add up to their line;
--   * document headers are rebuilt from the rounded lines.
--
-- Per-unit values (unit_price, cost_price, price) are deliberately left alone: an average
-- cost price is legitimately fractional, and it is the amount it produces that is money.
-- acc_reg_entry is not touched either — its column already holds the rounded value, which is
-- exactly what the documents are being brought into line with.
--
-- Rows already at two decimals are not written, so re-running the script changes nothing.

begin;

create table if not exists doc_money_rounding_backup_20260914
(
    table_name character varying(64) not null,
    row_id bigint not null,
    amount numeric(24, 8),
    vat_amount numeric(24, 8),
    total_amount numeric(24, 8),
    final_amount numeric(24, 8),
    backed_up_at timestamp without time zone not null default now(),
    constraint doc_money_rounding_backup_20260914_pkey primary key (table_name, row_id)
);

do $$
declare
    line_table text;
    item_table text;
    header_table text;
    families text[][] := array[
        ['sale_doc_product', 'sale_doc_table', 'sale_doc'],
        ['pur_doc_product', 'pur_doc_table', 'pur_doc'],
        ['rtl_sale_doc_product', 'rtl_sale_doc_table', 'rtl_sale_doc']
    ];
    family text[];
begin
    foreach family slice 1 in array families
    loop
        line_table := family[1];
        item_table := family[2];
        header_table := family[3];

        -- 1. Lines.
        execute format($f$
            insert into doc_money_rounding_backup_20260914 (table_name, row_id, amount, vat_amount, total_amount)
            select %L, id, amount, vat_amount, total_amount
            from %I
            where amount <> round(amount, 2)
               or vat_amount <> round(vat_amount, 2)
               or total_amount <> round(amount, 2) + round(vat_amount, 2)
            on conflict (table_name, row_id) do nothing
        $f$, line_table, line_table);

        execute format($f$
            update %I
            set amount = round(amount, 2),
                vat_amount = round(vat_amount, 2),
                total_amount = round(amount, 2) + round(vat_amount, 2)
            where amount <> round(amount, 2)
               or vat_amount <> round(vat_amount, 2)
               or total_amount <> round(amount, 2) + round(vat_amount, 2)
        $f$, line_table);

        -- 2. Marked items: split each line's VAT so the parts add up to it again.
        execute format($f$
            insert into doc_money_rounding_backup_20260914 (table_name, row_id, amount, vat_amount, total_amount)
            select %L, id, amount, vat_amount, total_amount from %I
            on conflict (table_name, row_id) do nothing
        $f$, item_table, item_table);

        execute format($f$
            with numbered as (
                select t.id,
                       row_number() over (partition by t.owner_id order by t.id) as position,
                       count(*) over (partition by t.owner_id) as item_count,
                       p.vat_amount as line_vat,
                       t.amount as item_amount
                from %I t
                join %I p on p.id = t.owner_id
            ),
            shares as (
                select id,
                       round(item_amount, 2) as new_amount,
                       trunc(line_vat / item_count, 2)
                           + case
                                 when position <= round((line_vat - trunc(line_vat / item_count, 2) * item_count) * 100)
                                 then 0.01
                                 else 0
                             end as new_vat
                from numbered
            )
            update %I t
            set amount = s.new_amount,
                vat_amount = s.new_vat,
                total_amount = s.new_amount + s.new_vat
            from shares s
            where t.id = s.id
              and (t.amount <> s.new_amount
                or t.vat_amount <> s.new_vat
                or t.total_amount <> s.new_amount + s.new_vat)
        $f$, item_table, line_table, item_table);

        -- 3. Headers, rebuilt from the rounded lines.
        execute format($f$
            insert into doc_money_rounding_backup_20260914 (table_name, row_id, total_amount, vat_amount, final_amount)
            select %L, id, total_amount, vat_amount, final_amount from %I
            on conflict (table_name, row_id) do nothing
        $f$, header_table, header_table);

        execute format($f$
            with totals as (
                select owner_id,
                       sum(amount) as total_amount,
                       sum(vat_amount) as vat_amount,
                       sum(total_amount) as final_amount
                from %I
                group by owner_id
            )
            update %I d
            set total_amount = t.total_amount,
                vat_amount = t.vat_amount,
                final_amount = t.final_amount
            from totals t
            where d.id = t.owner_id
              and (d.total_amount <> t.total_amount
                or d.vat_amount <> t.vat_amount
                or d.final_amount <> t.final_amount)
        $f$, line_table, header_table);
    end loop;
end
$$;

-- Fixed-asset receipts keep the same shape but link their lines by receipt_doc_id.
insert into doc_money_rounding_backup_20260914 (table_name, row_id, amount, vat_amount, total_amount)
select 'fa_receipt_doc_line', id, amount, vat_amount, total_amount
from fa_receipt_doc_line
where amount <> round(amount, 2)
   or vat_amount <> round(vat_amount, 2)
   or total_amount <> round(amount, 2) + round(vat_amount, 2)
on conflict (table_name, row_id) do nothing;

update fa_receipt_doc_line
set amount = round(amount, 2),
    vat_amount = round(vat_amount, 2),
    total_amount = round(amount, 2) + round(vat_amount, 2)
where amount <> round(amount, 2)
   or vat_amount <> round(vat_amount, 2)
   or total_amount <> round(amount, 2) + round(vat_amount, 2);

insert into doc_money_rounding_backup_20260914 (table_name, row_id, total_amount, vat_amount, final_amount)
select 'fa_receipt_doc', id, total_amount, vat_amount, final_amount from fa_receipt_doc
on conflict (table_name, row_id) do nothing;

with totals as (
    select receipt_doc_id,
           sum(amount) as total_amount,
           sum(vat_amount) as vat_amount,
           sum(total_amount) as final_amount
    from fa_receipt_doc_line
    group by receipt_doc_id
)
update fa_receipt_doc d
set total_amount = t.total_amount,
    vat_amount = t.vat_amount,
    final_amount = t.final_amount
from totals t
where d.id = t.receipt_doc_id
  and (d.total_amount <> t.total_amount
    or d.vat_amount <> t.vat_amount
    or d.final_amount <> t.final_amount);

do $$
declare
    unrounded bigint;
    line_mismatch bigint;
    item_mismatch bigint;
    header_mismatch bigint;
    payment_mismatch bigint;
begin
    select count(*) into unrounded
    from (
        select amount, vat_amount, total_amount from sale_doc_product
        union all select amount, vat_amount, total_amount from sale_doc_table
        union all select amount, vat_amount, total_amount from pur_doc_product
        union all select amount, vat_amount, total_amount from pur_doc_table
        union all select amount, vat_amount, total_amount from rtl_sale_doc_product
        union all select amount, vat_amount, total_amount from rtl_sale_doc_table
        union all select amount, vat_amount, total_amount from fa_receipt_doc_line
    ) rows
    where amount <> round(amount, 2)
       or vat_amount <> round(vat_amount, 2)
       or total_amount <> round(total_amount, 2);

    select count(*) into line_mismatch
    from (
        select amount, vat_amount, total_amount from sale_doc_product
        union all select amount, vat_amount, total_amount from pur_doc_product
        union all select amount, vat_amount, total_amount from rtl_sale_doc_product
        union all select amount, vat_amount, total_amount from fa_receipt_doc_line
    ) rows
    where total_amount <> amount + vat_amount;

    -- Every line's marked items must add back up to the line's VAT.
    select count(*) into item_mismatch
    from (
        select p.id, p.vat_amount, coalesce(sum(t.vat_amount), 0) as item_vat
        from sale_doc_product p join sale_doc_table t on t.owner_id = p.id
        group by p.id, p.vat_amount
        union all
        select p.id, p.vat_amount, coalesce(sum(t.vat_amount), 0)
        from pur_doc_product p join pur_doc_table t on t.owner_id = p.id
        group by p.id, p.vat_amount
        union all
        select p.id, p.vat_amount, coalesce(sum(t.vat_amount), 0)
        from rtl_sale_doc_product p join rtl_sale_doc_table t on t.owner_id = p.id
        group by p.id, p.vat_amount
    ) lines
    where vat_amount <> item_vat;

    select count(*) into header_mismatch
    from (
        select d.id, d.total_amount, d.vat_amount, d.final_amount,
               sum(p.amount) as line_total, sum(p.vat_amount) as line_vat, sum(p.total_amount) as line_final
        from sale_doc d join sale_doc_product p on p.owner_id = d.id
        group by d.id, d.total_amount, d.vat_amount, d.final_amount
        union all
        select d.id, d.total_amount, d.vat_amount, d.final_amount,
               sum(p.amount), sum(p.vat_amount), sum(p.total_amount)
        from pur_doc d join pur_doc_product p on p.owner_id = d.id
        group by d.id, d.total_amount, d.vat_amount, d.final_amount
        union all
        select d.id, d.total_amount, d.vat_amount, d.final_amount,
               sum(p.amount), sum(p.vat_amount), sum(p.total_amount)
        from rtl_sale_doc d join rtl_sale_doc_product p on p.owner_id = d.id
        group by d.id, d.total_amount, d.vat_amount, d.final_amount
        union all
        select d.id, d.total_amount, d.vat_amount, d.final_amount,
               sum(l.amount), sum(l.vat_amount), sum(l.total_amount)
        from fa_receipt_doc d join fa_receipt_doc_line l on l.receipt_doc_id = d.id
        group by d.id, d.total_amount, d.vat_amount, d.final_amount
    ) headers
    where total_amount <> line_total
       or vat_amount <> line_vat
       or final_amount <> line_final;

    -- Retail payments are not rewritten (they drive cash postings), so a rounded receipt
    -- total must still match what was paid for it.
    select count(*) into payment_mismatch
    from rtl_sale_doc d
    join (
        select owner_id, sum(amount) as paid
        from rtl_sale_doc_payment
        group by owner_id
    ) p on p.owner_id = d.id
    where d.final_amount <> p.paid;

    if unrounded > 0 then
        raise exception 'document money rounding incomplete: % row(s) still carry more than two decimals', unrounded;
    end if;
    if line_mismatch > 0 then
        raise exception 'document money rounding incomplete: % line(s) whose total is not amount + vat', line_mismatch;
    end if;
    if item_mismatch > 0 then
        raise exception 'document money rounding incomplete: % line(s) whose marked items do not add up to the line VAT', item_mismatch;
    end if;
    if header_mismatch > 0 then
        raise exception 'document money rounding incomplete: % document header(s) disagree with their lines', header_mismatch;
    end if;
    if payment_mismatch > 0 then
        raise exception 'document money rounding left % retail receipt(s) whose payments no longer match the total', payment_mismatch;
    end if;

    raise notice 'document money rounding: % row(s) backed up', (select count(*) from doc_money_rounding_backup_20260914);
end
$$;

commit;
