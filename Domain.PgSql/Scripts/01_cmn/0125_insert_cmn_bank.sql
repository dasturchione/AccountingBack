insert into cmn_bank (code, name, mfo, state_id)
values
('NBU', 'Milliy bank', null, 1),
('IPOTEKA', 'Ipoteka bank', null, 1),
('KAPITAL', 'Kapitalbank', null, 1),
('HAMKOR', 'Hamkorbank', null, 1)
on conflict do nothing;
