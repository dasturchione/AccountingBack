begin;

insert into acc_document_account_type (id, code, name, description, state_id)
values
    (9, 'payroll_accrual', 'Ish haqi hisoblash', 'Ish haqi, ushlanmalar va ish beruvchi soliqlarini hisoblash.', 1)
on conflict (id) do update
set code = excluded.code,
    name = excluded.name,
    description = excluded.description,
    state_id = excluded.state_id;

insert into acc_document_account_role (id, code, name, description, state_id)
values
    (12, 'salary_expense', 'Ish haqi xarajati', 'Ish haqi xarajatining debet hisobi.', 1),
    (13, 'salary_payable', 'Xodimlar bilan hisob-kitob', 'Hisoblangan ish haqi majburiyati.', 1),
    (14, 'deduction_payable', 'Ushlanmalar majburiyati', 'JShDS va boshqa ushlanmalar majburiyati.', 1),
    (15, 'employer_tax_expense', 'Ish beruvchi soligi xarajati', 'Ijtimoiy soliq va boshqa ish beruvchi tolovlari xarajati.', 1),
    (16, 'employer_tax_payable', 'Ish beruvchi soligi majburiyati', 'Ijtimoiy soliq va boshqa ish beruvchi tolovlari majburiyati.', 1),
    (17, 'advance_receivable', 'Xodimga berilgan avans', 'Xodimga oldindan berilgan ish haqi avansi.', 1)
on conflict (id) do update
set code = excluded.code,
    name = excluded.name,
    description = excluded.description,
    state_id = excluded.state_id;

insert into acc_document_account_type_role
    (id, document_account_type_id, document_account_role_id, account_side, is_required, sort_order)
values
    (25, 9, 12, 'debit', true, 1),
    (26, 9, 13, 'credit', true, 2),
    (27, 9, 14, 'credit', true, 3),
    (28, 9, 15, 'debit', true, 4),
    (29, 9, 16, 'credit', true, 5),
    (30, 9, 17, 'credit', true, 6)
on conflict (id) do update
set document_account_type_id = excluded.document_account_type_id,
    document_account_role_id = excluded.document_account_role_id,
    account_side = excluded.account_side,
    is_required = excluded.is_required,
    sort_order = excluded.sort_order;

update acc_subkonto_type
set source_table = 'pay_employee'
where id = 12;

select setval(pg_get_serial_sequence('acc_document_account_type', 'id'),
              (select max(id) from acc_document_account_type));
select setval(pg_get_serial_sequence('acc_document_account_role', 'id'),
              (select max(id) from acc_document_account_role));
select setval(pg_get_serial_sequence('acc_document_account_type_role', 'id'),
              (select max(id) from acc_document_account_type_role));

commit;
