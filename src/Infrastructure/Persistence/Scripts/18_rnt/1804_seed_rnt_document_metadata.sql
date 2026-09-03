insert into cmn_document_type(id, state_id, code, name)
values (26, 1, 'rental_accrual', 'Ijara hisob-kitobi')
on conflict (id) do update set
    state_id = excluded.state_id,
    code = excluded.code,
    name = excluded.name;

insert into cmn_document_type_translation(document_type_id, language_id, name)
select document_type.id, language.id, translation.name
from
(
    values
        ('uz', 'Ijara hisob-kitobi'),
        ('uz_Cyrl', 'Ижара ҳисоб-китоби'),
        ('ru', 'Начисление аренды'),
        ('en', 'Rental accrual')
) as translation(language_code, name)
join cmn_document_type document_type on document_type.code = 'rental_accrual'
join cmn_language language on language.code = translation.language_code
on conflict (document_type_id, language_id) do update set
    name = excluded.name;

insert into acc_document_account_type(code, name, description, state_id)
values ('rental_accrual', 'Ijara hisob-kitobi', 'Jismoniy shaxsdan ijara bo''yicha hisob-kitob.', 1)
on conflict (code) do update set
    name = excluded.name,
    description = excluded.description,
    state_id = excluded.state_id;

insert into acc_document_account_type_translation(language_id, document_account_type_id, name, description)
select language.id, account_type.id, translation.name, translation.description
from
(
    values
        ('uz', 'Ijara hisob-kitobi', 'Ijara xarajati, jismoniy shaxs va JShDS hisoblari uchun tavsiyalar.'),
        ('uz_Cyrl', 'Ижара ҳисоб-китоби', 'Ижара харажати, жисмоний шахс ва ЖШДС ҳисоблари учун тавсиялар.'),
        ('ru', 'Начисление аренды', 'Рекомендации счетов расхода, физлица и НДФЛ.'),
        ('en', 'Rental accrual', 'Suggested expense, lessor and personal income tax accounts.')
) as translation(language_code, name, description)
join cmn_language language on language.code = translation.language_code
join acc_document_account_type account_type on account_type.code = 'rental_accrual'
on conflict (language_id, document_account_type_id) do update set
    name = excluded.name,
    description = excluded.description;

insert into acc_document_account_role(code, name, description, state_id)
values
    ('rent_expense', 'Ijara xarajati', 'Ijara hisob-kitobi debet xarajat hisobi uchun tavsiya.', 1),
    ('lessor_payable', 'Ijara beruvchiga qarz', 'Jismoniy shaxsga to''lanadigan summa krediti uchun tavsiya.', 1),
    ('tax_payable', 'JShDS majburiyati', 'Ijara bo''yicha JShDS krediti uchun tavsiya.', 1)
on conflict (code) do update set
    name = excluded.name,
    description = excluded.description,
    state_id = excluded.state_id;

insert into acc_document_account_role_translation(document_account_role_id, language_id, name, description)
select role.id, language.id, translation.name, translation.description
from
(
    values
        ('rent_expense', 'uz', 'Ijara xarajati', 'Ijara xarajati debet hisobi.'),
        ('lessor_payable', 'uz', 'Ijara beruvchiga qarz', 'Jismoniy shaxsga to''lov majburiyati.'),
        ('tax_payable', 'uz', 'JShDS majburiyati', 'Ijara bo''yicha JShDS majburiyati.'),
        ('rent_expense', 'uz_Cyrl', 'Ижара харажати', 'Ижара харажати дебет ҳисоби.'),
        ('lessor_payable', 'uz_Cyrl', 'Ижара берувчига қарз', 'Жисмоний шахсга тўлов мажбурияти.'),
        ('tax_payable', 'uz_Cyrl', 'ЖШДС мажбурияти', 'Ижара бўйича ЖШДС мажбурияти.'),
        ('rent_expense', 'ru', 'Расходы по аренде', 'Дебетовый счёт расходов по аренде.'),
        ('lessor_payable', 'ru', 'Задолженность арендодателю', 'Обязательство перед физическим лицом.'),
        ('tax_payable', 'ru', 'НДФЛ к уплате', 'Обязательство по НДФЛ с аренды.'),
        ('rent_expense', 'en', 'Rental expense', 'Rental expense debit account.'),
        ('lessor_payable', 'en', 'Lessor payable', 'Liability payable to the individual lessor.'),
        ('tax_payable', 'en', 'Personal income tax payable', 'Rental personal income tax liability.')
) as translation(role_code, language_code, name, description)
join acc_document_account_role role on role.code = translation.role_code
join cmn_language language on language.code = translation.language_code
on conflict (document_account_role_id, language_id) do update set
    name = excluded.name,
    description = excluded.description;

insert into acc_document_account_type_role
    (document_account_type_id, document_account_role_id, account_side, is_required, sort_order)
select account_type.id, role.id, mapping.account_side, true, mapping.sort_order
from
(
    values
        ('rent_expense', 'debit', 1),
        ('lessor_payable', 'credit', 2),
        ('tax_payable', 'credit', 3)
) as mapping(role_code, account_side, sort_order)
join acc_document_account_type account_type on account_type.code = 'rental_accrual'
join acc_document_account_role role on role.code = mapping.role_code
on conflict (document_account_type_id, document_account_role_id) do update set
    account_side = excluded.account_side,
    is_required = excluded.is_required,
    sort_order = excluded.sort_order;
