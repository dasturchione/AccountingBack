insert into cmn_operation_type (code, name, state_id)
values
('in', 'Kirim', 1),
('out', 'Chiqim', 1),
('transfer', 'O''tkazma', 1),
('debt_increase', 'Qarz oshishi', 1),
('debt_decrease', 'Qarz kamayishi', 1)
on conflict do nothing;
