begin;

-- purchase_goods
insert into acc_document_account_type_role
(
    document_account_type_id,
    document_account_role_id,
    account_side,
    is_required,
    sort_order
)
select
    t.id,
    r.id,
    v.account_side,
    v.is_required,
    v.sort_order
from (
    values
        ('supplier_settlement', 'credit', true,  1),
        ('purchase_debit',      'debit',  true,  2),
        ('purchase_vat',        'debit',  false, 3)
) as v(role_code, account_side, is_required, sort_order)
join acc_document_account_type t
    on t.code = 'purchase_goods'
join acc_document_account_role r
    on r.code = v.role_code
on conflict (document_account_type_id, document_account_role_id)
do update set
    account_side = excluded.account_side,
    is_required  = excluded.is_required,
    sort_order   = excluded.sort_order;


-- purchase_service
insert into acc_document_account_type_role
(
    document_account_type_id,
    document_account_role_id,
    account_side,
    is_required,
    sort_order
)
select
    t.id,
    r.id,
    v.account_side,
    v.is_required,
    v.sort_order
from (
    values
        ('supplier_settlement', 'credit', true,  1),
        ('purchase_debit',      'debit',  true,  2),
        ('purchase_vat',        'debit',  false, 3)
) as v(role_code, account_side, is_required, sort_order)
join acc_document_account_type t
    on t.code = 'purchase_service'
join acc_document_account_role r
    on r.code = v.role_code
on conflict (document_account_type_id, document_account_role_id)
do update set
    account_side = excluded.account_side,
    is_required  = excluded.is_required,
    sort_order   = excluded.sort_order;


-- sale_goods
insert into acc_document_account_type_role
(
    document_account_type_id,
    document_account_role_id,
    account_side,
    is_required,
    sort_order
)
select
    t.id,
    r.id,
    v.account_side,
    v.is_required,
    v.sort_order
from (
    values
        ('customer_settlement', 'debit',  true,  1),
        ('sale_income',         'credit', true,  2),
        ('sale_vat',            'credit', false, 3),
        ('sale_cost',           'debit',  true,  4),
        ('sale_inventory',      'credit', true,  5)
) as v(role_code, account_side, is_required, sort_order)
join acc_document_account_type t
    on t.code = 'sale_goods'
join acc_document_account_role r
    on r.code = v.role_code
on conflict (document_account_type_id, document_account_role_id)
do update set
    account_side = excluded.account_side,
    is_required  = excluded.is_required,
    sort_order   = excluded.sort_order;


-- sale_service
insert into acc_document_account_type_role
(
    document_account_type_id,
    document_account_role_id,
    account_side,
    is_required,
    sort_order
)
select
    t.id,
    r.id,
    v.account_side,
    v.is_required,
    v.sort_order
from (
    values
        ('customer_settlement', 'debit',  true,  1),
        ('sale_income',         'credit', true,  2),
        ('sale_vat',            'credit', false, 3),
        ('sale_cost',           'debit',  false, 4),
        ('sale_inventory',      'credit', false, 5)
) as v(role_code, account_side, is_required, sort_order)
join acc_document_account_type t
    on t.code = 'sale_service'
join acc_document_account_role r
    on r.code = v.role_code
on conflict (document_account_type_id, document_account_role_id)
do update set
    account_side = excluded.account_side,
    is_required  = excluded.is_required,
    sort_order   = excluded.sort_order;


-- bank_income
insert into acc_document_account_type_role
(
    document_account_type_id,
    document_account_role_id,
    account_side,
    is_required,
    sort_order
)
select
    t.id,
    r.id,
    v.account_side,
    v.is_required,
    v.sort_order
from (
    values
        ('bank_account',   'debit',  true, 1),
        ('offset_account', 'credit', true, 2)
) as v(role_code, account_side, is_required, sort_order)
join acc_document_account_type t
    on t.code = 'bank_income'
join acc_document_account_role r
    on r.code = v.role_code
on conflict (document_account_type_id, document_account_role_id)
do update set
    account_side = excluded.account_side,
    is_required  = excluded.is_required,
    sort_order   = excluded.sort_order;


