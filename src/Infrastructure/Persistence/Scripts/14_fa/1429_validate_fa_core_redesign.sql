begin;

do $$
begin
    if exists (
        select 1
        from fa_receipt_doc_asset
        where fa_asset_id is not null
        group by fa_asset_id
        having count(*) > 1
    ) then
        raise exception 'FA core redesign: fa_receipt_doc_asset contains duplicate non-null fa_asset_id values.';
    end if;

    if exists (
        select 1
        from fa_asset a
        left join fa_receipt_doc_asset ra on ra.fa_asset_id = a.id
        where coalesce(a.asset_account_id, ra.asset_account_id) is null
    ) then
        raise exception 'FA core redesign: every existing fa_asset must have asset_account_id, directly or through fa_receipt_doc_asset.';
    end if;

    if exists (
        select 1
        from fa_receipt_doc_asset ra
        left join fa_asset a on a.id = ra.fa_asset_id
        where coalesce(ra.asset_account_id, a.asset_account_id) is null
    ) then
        raise exception 'FA core redesign: every fa_receipt_doc_asset must have asset_account_id.';
    end if;

    if exists (
        select 1
        from fa_receipt_doc
        where length(doc_number) > 50
    ) then
        raise exception 'FA core redesign: fa_receipt_doc.doc_number contains values longer than 50 characters.';
    end if;

    if exists (
        select 1
        from fa_receipt_doc_line
        where quantity <> trunc(quantity)
           or quantity > 2147483647
    ) then
        raise exception 'FA core redesign: fa_receipt_doc_line.quantity must contain positive integer values within integer range.';
    end if;

    if exists (
        select 1
        from fa_receipt_doc
        where total_amount < 0
           or vat_amount < 0
           or final_amount < 0
           or final_amount <> total_amount + vat_amount
    ) then
        raise exception 'FA core redesign: fa_receipt_doc contains invalid totals.';
    end if;

    if exists (
        select 1
        from fa_receipt_doc_line
        where price < 0
           or amount < 0
           or vat_amount < 0
           or total_amount < 0
           or amount <> price * quantity
           or total_amount <> amount + vat_amount
    ) then
        raise exception 'FA core redesign: fa_receipt_doc_line contains invalid amounts.';
    end if;
end
$$;

commit;
