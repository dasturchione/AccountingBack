begin;

insert into acc_document_account_role_translation
(
    language_id,
    document_account_role_id,
    name,
    description
)
select
    l.id,
    r.id,
    v.name,
    v.description
from (
    values
        -- =========================================================
        -- supplier_settlement
        -- =========================================================
        ('supplier_settlement', 'uz',
         'Yetkazib beruvchi hisobi',
         'Xarid hujjatida yetkazib beruvchi qarzi uchun ishlatiladigan hisob roli.'),

        ('supplier_settlement', 'ru',
         'Расчеты с поставщиком',
         'Счет расчетов с поставщиком, используемый в документах закупки.'),

        ('supplier_settlement', 'en',
         'Supplier settlement',
         'Account role used for settlements with suppliers in purchase documents.'),

        -- =========================================================
        -- purchase_debit
        -- =========================================================
        ('purchase_debit', 'uz',
         'Xarid debet hisobi',
         'Xarid qilingan tovar, material, xizmat yoki xarajat uchun debet hisob roli.'),

        ('purchase_debit', 'ru',
         'Дебетовый счет закупки',
         'Дебетовый счет для приобретенных товаров, материалов, услуг или расходов.'),

        ('purchase_debit', 'en',
         'Purchase debit account',
         'Debit account role for purchased goods, materials, services or expenses.'),

        -- =========================================================
        -- purchase_vat
        -- =========================================================
        ('purchase_vat', 'uz',
         'Xarid QQS hisobi',
         'Xarid bo‘yicha QQS summasi uchun hisob roli.'),

        ('purchase_vat', 'ru',
         'НДС по закупке',
         'Счет для учета суммы НДС по закупке.'),

        ('purchase_vat', 'en',
         'Purchase VAT',
         'Account role for VAT amounts on purchases.'),

        -- =========================================================
        -- customer_settlement
        -- =========================================================
        ('customer_settlement', 'uz',
         'Xaridor hisobi',
         'Sotuv hujjatida xaridor qarzi uchun ishlatiladigan hisob roli.'),

        ('customer_settlement', 'ru',
         'Расчеты с покупателем',
         'Счет расчетов с покупателем, используемый в документах продажи.'),

        ('customer_settlement', 'en',
         'Customer settlement',
         'Account role used for settlements with customers in sales documents.'),

        -- =========================================================
        -- sale_vat
        -- =========================================================
        ('sale_vat', 'uz',
         'Sotuv QQS hisobi',
         'Sotuv bo‘yicha QQS majburiyati uchun hisob roli.'),

        ('sale_vat', 'ru',
         'НДС по продаже',
         'Счет для учета обязательства по НДС при реализации.'),

        ('sale_vat', 'en',
         'Sales VAT',
         'Account role for VAT liabilities arising from sales.'),

        -- =========================================================
        -- sale_inventory
        -- =========================================================
        ('sale_inventory', 'uz',
         'Sotuv tovar/material hisobi',
         'Sotuvda tovar yoki materialni ombordan chiqarish uchun hisob roli.'),

        ('sale_inventory', 'ru',
         'Счет товаров и материалов',
         'Счет товаров или материалов, используемый при списании запасов при продаже.'),

        ('sale_inventory', 'en',
         'Sales inventory account',
         'Account role used to write off goods or materials from inventory upon sale.'),

        -- =========================================================
        -- sale_income
        -- =========================================================
        ('sale_income', 'uz',
         'Sotuv daromad hisobi',
         'Tovar, ish yoki xizmat sotishdan daromad uchun hisob roli.'),

        ('sale_income', 'ru',
         'Доход от реализации',
         'Счет доходов от реализации товаров, работ или услуг.'),

        ('sale_income', 'en',
         'Sales income',
         'Account role for income from the sale of goods, works or services.'),

        -- =========================================================
        -- sale_cost
        -- =========================================================
        ('sale_cost', 'uz',
         'Sotuv tannarx hisobi',
         'Sotilgan tovar, ish yoki xizmat tannarxi uchun hisob roli.'),

        ('sale_cost', 'ru',
         'Себестоимость реализации',
         'Счет себестоимости реализованных товаров, работ или услуг.'),

        ('sale_cost', 'en',
         'Cost of sales',
         'Account role for the cost of goods, works or services sold.'),

        -- =========================================================
        -- bank_account
        -- =========================================================
        ('bank_account', 'uz',
         'Bank hisobi',
         'Bank operatsiyasida asosiy bank buxgalteriya hisobi roli.'),

        ('bank_account', 'ru',
         'Банковский счет',
         'Основной бухгалтерский счет банка, используемый в банковской операции.'),

        ('bank_account', 'en',
         'Bank account',
         'Primary bank accounting account role used in bank transactions.'),

        -- =========================================================
        -- cash_account
        -- =========================================================
        ('cash_account', 'uz',
         'Kassa hisobi',
         'Kassa operatsiyasida asosiy kassa buxgalteriya hisobi roli.'),

        ('cash_account', 'ru',
         'Счет кассы',
         'Основной бухгалтерский счет кассы, используемый в кассовой операции.'),

        ('cash_account', 'en',
         'Cash account',
         'Primary cash accounting account role used in cash transactions.'),

        -- =========================================================
        -- offset_account
        -- =========================================================
        ('offset_account', 'uz',
         'Qarshi hisob',
         'Bank yoki kassa operatsiyasida ikkinchi tomon hisob roli.'),

        ('offset_account', 'ru',
         'Корреспондирующий счет',
         'Счет второй стороны проводки в банковской или кассовой операции.'),

        ('offset_account', 'en',
         'Offset account',
         'Account role for the other side of a bank or cash transaction.'),

        -- =========================================================
        -- salary_expense
        -- =========================================================
        ('salary_expense', 'uz',
         'Ish haqi xarajati',
         'Ish haqi xarajatining debet hisobi.'),

        ('salary_expense', 'ru',
         'Расходы на оплату труда',
         'Дебетовый счет расходов на оплату труда.'),

        ('salary_expense', 'en',
         'Salary expense',
         'Debit account role for salary expenses.'),

        -- =========================================================
        -- salary_payable
        -- =========================================================
        ('salary_payable', 'uz',
         'Xodimlar bilan hisob-kitob',
         'Hisoblangan ish haqi majburiyati.'),

        ('salary_payable', 'ru',
         'Расчеты с сотрудниками по зарплате',
         'Счет обязательств по начисленной заработной плате.'),

        ('salary_payable', 'en',
         'Salary payable',
         'Liability account role for accrued salaries.'),

        -- =========================================================
        -- deduction_payable
        -- =========================================================
        ('deduction_payable', 'uz',
         'Ushlanmalar majburiyati',
         'JShDS va boshqa ushlanmalar majburiyati.'),

        ('deduction_payable', 'ru',
         'Обязательства по удержаниям',
         'Счет обязательств по НДФЛ и другим удержаниям из заработной платы.'),

        ('deduction_payable', 'en',
         'Deductions payable',
         'Liability account role for personal income tax and other payroll deductions.'),

        -- =========================================================
        -- employer_tax_expense
        -- =========================================================
        ('employer_tax_expense', 'uz',
         'Ish beruvchi solig‘i xarajati',
         'Ijtimoiy soliq va boshqa ish beruvchi to‘lovlari xarajati.'),

        ('employer_tax_expense', 'ru',
         'Расходы по налогам работодателя',
         'Счет расходов по социальному налогу и другим платежам работодателя.'),

        ('employer_tax_expense', 'en',
         'Employer tax expense',
         'Expense account role for social tax and other employer contributions.'),

        -- =========================================================
        -- employer_tax_payable
        -- =========================================================
        ('employer_tax_payable', 'uz',
         'Ish beruvchi solig‘i majburiyati',
         'Ijtimoiy soliq va boshqa ish beruvchi to‘lovlari majburiyati.'),

        ('employer_tax_payable', 'ru',
         'Обязательства по налогам работодателя',
         'Счет обязательств по социальному налогу и другим платежам работодателя.'),

        ('employer_tax_payable', 'en',
         'Employer tax payable',
         'Liability account role for social tax and other employer contributions.'),

        -- =========================================================
        -- advance_receivable
        -- =========================================================
        ('advance_receivable', 'uz',
         'Xodimga berilgan avans',
         'Xodimga oldindan berilgan ish haqi avansi.'),

        ('advance_receivable', 'ru',
         'Аванс, выданный сотруднику',
         'Счет для учета аванса по заработной плате, ранее выданного сотруднику.'),

        ('advance_receivable', 'en',
         'Employee advance receivable',
         'Account role for salary advances previously paid to employees.'),

        -- =========================================================
        -- acquiring_clearing
        -- =========================================================
        ('acquiring_clearing', 'uz',
         'Ekvayring bo‘yicha oraliq hisob',
         'Bank kartasi va mobil to‘lovlar bo‘yicha mablag‘lar bank hisobvarag‘iga tushguniga qadar ishlatiladigan oraliq hisob roli.'),

        ('acquiring_clearing', 'ru',
         'Промежуточный счет эквайринга',
         'Промежуточный счет для карточных и мобильных платежей до фактического поступления денежных средств на банковский счет.'),

        ('acquiring_clearing', 'en',
         'Acquiring clearing account',
         'Clearing account role for card and mobile payments until the funds are actually credited to the bank account.')

) as v
(
    role_code,
    language_code,
    name,
    description
)
join acc_document_account_role r
    on r.code = v.role_code
join cmn_language l
    on l.code = v.language_code

on conflict (language_id, document_account_role_id)
do update set
    name        = excluded.name,
    description = excluded.description;

commit;