-- bank_expense
insert into acc_document_account_type_role
(
    document_account_type_id,
    document_account_role_id,
    account_side,
    is_required,
    sort_order
)
select
    t.id,
    r.id,
    v.account_side,
    v.is_required,
    v.sort_order
from (
    values
        ('bank_account',   'credit', true, 1),
        ('offset_account', 'debit',  true, 2)
) as v(role_code, account_side, is_required, sort_order)
join acc_document_account_type t
    on t.code = 'bank_expense'
join acc_document_account_role r
    on r.code = v.role_code
on conflict (document_account_type_id, document_account_role_id)
do update set
    account_side = excluded.account_side,
    is_required  = excluded.is_required,
    sort_order   = excluded.sort_order;


-- cash_income
insert into acc_document_account_type_role
(
    document_account_type_id,
    document_account_role_id,
    account_side,
    is_required,
    sort_order
)
select
    t.id,
    r.id,
    v.account_side,
    v.is_required,
    v.sort_order
from (
    values
        ('cash_account',   'debit',  true, 1),
        ('offset_account', 'credit', true, 2)
) as v(role_code, account_side, is_required, sort_order)
join acc_document_account_type t
    on t.code = 'cash_income'
join acc_document_account_role r
    on r.code = v.role_code
on conflict (document_account_type_id, document_account_role_id)
do update set
    account_side = excluded.account_side,
    is_required  = excluded.is_required,
    sort_order   = excluded.sort_order;


-- cash_expense
insert into acc_document_account_type_role
(
    document_account_type_id,
    document_account_role_id,
    account_side,
    is_required,
    sort_order
)
select
    t.id,
    r.id,
    v.account_side,
    v.is_required,
    v.sort_order
from (
    values
        ('cash_account',   'credit', true, 1),
        ('offset_account', 'debit',  true, 2)
) as v(role_code, account_side, is_required, sort_order)
join acc_document_account_type t
    on t.code = 'cash_expense'
join acc_document_account_role r
    on r.code = v.role_code
on conflict (document_account_type_id, document_account_role_id)
do update set
    account_side = excluded.account_side,
    is_required  = excluded.is_required,
    sort_order   = excluded.sort_order;


-- payroll_accrual / Ish haqi hisoblash
insert into acc_document_account_type_role
(
    document_account_type_id,
    document_account_role_id,
    account_side,
    is_required,
    sort_order
)
select
    t.id,
    r.id,
    v.account_side,
    v.is_required,
    v.sort_order
from (
    values
        ('salary_expense',        'debit',  true, 1),
        ('salary_payable',        'credit', true, 2),
        ('deduction_payable',     'credit', true, 3),
        ('employer_tax_expense',  'debit',  true, 4),
        ('employer_tax_payable',  'credit', true, 5),
        ('advance_receivable',    'credit', true, 6)
) as v(role_code, account_side, is_required, sort_order)
join acc_document_account_type t
    on t.code = 'payroll_accrual'
join acc_document_account_role r
    on r.code = v.role_code
on conflict (document_account_type_id, document_account_role_id)
do update set
    account_side = excluded.account_side,
    is_required = excluded.is_required,
    sort_order = excluded.sort_order;


-- retail_sale_goods
insert into acc_document_account_type_role
(
    document_account_type_id,
    document_account_role_id,
    account_side,
    is_required,
    sort_order
)
select
    t.id,
    r.id,
    v.account_side,
    v.is_required,
    v.sort_order
from (
    values
        ('customer_settlement',         'debit',  true, 1),
        ('sale_income',                 'credit', true, 2),
        ('sale_vat',                    'credit', true, 3),
        ('sale_cost',                   'debit',  true, 4),
        ('sale_inventory',              'credit', true, 5)
) as v(role_code, account_side, is_required, sort_order)
join acc_document_account_type t
    on t.code = 'retail_sale_goods'
join acc_document_account_role r
    on r.code = v.role_code
on conflict (document_account_type_id, document_account_role_id)
do update set
    account_side = excluded.account_side,
    is_required = excluded.is_required,
    sort_order = excluded.sort_order;


