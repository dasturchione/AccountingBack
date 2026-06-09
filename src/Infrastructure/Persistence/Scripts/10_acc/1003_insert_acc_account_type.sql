insert into acc_account_type (id, code, name, state_id)
values
(1, 'active', 'Aktiv', 1),
(2, 'passive', 'Passiv', 1),
(3, 'active_passive', 'Aktiv-passiv', 1),
(4, 'off_balance', 'Balansdan tashqari', 1)
on conflict do nothing;
