begin;

-- DDL needs an ACCESS EXCLUSIVE lock on pay_payroll_doc.  Do not leave a
-- migration session waiting forever when another transaction has the table
-- open (for example an idle-in-transaction API/query session).
set local lock_timeout = '5s';
set local statement_timeout = '60s';

-- Payroll postings now use salary accounts from the document and
-- component-level accounts for deductions and employer taxes. Advance
-- offsets are no longer posted from a document-level account selector.
drop index if exists idx_pay_payroll_doc_deduction_payable_account_id;
drop index if exists idx_pay_payroll_doc_employer_tax_expense_account_id;
drop index if exists idx_pay_payroll_doc_employer_tax_payable_account_id;
drop index if exists idx_pay_payroll_doc_advance_receivable_account_id;

alter table pay_payroll_doc
    drop constraint if exists pay_payroll_doc_deduction_payable_account_id_fkey,
    drop constraint if exists pay_payroll_doc_employer_tax_expense_account_id_fkey,
    drop constraint if exists pay_payroll_doc_employer_tax_payable_account_id_fkey,
    drop constraint if exists pay_payroll_doc_advance_receivable_account_id_fkey,
    drop column if exists deduction_payable_account_id,
    drop column if exists employer_tax_expense_account_id,
    drop column if exists employer_tax_payable_account_id,
    drop column if exists advance_receivable_account_id;

commit;