-- retail_payment_cash
insert into acc_document_account_type_role
(
    document_account_type_id,
    document_account_role_id,
    account_side,
    is_required,
    sort_order
)
select
    t.id,
    r.id,
    v.account_side,
    v.is_required,
    v.sort_order
from (
    values
        ('cash_account',        'debit',    true, 1),
        ('customer_settlement', 'credit',   true, 2)
) as v(role_code, account_side, is_required, sort_order)
join acc_document_account_type t
    on t.code = 'retail_payment_cash'
join acc_document_account_role r
    on r.code = v.role_code
on conflict (document_account_type_id, document_account_role_id)
do update set
    account_side = excluded.account_side,
    is_required  = excluded.is_required,
    sort_order   = excluded.sort_order;


-- retail_payment_card
insert into acc_document_account_type_role
(
    document_account_type_id,
    document_account_role_id,
    account_side,
    is_required,
    sort_order
)
select
    t.id,
    r.id,
    v.account_side,
    v.is_required,
    v.sort_order
from (
    values
        ('acquiring_clearing',  'debit',    true, 1),
        ('customer_settlement', 'credit',   true, 2)
) as v(role_code, account_side, is_required, sort_order)
join acc_document_account_type t
    on t.code = 'retail_payment_card'
join acc_document_account_role r
    on r.code = v.role_code
on conflict (document_account_type_id, document_account_role_id)
do update set
    account_side = excluded.account_side,
    is_required  = excluded.is_required,
    sort_order   = excluded.sort_order;


-- retail_payment_acquiring
insert into acc_document_account_type_role
(
    document_account_type_id,
    document_account_role_id,
    account_side,
    is_required,
    sort_order
)
select
    t.id,
    r.id,
    v.account_side,
    v.is_required,
    v.sort_order
from (
    values
        ('acquiring_clearing',  'debit',    true, 1),
        ('customer_settlement', 'credit',   true, 2)
) as v(role_code, account_side, is_required, sort_order)
join acc_document_account_type t
    on t.code = 'retail_payment_acquiring'
join acc_document_account_role r
    on r.code = v.role_code
on conflict (document_account_type_id, document_account_role_id)
do update set
    account_side = excluded.account_side,
    is_required  = excluded.is_required,
    sort_order   = excluded.sort_order;


-- retail_payment_bank_transfer
insert into acc_document_account_type_role
(
    document_account_type_id,
    document_account_role_id,
    account_side,
    is_required,
    sort_order
)
select
    t.id,
    r.id,
    v.account_side,
    v.is_required,
    v.sort_order
from (
    values
        ('bank_account',        'debit',    true, 1),
        ('customer_settlement', 'credit',   true, 2)
) as v(role_code, account_side, is_required, sort_order)
join acc_document_account_type t
    on t.code = 'retail_payment_bank_transfer'
join acc_document_account_role r
    on r.code = v.role_code
on conflict (document_account_type_id, document_account_role_id)
do update set
    account_side = excluded.account_side,
    is_required  = excluded.is_required,
    sort_order   = excluded.sort_order;


-- retail_acquiring_settlement
insert into acc_document_account_type_role
(
    document_account_type_id,
    document_account_role_id,
    account_side,
    is_required,
    sort_order
)
select
    t.id,
    r.id,
    v.account_side,
    v.is_required,
    v.sort_order
from (
    values
        ('bank_account',        'debit',  true, 1),
        ('acquiring_clearing',  'credit', true, 2)
) as v(role_code, account_side, is_required, sort_order)
join acc_document_account_type t
    on t.code = 'retail_acquiring_settlement'
join acc_document_account_role r
    on r.code = v.role_code
on conflict (document_account_type_id, document_account_role_id)
do update set
    account_side = excluded.account_side,
    is_required  = excluded.is_required,
    sort_order   = excluded.sort_order;

-- fa_receipt
insert into acc_document_account_type_role
(
    document_account_type_id,
    document_account_role_id,
    account_side,
    is_required,
    sort_order
)
select
    t.id,
    r.id,
    v.account_side,
    v.is_required,
    v.sort_order
