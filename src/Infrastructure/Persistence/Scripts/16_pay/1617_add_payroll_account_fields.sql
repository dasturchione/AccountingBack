begin;

alter table pay_payroll_doc
    add column if not exists salary_expense_account_id int references acc_chart_account(id),
    add column if not exists salary_payable_account_id int references acc_chart_account(id),
    add column if not exists deduction_payable_account_id int references acc_chart_account(id),
    add column if not exists employer_tax_expense_account_id int references acc_chart_account(id),
    add column if not exists employer_tax_payable_account_id int references acc_chart_account(id),
    add column if not exists advance_receivable_account_id int references acc_chart_account(id);

alter table pay_payroll_calc_line
    add column if not exists debit_account_id int references acc_chart_account(id),
    add column if not exists credit_account_id int references acc_chart_account(id);

alter table pay_component
    drop constraint if exists ck_pay_component_type;

alter table pay_component
    add constraint ck_pay_component_type
        check (component_type in ('EARNING', 'DEDUCTION', 'EMPLOYER_TAX', 'RECLASSIFICATION'));

create index if not exists idx_pay_payroll_doc_salary_expense_account_id
    on pay_payroll_doc (salary_expense_account_id);

create index if not exists idx_pay_payroll_doc_salary_payable_account_id
    on pay_payroll_doc (salary_payable_account_id);

create index if not exists idx_pay_payroll_doc_deduction_payable_account_id
    on pay_payroll_doc (deduction_payable_account_id);

create index if not exists idx_pay_payroll_doc_employer_tax_expense_account_id
    on pay_payroll_doc (employer_tax_expense_account_id);

create index if not exists idx_pay_payroll_doc_employer_tax_payable_account_id
    on pay_payroll_doc (employer_tax_payable_account_id);

create index if not exists idx_pay_payroll_doc_advance_receivable_account_id
    on pay_payroll_doc (advance_receivable_account_id);

create index if not exists idx_pay_payroll_calc_line_debit_account_id
    on pay_payroll_calc_line (debit_account_id);

create index if not exists idx_pay_payroll_calc_line_credit_account_id
    on pay_payroll_calc_line (credit_account_id);

commit;
