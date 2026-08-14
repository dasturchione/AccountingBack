begin;

insert into acc_document_account_type
(
    code,
    name,
    description,
    state_id
)
values
    ('purchase_goods',   'Tovar xaridi',            'Tovarlarni xarid qilish hujjati.', 1),
    ('purchase_service', 'Xizmat xaridi',           'Xizmatlarni xarid qilish hujjati.', 1),

    ('sale_goods',       'Tovar realizatsiyasi',    'Tovarlarni sotish hujjati.', 1),
    ('sale_service',     'Xizmat ko‘rsatish',       'Xizmatlarni sotish yoki ko‘rsatish hujjati.', 1),

    ('bank_income',      'Bank kirim',              'Bank hisobvarag‘iga pul kelib tushishi.', 1),
    ('bank_expense',     'Bank chiqim',             'Bank hisobvarag‘idan pul chiqishi.', 1),

    ('cash_income',      'Kassa kirim',             'Kassaga naqd pul kirimi.', 1),
    ('cash_expense',     'Kassa chiqim',            'Kassadan naqd pul chiqimi.', 1),

    ('payroll_accrual',  'Ish haqi hisoblash',      'Ish haqi, ushlanmalar va ish beruvchi soliqlarini hisoblash.', 1),

    ('retail_sale_goods', 'Tovar chakana sotuv', 'Chakana savdo uchun tovarlarni sotish hujjati.', 1),
    ('retail_payment_cash', 'Chakana savdo naqd to‘lov', 'Chakana savdo uchun naqd to‘lov hujjati.', 1),
    ('retail_payment_card', 'Chakana savdo karta to‘lov', 'Chakana savdo uchun karta orqali to‘lov hujjati.', 1),
    ('retail_payment_acquiring', 'Chakana savdo mobil to‘lovi', 'Chakana savdoda mobil va elektron to‘lov xizmatlari orqali amalga oshirilgan to‘lov.', 1),
    ('retail_payment_bank_transfer', 'Chakana savdo bank o‘tkazma to‘lov', 'Chakana savdo uchun bank o‘tkazmasi orqali to‘lov hujjati.', 1),
    ('retail_acquiring_settlement', 'Ekvayring bo‘yicha tushum', 'Ekvayring orqali qabul qilingan mablag‘larning bank hisobvarag‘iga kelib tushishi.', 1),

    ('fa_receipt',       'Asosiy vositalarni qabul qilish',       'Asosiy vositalarni qabul qilish hujjati.', 1),
    ('fa_commissioning', 'Asosiy vositalarni ishga tushirish',    'Asosiy vositalarni foydalanishga topshirish hujjati.', 1),
    ('fa_depreciation',  'Asosiy vositalar amortizatsiyasi',      'Asosiy vositalar bo‘yicha amortizatsiyani hisoblash hujjati.', 1),
    ('fa_revaluation',   'Asosiy vositalarni qayta baholash',     'Asosiy vositalarning balans qiymatini qayta baholash hujjati.', 1),
    ('fa_disposal',      'Asosiy vositalarning chiqib ketishi',   'Asosiy vositalarni sotish yoki hisobdan chiqarish hujjati.', 1)
on conflict (code)
do update set
    name        = excluded.name,
    description = excluded.description,
    state_id    = excluded.state_id;

commit;
