create table if not exists acc_document_account_role
(
	id					smallserial not null primary key,
	code				varchar(50) not null unique,
	name				varchar(250) not null,
	description			varchar(500),
	state_id			smallint not null references cmn_state(id)
);

begin;

insert into acc_document_account_role
(
    code,
    name,
    description,
    state_id
)
values
    (
        'supplier_settlement',
        'Yetkazib beruvchi hisobi',
        'Xarid hujjatida yetkazib beruvchi qarzi uchun ishlatiladigan hisob roli.',
        1
    ),
    (
        'purchase_debit',
        'Xarid debet hisobi',
        'Xarid qilingan tovar, material, xizmat yoki xarajat uchun debet hisob roli.',
        1
    ),
    (
        'purchase_vat',
        'Xarid QQS hisobi',
        'Xarid bo‘yicha QQS summasi uchun hisob roli.',
        1
    ),
    (
        'customer_settlement',
        'Xaridor hisobi',
        'Sotuv hujjatida xaridor qarzi uchun ishlatiladigan hisob roli.',
        1
    ),
    (
        'sale_vat',
        'Sotuv QQS hisobi',
        'Sotuv bo‘yicha QQS majburiyati uchun hisob roli.',
        1
    ),
    (
        'sale_inventory',
        'Sotuv tovar/material hisobi',
        'Sotuvda tovar yoki materialni ombordan chiqarish uchun hisob roli.',
        1
    ),
    (
        'sale_income',
        'Sotuv daromad hisobi',
        'Tovar, ish yoki xizmat sotishdan daromad uchun hisob roli.',
        1
    ),
    (
        'sale_cost',
        'Sotuv tannarx hisobi',
        'Sotilgan tovar, ish yoki xizmat tannarxi uchun hisob roli.',
        1
    ),
    (
        'bank_account',
        'Bank hisobi',
        'Bank operatsiyasida asosiy bank buxgalteriya hisobi roli.',
        1
    ),
    (
        'cash_account',
        'Kassa hisobi',
        'Kassa operatsiyasida asosiy kassa buxgalteriya hisobi roli.',
        1
    ),
    (
        'offset_account',
        'Qarshi hisob',
        'Bank yoki kassa operatsiyasida ikkinchi tomon hisob roli.',
        1
    ),
    (
        'salary_expense',
        'Ish haqi xarajati',
        'Ish haqi xarajatining debet hisobi.',
        1
    ),
    (
        'salary_payable',
        'Xodimlar bilan hisob-kitob',
        'Hisoblangan ish haqi majburiyati.',
        1
    ),
    (
        'deduction_payable',
        'Ushlanmalar majburiyati',
        'JShDS va boshqa ushlanmalar majburiyati.',
        1
    ),
    (
        'employer_tax_expense',
        'Ish beruvchi soligi xarajati',
        'Ijtimoiy soliq va boshqa ish beruvchi tolovlari xarajati.',
        1
    ),
    (
        'employer_tax_payable',
        'Ish beruvchi soligi majburiyati',
        'Ijtimoiy soliq va boshqa ish beruvchi tolovlari majburiyati.',
        1
    ),
    (
        'advance_receivable',
        'Xodimga berilgan avans',
        'Xodimga oldindan berilgan ish haqi avansi.',
        1
    ),
    (
        'acquiring_clearing',
        'Ekvayring bo‘yicha oraliq hisob',
        'Bank kartasi va mobil to‘lovlar bo‘yicha mablag‘lar bank hisobvarag‘iga tushguniga qadar ishlatiladigan oraliq hisob roli.',
        1
    )
on conflict (code)
do update set
    name        = excluded.name,
    description = excluded.description,
    state_id    = excluded.state_id;

commit;