from (
    values
        ('supplier_settlement', 'credit', true,  1),
        ('capital_investment',   'debit',  true,  2),
        ('input_vat',            'debit',  false, 3),
        ('fixed_asset',          'debit',  true,  4)
) as v(role_code, account_side, is_required, sort_order)
join acc_document_account_type t
    on t.code = 'fa_receipt'
join acc_document_account_role r
    on r.code = v.role_code
on conflict (document_account_type_id, document_account_role_id)
do update set
    account_side = excluded.account_side,
    is_required  = excluded.is_required,
    sort_order   = excluded.sort_order;


-- fa_commissioning
insert into acc_document_account_type_role
(
    document_account_type_id,
    document_account_role_id,
    account_side,
    is_required,
    sort_order
)
select
    t.id,
    r.id,
    v.account_side,
    v.is_required,
    v.sort_order
from (
    values
        ('fixed_asset',                 'debit',  true, 1),
        ('capital_investment',          'credit', true, 2),
        ('accumulated_depreciation',    'credit', true, 3),
        ('depreciation_expense',        'debit',  true, 4)
) as v(role_code, account_side, is_required, sort_order)
join acc_document_account_type t
    on t.code = 'fa_commissioning'
join acc_document_account_role r
    on r.code = v.role_code
on conflict (document_account_type_id, document_account_role_id)
do update set
    account_side = excluded.account_side,
    is_required  = excluded.is_required,
    sort_order   = excluded.sort_order;


-- fa_depreciation
insert into acc_document_account_type_role
(
    document_account_type_id,
    document_account_role_id,
    account_side,
    is_required,
    sort_order
)
select
    t.id,
    r.id,
    v.account_side,
    v.is_required,
    v.sort_order
from (
    values
        ('depreciation_expense',     'debit',  true, 1),
        ('accumulated_depreciation', 'credit', true, 2)
) as v(role_code, account_side, is_required, sort_order)
join acc_document_account_type t
    on t.code = 'fa_depreciation'
join acc_document_account_role r
    on r.code = v.role_code
on conflict (document_account_type_id, document_account_role_id)
do update set
    account_side = excluded.account_side,
    is_required  = excluded.is_required,
    sort_order   = excluded.sort_order;


-- fa_revaluation
insert into acc_document_account_type_role
(
    document_account_type_id,
    document_account_role_id,
    account_side,
    is_required,
    sort_order
)
select
    t.id,
    r.id,
    v.account_side,
    v.is_required,
    v.sort_order
from (
    values
        ('fixed_asset',         'debit',  true,  1),
        ('revaluation_reserve', 'credit', false, 2),
        ('revaluation_loss',    'debit',  false, 3)
) as v(role_code, account_side, is_required, sort_order)
join acc_document_account_type t
    on t.code = 'fa_revaluation'
join acc_document_account_role r
    on r.code = v.role_code
on conflict (document_account_type_id, document_account_role_id)
do update set
    account_side = excluded.account_side,
    is_required  = excluded.is_required,
    sort_order   = excluded.sort_order;


-- fa_disposal
insert into acc_document_account_type_role
(
    document_account_type_id,
    document_account_role_id,
    account_side,
    is_required,
    sort_order
)
select
    t.id,
    r.id,
    v.account_side,
    v.is_required,
    v.sort_order
from (
    values
        ('fixed_asset',              'credit', true,  1),
        ('accumulated_depreciation', 'debit',  false, 2),
        ('disposal',                 'debit',  true,  3),
        ('customer_settlement',      'debit',  false, 4),
        ('disposal_gain',            'credit', false, 5),
        ('disposal_loss',            'debit',  false, 6)
) as v(role_code, account_side, is_required, sort_order)
join acc_document_account_type t
    on t.code = 'fa_disposal'
join acc_document_account_role r
    on r.code = v.role_code
on conflict (document_account_type_id, document_account_role_id)
do update set
    account_side = excluded.account_side,
    is_required  = excluded.is_required,
    sort_order   = excluded.sort_order;

commit;
