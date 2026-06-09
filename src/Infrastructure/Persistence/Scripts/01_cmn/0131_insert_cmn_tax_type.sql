insert into cmn_tax_type (code, name, state_id)
values
('none', 'Soliqsiz', 1),
('vat', 'QQS', 1)
on conflict do nothing;
