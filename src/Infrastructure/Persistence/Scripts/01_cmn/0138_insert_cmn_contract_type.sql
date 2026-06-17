insert into cmn_contract_type (id, code, name, state_id)
values
(1, 'supplier', 'Postavshik bilan shartnoma', 1),
(2, 'customer', 'Xaridor bilan shartnoma', 1)
on conflict do nothing;
