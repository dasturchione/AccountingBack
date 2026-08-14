insert into acc_document_account_type_translation
(
    language_id,
    document_account_type_id,
    name,
    description
)
select
    l.id,
    t.id,
    v.name,
    v.description
from
(
    values
        -- =========================================================
        -- purchase_goods
        -- =========================================================
        (
            'purchase_goods',
            'uz',
            'Tovar xaridi',
            'Tovarlarni xarid qilish hujjati.'
        ),
        (
            'purchase_goods',
            'ru',
            'Закупка товара',
            'Документ закупки товаров.'
        ),
        (
            'purchase_goods',
            'en',
            'Purchase of goods',
            'Document for purchasing goods.'
        ),

        -- =========================================================
        -- purchase_service
        -- =========================================================
        (
            'purchase_service',
            'uz',
            'Xizmat xaridi',
            'Xizmatlarni xarid qilish hujjati.'
        ),
        (
            'purchase_service',
            'ru',
            'Закупка услуги',
            'Документ закупки услуг.'
        ),
        (
            'purchase_service',
            'en',
            'Purchase of services',
            'Document for purchasing services.'
        ),

        -- =========================================================
        -- sale_goods
        -- =========================================================
        (
            'sale_goods',
            'uz',
            'Tovar realizatsiyasi',
            'Tovarlarni sotish hujjati.'
        ),
        (
            'sale_goods',
            'ru',
            'Реализация товара',
            'Документ продажи товаров.'
        ),
        (
            'sale_goods',
            'en',
            'Sale of goods',
            'Document for selling goods.'
        ),

        -- =========================================================
        -- sale_service
        -- =========================================================
        (
            'sale_service',
            'uz',
            'Xizmat ko‘rsatish',
            'Xizmatlarni sotish yoki ko‘rsatish hujjati.'
        ),
        (
            'sale_service',
            'ru',
            'Оказание услуг',
            'Документ продажи или оказания услуг.'
        ),
        (
            'sale_service',
            'en',
            'Sale of services',
            'Document for selling or providing services.'
        ),

        -- =========================================================
        -- bank_income
        -- =========================================================
        (
            'bank_income',
            'uz',
            'Bank kirim',
            'Bank hisobvarag‘iga pul kelib tushishi.'
        ),
        (
            'bank_income',
            'ru',
            'Банковский приход',
            'Поступление денежных средств на банковский счет.'
        ),
        (
            'bank_income',
            'en',
            'Bank income',
            'Receipt of funds to a bank account.'
        ),

        -- =========================================================
        -- bank_expense
        -- =========================================================
        (
            'bank_expense',
            'uz',
            'Bank chiqim',
            'Bank hisobvarag‘idan pul chiqishi.'
        ),
        (
            'bank_expense',
            'ru',
            'Банковский расход',
            'Списание денежных средств с банковского счета.'
        ),
        (
            'bank_expense',
            'en',
            'Bank expense',
            'Withdrawal of funds from a bank account.'
        ),

        -- =========================================================
        -- cash_income
        -- =========================================================
        (
            'cash_income',
            'uz',
            'Kassa kirim',
            'Kassaga naqd pul kirimi.'
        ),
        (
            'cash_income',
            'ru',
            'Кассовый приход',
            'Поступление наличных денежных средств в кассу.'
        ),
        (
            'cash_income',
            'en',
            'Cash income',
            'Receipt of cash into the cash box.'
        ),

        -- =========================================================
        -- cash_expense
        -- =========================================================
        (
            'cash_expense',
            'uz',
            'Kassa chiqim',
            'Kassadan naqd pul chiqimi.'
        ),
        (
            'cash_expense',
            'ru',
            'Кассовый расход',
            'Выдача наличных денежных средств из кассы.'
        ),
        (
            'cash_expense',
            'en',
            'Cash expense',
            'Cash withdrawal from the cash box.'
        ),

        -- =========================================================
        -- payroll_accrual
        -- =========================================================
        (
            'payroll_accrual',
            'uz',
            'Ish haqi hisoblash',
            'Ish haqi, ushlanmalar va ish beruvchi soliqlarini hisoblash.'
        ),
        (
            'payroll_accrual',
            'ru',
            'Начисление заработной платы',
            'Начисление заработной платы, удержаний и налогов работодателя.'
        ),
        (
            'payroll_accrual',
            'en',
            'Payroll accrual',
            'Accrual of salaries, deductions and employer taxes.'
        ),

        -- =========================================================
        -- retail_sale_goods
        -- =========================================================
        (
            'retail_sale_goods',
            'uz',
            'Tovar chakana sotuv',
            'Chakana savdo uchun tovarlarni sotish hujjati.'
        ),
        (
            'retail_sale_goods',
            'ru',
            'Розничная продажа товаров',
            'Документ розничной продажи товаров.'
        ),
        (
            'retail_sale_goods',
            'en',
            'Retail sale of goods',
            'Document for retail sale of goods.'
        ),

        -- =========================================================
        -- retail_payment_cash
        -- =========================================================
        (
            'retail_payment_cash',
            'uz',
            'Chakana savdo naqd to‘lov',
            'Chakana savdo uchun naqd to‘lov hujjati.'
        ),
        (
            'retail_payment_cash',
            'ru',
            'Наличная оплата в розничной торговле',
            'Документ наличной оплаты в розничной торговле.'
        ),
        (
            'retail_payment_cash',
            'en',
            'Retail cash payment',
            'Document for cash payment in retail sales.'
        ),

        -- =========================================================
        -- retail_payment_card
        -- =========================================================
        (
            'retail_payment_card',
            'uz',
            'Chakana savdo karta to‘lovi',
            'Chakana savdo uchun karta orqali to‘lov hujjati.'
        ),
        (
            'retail_payment_card',
            'ru',
            'Оплата картой в розничной торговле',
            'Документ оплаты банковской картой в розничной торговле.'
        ),
        (
            'retail_payment_card',
            'en',
            'Retail card payment',
            'Document for bank card payment in retail sales.'
        ),

        -- =========================================================
        -- retail_payment_acquiring
        -- =========================================================
        (
            'retail_payment_acquiring',
            'uz',
            'Chakana savdo elektron to‘lovi',
            'Chakana savdoda mobil va elektron to‘lov xizmatlari orqali amalga oshirilgan to‘lov.'
        ),
        (
            'retail_payment_acquiring',
            'ru',
            'Электронная оплата в розничной торговле',
            'Оплата в розничной торговле через мобильные и электронные платежные сервисы.'
        ),
        (
            'retail_payment_acquiring',
            'en',
            'Retail electronic payment',
            'Payment in retail sales through mobile and electronic payment services.'
        ),

        -- =========================================================
        -- retail_payment_bank_transfer
        -- =========================================================
        (
            'retail_payment_bank_transfer',
            'uz',
            'Chakana savdo bank o‘tkazma to‘lovi',
            'Chakana savdo uchun bank o‘tkazmasi orqali to‘lov hujjati.'
        ),
        (
            'retail_payment_bank_transfer',
            'ru',
            'Оплата банковским переводом в розничной торговле',
            'Документ оплаты банковским переводом в розничной торговле.'
        ),
        (
            'retail_payment_bank_transfer',
            'en',
            'Retail bank transfer payment',
            'Document for bank transfer payment in retail sales.'
        ),

        -- =========================================================
        -- retail_acquiring_settlement
        -- Фактическое поступление эквайринговых денег на банк
        -- =========================================================
        (
            'retail_acquiring_settlement',
            'uz',
            'Ekvayring mablag‘larini bank hisobvarag‘iga o‘tkazish',
            'Ekvayring orqali qabul qilingan mablag‘larning bank hisobvarag‘iga kelib tushishi.'
        ),
        (
            'retail_acquiring_settlement',
            'ru',
            'Зачисление эквайринговых средств',
            'Поступление на банковский счет денежных средств, ранее принятых через эквайринг.'
        ),
        (
            'retail_acquiring_settlement',
            'en',
            'Acquiring settlement',
            'Receipt to the bank account of funds previously accepted through acquiring.'
        ),

        -- =========================================================
        -- fa_receipt
        -- =========================================================
        (
            'fa_receipt',
            'uz',
            'Asosiy vositalarni qabul qilish',
            'Asosiy vositalarni qabul qilish hujjati.'
        ),
        (
            'fa_receipt',
            'ru',
            'Поступление основных средств',
            'Документ поступления основных средств.'
        ),
        (
            'fa_receipt',
            'en',
            'Fixed asset receipt',
            'Document for the receipt of fixed assets.'
        ),

        -- =========================================================
        -- fa_commissioning
        -- =========================================================
        (
            'fa_commissioning',
            'uz',
            'Asosiy vositalarni ishga tushirish',
            'Asosiy vositalarni foydalanishga topshirish hujjati.'
        ),
        (
            'fa_commissioning',
            'ru',
            'Ввод основных средств в эксплуатацию',
            'Документ ввода основных средств в эксплуатацию.'
        ),
        (
            'fa_commissioning',
            'en',
            'Fixed asset commissioning',
            'Document for commissioning fixed assets.'
        ),

        -- =========================================================
        -- fa_depreciation
        -- =========================================================
        (
            'fa_depreciation',
            'uz',
            'Asosiy vositalar amortizatsiyasi',
            'Asosiy vositalar bo‘yicha amortizatsiyani hisoblash hujjati.'
        ),
        (
            'fa_depreciation',
            'ru',
            'Амортизация основных средств',
            'Документ начисления амортизации основных средств.'
        ),
        (
            'fa_depreciation',
            'en',
            'Fixed asset depreciation',
            'Document for calculating fixed asset depreciation.'
        ),

        -- =========================================================
        -- fa_revaluation
        -- =========================================================
        (
            'fa_revaluation',
            'uz',
            'Asosiy vositalarni qayta baholash',
            'Asosiy vositalarning balans qiymatini qayta baholash hujjati.'
        ),
        (
            'fa_revaluation',
            'ru',
            'Переоценка основных средств',
            'Документ переоценки балансовой стоимости основных средств.'
        ),
        (
            'fa_revaluation',
            'en',
            'Fixed asset revaluation',
            'Document for revaluing the carrying amount of fixed assets.'
        ),

        -- =========================================================
        -- fa_disposal
        -- =========================================================
        (
            'fa_disposal',
            'uz',
            'Asosiy vositalarning chiqib ketishi',
            'Asosiy vositalarni sotish yoki hisobdan chiqarish hujjati.'
        ),
        (
            'fa_disposal',
            'ru',
            'Выбытие основных средств',
            'Документ продажи или списания основных средств.'
        ),
        (
            'fa_disposal',
            'en',
            'Fixed asset disposal',
            'Document for the sale or write-off of fixed assets.'
        )

) as v
(
    type_code,
    language_code,
    name,
    description
)
join acc_document_account_type t
    on t.code = v.type_code
join cmn_language l
    on l.code = v.language_code

on conflict (language_id, document_account_type_id)
do update set
    name        = excluded.name,
    description = excluded.description;
