insert into acc_chart_account
(
    code,
    name,
    is_group,
    account_type_id,
    is_quantity,
    is_currency,
    state_id
)
select
    v.code,
    v.name,
    false,
    at.id,
    v.is_quantity,
    v.is_currency,
    1
from (
    values
    ('2010', 'Asosiy ishlab chiqarish', 'active', false, true),
    ('9420', 'Ma''muriy xarajatlar', 'active', false, true),
    ('9430', 'Boshqa operatsion xarajatlar', 'active', false, true)
) as v(code, name, account_type_code, is_quantity, is_currency)
join acc_account_type at on at.code = v.account_type_code
where not exists (
    select 1
    from acc_chart_account a
    where a.code = v.code
);