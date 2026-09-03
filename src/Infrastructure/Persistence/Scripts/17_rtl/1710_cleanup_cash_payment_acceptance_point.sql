do
$$
declare
    cleared_count bigint;
begin
    if to_regclass('public.rtl_sale_doc_payment') is null
       or to_regclass('public.rtl_payment_method') is null
    then
        return;
    end if;

    if exists
    (
        select 1
        from information_schema.columns
        where table_schema = 'public'
          and table_name = 'rtl_sale_doc_payment'
          and column_name = 'payment_acceptance_point_id'
    )
    then
        execute
        $sql$
            update rtl_sale_doc_payment payment
            set payment_acceptance_point_id = null
            from rtl_payment_method payment_method
            where payment_method.id = payment.payment_method_id
              and payment_method.code = 'CASH'
              and payment.payment_acceptance_point_id is not null
        $sql$;

        get diagnostics cleared_count = row_count;
        raise notice
            'Cleared payment_acceptance_point_id for % CASH payments',
            cleared_count;
    end if;

    if exists
    (
        select 1
        from information_schema.columns
        where table_schema = 'public'
          and table_name = 'rtl_sale_doc_payment'
          and column_name = 'bank_terminal_id'
    )
    then
        execute
        $sql$
            update rtl_sale_doc_payment payment
            set bank_terminal_id = null
            from rtl_payment_method payment_method
            where payment_method.id = payment.payment_method_id
              and payment_method.code = 'CASH'
              and payment.bank_terminal_id is not null
        $sql$;

        get diagnostics cleared_count = row_count;
        raise notice
            'Cleared bank_terminal_id for % CASH payments',
            cleared_count;
    end if;
end;
$$;
