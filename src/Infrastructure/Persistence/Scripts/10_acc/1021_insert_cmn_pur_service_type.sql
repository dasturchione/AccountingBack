insert into cmn_pur_service_type (name, account_id, vat_applicable, state_id)
select v.name, a.id, v.vat_applicable, 1
from (
    values
        ('Ma''muriy xizmatlar (IT/ijara/klining/buxgalteriya)', '9420', true),
        ('Ishlab chiqarish xizmatlari', '2010', true),
        ('Sotish bilan bog''liq xizmatlar', '9430', true)
) as v(name, account_code, vat_applicable)
join acc_chart_account a on a.code = v.account_code
where not exists (
    select 1
    from cmn_pur_service_type t
    where t.name = v.name
);
