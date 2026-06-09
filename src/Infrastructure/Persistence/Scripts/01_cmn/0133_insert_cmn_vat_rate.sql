insert into cmn_vat_rate (code, name, rate, state_id)
values
('vat_0', 'QQS 0%', 0, 1),
('vat_12', 'QQS 12%', 12, 1),
('vat_15', 'QQS 15%', 15, 1)
on conflict do nothing;
